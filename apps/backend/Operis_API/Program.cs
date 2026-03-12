using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.OpenApi;
using Operis_API.Shared.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Operis_API.Infrastructure.Persistence;
using Operis_API.Modules.Users.Infrastructure;
using Operis_API.Shared.Modules;

var builder = WebApplication.CreateBuilder(args);

builder.Configuration.ApplyDatabaseUrlOverride();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    var bearerScheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Paste a Keycloak access token. Example: Bearer eyJ..."
    };

    options.AddSecurityDefinition("Bearer", bearerScheme);

    options.AddSecurityRequirement(_ => new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecuritySchemeReference("Bearer", null!, string.Empty),
            []
        }
    });
});
builder.Services.AddHealthChecks();

var keycloakOptions = builder.Configuration.GetSection(KeycloakOptions.SectionName).Get<KeycloakOptions>()
                      ?? throw new InvalidOperationException("Keycloak configuration is missing.");
if (string.IsNullOrWhiteSpace(keycloakOptions.BaseUrl) || string.IsNullOrWhiteSpace(keycloakOptions.Realm))
{
    throw new InvalidOperationException("Keycloak BaseUrl and Realm must be configured.");
}

var keycloakAuthority = $"{keycloakOptions.BaseUrl.TrimEnd('/')}/realms/{keycloakOptions.Realm}";
var keycloakBaseUri = new Uri(keycloakOptions.BaseUrl, UriKind.Absolute);
var expectedAudience = builder.Configuration[$"{KeycloakOptions.SectionName}:ApiAudience"];

builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.Authority = keycloakAuthority;
        options.RequireHttpsMetadata = keycloakBaseUri.Scheme.Equals(Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase);
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidIssuer = keycloakAuthority,
            ValidateAudience = !string.IsNullOrWhiteSpace(expectedAudience),
            ValidAudience = expectedAudience,
            NameClaimType = "preferred_username",
            RoleClaimType = ClaimTypes.Role
        };
        options.Events = new JwtBearerEvents
        {
            OnTokenValidated = context =>
            {
                if (context.Principal?.Identity is ClaimsIdentity identity)
                {
                    AddRoleClaims(identity, "realm_access");
                    AddResourceAccessRoleClaims(identity, "resource_access");
                }

                return Task.CompletedTask;
            }
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = options.DefaultPolicy;
});

builder.Services.AddDbContext<OperisDbContext>(options =>
{
    var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
                           ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured.");
    options.UseNpgsql(connectionString);
});
builder.Services.AddModules(builder.Configuration);

var app = builder.Build();
var serverUrls = builder.Configuration["ASPNETCORE_URLS"] ?? builder.Configuration["urls"];
var hasHttpsEndpoint = !string.IsNullOrWhiteSpace(serverUrls)
                       && serverUrls.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
                           .Any(url => url.StartsWith("https://", StringComparison.OrdinalIgnoreCase));
var enableSwagger = app.Environment.IsDevelopment() || app.Environment.IsEnvironment("Local");

if (enableSwagger)
{
    app.MapSwagger().AllowAnonymous();
    app.UseSwaggerUI();
}

if (hasHttpsEndpoint)
{
    app.UseHttpsRedirection();
}

app.UseAuthentication();
app.UseAuthorization();

app.MapHealthChecks("/health").AllowAnonymous();
app.MapModules();

app.Run();

static void AddRoleClaims(ClaimsIdentity identity, string claimType)
{
    var value = identity.FindFirst(claimType)?.Value;
    if (string.IsNullOrWhiteSpace(value))
    {
        return;
    }

    using var document = JsonDocument.Parse(value);
    if (!document.RootElement.TryGetProperty("roles", out var roles) || roles.ValueKind != JsonValueKind.Array)
    {
        return;
    }

    foreach (var role in roles.EnumerateArray())
    {
        var roleName = role.GetString();
        if (string.IsNullOrWhiteSpace(roleName))
        {
            continue;
        }

        if (!identity.HasClaim(ClaimTypes.Role, roleName))
        {
            identity.AddClaim(new Claim(ClaimTypes.Role, roleName));
        }
    }
}

static void AddResourceAccessRoleClaims(ClaimsIdentity identity, string claimType)
{
    var value = identity.FindFirst(claimType)?.Value;
    if (string.IsNullOrWhiteSpace(value))
    {
        return;
    }

    using var document = JsonDocument.Parse(value);
    if (document.RootElement.ValueKind != JsonValueKind.Object)
    {
        return;
    }

    foreach (var clientEntry in document.RootElement.EnumerateObject())
    {
        var client = clientEntry.Value;
        if (!client.TryGetProperty("roles", out var roles) || roles.ValueKind != JsonValueKind.Array)
        {
            continue;
        }

        foreach (var role in roles.EnumerateArray())
        {
            var roleName = role.GetString();
            if (string.IsNullOrWhiteSpace(roleName))
            {
                continue;
            }

            if (!identity.HasClaim(ClaimTypes.Role, roleName))
            {
                identity.AddClaim(new Claim(ClaimTypes.Role, roleName));
            }
        }
    }
}
