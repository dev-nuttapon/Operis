using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Text.Encodings.Web;
using System.IO;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using Operis_API.Infrastructure.Persistence;
using Operis_API.Shared.Security;
using Operis_API.Shared.Auditing;
using Operis_API.Tests.Support;

namespace Operis_API.Tests.IntegrationTests.Support;

public class OperisApiFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // 1. Point to the API project directory explicitly
        var assemblyPath = typeof(OperisApiFactory).Assembly.Location;
        var directory = new DirectoryInfo(Path.GetDirectoryName(assemblyPath)!);
        while (directory != null && directory.Name != "backend") directory = directory.Parent;
        
        if (directory != null)
        {
            var apiPath = Path.Combine(directory.FullName, "Operis_API");
            builder.UseContentRoot(apiPath);
        }

        builder.UseEnvironment("Testing");

        builder.ConfigureServices(services =>
        {
            // 2. Remove ALL registrations of OperisDbContext and its options
            var descriptors = services.Where(d => 
                d.ServiceType == typeof(OperisDbContext) || 
                d.ServiceType == typeof(DbContextOptions<OperisDbContext>) ||
                d.ServiceType == typeof(DbContextOptions)).ToList();

            foreach (var descriptor in descriptors)
            {
                services.Remove(descriptor);
            }

            // 3. Register using ONLY In-Memory provider
            services.AddDbContext<OperisDbContext>(options =>
            {
                options.UseInMemoryDatabase("IntegrationTestDb");
            });

            // 4. Mock core services
            services.AddSingleton<IPermissionMatrix, PermissionMatrix>();
            services.AddSingleton<IAuditLogWriter, FakeAuditLogWriter>();

            // 5. Mock Authentication
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = "TestScheme";
                options.DefaultChallengeScheme = "TestScheme";
            })
            .AddScheme<AuthenticationSchemeOptions, MockAuthHandler>("TestScheme", options => { });
        });
    }

    private class MockAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
    {
        public MockAuthHandler(IOptionsMonitor<AuthenticationSchemeOptions> options, ILoggerFactory logger, UrlEncoder encoder) 
            : base(options, logger, encoder) { }

        protected override Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            var claims = new[] { 
                new Claim(ClaimTypes.Name, "TestUser"),
                new Claim(ClaimTypes.Role, "operis:super_admin"),
                new Claim("preferred_username", "testuser")
            };
            var identity = new ClaimsIdentity(claims, "Test");
            var principal = new ClaimsPrincipal(identity);
            var ticket = new AuthenticationTicket(principal, "TestScheme");
            return Task.FromResult(AuthenticateResult.Success(ticket));
        }
    }
}
