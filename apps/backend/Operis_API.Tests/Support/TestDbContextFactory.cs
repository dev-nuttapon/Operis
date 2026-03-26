using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace Operis_API.Tests.Support;

internal static class TestDbContextFactory
{
    public static Operis_API.Infrastructure.Persistence.OperisDbContext Create()
    {
        var options = new DbContextOptionsBuilder<Operis_API.Infrastructure.Persistence.OperisDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .ConfigureWarnings(x => x.Ignore(InMemoryEventId.TransactionIgnoredWarning))
            .Options;

        return new Operis_API.Infrastructure.Persistence.OperisDbContext(options);
    }
}
