using System.Net;
using System.Net.Http.Json;
using Microsoft.Extensions.DependencyInjection;
using Operis_API.Infrastructure.Persistence;
using Operis_API.Shared.ActivityLogging;
using Operis_API.Tests.IntegrationTests.Support;
using Microsoft.EntityFrameworkCore;

namespace Operis_API.Tests.IntegrationTests;

public class ActivitiesApiTests : IClassFixture<OperisApiFactory>
{
    private readonly HttpClient _client;
    private readonly OperisApiFactory _factory;

    public ActivitiesApiTests(OperisApiFactory factory)
    {
        _factory = factory;
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetActivities_ReturnsOkAndData()
    {
        // Arrange
        using (var scope = _factory.Services.CreateScope())
        {
            var db = scope.ServiceProvider.GetRequiredService<OperisDbContext>();
            await db.Database.EnsureCreatedAsync();

            db.ActivityLogs.Add(new ActivityLogEntity
            {
                Id = Guid.NewGuid(),
                OccurredAt = DateTimeOffset.UtcNow,
                Module = "test-module",
                Action = "test-action",
                EntityType = "test-entity",
                Status = "success",
                Source = "test-source",
                ActorType = "user",
                CreatedAt = DateTimeOffset.UtcNow
            });
            await db.SaveChangesAsync();
        }

        // Act: Call the correct API endpoint path found in ActivitiesModule.cs
        var response = await _client.GetAsync("/api/v1/activity-logs");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        
        var data = await response.Content.ReadFromJsonAsync<dynamic>();
        Assert.NotNull(data);
    }
}
