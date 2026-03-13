using Microsoft.EntityFrameworkCore;
using Operis_API.Infrastructure.Persistence;

namespace Operis_API.Tests.Support;

internal static class TestDbContextFactory
{
    public static OperisDbContext Create()
    {
        var options = new DbContextOptionsBuilder<OperisDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString("N"))
            .Options;

        return new OperisDbContext(options);
    }
}
