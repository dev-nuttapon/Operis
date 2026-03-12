using Microsoft.EntityFrameworkCore;
using Operis_API.Modules.Documents.Infrastructure;
using Operis_API.Modules.Users.Infrastructure;

namespace Operis_API.Infrastructure.Persistence;

public sealed class OperisDbContext(DbContextOptions<OperisDbContext> options) : DbContext(options)
{
    public DbSet<DocumentEntity> Documents => Set<DocumentEntity>();
    public DbSet<UserEntity> Users => Set<UserEntity>();
    public DbSet<UserRegistrationRequestEntity> UserRegistrationRequests => Set<UserRegistrationRequestEntity>();
    public DbSet<UserInvitationEntity> UserInvitations => Set<UserInvitationEntity>();

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

        modelBuilder.Entity<UserEntity>(entity =>
        {
            entity.ToTable("users");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.Email).HasColumnName("email").HasMaxLength(320);
            entity.Property(x => x.FirstName).HasColumnName("first_name").HasMaxLength(120);
            entity.Property(x => x.LastName).HasColumnName("last_name").HasMaxLength(120);
            entity.Property(x => x.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(20);
            entity.Property(x => x.CreatedAt).HasColumnName("created_at");
            entity.Property(x => x.CreatedBy).HasColumnName("created_by").HasMaxLength(120);
            entity.Property(x => x.ApprovedAt).HasColumnName("approved_at");
            entity.HasIndex(x => x.Email).IsUnique();
        });

        modelBuilder.Entity<UserRegistrationRequestEntity>(entity =>
        {
            entity.ToTable("user_registration_requests");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.Email).HasColumnName("email").HasMaxLength(320);
            entity.Property(x => x.FirstName).HasColumnName("first_name").HasMaxLength(120);
            entity.Property(x => x.LastName).HasColumnName("last_name").HasMaxLength(120);
            entity.Property(x => x.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(20);
            entity.Property(x => x.RequestedAt).HasColumnName("requested_at");
            entity.Property(x => x.ReviewedAt).HasColumnName("reviewed_at");
            entity.Property(x => x.ReviewedBy).HasColumnName("reviewed_by").HasMaxLength(120);
            entity.Property(x => x.RejectionReason).HasColumnName("rejection_reason").HasMaxLength(500);
            entity.HasIndex(x => new { x.Email, x.Status });
        });

        modelBuilder.Entity<UserInvitationEntity>(entity =>
        {
            entity.ToTable("user_invitations");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.Email).HasColumnName("email").HasMaxLength(320);
            entity.Property(x => x.InvitedBy).HasColumnName("invited_by").HasMaxLength(120);
            entity.Property(x => x.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(20);
            entity.Property(x => x.InvitedAt).HasColumnName("invited_at");
            entity.Property(x => x.ExpiresAt).HasColumnName("expires_at");
            entity.Property(x => x.AcceptedAt).HasColumnName("accepted_at");
            entity.Property(x => x.RejectedAt).HasColumnName("rejected_at");
            entity.HasIndex(x => new { x.Email, x.Status });
        });
    }
}
