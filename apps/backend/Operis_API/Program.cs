using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http.Json;
using Microsoft.EntityFrameworkCore;
using Operis_API.Infrastructure.Persistence;
using Operis_API.Modules.Users.Infrastructure;
using Operis_API.Shared.Auditing;
using Operis_API.Shared.Configuration;
using Operis_API.Shared.Modules;
using Operis_API.Shared.Security;

var builder = WebApplication.CreateBuilder(args);
if (!builder.Environment.IsEnvironment("Testing"))
{
    _ = Phase0ConfigurationValidator.Validate(builder.Configuration);
}

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

builder.Services.AddProblemDetails();

builder.Services.Configure<JsonOptions>(options =>
{
    options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
    options.SerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    options.SerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.WithOrigins(
                "http://localhost:3000",
                "http://localhost:5173",
                "http://127.0.0.1:3000",
                "http://127.0.0.1:5173",
                "http://localhost:4173",
                "http://127.0.0.1:4173")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

var keycloakOptions = builder.Configuration.GetSection(KeycloakOptions.SectionName).Get<KeycloakOptions>();
if (keycloakOptions == null || string.IsNullOrWhiteSpace(keycloakOptions.BaseUrl) || string.IsNullOrWhiteSpace(keycloakOptions.Realm))
{
    // Provide safe defaults for testing/dev if not configured
    keycloakOptions = new KeycloakOptions { BaseUrl = "http://localhost:8080", Realm = "operis" };
}

var keycloakAuthority = $"{keycloakOptions.BaseUrl.TrimEnd('/')}/realms/{keycloakOptions.Realm}";
var keycloakBaseUri = new Uri(keycloakOptions.BaseUrl);
var expectedAudience = builder.Configuration[$"{KeycloakOptions.SectionName}:ApiAudience"];

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = keycloakAuthority;
        options.Audience = expectedAudience;
        options.RequireHttpsMetadata = false;
        options.MapInboundClaims = false;

        options.TokenValidationParameters = new Microsoft.IdentityModel.Tokens.TokenValidationParameters
        {
            ValidateAudience = !string.IsNullOrWhiteSpace(expectedAudience),
            RoleClaimType = ClaimTypes.Role,
            NameClaimType = "preferred_username",
            ValidateIssuer = true
        };

        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = context =>
            {
                MapKeycloakRolesToClaims(context);
                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = options.DefaultPolicy;
});

builder.Services.Configure<Phase0SecurityOptions>(builder.Configuration.GetSection(Phase0SecurityOptions.SectionName));

// Register core shared services
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IPermissionMatrix, PermissionMatrix>();
builder.Services.AddScoped<IAuditLogWriter, AuditLogWriter>();

if (!builder.Environment.IsEnvironment("Testing"))
{
    builder.Services.AddDbContext<OperisDbContext>(options =>
    {
        var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                               ?? throw new InvalidOperationException("String 'DefaultConnection' is not configured.");
        options.UseNpgsql(connectionString);
    });
}

var redisConnectionString = builder.Configuration["Redis:ConnectionString"];
if (!string.IsNullOrWhiteSpace(redisConnectionString))
{
    builder.Services.AddStackExchangeRedisCache(options =>
    {
        options.Configuration = redisConnectionString;
        options.InstanceName = "operis:";
    });
}
else
{
    builder.Services.AddDistributedMemoryCache();
}

builder.Services.AddModules(builder.Configuration);

var app = builder.Build();

app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<AuditFailureLoggingMiddleware>();

app.MapModules();

app.MapControllers();

app.Run();

static void MapKeycloakRolesToClaims(TokenValidatedContext context)
{
    if (context.Principal?.Identity is ClaimsIdentity identity && context.SecurityToken is System.IdentityModel.Tokens.Jwt.JwtSecurityToken jwtToken)
    {
        var realmAccessClaim = jwtToken.Claims.FirstOrDefault(c => c.Type == "realm_access")?.Value;
        if (string.IsNullOrWhiteSpace(realmAccessClaim)) return;

        using var doc = JsonDocument.Parse(realmAccessClaim);
        if (doc.RootElement.TryGetProperty("roles", out var roles))
        {
            foreach (var role in roles.EnumerateArray())
            {
                var roleName = role.GetString();
                if (string.IsNullOrWhiteSpace(roleName)) continue;
                if (!identity.HasClaim(ClaimTypes.Role, roleName))
                {
                    identity.AddClaim(new Claim(ClaimTypes.Role, roleName));
                }
            }
        }
    }
}

public partial class Program { }
