using Microsoft.EntityFrameworkCore;
using Operis_API.Modules.Audits.Infrastructure;
using Operis_API.Modules.Documents.Infrastructure;
using Operis_API.Modules.Users.Infrastructure;
using Operis_API.Modules.Workflows.Infrastructure;
using Operis_API.Shared.ActivityLogging;
using Operis_API.Shared.Auditing;

namespace Operis_API.Infrastructure.Persistence;

public sealed class OperisDbContext(DbContextOptions<OperisDbContext> options) : DbContext(options)
{
    public DbSet<DocumentEntity> Documents => Set<DocumentEntity>();
    public DbSet<DocumentVersionEntity> DocumentVersions => Set<DocumentVersionEntity>();
    public DbSet<DocumentHistoryEntity> DocumentHistories => Set<DocumentHistoryEntity>();
    public DbSet<DocumentTemplateEntity> DocumentTemplates => Set<DocumentTemplateEntity>();
    public DbSet<DocumentTemplateItemEntity> DocumentTemplateItems => Set<DocumentTemplateItemEntity>();
    public DbSet<DocumentTemplateHistoryEntity> DocumentTemplateHistories => Set<DocumentTemplateHistoryEntity>();
    public DbSet<UserEntity> Users => Set<UserEntity>();
    public DbSet<DivisionEntity> Divisions => Set<DivisionEntity>();
    public DbSet<DepartmentEntity> Departments => Set<DepartmentEntity>();
    public DbSet<JobTitleEntity> JobTitles => Set<JobTitleEntity>();
    public DbSet<ProjectRoleEntity> ProjectRoles => Set<ProjectRoleEntity>();
    public DbSet<UserOrgAssignmentEntity> UserOrgAssignments => Set<UserOrgAssignmentEntity>();
    public DbSet<ReportingLineEntity> ReportingLines => Set<ReportingLineEntity>();
    public DbSet<ProjectEntity> Projects => Set<ProjectEntity>();
    public DbSet<ProjectHistoryEntity> ProjectHistories => Set<ProjectHistoryEntity>();
    public DbSet<ProjectTypeTemplateEntity> ProjectTypeTemplates => Set<ProjectTypeTemplateEntity>();
    public DbSet<ProjectTypeRoleRequirementEntity> ProjectTypeRoleRequirements => Set<ProjectTypeRoleRequirementEntity>();
    public DbSet<UserProjectAssignmentEntity> UserProjectAssignments => Set<UserProjectAssignmentEntity>();
    public DbSet<AppRoleEntity> AppRoles => Set<AppRoleEntity>();
    public DbSet<UserRegistrationRequestEntity> UserRegistrationRequests => Set<UserRegistrationRequestEntity>();
    public DbSet<UserInvitationEntity> UserInvitations => Set<UserInvitationEntity>();
    public DbSet<ActivityLogEntity> ActivityLogs => Set<ActivityLogEntity>();
    public DbSet<AuditLogEntity> AuditLogs => Set<AuditLogEntity>();
    public DbSet<BusinessAuditEventEntity> BusinessAuditEvents => Set<BusinessAuditEventEntity>();
    public DbSet<WorkflowDefinitionEntity> WorkflowDefinitions => Set<WorkflowDefinitionEntity>();
    public DbSet<WorkflowStepEntity> WorkflowSteps => Set<WorkflowStepEntity>();
    public DbSet<WorkflowStepRoleEntity> WorkflowStepRoles => Set<WorkflowStepRoleEntity>();
    public DbSet<WorkflowStepRouteEntity> WorkflowStepRoutes => Set<WorkflowStepRouteEntity>();
    public DbSet<WorkflowInstanceEntity> WorkflowInstances => Set<WorkflowInstanceEntity>();
    public DbSet<WorkflowInstanceStepEntity> WorkflowInstanceSteps => Set<WorkflowInstanceStepEntity>();
    public DbSet<WorkflowInstanceActionEntity> WorkflowInstanceActions => Set<WorkflowInstanceActionEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<DocumentEntity>(entity =>
        {
            entity.ToTable("documents");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.DocumentName).HasColumnName("document_name").HasMaxLength(256);
            entity.Property(x => x.PublishedVersionId).HasColumnName("published_version_id");
            entity.Property(x => x.UploadedByUserId).HasColumnName("uploaded_by_user_id").HasMaxLength(64);
            entity.Property(x => x.UploadedAt).HasColumnName("uploaded_at");
            entity.Property(x => x.IsDeleted).HasColumnName("is_deleted");
            entity.Property(x => x.DeletedByUserId).HasColumnName("deleted_by_user_id").HasMaxLength(64);
            entity.Property(x => x.DeletedAt).HasColumnName("deleted_at");
            entity.Property(x => x.DeletedReason).HasColumnName("deleted_reason").HasMaxLength(512);
            entity.HasOne<DocumentVersionEntity>()
                .WithMany()
                .HasForeignKey(x => x.PublishedVersionId)
                .OnDelete(DeleteBehavior.SetNull);
            entity.HasIndex(x => x.UploadedAt);
            entity.HasIndex(x => x.IsDeleted);
            entity.HasIndex(x => x.PublishedVersionId);
            entity.HasIndex(x => new { x.IsDeleted, x.UploadedAt });
        });

        modelBuilder.Entity<DocumentVersionEntity>(entity =>
        {
            entity.ToTable("document_versions");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.DocumentId).HasColumnName("document_id");
            entity.Property(x => x.Revision).HasColumnName("revision");
            entity.Property(x => x.VersionCode).HasColumnName("version_code").HasMaxLength(64);
            entity.Property(x => x.FileName).HasColumnName("file_name").HasMaxLength(256);
            entity.Property(x => x.ObjectKey).HasColumnName("object_key").HasMaxLength(512);
            entity.Property(x => x.BucketName).HasColumnName("bucket_name").HasMaxLength(128);
            entity.Property(x => x.ContentType).HasColumnName("content_type").HasMaxLength(256);
            entity.Property(x => x.SizeBytes).HasColumnName("size_bytes");
            entity.Property(x => x.UploadedByUserId).HasColumnName("uploaded_by_user_id").HasMaxLength(64);
            entity.Property(x => x.UploadedAt).HasColumnName("uploaded_at");
            entity.Property(x => x.IsDeleted).HasColumnName("is_deleted");
            entity.Property(x => x.DeletedByUserId).HasColumnName("deleted_by_user_id").HasMaxLength(64);
            entity.Property(x => x.DeletedAt).HasColumnName("deleted_at");
            entity.Property(x => x.DeletedReason).HasColumnName("deleted_reason").HasMaxLength(512);
            entity.HasIndex(x => x.DocumentId);
            entity.HasIndex(x => new { x.DocumentId, x.Revision }).IsUnique();
            entity.HasIndex(x => new { x.DocumentId, x.VersionCode }).IsUnique();
            entity.HasIndex(x => x.ObjectKey).IsUnique().HasFilter("\"object_key\" IS NOT NULL");
            entity.HasIndex(x => x.IsDeleted);
            entity.HasIndex(x => new { x.DocumentId, x.IsDeleted, x.Revision });
            entity.HasIndex(x => new { x.DocumentId, x.IsDeleted, x.UploadedAt });
        });

        modelBuilder.Entity<DocumentHistoryEntity>(entity =>
        {
            entity.ToTable("document_histories");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.DocumentId).HasColumnName("document_id");
            entity.Property(x => x.EventType).HasColumnName("event_type").HasMaxLength(64);
            entity.Property(x => x.Summary).HasColumnName("summary").HasMaxLength(512);
            entity.Property(x => x.Reason).HasColumnName("reason").HasMaxLength(512);
            entity.Property(x => x.ActorUserId).HasColumnName("actor_user_id").HasMaxLength(64);
            entity.Property(x => x.ActorEmail).HasColumnName("actor_email").HasMaxLength(128);
            entity.Property(x => x.ActorDisplayName).HasColumnName("actor_display_name").HasMaxLength(128);
            entity.Property(x => x.BeforeJson).HasColumnName("before_json");
            entity.Property(x => x.AfterJson).HasColumnName("after_json");
            entity.Property(x => x.MetadataJson).HasColumnName("metadata_json");
            entity.Property(x => x.OccurredAt).HasColumnName("occurred_at");
            entity.Property(x => x.CreatedAt).HasColumnName("created_at");
            entity.HasIndex(x => x.DocumentId);
            entity.HasIndex(x => x.EventType);
            entity.HasIndex(x => x.OccurredAt);
            entity.HasIndex(x => new { x.DocumentId, x.OccurredAt });
        });

        modelBuilder.Entity<DocumentTemplateEntity>(entity =>
        {
            entity.ToTable("document_templates");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.Name).HasColumnName("name").HasMaxLength(256);
            entity.Property(x => x.CreatedByUserId).HasColumnName("created_by_user_id").HasMaxLength(64);
            entity.Property(x => x.CreatedAt).HasColumnName("created_at");
            entity.Property(x => x.IsDeleted).HasColumnName("is_deleted");
            entity.Property(x => x.DeletedByUserId).HasColumnName("deleted_by_user_id").HasMaxLength(64);
            entity.Property(x => x.DeletedAt).HasColumnName("deleted_at");
            entity.Property(x => x.DeletedReason).HasColumnName("deleted_reason").HasMaxLength(512);
            entity.HasIndex(x => x.Name);
            entity.HasIndex(x => x.IsDeleted);
            entity.HasIndex(x => new { x.IsDeleted, x.CreatedAt });
        });

        modelBuilder.Entity<ProjectHistoryEntity>(entity =>
        {
            entity.ToTable("project_histories");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.ProjectId).HasColumnName("project_id");
            entity.Property(x => x.EventType).HasColumnName("event_type").HasMaxLength(64);
            entity.Property(x => x.Summary).HasColumnName("summary").HasMaxLength(512);
            entity.Property(x => x.Reason).HasColumnName("reason").HasMaxLength(512);
            entity.Property(x => x.ActorUserId).HasColumnName("actor_user_id").HasMaxLength(64);
            entity.Property(x => x.ActorEmail).HasColumnName("actor_email").HasMaxLength(128);
            entity.Property(x => x.ActorDisplayName).HasColumnName("actor_display_name").HasMaxLength(128);
            entity.Property(x => x.BeforeJson).HasColumnName("before_json");
            entity.Property(x => x.AfterJson).HasColumnName("after_json");
            entity.Property(x => x.MetadataJson).HasColumnName("metadata_json");
            entity.Property(x => x.OccurredAt).HasColumnName("occurred_at");
            entity.Property(x => x.CreatedAt).HasColumnName("created_at");
            entity.HasIndex(x => x.ProjectId);
            entity.HasIndex(x => x.EventType);
            entity.HasIndex(x => x.OccurredAt);
            entity.HasIndex(x => new { x.ProjectId, x.OccurredAt });
        });

        modelBuilder.Entity<DocumentTemplateItemEntity>(entity =>
        {
            entity.ToTable("document_template_items");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.TemplateId).HasColumnName("template_id");
            entity.Property(x => x.DocumentId).HasColumnName("document_id");
            entity.Property(x => x.DisplayOrder).HasColumnName("display_order");
            entity.HasOne<DocumentTemplateEntity>()
                .WithMany()
                .HasForeignKey(x => x.TemplateId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne<DocumentEntity>()
                .WithMany()
                .HasForeignKey(x => x.DocumentId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(x => x.TemplateId);
            entity.HasIndex(x => x.DocumentId);
            entity.HasIndex(x => new { x.TemplateId, x.DocumentId }).IsUnique();
        });

        modelBuilder.Entity<DocumentTemplateHistoryEntity>(entity =>
        {
            entity.ToTable("document_template_histories");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.TemplateId).HasColumnName("template_id");
            entity.Property(x => x.EventType).HasColumnName("event_type").HasMaxLength(64);
            entity.Property(x => x.Summary).HasColumnName("summary").HasMaxLength(512);
            entity.Property(x => x.Reason).HasColumnName("reason").HasMaxLength(512);
            entity.Property(x => x.ActorUserId).HasColumnName("actor_user_id").HasMaxLength(64);
            entity.Property(x => x.ActorEmail).HasColumnName("actor_email").HasMaxLength(128);
            entity.Property(x => x.ActorDisplayName).HasColumnName("actor_display_name").HasMaxLength(128);
            entity.Property(x => x.BeforeJson).HasColumnName("before_json");
            entity.Property(x => x.AfterJson).HasColumnName("after_json");
            entity.Property(x => x.MetadataJson).HasColumnName("metadata_json");
            entity.Property(x => x.OccurredAt).HasColumnName("occurred_at");
            entity.Property(x => x.CreatedAt).HasColumnName("created_at");
            entity.HasIndex(x => x.TemplateId);
            entity.HasIndex(x => x.EventType);
            entity.HasIndex(x => x.OccurredAt);
            entity.HasIndex(x => new { x.TemplateId, x.OccurredAt });
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

        modelBuilder.Entity<DivisionEntity>(entity =>
        {
            entity.ToTable("divisions");
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

        modelBuilder.Entity<DepartmentEntity>(entity =>
        {
            entity.ToTable("departments");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.DivisionId).HasColumnName("division_id");
            entity.Property(x => x.Name).HasColumnName("name").HasMaxLength(120);
            entity.Property(x => x.DisplayOrder).HasColumnName("display_order");
            entity.Property(x => x.CreatedAt).HasColumnName("created_at");
            entity.Property(x => x.UpdatedAt).HasColumnName("updated_at");
            entity.Property(x => x.DeletedReason).HasColumnName("deleted_reason").HasMaxLength(500);
            entity.Property(x => x.DeletedBy).HasColumnName("deleted_by").HasMaxLength(120);
            entity.Property(x => x.DeletedAt).HasColumnName("deleted_at");
            entity.HasOne<DivisionEntity>().WithMany().HasForeignKey(x => x.DivisionId).OnDelete(DeleteBehavior.SetNull);
            entity.HasIndex(x => x.Name).IsUnique().HasFilter("\"deleted_at\" IS NULL");
            entity.HasIndex(x => new { x.DeletedAt, x.DisplayOrder, x.Name });
            entity.HasIndex(x => x.DivisionId);
        });

        modelBuilder.Entity<JobTitleEntity>(entity =>
        {
            entity.ToTable("job_titles");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.DepartmentId).HasColumnName("department_id");
            entity.Property(x => x.Name).HasColumnName("name").HasMaxLength(120);
            entity.Property(x => x.DisplayOrder).HasColumnName("display_order");
            entity.Property(x => x.CreatedAt).HasColumnName("created_at");
            entity.Property(x => x.UpdatedAt).HasColumnName("updated_at");
            entity.Property(x => x.DeletedReason).HasColumnName("deleted_reason").HasMaxLength(500);
            entity.Property(x => x.DeletedBy).HasColumnName("deleted_by").HasMaxLength(120);
            entity.Property(x => x.DeletedAt).HasColumnName("deleted_at");
            entity.HasOne<DepartmentEntity>().WithMany().HasForeignKey(x => x.DepartmentId).OnDelete(DeleteBehavior.SetNull);
            entity.HasIndex(x => x.Name).IsUnique().HasFilter("\"deleted_at\" IS NULL");
            entity.HasIndex(x => new { x.DeletedAt, x.DisplayOrder, x.Name });
            entity.HasIndex(x => x.DepartmentId);
        });

        modelBuilder.Entity<ProjectRoleEntity>(entity =>
        {
            entity.ToTable("project_roles");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.Name).HasColumnName("name").HasMaxLength(120);
            entity.Property(x => x.Code).HasColumnName("code").HasMaxLength(80);
            entity.Property(x => x.Description).HasColumnName("description").HasMaxLength(500);
            entity.Property(x => x.Responsibilities).HasColumnName("responsibilities").HasMaxLength(2000);
            entity.Property(x => x.AuthorityScope).HasColumnName("authority_scope").HasMaxLength(500);
            entity.Property(x => x.DisplayOrder).HasColumnName("display_order");
            entity.Property(x => x.CreatedAt).HasColumnName("created_at");
            entity.Property(x => x.UpdatedAt).HasColumnName("updated_at");
            entity.Property(x => x.DeletedReason).HasColumnName("deleted_reason").HasMaxLength(500);
            entity.Property(x => x.DeletedBy).HasColumnName("deleted_by").HasMaxLength(120);
            entity.Property(x => x.DeletedAt).HasColumnName("deleted_at");
            entity.HasIndex(x => x.Name).IsUnique().HasFilter("\"deleted_at\" IS NULL");
            entity.HasIndex(x => x.Code).IsUnique().HasFilter("\"deleted_at\" IS NULL AND \"code\" IS NOT NULL");
            entity.HasIndex(x => new { x.DeletedAt, x.DisplayOrder, x.Name });
        });

        modelBuilder.Entity<UserOrgAssignmentEntity>(entity =>
        {
            entity.ToTable("user_org_assignments");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.UserId).HasColumnName("user_id").HasMaxLength(64);
            entity.Property(x => x.DivisionId).HasColumnName("division_id");
            entity.Property(x => x.DepartmentId).HasColumnName("department_id");
            entity.Property(x => x.PositionId).HasColumnName("position_id");
            entity.Property(x => x.IsPrimary).HasColumnName("is_primary");
            entity.Property(x => x.IsDivisionHead).HasColumnName("is_division_head");
            entity.Property(x => x.IsDepartmentHead).HasColumnName("is_department_head");
            entity.Property(x => x.StartAt).HasColumnName("start_at");
            entity.Property(x => x.EndAt).HasColumnName("end_at");
            entity.Property(x => x.CreatedAt).HasColumnName("created_at");
            entity.Property(x => x.UpdatedAt).HasColumnName("updated_at");
            entity.HasOne<UserEntity>().WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne<DivisionEntity>().WithMany().HasForeignKey(x => x.DivisionId).OnDelete(DeleteBehavior.SetNull);
            entity.HasOne<DepartmentEntity>().WithMany().HasForeignKey(x => x.DepartmentId).OnDelete(DeleteBehavior.SetNull);
            entity.HasOne<JobTitleEntity>().WithMany().HasForeignKey(x => x.PositionId).OnDelete(DeleteBehavior.SetNull);
            entity.HasIndex(x => new { x.UserId, x.IsPrimary });
            entity.HasIndex(x => x.DivisionId);
            entity.HasIndex(x => x.DepartmentId);
            entity.HasIndex(x => x.PositionId);
            entity.HasIndex(x => new { x.DepartmentId, x.IsDepartmentHead });
            entity.HasIndex(x => new { x.DivisionId, x.IsDivisionHead });
        });

        modelBuilder.Entity<ReportingLineEntity>(entity =>
        {
            entity.ToTable("reporting_lines");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.UserId).HasColumnName("user_id").HasMaxLength(64);
            entity.Property(x => x.ReportsToUserId).HasColumnName("reports_to_user_id").HasMaxLength(64);
            entity.Property(x => x.DepartmentId).HasColumnName("department_id");
            entity.Property(x => x.IsPrimary).HasColumnName("is_primary");
            entity.Property(x => x.EffectiveFrom).HasColumnName("effective_from");
            entity.Property(x => x.EffectiveTo).HasColumnName("effective_to");
            entity.Property(x => x.CreatedAt).HasColumnName("created_at");
            entity.Property(x => x.UpdatedAt).HasColumnName("updated_at");
            entity.HasOne<UserEntity>().WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne<UserEntity>().WithMany().HasForeignKey(x => x.ReportsToUserId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<DepartmentEntity>().WithMany().HasForeignKey(x => x.DepartmentId).OnDelete(DeleteBehavior.SetNull);
            entity.HasIndex(x => new { x.UserId, x.IsPrimary });
            entity.HasIndex(x => x.ReportsToUserId);
            entity.HasIndex(x => x.DepartmentId);
        });

        modelBuilder.Entity<ProjectEntity>(entity =>
        {
            entity.ToTable("projects");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.Code).HasColumnName("code").HasMaxLength(120);
            entity.Property(x => x.Name).HasColumnName("name").HasMaxLength(200);
            entity.Property(x => x.ProjectType).HasColumnName("project_type").HasMaxLength(80);
            entity.Property(x => x.OwnerUserId).HasColumnName("owner_user_id").HasMaxLength(64);
            entity.Property(x => x.SponsorUserId).HasColumnName("sponsor_user_id").HasMaxLength(64);
            entity.Property(x => x.Methodology).HasColumnName("methodology").HasMaxLength(80);
            entity.Property(x => x.Phase).HasColumnName("phase").HasMaxLength(80);
            entity.Property(x => x.Status).HasColumnName("status").HasMaxLength(32);
            entity.Property(x => x.StatusReason).HasColumnName("status_reason").HasMaxLength(500);
            entity.Property(x => x.WorkflowDefinitionId).HasColumnName("workflow_definition_id");
            entity.Property(x => x.DocumentTemplateId).HasColumnName("document_template_id");
            entity.Property(x => x.PlannedStartAt).HasColumnName("planned_start_at");
            entity.Property(x => x.PlannedEndAt).HasColumnName("planned_end_at");
            entity.Property(x => x.StartAt).HasColumnName("start_at");
            entity.Property(x => x.EndAt).HasColumnName("end_at");
            entity.Property(x => x.CreatedAt).HasColumnName("created_at");
            entity.Property(x => x.UpdatedAt).HasColumnName("updated_at");
            entity.Property(x => x.DeletedReason).HasColumnName("deleted_reason").HasMaxLength(500);
            entity.Property(x => x.DeletedBy).HasColumnName("deleted_by").HasMaxLength(120);
            entity.Property(x => x.DeletedAt).HasColumnName("deleted_at");
            entity.HasOne<UserEntity>().WithMany().HasForeignKey(x => x.OwnerUserId).OnDelete(DeleteBehavior.SetNull);
            entity.HasOne<UserEntity>().WithMany().HasForeignKey(x => x.SponsorUserId).OnDelete(DeleteBehavior.SetNull);
            entity.HasOne<WorkflowDefinitionEntity>().WithMany().HasForeignKey(x => x.WorkflowDefinitionId).OnDelete(DeleteBehavior.SetNull);
            entity.HasOne<DocumentTemplateEntity>().WithMany().HasForeignKey(x => x.DocumentTemplateId).OnDelete(DeleteBehavior.SetNull);
            entity.HasIndex(x => x.Code).IsUnique().HasFilter("\"deleted_at\" IS NULL");
            entity.HasIndex(x => new { x.DeletedAt, x.Status, x.CreatedAt });
            entity.HasIndex(x => x.ProjectType);
            entity.HasIndex(x => x.Phase);
            entity.HasIndex(x => x.OwnerUserId);
            entity.HasIndex(x => x.SponsorUserId);
            entity.HasIndex(x => x.WorkflowDefinitionId);
            entity.HasIndex(x => x.DocumentTemplateId);
        });

        modelBuilder.Entity<ProjectTypeTemplateEntity>(entity =>
        {
            entity.ToTable("project_type_templates");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.ProjectType).HasColumnName("project_type").HasMaxLength(80);
            entity.Property(x => x.RequireSponsor).HasColumnName("require_sponsor");
            entity.Property(x => x.RequirePlannedPeriod).HasColumnName("require_planned_period");
            entity.Property(x => x.RequireActiveTeam).HasColumnName("require_active_team");
            entity.Property(x => x.RequirePrimaryAssignment).HasColumnName("require_primary_assignment");
            entity.Property(x => x.RequireReportingRoot).HasColumnName("require_reporting_root");
            entity.Property(x => x.RequireDocumentCreator).HasColumnName("require_document_creator");
            entity.Property(x => x.RequireReviewer).HasColumnName("require_reviewer");
            entity.Property(x => x.RequireApprover).HasColumnName("require_approver");
            entity.Property(x => x.RequireReleaseRole).HasColumnName("require_release_role");
            entity.Property(x => x.CreatedAt).HasColumnName("created_at");
            entity.Property(x => x.UpdatedAt).HasColumnName("updated_at");
            entity.Property(x => x.DeletedReason).HasColumnName("deleted_reason").HasMaxLength(500);
            entity.Property(x => x.DeletedBy).HasColumnName("deleted_by").HasMaxLength(120);
            entity.Property(x => x.DeletedAt).HasColumnName("deleted_at");
            entity.HasIndex(x => x.ProjectType).IsUnique().HasFilter("\"deleted_at\" IS NULL");
        });

        modelBuilder.Entity<ProjectTypeRoleRequirementEntity>(entity =>
        {
            entity.ToTable("project_type_role_requirements");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.ProjectTypeTemplateId).HasColumnName("project_type_template_id");
            entity.Property(x => x.RoleName).HasColumnName("role_name").HasMaxLength(120);
            entity.Property(x => x.RoleCode).HasColumnName("role_code").HasMaxLength(80);
            entity.Property(x => x.Description).HasColumnName("description").HasMaxLength(500);
            entity.Property(x => x.DisplayOrder).HasColumnName("display_order");
            entity.Property(x => x.CreatedAt).HasColumnName("created_at");
            entity.Property(x => x.UpdatedAt).HasColumnName("updated_at");
            entity.Property(x => x.DeletedReason).HasColumnName("deleted_reason").HasMaxLength(500);
            entity.Property(x => x.DeletedBy).HasColumnName("deleted_by").HasMaxLength(120);
            entity.Property(x => x.DeletedAt).HasColumnName("deleted_at");
            entity.HasOne<ProjectTypeTemplateEntity>().WithMany().HasForeignKey(x => x.ProjectTypeTemplateId).OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(x => new { x.ProjectTypeTemplateId, x.RoleName }).HasFilter("\"deleted_at\" IS NULL");
            entity.HasIndex(x => new { x.ProjectTypeTemplateId, x.RoleCode }).HasFilter("\"deleted_at\" IS NULL AND \"role_code\" IS NOT NULL");
        });

        modelBuilder.Entity<UserProjectAssignmentEntity>(entity =>
        {
            entity.ToTable("user_project_assignments");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.UserId).HasColumnName("user_id").HasMaxLength(64);
            entity.Property(x => x.ProjectId).HasColumnName("project_id");
            entity.Property(x => x.ProjectRoleId).HasColumnName("project_role_id");
            entity.Property(x => x.ReportsToUserId).HasColumnName("reports_to_user_id").HasMaxLength(64);
            entity.Property(x => x.IsPrimary).HasColumnName("is_primary");
            entity.Property(x => x.Status).HasColumnName("status").HasMaxLength(32);
            entity.Property(x => x.ChangeReason).HasColumnName("change_reason").HasMaxLength(500);
            entity.Property(x => x.ReplacedByAssignmentId).HasColumnName("replaced_by_assignment_id");
            entity.Property(x => x.StartAt).HasColumnName("start_at");
            entity.Property(x => x.EndAt).HasColumnName("end_at");
            entity.Property(x => x.CreatedAt).HasColumnName("created_at");
            entity.Property(x => x.UpdatedAt).HasColumnName("updated_at");
            entity.HasOne<UserEntity>().WithMany().HasForeignKey(x => x.UserId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne<ProjectEntity>().WithMany().HasForeignKey(x => x.ProjectId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne<ProjectRoleEntity>().WithMany().HasForeignKey(x => x.ProjectRoleId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<UserEntity>().WithMany().HasForeignKey(x => x.ReportsToUserId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<UserProjectAssignmentEntity>().WithMany().HasForeignKey(x => x.ReplacedByAssignmentId).OnDelete(DeleteBehavior.SetNull);
            entity.HasIndex(x => new { x.UserId, x.ProjectId, x.ProjectRoleId });
            entity.HasIndex(x => new { x.ProjectId, x.IsPrimary });
            entity.HasIndex(x => x.ProjectRoleId);
            entity.HasIndex(x => x.ReportsToUserId);
            entity.HasIndex(x => new { x.ProjectId, x.Status, x.StartAt });
            entity.HasIndex(x => new { x.ProjectId, x.CreatedAt });
            entity.HasIndex(x => new { x.ProjectRoleId, x.Status });
            entity.HasIndex(x => new { x.ProjectId, x.Status, x.CreatedAt });
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

        modelBuilder.Entity<BusinessAuditEventEntity>(entity =>
        {
            entity.ToTable("business_audit_events");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.Module).HasColumnName("module").HasMaxLength(64);
            entity.Property(x => x.EventType).HasColumnName("event_type").HasMaxLength(64);
            entity.Property(x => x.EntityType).HasColumnName("entity_type").HasMaxLength(64);
            entity.Property(x => x.EntityId).HasColumnName("entity_id").HasMaxLength(64);
            entity.Property(x => x.Summary).HasColumnName("summary").HasMaxLength(512);
            entity.Property(x => x.Reason).HasColumnName("reason").HasMaxLength(512);
            entity.Property(x => x.ActorUserId).HasColumnName("actor_user_id").HasMaxLength(64);
            entity.Property(x => x.ActorEmail).HasColumnName("actor_email").HasMaxLength(128);
            entity.Property(x => x.ActorDisplayName).HasColumnName("actor_display_name").HasMaxLength(128);
            entity.Property(x => x.MetadataJson).HasColumnName("metadata_json").HasColumnType("jsonb");
            entity.Property(x => x.OccurredAt).HasColumnName("occurred_at");
            entity.Property(x => x.CreatedAt).HasColumnName("created_at");
            entity.HasIndex(x => x.Module);
            entity.HasIndex(x => x.EventType);
            entity.HasIndex(x => x.EntityType);
            entity.HasIndex(x => x.EntityId);
            entity.HasIndex(x => new { x.EntityType, x.EntityId, x.OccurredAt });
        });

        modelBuilder.Entity<ActivityLogEntity>(entity =>
        {
            entity.ToTable("activity_logs");
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

        modelBuilder.Entity<WorkflowDefinitionEntity>(entity =>
        {
            entity.ToTable("workflow_definitions");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.Code).HasColumnName("code").HasMaxLength(120);
            entity.Property(x => x.Name).HasColumnName("name").HasMaxLength(200);
            entity.Property(x => x.Status).HasColumnName("status").HasMaxLength(32);
            entity.Property(x => x.DocumentTemplateId).HasColumnName("document_template_id");
            entity.Property(x => x.CreatedAt).HasColumnName("created_at");
            entity.Property(x => x.UpdatedAt).HasColumnName("updated_at");
            entity.HasIndex(x => x.Code).IsUnique();
            entity.HasIndex(x => new { x.Status, x.CreatedAt });
        });

        modelBuilder.Entity<WorkflowStepEntity>(entity =>
        {
            entity.ToTable("workflow_steps");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.WorkflowDefinitionId).HasColumnName("workflow_definition_id");
            entity.Property(x => x.DocumentId).HasColumnName("document_id");
            entity.Property(x => x.Name).HasColumnName("name").HasMaxLength(200);
            entity.Property(x => x.StepType).HasColumnName("step_type").HasMaxLength(32);
            entity.Property(x => x.DisplayOrder).HasColumnName("display_order");
            entity.Property(x => x.IsRequired).HasColumnName("is_required");
            entity.Property(x => x.MinApprovals).HasColumnName("min_approvals");
            entity.Property(x => x.CreatedAt).HasColumnName("created_at");
            entity.Property(x => x.UpdatedAt).HasColumnName("updated_at");
            entity.HasIndex(x => new { x.WorkflowDefinitionId, x.DisplayOrder });
            entity.HasIndex(x => x.DocumentId);
        });

        modelBuilder.Entity<WorkflowStepRoleEntity>(entity =>
        {
            entity.ToTable("workflow_step_roles");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.WorkflowStepId).HasColumnName("workflow_step_id");
            entity.Property(x => x.ProjectRoleId).HasColumnName("project_role_id");
            entity.Property(x => x.CreatedAt).HasColumnName("created_at");
            entity.HasOne<WorkflowStepEntity>().WithMany().HasForeignKey(x => x.WorkflowStepId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne<ProjectRoleEntity>().WithMany().HasForeignKey(x => x.ProjectRoleId).OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(x => new { x.WorkflowStepId, x.ProjectRoleId }).IsUnique();
        });

        modelBuilder.Entity<WorkflowStepRouteEntity>(entity =>
        {
            entity.ToTable("workflow_step_routes");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.WorkflowStepId).HasColumnName("workflow_step_id");
            entity.Property(x => x.Action).HasColumnName("action").HasMaxLength(32);
            entity.Property(x => x.NextStepId).HasColumnName("next_step_id");
            entity.Property(x => x.CreatedAt).HasColumnName("created_at");
            entity.HasOne<WorkflowStepEntity>().WithMany().HasForeignKey(x => x.WorkflowStepId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne<WorkflowStepEntity>().WithMany().HasForeignKey(x => x.NextStepId).OnDelete(DeleteBehavior.SetNull);
            entity.HasIndex(x => new { x.WorkflowStepId, x.Action }).IsUnique();
            entity.HasIndex(x => x.NextStepId);
        });

        modelBuilder.Entity<WorkflowInstanceEntity>(entity =>
        {
            entity.ToTable("workflow_instances");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.ProjectId).HasColumnName("project_id");
            entity.Property(x => x.DocumentId).HasColumnName("document_id");
            entity.Property(x => x.WorkflowDefinitionId).HasColumnName("workflow_definition_id");
            entity.Property(x => x.Status).HasColumnName("status").HasMaxLength(32);
            entity.Property(x => x.CurrentStepOrder).HasColumnName("current_step_order");
            entity.Property(x => x.StartedAt).HasColumnName("started_at");
            entity.Property(x => x.CompletedAt).HasColumnName("completed_at");
            entity.Property(x => x.CreatedAt).HasColumnName("created_at");
            entity.Property(x => x.UpdatedAt).HasColumnName("updated_at");
            entity.HasOne<ProjectEntity>().WithMany().HasForeignKey(x => x.ProjectId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<DocumentEntity>().WithMany().HasForeignKey(x => x.DocumentId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<WorkflowDefinitionEntity>().WithMany().HasForeignKey(x => x.WorkflowDefinitionId).OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(x => x.ProjectId);
            entity.HasIndex(x => x.DocumentId);
            entity.HasIndex(x => x.WorkflowDefinitionId);
            entity.HasIndex(x => new { x.ProjectId, x.Status, x.CreatedAt });
        });

        modelBuilder.Entity<WorkflowInstanceStepEntity>(entity =>
        {
            entity.ToTable("workflow_instance_steps");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.WorkflowInstanceId).HasColumnName("workflow_instance_id");
            entity.Property(x => x.WorkflowStepId).HasColumnName("workflow_step_id");
            entity.Property(x => x.StepType).HasColumnName("step_type").HasMaxLength(32);
            entity.Property(x => x.DisplayOrder).HasColumnName("display_order");
            entity.Property(x => x.IsRequired).HasColumnName("is_required");
            entity.Property(x => x.Status).HasColumnName("status").HasMaxLength(32);
            entity.Property(x => x.StartedAt).HasColumnName("started_at");
            entity.Property(x => x.CompletedAt).HasColumnName("completed_at");
            entity.Property(x => x.CreatedAt).HasColumnName("created_at");
            entity.Property(x => x.UpdatedAt).HasColumnName("updated_at");
            entity.HasOne<WorkflowInstanceEntity>().WithMany().HasForeignKey(x => x.WorkflowInstanceId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne<WorkflowStepEntity>().WithMany().HasForeignKey(x => x.WorkflowStepId).OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(x => new { x.WorkflowInstanceId, x.DisplayOrder });
            entity.HasIndex(x => x.WorkflowStepId);
        });

        modelBuilder.Entity<WorkflowInstanceActionEntity>(entity =>
        {
            entity.ToTable("workflow_instance_actions");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.WorkflowInstanceStepId).HasColumnName("workflow_instance_step_id");
            entity.Property(x => x.Action).HasColumnName("action").HasMaxLength(32);
            entity.Property(x => x.ActorUserId).HasColumnName("actor_user_id").HasMaxLength(64);
            entity.Property(x => x.ActorEmail).HasColumnName("actor_email").HasMaxLength(128);
            entity.Property(x => x.ActorDisplayName).HasColumnName("actor_display_name").HasMaxLength(128);
            entity.Property(x => x.Comment).HasColumnName("comment").HasMaxLength(512);
            entity.Property(x => x.CreatedAt).HasColumnName("created_at");
            entity.HasOne<WorkflowInstanceStepEntity>().WithMany().HasForeignKey(x => x.WorkflowInstanceStepId).OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(x => x.WorkflowInstanceStepId);
            entity.HasIndex(x => new { x.ActorUserId, x.CreatedAt });
        });
    }
}
