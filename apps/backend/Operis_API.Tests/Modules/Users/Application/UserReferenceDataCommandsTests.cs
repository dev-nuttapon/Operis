using Microsoft.EntityFrameworkCore;
using Operis_API.Modules.Users.Application;
using Operis_API.Modules.Users.Contracts;
using Operis_API.Modules.Users.Infrastructure;
using Operis_API.Tests.Support;

namespace Operis_API.Tests.Modules.Users.Application;

public sealed class UserReferenceDataCommandsTests
{
    [Fact]
    public async Task CreateDepartmentAsync_WhenSuccessful_PersistsAndInvalidatesCacheOnce()
    {
        await using var dbContext = TestDbContextFactory.Create();
        var auditLogWriter = new FakeAuditLogWriter();
        var referenceDataCache = new TestReferenceDataCache();
        var sut = new UserReferenceDataCommands(dbContext, auditLogWriter, referenceDataCache);

        var result = await sut.CreateDepartmentAsync(
            new CreateMasterDataRequest("Quality", 10),
            CancellationToken.None);

        var department = await dbContext.Departments.SingleAsync(x => x.Name == "Quality");

        Assert.Equal(MasterDataCommandStatus.Success, result.Status);
        Assert.Equal(department.Id, result.Response!.Id);
        Assert.Equal(1, referenceDataCache.InvalidateDepartmentsCalls);
        Assert.Single(auditLogWriter.Entries);
    }

    [Fact]
    public async Task CreateDepartmentAsync_WhenConflict_DoesNotInvalidateCacheOrWriteAudit()
    {
        await using var dbContext = TestDbContextFactory.Create();
        dbContext.Departments.Add(new DepartmentEntity
        {
            Id = Guid.NewGuid(),
            Name = "Quality",
            DisplayOrder = 1,
            CreatedAt = DateTimeOffset.UtcNow
        });
        await dbContext.SaveChangesAsync();

        var auditLogWriter = new FakeAuditLogWriter();
        var referenceDataCache = new TestReferenceDataCache();
        var sut = new UserReferenceDataCommands(dbContext, auditLogWriter, referenceDataCache);

        var result = await sut.CreateDepartmentAsync(
            new CreateMasterDataRequest("Quality", 10),
            CancellationToken.None);

        Assert.Equal(MasterDataCommandStatus.Conflict, result.Status);
        Assert.Equal(0, referenceDataCache.InvalidateDepartmentsCalls);
        Assert.Empty(auditLogWriter.Entries);
    }
}
