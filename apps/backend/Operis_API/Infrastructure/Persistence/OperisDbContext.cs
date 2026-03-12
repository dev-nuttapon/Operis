using Microsoft.EntityFrameworkCore;
using Operis_API.Modules.Documents.Infrastructure;

namespace Operis_API.Infrastructure.Persistence;

public sealed class OperisDbContext(DbContextOptions<OperisDbContext> options) : DbContext(options)
{
    public DbSet<DocumentEntity> Documents => Set<DocumentEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<DocumentEntity>(entity =>
        {
            entity.ToTable("documents");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.FileName).HasColumnName("file_name").HasMaxLength(256);
            entity.Property(x => x.UploadedAt).HasColumnName("uploaded_at");
        });
    }
}
