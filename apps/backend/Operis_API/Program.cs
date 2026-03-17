using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.OpenApi;
using Operis_API.Shared.Configuration;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Operis_API.Infrastructure.Persistence;
using Operis_API.Modules.Users.Infrastructure;
using Operis_API.Shared.Auditing;
using Operis_API.Shared.Modules;
using Operis_API.Shared.Security;
using System.Globalization;

var builder = WebApplication.CreateBuilder(args);

// Ensure AWS SigV4 date formatting uses Gregorian calendar (avoid Thai Buddhist year in local culture).
CultureInfo.DefaultThreadCurrentCulture = CultureInfo.InvariantCulture;
CultureInfo.DefaultThreadCurrentUICulture = CultureInfo.InvariantCulture;

builder.Configuration.ApplyDatabaseUrlOverride();
builder.Configuration.ApplyRedisUrlOverride();

if (builder.Environment.IsEnvironment("Local"))
{
    var endpoint = builder.Configuration["Minio:Endpoint"];
    var accessKey = builder.Configuration["Minio:AccessKey"];
    var secretKey = builder.Configuration["Minio:SecretKey"];

    var endpointMissingHost = !string.IsNullOrWhiteSpace(endpoint) && endpoint.StartsWith(":", StringComparison.Ordinal);
    if (string.IsNullOrWhiteSpace(endpoint) || endpointMissingHost ||
        string.IsNullOrWhiteSpace(accessKey) || string.IsNullOrWhiteSpace(secretKey))
    {
        var localConfig = new ConfigurationBuilder()
            .AddJsonFile("appsettings.Local.json", optional: true)
            .Build();

        var fallbackEndpoint = localConfig["Minio:Endpoint"];
        var fallbackAccessKey = localConfig["Minio:AccessKey"];
        var fallbackSecretKey = localConfig["Minio:SecretKey"];
        var fallbackBucket = localConfig["Minio:BucketName"];
        var fallbackUseSsl = localConfig["Minio:UseSsl"];
        var fallbackMaxSize = localConfig["Minio:MaxFileSizeBytes"];

        var overrides = new Dictionary<string, string?>();
        if ((string.IsNullOrWhiteSpace(endpoint) || endpointMissingHost) && !string.IsNullOrWhiteSpace(fallbackEndpoint))
        {
            overrides["Minio:Endpoint"] = fallbackEndpoint;
        }
        if (string.IsNullOrWhiteSpace(accessKey) && !string.IsNullOrWhiteSpace(fallbackAccessKey))
        {
            overrides["Minio:AccessKey"] = fallbackAccessKey;
        }
        if (string.IsNullOrWhiteSpace(secretKey) && !string.IsNullOrWhiteSpace(fallbackSecretKey))
        {
            overrides["Minio:SecretKey"] = fallbackSecretKey;
        }
        if (!string.IsNullOrWhiteSpace(fallbackBucket))
        {
            overrides["Minio:BucketName"] = fallbackBucket;
        }
        if (!string.IsNullOrWhiteSpace(fallbackUseSsl))
        {
            overrides["Minio:UseSsl"] = fallbackUseSsl;
        }
        if (!string.IsNullOrWhiteSpace(fallbackMaxSize))
        {
            overrides["Minio:MaxFileSizeBytes"] = fallbackMaxSize;
        }

        if (overrides.Count > 0)
        {
            builder.Configuration.AddInMemoryCollection(overrides);
        }
    }

    var resolvedEndpoint = builder.Configuration["Minio:Endpoint"];
    var resolvedAccessKey = builder.Configuration["Minio:AccessKey"];
    var resolvedSecretKey = builder.Configuration["Minio:SecretKey"];
    var resolvedBucket = builder.Configuration["Minio:BucketName"];
    var resolvedUseSsl = builder.Configuration["Minio:UseSsl"];
    var resolvedMaxSize = builder.Configuration["Minio:MaxFileSizeBytes"];

    Console.WriteLine("[Local] Minio config loaded:");
    Console.WriteLine($"Endpoint={resolvedEndpoint ?? "<null>"}");
    Console.WriteLine($"AccessKey={resolvedAccessKey ?? "<null>"}");
    if (string.IsNullOrWhiteSpace(resolvedSecretKey))
    {
        Console.WriteLine("SecretKey=<null/empty>");
    }
    else
    {
        var tail = resolvedSecretKey.Length >= 4 ? resolvedSecretKey[^4..] : resolvedSecretKey;
        Console.WriteLine($"SecretKey=<set,len={resolvedSecretKey.Length},tail={tail}>");
    }
    Console.WriteLine($"BucketName={resolvedBucket ?? "<null>"}");
    Console.WriteLine($"UseSsl={resolvedUseSsl ?? "<null>"}");
    Console.WriteLine($"MaxFileSizeBytes={resolvedMaxSize ?? "<null>"}");

    var envEndpoint = Environment.GetEnvironmentVariable("Minio__Endpoint");
    var envAccessKey = Environment.GetEnvironmentVariable("Minio__AccessKey");
    var envSecretKey = Environment.GetEnvironmentVariable("Minio__SecretKey");
    if (envEndpoint is not null || envAccessKey is not null || envSecretKey is not null)
    {
        Console.WriteLine("[Local] Minio env overrides detected:");
        Console.WriteLine($"Minio__Endpoint={(envEndpoint is null ? "<not set>" : (string.IsNullOrWhiteSpace(envEndpoint) ? "<empty>" : envEndpoint))}");
        Console.WriteLine($"Minio__AccessKey={(envAccessKey is null ? "<not set>" : (string.IsNullOrWhiteSpace(envAccessKey) ? "<empty>" : envAccessKey))}");
        if (envSecretKey is null)
        {
            Console.WriteLine("Minio__SecretKey=<not set>");
        }
        else if (string.IsNullOrWhiteSpace(envSecretKey))
        {
            Console.WriteLine("Minio__SecretKey=<empty>");
        }
        else
        {
            var envTail = envSecretKey.Length >= 4 ? envSecretKey[^4..] : envSecretKey;
            Console.WriteLine($"Minio__SecretKey=<set,len={envSecretKey.Length},tail={envTail}>");
        }
    }
}

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IAuditLogWriter, AuditLogWriter>();
builder.Services.AddSingleton<IPermissionMatrix, PermissionMatrix>();
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
});
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
builder.Services.AddCors(options =>
{
    options.AddPolicy("LocalFrontend", policy =>
    {
        policy
            .WithOrigins(
                "http://localhost:5173",
                "http://127.0.0.1:5173",
                "http://localhost:4173",
                "http://127.0.0.1:4173")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

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

app.UseForwardedHeaders(new ForwardedHeadersOptions
{
    ForwardedHeaders = Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedFor | Microsoft.AspNetCore.HttpOverrides.ForwardedHeaders.XForwardedProto
});

app.UseCors("LocalFrontend");
app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<AuditFailureLoggingMiddleware>();

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
