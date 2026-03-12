using Microsoft.EntityFrameworkCore;
using Operis_API.Modules.Documents.Infrastructure;
using Operis_API.Modules.Users.Infrastructure;
using Operis_API.Shared.Auditing;

namespace Operis_API.Infrastructure.Persistence;

public sealed class OperisDbContext(DbContextOptions<OperisDbContext> options) : DbContext(options)
{
    public DbSet<DocumentEntity> Documents => Set<DocumentEntity>();
    public DbSet<UserEntity> Users => Set<UserEntity>();
    public DbSet<DepartmentEntity> Departments => Set<DepartmentEntity>();
    public DbSet<JobTitleEntity> JobTitles => Set<JobTitleEntity>();
    public DbSet<AppRoleEntity> AppRoles => Set<AppRoleEntity>();
    public DbSet<UserRegistrationRequestEntity> UserRegistrationRequests => Set<UserRegistrationRequestEntity>();
    public DbSet<UserInvitationEntity> UserInvitations => Set<UserInvitationEntity>();
    public DbSet<AuditLogEntity> AuditLogs => Set<AuditLogEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<DocumentEntity>(entity =>
        {
            entity.ToTable("documents");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.FileName).HasColumnName("file_name").HasMaxLength(256);
            entity.Property(x => x.UploadedAt).HasColumnName("uploaded_at");
            entity.HasIndex(x => x.UploadedAt);
        });

        modelBuilder.Entity<UserEntity>(entity =>
        {
            entity.ToTable("users");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id").HasMaxLength(64);
            entity.Property(x => x.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(20);
            entity.Property(x => x.CreatedAt).HasColumnName("created_at");
            entity.Property(x => x.CreatedBy).HasColumnName("created_by").HasMaxLength(120);
            entity.Property(x => x.DepartmentId).HasColumnName("department_id");
            entity.Property(x => x.JobTitleId).HasColumnName("job_title_id");
            entity.Property(x => x.PreferredLanguage).HasColumnName("preferred_language").HasMaxLength(16);
            entity.Property(x => x.PreferredTheme).HasColumnName("preferred_theme").HasMaxLength(16);
            entity.Property(x => x.DeletedReason).HasColumnName("deleted_reason").HasMaxLength(500);
            entity.Property(x => x.DeletedBy).HasColumnName("deleted_by").HasMaxLength(120);
            entity.Property(x => x.DeletedAt).HasColumnName("deleted_at");
            entity.HasOne<DepartmentEntity>().WithMany().HasForeignKey(x => x.DepartmentId).OnDelete(DeleteBehavior.SetNull);
            entity.HasOne<JobTitleEntity>().WithMany().HasForeignKey(x => x.JobTitleId).OnDelete(DeleteBehavior.SetNull);
            entity.HasIndex(x => new { x.DeletedAt, x.CreatedAt });
            entity.HasIndex(x => x.DepartmentId);
            entity.HasIndex(x => x.JobTitleId);
        });

        modelBuilder.Entity<DepartmentEntity>(entity =>
        {
            entity.ToTable("departments");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.Name).HasColumnName("name").HasMaxLength(120);
            entity.Property(x => x.DisplayOrder).HasColumnName("display_order");
            entity.Property(x => x.CreatedAt).HasColumnName("created_at");
            entity.Property(x => x.UpdatedAt).HasColumnName("updated_at");
            entity.Property(x => x.DeletedReason).HasColumnName("deleted_reason").HasMaxLength(500);
            entity.Property(x => x.DeletedBy).HasColumnName("deleted_by").HasMaxLength(120);
            entity.Property(x => x.DeletedAt).HasColumnName("deleted_at");
            entity.HasIndex(x => x.Name).IsUnique().HasFilter("\"deleted_at\" IS NULL");
            entity.HasIndex(x => new { x.DeletedAt, x.DisplayOrder, x.Name });
        });

        modelBuilder.Entity<JobTitleEntity>(entity =>
        {
            entity.ToTable("job_titles");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.Name).HasColumnName("name").HasMaxLength(120);
            entity.Property(x => x.DisplayOrder).HasColumnName("display_order");
            entity.Property(x => x.CreatedAt).HasColumnName("created_at");
            entity.Property(x => x.UpdatedAt).HasColumnName("updated_at");
            entity.Property(x => x.DeletedReason).HasColumnName("deleted_reason").HasMaxLength(500);
            entity.Property(x => x.DeletedBy).HasColumnName("deleted_by").HasMaxLength(120);
            entity.Property(x => x.DeletedAt).HasColumnName("deleted_at");
            entity.HasIndex(x => x.Name).IsUnique().HasFilter("\"deleted_at\" IS NULL");
            entity.HasIndex(x => new { x.DeletedAt, x.DisplayOrder, x.Name });
        });

        modelBuilder.Entity<AppRoleEntity>(entity =>
        {
            entity.ToTable("app_roles");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.Name).HasColumnName("name").HasMaxLength(120);
            entity.Property(x => x.KeycloakRoleName).HasColumnName("keycloak_role_name").HasMaxLength(120);
            entity.Property(x => x.Description).HasColumnName("description").HasMaxLength(500);
            entity.Property(x => x.DisplayOrder).HasColumnName("display_order");
            entity.Property(x => x.CreatedAt).HasColumnName("created_at");
            entity.Property(x => x.UpdatedAt).HasColumnName("updated_at");
            entity.Property(x => x.DeletedReason).HasColumnName("deleted_reason").HasMaxLength(500);
            entity.Property(x => x.DeletedBy).HasColumnName("deleted_by").HasMaxLength(120);
            entity.Property(x => x.DeletedAt).HasColumnName("deleted_at");
            entity.HasIndex(x => x.Name).IsUnique().HasFilter("\"deleted_at\" IS NULL");
            entity.HasIndex(x => x.KeycloakRoleName).IsUnique().HasFilter("\"deleted_at\" IS NULL");
            entity.HasIndex(x => new { x.DeletedAt, x.DisplayOrder, x.Name });
        });

        modelBuilder.Entity<UserRegistrationRequestEntity>(entity =>
        {
            entity.ToTable("user_registration_requests");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.Email).HasColumnName("email").HasMaxLength(320);
            entity.Property(x => x.FirstName).HasColumnName("first_name").HasMaxLength(120);
            entity.Property(x => x.LastName).HasColumnName("last_name").HasMaxLength(120);
            entity.Property(x => x.DepartmentId).HasColumnName("department_id");
            entity.Property(x => x.JobTitleId).HasColumnName("job_title_id");
            entity.Property(x => x.ProvisionedUserId).HasColumnName("provisioned_user_id").HasMaxLength(64);
            entity.Property(x => x.PasswordSetupToken).HasColumnName("password_setup_token").HasMaxLength(128);
            entity.Property(x => x.PasswordSetupExpiresAt).HasColumnName("password_setup_expires_at");
            entity.Property(x => x.PasswordSetupCompletedAt).HasColumnName("password_setup_completed_at");
            entity.Property(x => x.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(20);
            entity.Property(x => x.RequestedAt).HasColumnName("requested_at");
            entity.Property(x => x.ReviewedAt).HasColumnName("reviewed_at");
            entity.Property(x => x.ReviewedBy).HasColumnName("reviewed_by").HasMaxLength(120);
            entity.Property(x => x.RejectionReason).HasColumnName("rejection_reason").HasMaxLength(500);
            entity.HasOne<DepartmentEntity>().WithMany().HasForeignKey(x => x.DepartmentId).OnDelete(DeleteBehavior.SetNull);
            entity.HasOne<JobTitleEntity>().WithMany().HasForeignKey(x => x.JobTitleId).OnDelete(DeleteBehavior.SetNull);
            entity.HasIndex(x => new { x.Email, x.Status });
            entity.HasIndex(x => new { x.Status, x.RequestedAt });
            entity.HasIndex(x => x.DepartmentId);
            entity.HasIndex(x => x.JobTitleId);
            entity.HasIndex(x => x.PasswordSetupToken).IsUnique();
        });

        modelBuilder.Entity<UserInvitationEntity>(entity =>
        {
            entity.ToTable("user_invitations");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.Email).HasColumnName("email").HasMaxLength(320);
            entity.Property(x => x.InvitationToken).HasColumnName("invitation_token").HasMaxLength(128);
            entity.Property(x => x.InvitedBy).HasColumnName("invited_by").HasMaxLength(120);
            entity.Property(x => x.DepartmentId).HasColumnName("department_id");
            entity.Property(x => x.JobTitleId).HasColumnName("job_title_id");
            entity.Property(x => x.Status).HasColumnName("status").HasConversion<string>().HasMaxLength(20);
            entity.Property(x => x.InvitedAt).HasColumnName("invited_at");
            entity.Property(x => x.ExpiresAt).HasColumnName("expires_at");
            entity.Property(x => x.AcceptedAt).HasColumnName("accepted_at");
            entity.Property(x => x.RejectedAt).HasColumnName("rejected_at");
            entity.HasOne<DepartmentEntity>().WithMany().HasForeignKey(x => x.DepartmentId).OnDelete(DeleteBehavior.SetNull);
            entity.HasOne<JobTitleEntity>().WithMany().HasForeignKey(x => x.JobTitleId).OnDelete(DeleteBehavior.SetNull);
            entity.HasIndex(x => new { x.Email, x.Status });
            entity.HasIndex(x => new { x.Status, x.InvitedAt });
            entity.HasIndex(x => x.DepartmentId);
            entity.HasIndex(x => x.JobTitleId);
            entity.HasIndex(x => x.InvitationToken).IsUnique();
        });

        modelBuilder.Entity<AuditLogEntity>(entity =>
        {
            entity.ToTable("audit_logs");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.OccurredAt).HasColumnName("occurred_at");
            entity.Property(x => x.Module).HasColumnName("module").HasMaxLength(64);
            entity.Property(x => x.Action).HasColumnName("action").HasMaxLength(64);
            entity.Property(x => x.EntityType).HasColumnName("entity_type").HasMaxLength(64);
            entity.Property(x => x.EntityId).HasColumnName("entity_id").HasMaxLength(128);
            entity.Property(x => x.ActorType).HasColumnName("actor_type").HasMaxLength(32);
            entity.Property(x => x.ActorUserId).HasColumnName("actor_user_id").HasMaxLength(128);
            entity.Property(x => x.ActorEmail).HasColumnName("actor_email").HasMaxLength(320);
            entity.Property(x => x.ActorDisplayName).HasColumnName("actor_display_name").HasMaxLength(256);
            entity.Property(x => x.DepartmentId).HasColumnName("department_id");
            entity.Property(x => x.TenantId).HasColumnName("tenant_id").HasMaxLength(128);
            entity.Property(x => x.RequestId).HasColumnName("request_id").HasMaxLength(128);
            entity.Property(x => x.TraceId).HasColumnName("trace_id").HasMaxLength(128);
            entity.Property(x => x.CorrelationId).HasColumnName("correlation_id").HasMaxLength(128);
            entity.Property(x => x.HttpMethod).HasColumnName("http_method").HasMaxLength(16);
            entity.Property(x => x.RequestPath).HasColumnName("request_path").HasMaxLength(512);
            entity.Property(x => x.IpAddress).HasColumnName("ip_address").HasMaxLength(64);
            entity.Property(x => x.UserAgent).HasColumnName("user_agent").HasMaxLength(1024);
            entity.Property(x => x.Status).HasColumnName("status").HasMaxLength(32);
            entity.Property(x => x.StatusCode).HasColumnName("status_code");
            entity.Property(x => x.ErrorCode).HasColumnName("error_code").HasMaxLength(128);
            entity.Property(x => x.ErrorMessage).HasColumnName("error_message");
            entity.Property(x => x.Reason).HasColumnName("reason");
            entity.Property(x => x.Source).HasColumnName("source").HasMaxLength(64);
            entity.Property(x => x.BeforeJson).HasColumnName("before_json").HasColumnType("jsonb");
            entity.Property(x => x.AfterJson).HasColumnName("after_json").HasColumnType("jsonb");
            entity.Property(x => x.ChangesJson).HasColumnName("changes_json").HasColumnType("jsonb");
            entity.Property(x => x.MetadataJson).HasColumnName("metadata_json").HasColumnType("jsonb");
            entity.Property(x => x.IsSensitive).HasColumnName("is_sensitive");
            entity.Property(x => x.RetentionClass).HasColumnName("retention_class").HasMaxLength(32);
            entity.Property(x => x.CreatedAt).HasColumnName("created_at");

            entity.HasIndex(x => x.OccurredAt);
            entity.HasIndex(x => new { x.Module, x.OccurredAt });
            entity.HasIndex(x => new { x.EntityType, x.EntityId });
            entity.HasIndex(x => new { x.ActorUserId, x.OccurredAt });
            entity.HasIndex(x => new { x.ActorEmail, x.OccurredAt });
            entity.HasIndex(x => new { x.Action, x.OccurredAt });
            entity.HasIndex(x => x.RequestId);
            entity.HasIndex(x => new { x.Status, x.OccurredAt });
            entity.HasIndex(x => new { x.DepartmentId, x.OccurredAt });
        });
    }
}
