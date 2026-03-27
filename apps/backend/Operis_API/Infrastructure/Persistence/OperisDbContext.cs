using Microsoft.EntityFrameworkCore;
using Operis_API.Modules.Audits.Infrastructure;
using Operis_API.Modules.ChangeControl.Infrastructure;
using Operis_API.Modules.Defects.Infrastructure;
using Operis_API.Modules.Documents.Infrastructure;
using Operis_API.Modules.Governance.Infrastructure;
using Operis_API.Modules.Knowledge.Infrastructure;
using Operis_API.Modules.Meetings.Infrastructure;
using Operis_API.Modules.Metrics.Infrastructure;
using Operis_API.Modules.Operations.Infrastructure;
using Operis_API.Modules.Notifications.Infrastructure;
using Operis_API.Modules.Releases.Infrastructure;
using Operis_API.Modules.Requirements.Infrastructure;
using Operis_API.Modules.Risks.Infrastructure;
using Operis_API.Modules.Users.Infrastructure;
using Operis_API.Modules.Verification.Infrastructure;
using Operis_API.Modules.Workflows.Infrastructure;
using Operis_API.Shared.ActivityLogging;
using Operis_API.Shared.Auditing;

namespace Operis_API.Infrastructure.Persistence;

public sealed class OperisDbContext(DbContextOptions<OperisDbContext> options) : DbContext(options)
{
    public DbSet<DocumentEntity> Documents => Set<DocumentEntity>();
    public DbSet<DocumentVersionEntity> DocumentVersions => Set<DocumentVersionEntity>();
    public DbSet<DocumentTypeEntity> DocumentTypes => Set<DocumentTypeEntity>();
    public DbSet<DocumentApprovalEntity> DocumentApprovals => Set<DocumentApprovalEntity>();
    public DbSet<DocumentLinkEntity> DocumentLinks => Set<DocumentLinkEntity>();
    public DbSet<DocumentHistoryEntity> DocumentHistories => Set<DocumentHistoryEntity>();
    public DbSet<DocumentTemplateEntity> DocumentTemplates => Set<DocumentTemplateEntity>();
    public DbSet<DocumentTemplateItemEntity> DocumentTemplateItems => Set<DocumentTemplateItemEntity>();
    public DbSet<DocumentTemplateHistoryEntity> DocumentTemplateHistories => Set<DocumentTemplateHistoryEntity>();
    public DbSet<UserEntity> Users => Set<UserEntity>();
    public DbSet<DivisionEntity> Divisions => Set<DivisionEntity>();
    public DbSet<DepartmentEntity> Departments => Set<DepartmentEntity>();
    public DbSet<JobTitleEntity> JobTitles => Set<JobTitleEntity>();
    public DbSet<ProjectRoleEntity> ProjectRoles => Set<ProjectRoleEntity>();
    public DbSet<PhaseApprovalRequestEntity> PhaseApprovalRequests => Set<PhaseApprovalRequestEntity>();
    public DbSet<MasterDataItemEntity> MasterDataItems => Set<MasterDataItemEntity>();
    public DbSet<MasterDataChangeEntity> MasterDataChanges => Set<MasterDataChangeEntity>();
    public DbSet<AccessReviewEntity> AccessReviews => Set<AccessReviewEntity>();
    public DbSet<SecurityReviewEntity> SecurityReviews => Set<SecurityReviewEntity>();
    public DbSet<ExternalDependencyEntity> ExternalDependencies => Set<ExternalDependencyEntity>();
    public DbSet<SupplierEntity> Suppliers => Set<SupplierEntity>();
    public DbSet<SupplierAgreementEntity> SupplierAgreements => Set<SupplierAgreementEntity>();
    public DbSet<ConfigurationAuditEntity> ConfigurationAudits => Set<ConfigurationAuditEntity>();
    public DbSet<AccessRecertificationScheduleEntity> AccessRecertificationSchedules => Set<AccessRecertificationScheduleEntity>();
    public DbSet<AccessRecertificationDecisionEntity> AccessRecertificationDecisions => Set<AccessRecertificationDecisionEntity>();
    public DbSet<SecurityIncidentEntity> SecurityIncidents => Set<SecurityIncidentEntity>();
    public DbSet<VulnerabilityRecordEntity> VulnerabilityRecords => Set<VulnerabilityRecordEntity>();
    public DbSet<SecretRotationEntity> SecretRotations => Set<SecretRotationEntity>();
    public DbSet<PrivilegedAccessEventEntity> PrivilegedAccessEvents => Set<PrivilegedAccessEventEntity>();
    public DbSet<DataClassificationPolicyEntity> DataClassificationPolicies => Set<DataClassificationPolicyEntity>();
    public DbSet<BackupEvidenceEntity> BackupEvidence => Set<BackupEvidenceEntity>();
    public DbSet<RestoreVerificationEntity> RestoreVerifications => Set<RestoreVerificationEntity>();
    public DbSet<DrDrillEntity> DrDrills => Set<DrDrillEntity>();
    public DbSet<LegalHoldEntity> LegalHolds => Set<LegalHoldEntity>();
    public DbSet<CapaRecordEntity> CapaRecords => Set<CapaRecordEntity>();
    public DbSet<CapaActionEntity> CapaActions => Set<CapaActionEntity>();
    public DbSet<EscalationEventEntity> EscalationEvents => Set<EscalationEventEntity>();
    public DbSet<UserOrgAssignmentEntity> UserOrgAssignments => Set<UserOrgAssignmentEntity>();
    public DbSet<ReportingLineEntity> ReportingLines => Set<ReportingLineEntity>();
    public DbSet<ProjectEntity> Projects => Set<ProjectEntity>();
    public DbSet<ProjectHistoryEntity> ProjectHistories => Set<ProjectHistoryEntity>();
    public DbSet<ProjectTypeTemplateEntity> ProjectTypeTemplates => Set<ProjectTypeTemplateEntity>();
    public DbSet<ProjectTypeRoleRequirementEntity> ProjectTypeRoleRequirements => Set<ProjectTypeRoleRequirementEntity>();
    public DbSet<UserProjectAssignmentEntity> UserProjectAssignments => Set<UserProjectAssignmentEntity>();
    public DbSet<AppRoleEntity> AppRoles => Set<AppRoleEntity>();
    public DbSet<PermissionMatrixEntryEntity> PermissionMatrixEntries => Set<PermissionMatrixEntryEntity>();
    public DbSet<SystemSettingEntity> SystemSettings => Set<SystemSettingEntity>();
    public DbSet<UserRegistrationRequestEntity> UserRegistrationRequests => Set<UserRegistrationRequestEntity>();
    public DbSet<UserInvitationEntity> UserInvitations => Set<UserInvitationEntity>();
    public DbSet<ActivityLogEntity> ActivityLogs => Set<ActivityLogEntity>();
    public DbSet<AuditLogEntity> AuditLogs => Set<AuditLogEntity>();
    public DbSet<BusinessAuditEventEntity> BusinessAuditEvents => Set<BusinessAuditEventEntity>();
    public DbSet<AuditPlanEntity> AuditPlans => Set<AuditPlanEntity>();
    public DbSet<AuditFindingEntity> AuditFindings => Set<AuditFindingEntity>();
    public DbSet<EvidenceExportEntity> EvidenceExports => Set<EvidenceExportEntity>();
    public DbSet<NotificationEntity> Notifications => Set<NotificationEntity>();
    public DbSet<NotificationQueueEntity> NotificationQueue => Set<NotificationQueueEntity>();
    public DbSet<WorkflowDefinitionEntity> WorkflowDefinitions => Set<WorkflowDefinitionEntity>();
    public DbSet<WorkflowStepEntity> WorkflowSteps => Set<WorkflowStepEntity>();
    public DbSet<WorkflowStepRoleEntity> WorkflowStepRoles => Set<WorkflowStepRoleEntity>();
    public DbSet<WorkflowStepRouteEntity> WorkflowStepRoutes => Set<WorkflowStepRouteEntity>();
    public DbSet<WorkflowInstanceEntity> WorkflowInstances => Set<WorkflowInstanceEntity>();
    public DbSet<WorkflowInstanceStepEntity> WorkflowInstanceSteps => Set<WorkflowInstanceStepEntity>();
    public DbSet<WorkflowInstanceActionEntity> WorkflowInstanceActions => Set<WorkflowInstanceActionEntity>();
    public DbSet<ProcessAssetEntity> ProcessAssets => Set<ProcessAssetEntity>();
    public DbSet<ProcessAssetVersionEntity> ProcessAssetVersions => Set<ProcessAssetVersionEntity>();
    public DbSet<QaChecklistEntity> QaChecklists => Set<QaChecklistEntity>();
    public DbSet<ProjectPlanEntity> ProjectPlans => Set<ProjectPlanEntity>();
    public DbSet<StakeholderEntity> Stakeholders => Set<StakeholderEntity>();
    public DbSet<TailoringRecordEntity> TailoringRecords => Set<TailoringRecordEntity>();
    public DbSet<RaciMapEntity> RaciMaps => Set<RaciMapEntity>();
    public DbSet<ApprovalEvidenceLogEntity> ApprovalEvidenceLogs => Set<ApprovalEvidenceLogEntity>();
    public DbSet<WorkflowOverrideLogEntity> WorkflowOverrideLogs => Set<WorkflowOverrideLogEntity>();
    public DbSet<SlaRuleEntity> SlaRules => Set<SlaRuleEntity>();
    public DbSet<RetentionPolicyEntity> RetentionPolicies => Set<RetentionPolicyEntity>();
    public DbSet<ArchitectureRecordEntity> ArchitectureRecords => Set<ArchitectureRecordEntity>();
    public DbSet<DesignReviewEntity> DesignReviews => Set<DesignReviewEntity>();
    public DbSet<IntegrationReviewEntity> IntegrationReviews => Set<IntegrationReviewEntity>();
    public DbSet<RequirementEntity> Requirements => Set<RequirementEntity>();
    public DbSet<RequirementVersionEntity> RequirementVersions => Set<RequirementVersionEntity>();
    public DbSet<RequirementBaselineEntity> RequirementBaselines => Set<RequirementBaselineEntity>();
    public DbSet<TraceabilityLinkEntity> TraceabilityLinks => Set<TraceabilityLinkEntity>();
    public DbSet<DefectEntity> Defects => Set<DefectEntity>();
    public DbSet<NonConformanceEntity> NonConformances => Set<NonConformanceEntity>();
    public DbSet<RiskEntity> Risks => Set<RiskEntity>();
    public DbSet<RiskReviewEntity> RiskReviews => Set<RiskReviewEntity>();
    public DbSet<IssueEntity> Issues => Set<IssueEntity>();
    public DbSet<IssueActionEntity> IssueActions => Set<IssueActionEntity>();
    public DbSet<MeetingRecordEntity> MeetingRecords => Set<MeetingRecordEntity>();
    public DbSet<MeetingMinutesEntity> MeetingMinutes => Set<MeetingMinutesEntity>();
    public DbSet<MeetingAttendeeEntity> MeetingAttendees => Set<MeetingAttendeeEntity>();
    public DbSet<DecisionEntity> Decisions => Set<DecisionEntity>();
    public DbSet<TestPlanEntity> TestPlans => Set<TestPlanEntity>();
    public DbSet<TestCaseEntity> TestCases => Set<TestCaseEntity>();
    public DbSet<TestExecutionEntity> TestExecutions => Set<TestExecutionEntity>();
    public DbSet<UatSignoffEntity> UatSignoffs => Set<UatSignoffEntity>();
    public DbSet<ChangeRequestEntity> ChangeRequests => Set<ChangeRequestEntity>();
    public DbSet<ChangeImpactEntity> ChangeImpacts => Set<ChangeImpactEntity>();
    public DbSet<ConfigurationItemEntity> ConfigurationItems => Set<ConfigurationItemEntity>();
    public DbSet<BaselineRegistryEntity> BaselineRegistry => Set<BaselineRegistryEntity>();
    public DbSet<MetricDefinitionEntity> MetricDefinitions => Set<MetricDefinitionEntity>();
    public DbSet<MetricCollectionScheduleEntity> MetricCollectionSchedules => Set<MetricCollectionScheduleEntity>();
    public DbSet<MetricResultEntity> MetricResults => Set<MetricResultEntity>();
    public DbSet<QualityGateResultEntity> QualityGateResults => Set<QualityGateResultEntity>();
    public DbSet<MetricReviewEntity> MetricReviews => Set<MetricReviewEntity>();
    public DbSet<TrendReportEntity> TrendReports => Set<TrendReportEntity>();
    public DbSet<PerformanceBaselineEntity> PerformanceBaselines => Set<PerformanceBaselineEntity>();
    public DbSet<CapacityReviewEntity> CapacityReviews => Set<CapacityReviewEntity>();
    public DbSet<SlowOperationReviewEntity> SlowOperationReviews => Set<SlowOperationReviewEntity>();
    public DbSet<PerformanceGateResultEntity> PerformanceGateResults => Set<PerformanceGateResultEntity>();
    public DbSet<ReleaseEntity> Releases => Set<ReleaseEntity>();
    public DbSet<DeploymentChecklistEntity> DeploymentChecklists => Set<DeploymentChecklistEntity>();
    public DbSet<ReleaseNoteEntity> ReleaseNotes => Set<ReleaseNoteEntity>();
    public DbSet<LessonLearnedEntity> LessonsLearned => Set<LessonLearnedEntity>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<DocumentEntity>(entity =>
        {
            entity.ToTable("documents");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.DocumentTypeId).HasColumnName("document_type_id");
            entity.Property(x => x.ProjectId).HasColumnName("project_id");
            entity.Property(x => x.PhaseCode).HasColumnName("phase_code").HasMaxLength(64);
            entity.Property(x => x.OwnerUserId).HasColumnName("owner_user_id").HasMaxLength(64);
            entity.Property(x => x.CurrentVersionId).HasColumnName("current_version_id");
            entity.Property(x => x.Status).HasColumnName("status").HasMaxLength(32);
            entity.Property(x => x.Classification).HasColumnName("classification").HasMaxLength(64);
            entity.Property(x => x.RetentionClass).HasColumnName("retention_class").HasMaxLength(64);
            entity.Property(x => x.Title).HasColumnName("title").HasMaxLength(512);
            entity.Property(x => x.TagsJson).HasColumnName("tags_json").HasColumnType("jsonb");
            entity.Property(x => x.IsDeleted).HasColumnName("is_deleted");
            entity.Property(x => x.DeletedByUserId).HasColumnName("deleted_by_user_id").HasMaxLength(64);
            entity.Property(x => x.DeletedAt).HasColumnName("deleted_at");
            entity.Property(x => x.DeletedReason).HasColumnName("deleted_reason").HasMaxLength(512);
            entity.Property(x => x.CreatedAt).HasColumnName("created_at");
            entity.Property(x => x.UpdatedAt).HasColumnName("updated_at");
            entity.HasOne<DocumentTypeEntity>().WithMany().HasForeignKey(x => x.DocumentTypeId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<ProjectEntity>().WithMany().HasForeignKey(x => x.ProjectId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<DocumentVersionEntity>()
                .WithMany()
                .HasForeignKey(x => x.CurrentVersionId)
                .OnDelete(DeleteBehavior.SetNull);
            entity.HasIndex(x => new { x.ProjectId, x.Status, x.PhaseCode });
            entity.HasIndex(x => new { x.DocumentTypeId, x.Status });
            entity.HasIndex(x => new { x.OwnerUserId, x.UpdatedAt });
            entity.HasIndex(x => x.IsDeleted);
            entity.HasIndex(x => x.CurrentVersionId);
            entity.HasIndex(x => new { x.IsDeleted, x.UpdatedAt });
        });

        modelBuilder.Entity<DocumentVersionEntity>(entity =>
        {
            entity.ToTable("document_versions");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.DocumentId).HasColumnName("document_id");
            entity.Property(x => x.VersionNumber).HasColumnName("version_number");
            entity.Property(x => x.StorageKey).HasColumnName("storage_key").HasMaxLength(512);
            entity.Property(x => x.FileName).HasColumnName("file_name").HasMaxLength(256);
            entity.Property(x => x.FileSize).HasColumnName("file_size");
            entity.Property(x => x.MimeType).HasColumnName("mime_type").HasMaxLength(128);
            entity.Property(x => x.UploadedBy).HasColumnName("uploaded_by").HasMaxLength(64);
            entity.Property(x => x.UploadedAt).HasColumnName("uploaded_at");
            entity.Property(x => x.Status).HasColumnName("status").HasMaxLength(32);
            entity.Property(x => x.IsDeleted).HasColumnName("is_deleted");
            entity.Property(x => x.DeletedByUserId).HasColumnName("deleted_by_user_id").HasMaxLength(64);
            entity.Property(x => x.DeletedAt).HasColumnName("deleted_at");
            entity.Property(x => x.DeletedReason).HasColumnName("deleted_reason").HasMaxLength(512);
            entity.HasIndex(x => x.DocumentId);
            entity.HasIndex(x => new { x.DocumentId, x.VersionNumber }).IsUnique();
            entity.HasIndex(x => x.StorageKey).IsUnique();
            entity.HasIndex(x => x.IsDeleted);
            entity.HasIndex(x => new { x.DocumentId, x.IsDeleted, x.VersionNumber });
            entity.HasIndex(x => new { x.DocumentId, x.IsDeleted, x.UploadedAt });
            entity.HasIndex(x => x.UploadedAt);
        });

        modelBuilder.Entity<DocumentTypeEntity>(entity =>
        {
            entity.ToTable("document_types");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.Code).HasColumnName("code").HasMaxLength(128);
            entity.Property(x => x.Name).HasColumnName("name").HasMaxLength(256);
            entity.Property(x => x.ModuleOwner).HasColumnName("module_owner").HasMaxLength(128);
            entity.Property(x => x.ClassificationDefault).HasColumnName("classification_default").HasMaxLength(64);
            entity.Property(x => x.RetentionClassDefault).HasColumnName("retention_class_default").HasMaxLength(64);
            entity.Property(x => x.Status).HasColumnName("status").HasMaxLength(32);
            entity.Property(x => x.ApprovalRequired).HasColumnName("approval_required");
            entity.Property(x => x.CreatedAt).HasColumnName("created_at");
            entity.Property(x => x.UpdatedAt).HasColumnName("updated_at");
            entity.HasIndex(x => x.Code).IsUnique();
            entity.HasIndex(x => x.Status);
        });

        modelBuilder.Entity<DocumentApprovalEntity>(entity =>
        {
            entity.ToTable("document_approvals");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.DocumentVersionId).HasColumnName("document_version_id");
            entity.Property(x => x.StepName).HasColumnName("step_name").HasMaxLength(128);
            entity.Property(x => x.ReviewerUserId).HasColumnName("reviewer_user_id").HasMaxLength(64);
            entity.Property(x => x.Decision).HasColumnName("decision").HasMaxLength(32);
            entity.Property(x => x.DecisionReason).HasColumnName("decision_reason").HasMaxLength(2000);
            entity.Property(x => x.DecidedAt).HasColumnName("decided_at");
            entity.Property(x => x.CreatedAt).HasColumnName("created_at");
            entity.HasOne<DocumentVersionEntity>().WithMany().HasForeignKey(x => x.DocumentVersionId).OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(x => x.DocumentVersionId);
        });

        modelBuilder.Entity<DocumentLinkEntity>(entity =>
        {
            entity.ToTable("document_links");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.SourceDocumentId).HasColumnName("source_document_id");
            entity.Property(x => x.TargetEntityType).HasColumnName("target_entity_type").HasMaxLength(64);
            entity.Property(x => x.TargetEntityId).HasColumnName("target_entity_id").HasMaxLength(64);
            entity.Property(x => x.LinkType).HasColumnName("link_type").HasMaxLength(64);
            entity.Property(x => x.CreatedAt).HasColumnName("created_at");
            entity.HasOne<DocumentEntity>().WithMany().HasForeignKey(x => x.SourceDocumentId).OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(x => x.SourceDocumentId);
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
            entity.Property(x => x.DocumentVersionId).HasColumnName("document_version_id");
            entity.Property(x => x.DisplayOrder).HasColumnName("display_order");
            entity.HasOne<DocumentTemplateEntity>()
                .WithMany()
                .HasForeignKey(x => x.TemplateId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne<DocumentEntity>()
                .WithMany()
                .HasForeignKey(x => x.DocumentId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<DocumentVersionEntity>()
                .WithMany()
                .HasForeignKey(x => x.DocumentVersionId)
                .OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(x => x.TemplateId);
            entity.HasIndex(x => x.DocumentId);
            entity.HasIndex(x => x.DocumentVersionId);
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
            entity.Property(x => x.ProjectId).HasColumnName("project_id");
            entity.Property(x => x.Name).HasColumnName("name").HasMaxLength(120);
            entity.Property(x => x.Code).HasColumnName("code").HasMaxLength(80);
            entity.Property(x => x.Status).HasColumnName("status").HasMaxLength(32);
            entity.Property(x => x.Description).HasColumnName("description").HasMaxLength(500);
            entity.Property(x => x.Responsibilities).HasColumnName("responsibilities").HasMaxLength(2000);
            entity.Property(x => x.AuthorityScope).HasColumnName("authority_scope").HasMaxLength(500);
            entity.Property(x => x.DisplayOrder).HasColumnName("display_order");
            entity.Property(x => x.CreatedAt).HasColumnName("created_at");
            entity.Property(x => x.UpdatedAt).HasColumnName("updated_at");
            entity.Property(x => x.DeletedReason).HasColumnName("deleted_reason").HasMaxLength(500);
            entity.Property(x => x.DeletedBy).HasColumnName("deleted_by").HasMaxLength(120);
            entity.Property(x => x.DeletedAt).HasColumnName("deleted_at");
            entity.HasOne<ProjectEntity>().WithMany().HasForeignKey(x => x.ProjectId).OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(x => new { x.ProjectId, x.Name }).HasFilter("\"deleted_at\" IS NULL");
            entity.HasIndex(x => new { x.ProjectId, x.Code }).IsUnique().HasFilter("\"deleted_at\" IS NULL AND \"code\" IS NOT NULL");
            entity.HasIndex(x => new { x.ProjectId, x.Status, x.DisplayOrder, x.Name });
            entity.HasIndex(x => new { x.DeletedAt, x.DisplayOrder, x.Name });
        });

        modelBuilder.Entity<MasterDataItemEntity>(entity =>
        {
            entity.ToTable("master_data_items");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.Domain).HasColumnName("domain").HasMaxLength(128);
            entity.Property(x => x.Code).HasColumnName("code").HasMaxLength(128);
            entity.Property(x => x.Name).HasColumnName("name").HasMaxLength(256);
            entity.Property(x => x.Status).HasColumnName("status").HasMaxLength(32);
            entity.Property(x => x.DisplayOrder).HasColumnName("display_order");
            entity.Property(x => x.CreatedAt).HasColumnName("created_at");
            entity.Property(x => x.UpdatedAt).HasColumnName("updated_at");
            entity.HasIndex(x => new { x.Domain, x.Code }).IsUnique();
            entity.HasIndex(x => new { x.Domain, x.Status, x.DisplayOrder });
        });

        modelBuilder.Entity<MasterDataChangeEntity>(entity =>
        {
            entity.ToTable("master_data_changes");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.MasterDataItemId).HasColumnName("master_data_item_id");
            entity.Property(x => x.ChangeType).HasColumnName("change_type").HasMaxLength(64);
            entity.Property(x => x.ChangedBy).HasColumnName("changed_by").HasMaxLength(64);
            entity.Property(x => x.ChangedAt).HasColumnName("changed_at");
            entity.Property(x => x.Reason).HasColumnName("reason").HasMaxLength(2000);
            entity.HasOne<MasterDataItemEntity>().WithMany().HasForeignKey(x => x.MasterDataItemId).OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(x => new { x.MasterDataItemId, x.ChangedAt });
        });

        modelBuilder.Entity<AccessReviewEntity>(entity =>
        {
            entity.ToTable("access_reviews");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.ScopeType).HasColumnName("scope_type").HasMaxLength(64);
            entity.Property(x => x.ScopeRef).HasColumnName("scope_ref").HasMaxLength(256);
            entity.Property(x => x.ReviewCycle).HasColumnName("review_cycle").HasMaxLength(64);
            entity.Property(x => x.ReviewedBy).HasColumnName("reviewed_by").HasMaxLength(64);
            entity.Property(x => x.Status).HasColumnName("status").HasMaxLength(32);
            entity.Property(x => x.Decision).HasColumnName("decision").HasMaxLength(64);
            entity.Property(x => x.DecisionRationale).HasColumnName("decision_rationale").HasMaxLength(2000);
            entity.Property(x => x.CreatedAt).HasColumnName("created_at");
            entity.Property(x => x.UpdatedAt).HasColumnName("updated_at");
            entity.HasIndex(x => new { x.ScopeType, x.Status });
        });

        modelBuilder.Entity<SecurityReviewEntity>(entity =>
        {
            entity.ToTable("security_reviews");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.ScopeType).HasColumnName("scope_type").HasMaxLength(64);
            entity.Property(x => x.ScopeRef).HasColumnName("scope_ref").HasMaxLength(256);
            entity.Property(x => x.ControlsReviewed).HasColumnName("controls_reviewed").HasMaxLength(2000);
            entity.Property(x => x.FindingsSummary).HasColumnName("findings_summary").HasMaxLength(2000);
            entity.Property(x => x.Status).HasColumnName("status").HasMaxLength(32);
            entity.Property(x => x.CreatedAt).HasColumnName("created_at");
            entity.Property(x => x.UpdatedAt).HasColumnName("updated_at");
            entity.HasIndex(x => new { x.ScopeType, x.Status });
        });

        modelBuilder.Entity<ExternalDependencyEntity>(entity =>
        {
            entity.ToTable("external_dependencies");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.Name).HasColumnName("name").HasMaxLength(256);
            entity.Property(x => x.DependencyType).HasColumnName("dependency_type").HasMaxLength(128);
            entity.Property(x => x.SupplierId).HasColumnName("supplier_id");
            entity.Property(x => x.OwnerUserId).HasColumnName("owner_user_id").HasMaxLength(64);
            entity.Property(x => x.Criticality).HasColumnName("criticality").HasMaxLength(32);
            entity.Property(x => x.Status).HasColumnName("status").HasMaxLength(32);
            entity.Property(x => x.ReviewDueAt).HasColumnName("review_due_at");
            entity.Property(x => x.CreatedAt).HasColumnName("created_at");
            entity.Property(x => x.UpdatedAt).HasColumnName("updated_at");
            entity.HasIndex(x => x.DependencyType);
            entity.HasIndex(x => x.SupplierId);
            entity.HasIndex(x => x.OwnerUserId);
            entity.HasIndex(x => new { x.Criticality, x.Status, x.ReviewDueAt });
        });

        modelBuilder.Entity<SupplierEntity>(entity =>
        {
            entity.ToTable("suppliers");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.Name).HasColumnName("name").HasMaxLength(256);
            entity.Property(x => x.SupplierType).HasColumnName("supplier_type").HasMaxLength(128);
            entity.Property(x => x.OwnerUserId).HasColumnName("owner_user_id").HasMaxLength(64);
            entity.Property(x => x.Status).HasColumnName("status").HasMaxLength(32);
            entity.Property(x => x.Criticality).HasColumnName("criticality").HasMaxLength(32);
            entity.Property(x => x.ReviewDueAt).HasColumnName("review_due_at");
            entity.Property(x => x.CreatedAt).HasColumnName("created_at");
            entity.Property(x => x.UpdatedAt).HasColumnName("updated_at");
            entity.HasIndex(x => x.Name).IsUnique();
            entity.HasIndex(x => x.SupplierType);
            entity.HasIndex(x => x.OwnerUserId);
            entity.HasIndex(x => new { x.Criticality, x.Status });
        });

        modelBuilder.Entity<SupplierAgreementEntity>(entity =>
        {
            entity.ToTable("supplier_agreements");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.SupplierId).HasColumnName("supplier_id");
            entity.Property(x => x.AgreementType).HasColumnName("agreement_type").HasMaxLength(128);
            entity.Property(x => x.EffectiveFrom).HasColumnName("effective_from");
            entity.Property(x => x.EffectiveTo).HasColumnName("effective_to");
            entity.Property(x => x.SlaTerms).HasColumnName("sla_terms").HasMaxLength(2000);
            entity.Property(x => x.EvidenceRef).HasColumnName("evidence_ref").HasMaxLength(512);
            entity.Property(x => x.Status).HasColumnName("status").HasMaxLength(32);
            entity.Property(x => x.CreatedAt).HasColumnName("created_at");
            entity.Property(x => x.UpdatedAt).HasColumnName("updated_at");
            entity.HasIndex(x => new { x.SupplierId, x.Status });
        });

        modelBuilder.Entity<ConfigurationAuditEntity>(entity =>
        {
            entity.ToTable("configuration_audits");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.ScopeRef).HasColumnName("scope_ref").HasMaxLength(256);
            entity.Property(x => x.PlannedAt).HasColumnName("planned_at");
            entity.Property(x => x.Status).HasColumnName("status").HasMaxLength(32);
            entity.Property(x => x.FindingCount).HasColumnName("finding_count");
            entity.Property(x => x.CreatedAt).HasColumnName("created_at");
            entity.Property(x => x.UpdatedAt).HasColumnName("updated_at");
            entity.HasIndex(x => new { x.ScopeRef, x.Status });
        });

        modelBuilder.Entity<AccessRecertificationScheduleEntity>(entity =>
        {
            entity.ToTable("access_recertification_schedules");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.ScopeType).HasColumnName("scope_type").HasMaxLength(64);
            entity.Property(x => x.ScopeRef).HasColumnName("scope_ref").HasMaxLength(256);
            entity.Property(x => x.PlannedAt).HasColumnName("planned_at");
            entity.Property(x => x.ReviewOwnerUserId).HasColumnName("review_owner_user_id").HasMaxLength(128);
            entity.Property(x => x.Status).HasColumnName("status").HasMaxLength(32);
            entity.Property(x => x.SubjectUsersJson).HasColumnName("subject_users_json").HasColumnType("jsonb");
            entity.Property(x => x.ExceptionNotes).HasColumnName("exception_notes").HasMaxLength(2000);
            entity.Property(x => x.CompletedAt).HasColumnName("completed_at");
            entity.Property(x => x.CreatedAt).HasColumnName("created_at");
            entity.Property(x => x.UpdatedAt).HasColumnName("updated_at");
            entity.HasIndex(x => new { x.Status, x.PlannedAt });
            entity.HasIndex(x => x.ReviewOwnerUserId);
        });

        modelBuilder.Entity<AccessRecertificationDecisionEntity>(entity =>
        {
            entity.ToTable("access_recertification_decisions");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.ScheduleId).HasColumnName("schedule_id");
            entity.Property(x => x.SubjectUserId).HasColumnName("subject_user_id").HasMaxLength(128);
            entity.Property(x => x.Decision).HasColumnName("decision").HasMaxLength(32);
            entity.Property(x => x.Reason).HasColumnName("reason").HasMaxLength(2000);
            entity.Property(x => x.DecidedBy).HasColumnName("decided_by").HasMaxLength(128);
            entity.Property(x => x.DecidedAt).HasColumnName("decided_at");
            entity.Property(x => x.CreatedAt).HasColumnName("created_at");
            entity.HasOne<AccessRecertificationScheduleEntity>().WithMany().HasForeignKey(x => x.ScheduleId).OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(x => new { x.ScheduleId, x.Decision });
            entity.HasIndex(x => x.SubjectUserId);
            entity.HasIndex(x => new { x.ScheduleId, x.SubjectUserId }).IsUnique();
        });

        modelBuilder.Entity<SecurityIncidentEntity>(entity =>
        {
            entity.ToTable("security_incidents");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.ProjectId).HasColumnName("project_id");
            entity.Property(x => x.Code).HasColumnName("code").HasMaxLength(128);
            entity.Property(x => x.Title).HasColumnName("title").HasMaxLength(512);
            entity.Property(x => x.Severity).HasColumnName("severity").HasMaxLength(32);
            entity.Property(x => x.ReportedAt).HasColumnName("reported_at");
            entity.Property(x => x.OwnerUserId).HasColumnName("owner_user_id").HasMaxLength(128);
            entity.Property(x => x.Status).HasColumnName("status").HasMaxLength(32);
            entity.Property(x => x.ResolutionSummary).HasColumnName("resolution_summary").HasMaxLength(4000);
            entity.Property(x => x.CreatedAt).HasColumnName("created_at");
            entity.Property(x => x.UpdatedAt).HasColumnName("updated_at");
            entity.HasOne<ProjectEntity>().WithMany().HasForeignKey(x => x.ProjectId).OnDelete(DeleteBehavior.SetNull);
            entity.HasIndex(x => x.Code).IsUnique();
            entity.HasIndex(x => new { x.Severity, x.Status, x.ReportedAt });
            entity.HasIndex(x => x.OwnerUserId);
            entity.HasIndex(x => x.ProjectId);
        });

        modelBuilder.Entity<VulnerabilityRecordEntity>(entity =>
        {
            entity.ToTable("vulnerability_records");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.AssetRef).HasColumnName("asset_ref").HasMaxLength(256);
            entity.Property(x => x.Title).HasColumnName("title").HasMaxLength(512);
            entity.Property(x => x.Severity).HasColumnName("severity").HasMaxLength(32);
            entity.Property(x => x.IdentifiedAt).HasColumnName("identified_at");
            entity.Property(x => x.PatchDueAt).HasColumnName("patch_due_at");
            entity.Property(x => x.OwnerUserId).HasColumnName("owner_user_id").HasMaxLength(128);
            entity.Property(x => x.Status).HasColumnName("status").HasMaxLength(32);
            entity.Property(x => x.VerificationSummary).HasColumnName("verification_summary").HasMaxLength(4000);
            entity.Property(x => x.CreatedAt).HasColumnName("created_at");
            entity.Property(x => x.UpdatedAt).HasColumnName("updated_at");
            entity.HasIndex(x => new { x.AssetRef, x.Status });
            entity.HasIndex(x => new { x.Severity, x.Status });
            entity.HasIndex(x => x.OwnerUserId);
        });

        modelBuilder.Entity<SecretRotationEntity>(entity =>
        {
            entity.ToTable("secret_rotations");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.SecretScope).HasColumnName("secret_scope").HasMaxLength(256);
            entity.Property(x => x.PlannedAt).HasColumnName("planned_at");
            entity.Property(x => x.RotatedAt).HasColumnName("rotated_at");
            entity.Property(x => x.VerifiedBy).HasColumnName("verified_by").HasMaxLength(128);
            entity.Property(x => x.VerifiedAt).HasColumnName("verified_at");
            entity.Property(x => x.Status).HasColumnName("status").HasMaxLength(32);
            entity.Property(x => x.CreatedAt).HasColumnName("created_at");
            entity.Property(x => x.UpdatedAt).HasColumnName("updated_at");
            entity.HasIndex(x => new { x.SecretScope, x.Status });
            entity.HasIndex(x => x.VerifiedBy);
        });

        modelBuilder.Entity<PrivilegedAccessEventEntity>(entity =>
        {
            entity.ToTable("privileged_access_events");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.RequestedBy).HasColumnName("requested_by").HasMaxLength(128);
            entity.Property(x => x.ApprovedBy).HasColumnName("approved_by").HasMaxLength(128);
            entity.Property(x => x.UsedBy).HasColumnName("used_by").HasMaxLength(128);
            entity.Property(x => x.RequestedAt).HasColumnName("requested_at");
            entity.Property(x => x.ApprovedAt).HasColumnName("approved_at");
            entity.Property(x => x.UsedAt).HasColumnName("used_at");
            entity.Property(x => x.ReviewedAt).HasColumnName("reviewed_at");
            entity.Property(x => x.Status).HasColumnName("status").HasMaxLength(32);
            entity.Property(x => x.Reason).HasColumnName("reason").HasMaxLength(2000);
            entity.Property(x => x.CreatedAt).HasColumnName("created_at");
            entity.Property(x => x.UpdatedAt).HasColumnName("updated_at");
            entity.HasIndex(x => new { x.RequestedBy, x.Status });
            entity.HasIndex(x => x.ApprovedBy);
            entity.HasIndex(x => x.UsedBy);
        });

        modelBuilder.Entity<DataClassificationPolicyEntity>(entity =>
        {
            entity.ToTable("data_classification_policies");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.PolicyCode).HasColumnName("policy_code").HasMaxLength(128);
            entity.Property(x => x.ClassificationLevel).HasColumnName("classification_level").HasMaxLength(64);
            entity.Property(x => x.Scope).HasColumnName("scope").HasMaxLength(256);
            entity.Property(x => x.Status).HasColumnName("status").HasMaxLength(32);
            entity.Property(x => x.HandlingRule).HasColumnName("handling_rule").HasMaxLength(2000);
            entity.Property(x => x.CreatedAt).HasColumnName("created_at");
            entity.Property(x => x.UpdatedAt).HasColumnName("updated_at");
            entity.HasIndex(x => x.PolicyCode).IsUnique();
            entity.HasIndex(x => new { x.ClassificationLevel, x.Status });
        });

        modelBuilder.Entity<BackupEvidenceEntity>(entity =>
        {
            entity.ToTable("backup_evidence");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.BackupScope).HasColumnName("backup_scope").HasMaxLength(128);
            entity.Property(x => x.ExecutedAt).HasColumnName("executed_at");
            entity.Property(x => x.ExecutedBy).HasColumnName("executed_by").HasMaxLength(128);
            entity.Property(x => x.Status).HasColumnName("status").HasMaxLength(32);
            entity.Property(x => x.EvidenceRef).HasColumnName("evidence_ref").HasMaxLength(512);
            entity.Property(x => x.CreatedAt).HasColumnName("created_at");
            entity.HasIndex(x => x.BackupScope);
            entity.HasIndex(x => x.ExecutedAt);
            entity.HasIndex(x => x.Status);
            entity.HasIndex(x => new { x.BackupScope, x.ExecutedAt });
        });

        modelBuilder.Entity<RestoreVerificationEntity>(entity =>
        {
            entity.ToTable("restore_verifications");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.BackupEvidenceId).HasColumnName("backup_evidence_id");
            entity.Property(x => x.ExecutedAt).HasColumnName("executed_at");
            entity.Property(x => x.ExecutedBy).HasColumnName("executed_by").HasMaxLength(128);
            entity.Property(x => x.Status).HasColumnName("status").HasMaxLength(32);
            entity.Property(x => x.ResultSummary).HasColumnName("result_summary").HasMaxLength(4000);
            entity.Property(x => x.CreatedAt).HasColumnName("created_at");
            entity.HasOne<BackupEvidenceEntity>().WithMany().HasForeignKey(x => x.BackupEvidenceId).OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(x => x.BackupEvidenceId);
            entity.HasIndex(x => x.ExecutedAt);
            entity.HasIndex(x => x.Status);
        });

        modelBuilder.Entity<DrDrillEntity>(entity =>
        {
            entity.ToTable("dr_drills");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.ScopeRef).HasColumnName("scope_ref").HasMaxLength(256);
            entity.Property(x => x.PlannedAt).HasColumnName("planned_at");
            entity.Property(x => x.ExecutedAt).HasColumnName("executed_at");
            entity.Property(x => x.Status).HasColumnName("status").HasMaxLength(32);
            entity.Property(x => x.FindingCount).HasColumnName("finding_count");
            entity.Property(x => x.Summary).HasColumnName("summary").HasMaxLength(4000);
            entity.Property(x => x.CreatedAt).HasColumnName("created_at");
            entity.Property(x => x.UpdatedAt).HasColumnName("updated_at");
            entity.HasIndex(x => new { x.ScopeRef, x.PlannedAt });
            entity.HasIndex(x => x.Status);
        });

        modelBuilder.Entity<LegalHoldEntity>(entity =>
        {
            entity.ToTable("legal_holds");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.ScopeType).HasColumnName("scope_type").HasMaxLength(64);
            entity.Property(x => x.ScopeRef).HasColumnName("scope_ref").HasMaxLength(256);
            entity.Property(x => x.PlacedAt).HasColumnName("placed_at");
            entity.Property(x => x.PlacedBy).HasColumnName("placed_by").HasMaxLength(128);
            entity.Property(x => x.Status).HasColumnName("status").HasMaxLength(32);
            entity.Property(x => x.Reason).HasColumnName("reason").HasMaxLength(2000);
            entity.Property(x => x.ReleasedAt).HasColumnName("released_at");
            entity.Property(x => x.ReleasedBy).HasColumnName("released_by").HasMaxLength(128);
            entity.Property(x => x.ReleaseReason).HasColumnName("release_reason").HasMaxLength(2000);
            entity.Property(x => x.CreatedAt).HasColumnName("created_at");
            entity.Property(x => x.UpdatedAt).HasColumnName("updated_at");
            entity.HasIndex(x => x.PlacedAt);
            entity.HasIndex(x => x.Status);
            entity.HasIndex(x => new { x.Status, x.PlacedAt });
        });

        modelBuilder.Entity<PhaseApprovalRequestEntity>(entity =>
        {
            entity.ToTable("phase_approval_requests");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.ProjectId).HasColumnName("project_id");
            entity.Property(x => x.PhaseCode).HasColumnName("phase_code").HasMaxLength(128);
            entity.Property(x => x.EntryCriteriaSummary).HasColumnName("entry_criteria_summary").HasMaxLength(4000);
            entity.Property(x => x.RequiredEvidenceRefsJson).HasColumnName("required_evidence_refs_json").HasColumnType("jsonb");
            entity.Property(x => x.Status).HasColumnName("status").HasMaxLength(32);
            entity.Property(x => x.SubmittedBy).HasColumnName("submitted_by").HasMaxLength(64);
            entity.Property(x => x.SubmittedAt).HasColumnName("submitted_at");
            entity.Property(x => x.Decision).HasColumnName("decision").HasMaxLength(32);
            entity.Property(x => x.DecisionReason).HasColumnName("decision_reason").HasMaxLength(2000);
            entity.Property(x => x.DecidedBy).HasColumnName("decided_by").HasMaxLength(64);
            entity.Property(x => x.DecidedAt).HasColumnName("decided_at");
            entity.Property(x => x.BaselineBy).HasColumnName("baseline_by").HasMaxLength(64);
            entity.Property(x => x.BaselinedAt).HasColumnName("baselined_at");
            entity.Property(x => x.CreatedAt).HasColumnName("created_at");
            entity.Property(x => x.UpdatedAt).HasColumnName("updated_at");
            entity.HasOne<ProjectEntity>().WithMany().HasForeignKey(x => x.ProjectId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne<UserEntity>().WithMany().HasForeignKey(x => x.SubmittedBy).OnDelete(DeleteBehavior.SetNull);
            entity.HasOne<UserEntity>().WithMany().HasForeignKey(x => x.DecidedBy).OnDelete(DeleteBehavior.SetNull);
            entity.HasOne<UserEntity>().WithMany().HasForeignKey(x => x.BaselineBy).OnDelete(DeleteBehavior.SetNull);
            entity.HasIndex(x => new { x.ProjectId, x.Status });
            entity.HasIndex(x => new { x.ProjectId, x.PhaseCode, x.CreatedAt });
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

        modelBuilder.Entity<PermissionMatrixEntryEntity>(entity =>
        {
            entity.ToTable("permission_matrix_entries");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.RoleKeycloakName).HasColumnName("role_keycloak_name").HasMaxLength(120);
            entity.Property(x => x.PermissionKey).HasColumnName("permission_key").HasMaxLength(160);
            entity.Property(x => x.IsGranted).HasColumnName("is_granted");
            entity.Property(x => x.AppliedAt).HasColumnName("applied_at");
            entity.Property(x => x.AppliedBy).HasColumnName("applied_by").HasMaxLength(120);
            entity.Property(x => x.Reason).HasColumnName("reason").HasMaxLength(2000);
            entity.HasIndex(x => new { x.RoleKeycloakName, x.PermissionKey }).IsUnique();
            entity.HasIndex(x => x.AppliedAt);
        });

        modelBuilder.Entity<SystemSettingEntity>(entity =>
        {
            entity.ToTable("system_settings");
            entity.HasKey(x => x.Key);
            entity.Property(x => x.Key).HasColumnName("key").HasMaxLength(120);
            entity.Property(x => x.Value).HasColumnName("value").HasMaxLength(4000);
            entity.Property(x => x.UpdatedAt).HasColumnName("updated_at");
            entity.Property(x => x.UpdatedBy).HasColumnName("updated_by").HasMaxLength(120);
            entity.Property(x => x.Reason).HasColumnName("reason").HasMaxLength(2000);
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

        modelBuilder.Entity<AuditPlanEntity>(entity =>
        {
            entity.ToTable("audit_plans");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.ProjectId).HasColumnName("project_id");
            entity.Property(x => x.Title).HasColumnName("title").HasMaxLength(512);
            entity.Property(x => x.Scope).HasColumnName("scope").HasMaxLength(4000);
            entity.Property(x => x.Criteria).HasColumnName("criteria").HasMaxLength(4000);
            entity.Property(x => x.PlannedAt).HasColumnName("planned_at");
            entity.Property(x => x.Status).HasColumnName("status").HasMaxLength(32);
            entity.Property(x => x.OwnerUserId).HasColumnName("owner_user_id").HasMaxLength(128);
            entity.Property(x => x.CreatedAt).HasColumnName("created_at");
            entity.Property(x => x.UpdatedAt).HasColumnName("updated_at");
            entity.HasOne<ProjectEntity>().WithMany().HasForeignKey(x => x.ProjectId).OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(x => new { x.ProjectId, x.Status, x.PlannedAt });
            entity.HasIndex(x => x.OwnerUserId);
        });

        modelBuilder.Entity<AuditFindingEntity>(entity =>
        {
            entity.ToTable("audit_findings");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.AuditPlanId).HasColumnName("audit_plan_id");
            entity.Property(x => x.Code).HasColumnName("code").HasMaxLength(128);
            entity.Property(x => x.Title).HasColumnName("title").HasMaxLength(512);
            entity.Property(x => x.Description).HasColumnName("description").HasMaxLength(4000);
            entity.Property(x => x.Severity).HasColumnName("severity").HasMaxLength(32);
            entity.Property(x => x.Status).HasColumnName("status").HasMaxLength(32);
            entity.Property(x => x.OwnerUserId).HasColumnName("owner_user_id").HasMaxLength(128);
            entity.Property(x => x.DueDate).HasColumnName("due_date");
            entity.Property(x => x.ResolutionSummary).HasColumnName("resolution_summary").HasMaxLength(4000);
            entity.Property(x => x.CreatedAt).HasColumnName("created_at");
            entity.Property(x => x.UpdatedAt).HasColumnName("updated_at");
            entity.HasOne<AuditPlanEntity>().WithMany().HasForeignKey(x => x.AuditPlanId).OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(x => new { x.AuditPlanId, x.Code }).IsUnique();
            entity.HasIndex(x => new { x.AuditPlanId, x.Status });
            entity.HasIndex(x => x.OwnerUserId);
        });

        modelBuilder.Entity<EvidenceExportEntity>(entity =>
        {
            entity.ToTable("evidence_exports");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.RequestedBy).HasColumnName("requested_by").HasMaxLength(128);
            entity.Property(x => x.ScopeType).HasColumnName("scope_type").HasMaxLength(64);
            entity.Property(x => x.ScopeRef).HasColumnName("scope_ref").HasMaxLength(256);
            entity.Property(x => x.RequestedAt).HasColumnName("requested_at");
            entity.Property(x => x.Status).HasColumnName("status").HasMaxLength(32);
            entity.Property(x => x.OutputRef).HasColumnName("output_ref").HasMaxLength(512);
            entity.Property(x => x.From).HasColumnName("from_at");
            entity.Property(x => x.To).HasColumnName("to_at");
            entity.Property(x => x.IncludedArtifactTypesJson).HasColumnName("included_artifact_types_json").HasColumnType("jsonb");
            entity.Property(x => x.FailureReason).HasColumnName("failure_reason").HasMaxLength(2000);
            entity.HasIndex(x => new { x.RequestedBy, x.RequestedAt });
            entity.HasIndex(x => new { x.Status, x.RequestedAt });
        });

        modelBuilder.Entity<MetricDefinitionEntity>(entity =>
        {
            entity.ToTable("metric_definitions");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.Code).HasColumnName("code").HasMaxLength(128);
            entity.Property(x => x.Name).HasColumnName("name").HasMaxLength(256);
            entity.Property(x => x.MetricType).HasColumnName("metric_type").HasMaxLength(128);
            entity.Property(x => x.OwnerUserId).HasColumnName("owner_user_id").HasMaxLength(128);
            entity.Property(x => x.TargetValue).HasColumnName("target_value").HasPrecision(18, 4);
            entity.Property(x => x.ThresholdValue).HasColumnName("threshold_value").HasPrecision(18, 4);
            entity.Property(x => x.Status).HasColumnName("status").HasMaxLength(32);
            entity.Property(x => x.CreatedAt).HasColumnName("created_at");
            entity.Property(x => x.UpdatedAt).HasColumnName("updated_at");
            entity.HasIndex(x => x.Code).IsUnique();
            entity.HasIndex(x => new { x.MetricType, x.Status });
            entity.HasIndex(x => x.OwnerUserId);
        });

        modelBuilder.Entity<MetricCollectionScheduleEntity>(entity =>
        {
            entity.ToTable("metric_collection_schedules");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.MetricDefinitionId).HasColumnName("metric_definition_id");
            entity.Property(x => x.CollectionFrequency).HasColumnName("collection_frequency").HasMaxLength(64);
            entity.Property(x => x.CollectorType).HasColumnName("collector_type").HasMaxLength(64);
            entity.Property(x => x.NextRunAt).HasColumnName("next_run_at");
            entity.Property(x => x.Status).HasColumnName("status").HasMaxLength(32);
            entity.Property(x => x.CreatedAt).HasColumnName("created_at");
            entity.Property(x => x.UpdatedAt).HasColumnName("updated_at");
            entity.HasOne<MetricDefinitionEntity>().WithMany().HasForeignKey(x => x.MetricDefinitionId).OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(x => new { x.MetricDefinitionId, x.Status });
            entity.HasIndex(x => x.NextRunAt);
        });

        modelBuilder.Entity<MetricResultEntity>(entity =>
        {
            entity.ToTable("metric_results");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.MetricDefinitionId).HasColumnName("metric_definition_id");
            entity.Property(x => x.QualityGateResultId).HasColumnName("quality_gate_result_id");
            entity.Property(x => x.MeasuredAt).HasColumnName("measured_at");
            entity.Property(x => x.MeasuredValue).HasColumnName("measured_value").HasPrecision(18, 4);
            entity.Property(x => x.Status).HasColumnName("status").HasMaxLength(32);
            entity.Property(x => x.SourceRef).HasColumnName("source_ref").HasMaxLength(512);
            entity.Property(x => x.CreatedAt).HasColumnName("created_at");
            entity.HasOne<MetricDefinitionEntity>().WithMany().HasForeignKey(x => x.MetricDefinitionId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne<QualityGateResultEntity>().WithMany().HasForeignKey(x => x.QualityGateResultId).OnDelete(DeleteBehavior.SetNull);
            entity.HasIndex(x => new { x.MetricDefinitionId, x.MeasuredAt });
            entity.HasIndex(x => x.Status);
        });

        modelBuilder.Entity<QualityGateResultEntity>(entity =>
        {
            entity.ToTable("quality_gate_results");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.ProjectId).HasColumnName("project_id");
            entity.Property(x => x.GateType).HasColumnName("gate_type").HasMaxLength(128);
            entity.Property(x => x.EvaluatedAt).HasColumnName("evaluated_at");
            entity.Property(x => x.Result).HasColumnName("result").HasMaxLength(32);
            entity.Property(x => x.Reason).HasColumnName("reason").HasMaxLength(2000);
            entity.Property(x => x.OverrideReason).HasColumnName("override_reason").HasMaxLength(2000);
            entity.Property(x => x.EvaluatedByUserId).HasColumnName("evaluated_by_user_id").HasMaxLength(128);
            entity.Property(x => x.OverriddenByUserId).HasColumnName("overridden_by_user_id").HasMaxLength(128);
            entity.Property(x => x.CreatedAt).HasColumnName("created_at");
            entity.Property(x => x.UpdatedAt).HasColumnName("updated_at");
            entity.HasOne<ProjectEntity>().WithMany().HasForeignKey(x => x.ProjectId).OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(x => new { x.ProjectId, x.GateType, x.EvaluatedAt });
            entity.HasIndex(x => new { x.Result, x.EvaluatedAt });
        });

        modelBuilder.Entity<MetricReviewEntity>(entity =>
        {
            entity.ToTable("metric_reviews");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.ProjectId).HasColumnName("project_id");
            entity.Property(x => x.ReviewPeriod).HasColumnName("review_period").HasMaxLength(64);
            entity.Property(x => x.ReviewedBy).HasColumnName("reviewed_by").HasMaxLength(128);
            entity.Property(x => x.Status).HasColumnName("status").HasMaxLength(32);
            entity.Property(x => x.Summary).HasColumnName("summary").HasMaxLength(4000);
            entity.Property(x => x.OpenActionCount).HasColumnName("open_action_count");
            entity.Property(x => x.CreatedAt).HasColumnName("created_at");
            entity.Property(x => x.UpdatedAt).HasColumnName("updated_at");
            entity.HasOne<ProjectEntity>().WithMany().HasForeignKey(x => x.ProjectId).OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(x => new { x.ProjectId, x.Status });
        });

        modelBuilder.Entity<TrendReportEntity>(entity =>
        {
            entity.ToTable("trend_reports");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.ProjectId).HasColumnName("project_id");
            entity.Property(x => x.MetricDefinitionId).HasColumnName("metric_definition_id");
            entity.Property(x => x.PeriodFrom).HasColumnName("period_from");
            entity.Property(x => x.PeriodTo).HasColumnName("period_to");
            entity.Property(x => x.Status).HasColumnName("status").HasMaxLength(32);
            entity.Property(x => x.ReportRef).HasColumnName("report_ref").HasMaxLength(512);
            entity.Property(x => x.TrendDirection).HasColumnName("trend_direction").HasMaxLength(64);
            entity.Property(x => x.Variance).HasColumnName("variance").HasPrecision(18, 4);
            entity.Property(x => x.RecommendedAction).HasColumnName("recommended_action").HasMaxLength(2000);
            entity.Property(x => x.CreatedAt).HasColumnName("created_at");
            entity.Property(x => x.UpdatedAt).HasColumnName("updated_at");
            entity.HasOne<ProjectEntity>().WithMany().HasForeignKey(x => x.ProjectId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne<MetricDefinitionEntity>().WithMany().HasForeignKey(x => x.MetricDefinitionId).OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(x => new { x.ProjectId, x.MetricDefinitionId });
            entity.HasIndex(x => x.Status);
        });

        modelBuilder.Entity<PerformanceBaselineEntity>(entity =>
        {
            entity.ToTable("performance_baselines");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.ScopeType).HasColumnName("scope_type").HasMaxLength(64);
            entity.Property(x => x.ScopeRef).HasColumnName("scope_ref").HasMaxLength(256);
            entity.Property(x => x.MetricName).HasColumnName("metric_name").HasMaxLength(128);
            entity.Property(x => x.TargetValue).HasColumnName("target_value").HasPrecision(18, 4);
            entity.Property(x => x.ThresholdValue).HasColumnName("threshold_value").HasPrecision(18, 4);
            entity.Property(x => x.Status).HasColumnName("status").HasMaxLength(32);
            entity.Property(x => x.CreatedAt).HasColumnName("created_at");
            entity.Property(x => x.UpdatedAt).HasColumnName("updated_at");
            entity.HasIndex(x => x.MetricName);
            entity.HasIndex(x => new { x.ScopeType, x.MetricName, x.Status });
        });

        modelBuilder.Entity<CapacityReviewEntity>(entity =>
        {
            entity.ToTable("capacity_reviews");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.ScopeRef).HasColumnName("scope_ref").HasMaxLength(256);
            entity.Property(x => x.ReviewPeriod).HasColumnName("review_period").HasMaxLength(64);
            entity.Property(x => x.ReviewedBy).HasColumnName("reviewed_by").HasMaxLength(128);
            entity.Property(x => x.Status).HasColumnName("status").HasMaxLength(32);
            entity.Property(x => x.Summary).HasColumnName("summary").HasMaxLength(4000);
            entity.Property(x => x.ActionCount).HasColumnName("action_count");
            entity.Property(x => x.CreatedAt).HasColumnName("created_at");
            entity.Property(x => x.UpdatedAt).HasColumnName("updated_at");
            entity.HasIndex(x => new { x.ScopeRef, x.Status });
            entity.HasIndex(x => x.ReviewedBy);
        });

        modelBuilder.Entity<SlowOperationReviewEntity>(entity =>
        {
            entity.ToTable("slow_operation_reviews");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.OperationType).HasColumnName("operation_type").HasMaxLength(64);
            entity.Property(x => x.OperationKey).HasColumnName("operation_key").HasMaxLength(256);
            entity.Property(x => x.ObservedLatencyMs).HasColumnName("observed_latency_ms");
            entity.Property(x => x.FrequencyPerHour).HasColumnName("frequency_per_hour");
            entity.Property(x => x.Status).HasColumnName("status").HasMaxLength(32);
            entity.Property(x => x.OwnerUserId).HasColumnName("owner_user_id").HasMaxLength(128);
            entity.Property(x => x.OptimizationSummary).HasColumnName("optimization_summary").HasMaxLength(4000);
            entity.Property(x => x.CreatedAt).HasColumnName("created_at");
            entity.Property(x => x.UpdatedAt).HasColumnName("updated_at");
            entity.HasIndex(x => new { x.OperationType, x.Status });
            entity.HasIndex(x => new { x.OwnerUserId, x.Status });
            entity.HasIndex(x => x.OperationKey);
        });

        modelBuilder.Entity<PerformanceGateResultEntity>(entity =>
        {
            entity.ToTable("performance_gate_results");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.ScopeRef).HasColumnName("scope_ref").HasMaxLength(256);
            entity.Property(x => x.EvaluatedAt).HasColumnName("evaluated_at");
            entity.Property(x => x.Result).HasColumnName("result").HasMaxLength(32);
            entity.Property(x => x.Reason).HasColumnName("reason").HasMaxLength(2000);
            entity.Property(x => x.OverrideReason).HasColumnName("override_reason").HasMaxLength(2000);
            entity.Property(x => x.EvidenceRef).HasColumnName("evidence_ref").HasMaxLength(512);
            entity.Property(x => x.EvaluatedByUserId).HasColumnName("evaluated_by_user_id").HasMaxLength(128);
            entity.Property(x => x.OverriddenByUserId).HasColumnName("overridden_by_user_id").HasMaxLength(128);
            entity.Property(x => x.CreatedAt).HasColumnName("created_at");
            entity.Property(x => x.UpdatedAt).HasColumnName("updated_at");
            entity.HasIndex(x => new { x.ScopeRef, x.EvaluatedAt });
            entity.HasIndex(x => new { x.Result, x.EvaluatedAt });
        });

        modelBuilder.Entity<ReleaseEntity>(entity =>
        {
            entity.ToTable("releases");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.ProjectId).HasColumnName("project_id");
            entity.Property(x => x.ReleaseCode).HasColumnName("release_code").HasMaxLength(128);
            entity.Property(x => x.Title).HasColumnName("title").HasMaxLength(512);
            entity.Property(x => x.PlannedAt).HasColumnName("planned_at");
            entity.Property(x => x.ReleasedAt).HasColumnName("released_at");
            entity.Property(x => x.Status).HasColumnName("status").HasMaxLength(32);
            entity.Property(x => x.QualityGateResultId).HasColumnName("quality_gate_result_id");
            entity.Property(x => x.QualityGateOverrideReason).HasColumnName("quality_gate_override_reason").HasMaxLength(2000);
            entity.Property(x => x.ApprovedByUserId).HasColumnName("approved_by_user_id").HasMaxLength(64);
            entity.Property(x => x.ApprovedAt).HasColumnName("approved_at");
            entity.Property(x => x.CreatedAt).HasColumnName("created_at");
            entity.Property(x => x.UpdatedAt).HasColumnName("updated_at");
            entity.HasOne<ProjectEntity>().WithMany().HasForeignKey(x => x.ProjectId).OnDelete(DeleteBehavior.Restrict);
            entity.HasOne<QualityGateResultEntity>().WithMany().HasForeignKey(x => x.QualityGateResultId).OnDelete(DeleteBehavior.SetNull);
            entity.HasIndex(x => new { x.ProjectId, x.ReleaseCode }).IsUnique();
            entity.HasIndex(x => new { x.ProjectId, x.Status, x.PlannedAt });
        });

        modelBuilder.Entity<DeploymentChecklistEntity>(entity =>
        {
            entity.ToTable("deployment_checklists");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.ReleaseId).HasColumnName("release_id");
            entity.Property(x => x.ChecklistItem).HasColumnName("checklist_item").HasMaxLength(512);
            entity.Property(x => x.OwnerUserId).HasColumnName("owner_user_id").HasMaxLength(64);
            entity.Property(x => x.Status).HasColumnName("status").HasMaxLength(32);
            entity.Property(x => x.CompletedAt).HasColumnName("completed_at");
            entity.Property(x => x.EvidenceRef).HasColumnName("evidence_ref").HasMaxLength(1024);
            entity.Property(x => x.CreatedAt).HasColumnName("created_at");
            entity.Property(x => x.UpdatedAt).HasColumnName("updated_at");
            entity.HasOne<ReleaseEntity>().WithMany().HasForeignKey(x => x.ReleaseId).OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(x => new { x.ReleaseId, x.Status });
        });

        modelBuilder.Entity<ReleaseNoteEntity>(entity =>
        {
            entity.ToTable("release_notes");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.ReleaseId).HasColumnName("release_id");
            entity.Property(x => x.Summary).HasColumnName("summary").HasMaxLength(2000);
            entity.Property(x => x.IncludedChanges).HasColumnName("included_changes").HasMaxLength(4000);
            entity.Property(x => x.KnownIssues).HasColumnName("known_issues").HasMaxLength(4000);
            entity.Property(x => x.Status).HasColumnName("status").HasMaxLength(32);
            entity.Property(x => x.PublishedAt).HasColumnName("published_at");
            entity.Property(x => x.CreatedAt).HasColumnName("created_at");
            entity.Property(x => x.UpdatedAt).HasColumnName("updated_at");
            entity.HasOne<ReleaseEntity>().WithMany().HasForeignKey(x => x.ReleaseId).OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(x => new { x.ReleaseId, x.Status });
        });

        modelBuilder.Entity<LessonLearnedEntity>(entity =>
        {
            entity.ToTable("lessons_learned");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.ProjectId).HasColumnName("project_id");
            entity.Property(x => x.Title).HasColumnName("title").HasMaxLength(512);
            entity.Property(x => x.Summary).HasColumnName("summary").HasMaxLength(4000);
            entity.Property(x => x.LessonType).HasColumnName("lesson_type").HasMaxLength(128);
            entity.Property(x => x.OwnerUserId).HasColumnName("owner_user_id").HasMaxLength(128);
            entity.Property(x => x.Status).HasColumnName("status").HasMaxLength(32);
            entity.Property(x => x.SourceRef).HasColumnName("source_ref").HasMaxLength(512);
            entity.Property(x => x.Context).HasColumnName("context").HasMaxLength(4000);
            entity.Property(x => x.WhatHappened).HasColumnName("what_happened").HasMaxLength(4000);
            entity.Property(x => x.WhatToRepeat).HasColumnName("what_to_repeat").HasMaxLength(4000);
            entity.Property(x => x.WhatToAvoid).HasColumnName("what_to_avoid").HasMaxLength(4000);
            entity.Property(x => x.LinkedEvidenceJson).HasColumnName("linked_evidence_json").HasColumnType("jsonb");
            entity.Property(x => x.PublishedAt).HasColumnName("published_at");
            entity.Property(x => x.CreatedAt).HasColumnName("created_at");
            entity.Property(x => x.UpdatedAt).HasColumnName("updated_at");
            entity.HasOne<ProjectEntity>().WithMany().HasForeignKey(x => x.ProjectId).OnDelete(DeleteBehavior.Restrict);
            entity.HasIndex(x => new { x.ProjectId, x.LessonType, x.Status });
            entity.HasIndex(x => x.OwnerUserId);
            entity.HasIndex(x => x.PublishedAt);
        });

        modelBuilder.Entity<NotificationEntity>(entity =>
        {
            entity.ToTable("notifications");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.RecipientUserId).HasColumnName("recipient_user_id").HasMaxLength(64);
            entity.Property(x => x.Title).HasColumnName("title").HasMaxLength(256);
            entity.Property(x => x.Description).HasColumnName("description").HasMaxLength(1024);
            entity.Property(x => x.Source).HasColumnName("source").HasMaxLength(64);
            entity.Property(x => x.Status).HasColumnName("status").HasMaxLength(32);
            entity.Property(x => x.CreatedAt).HasColumnName("created_at");
            entity.Property(x => x.ReadAt).HasColumnName("read_at");
            entity.HasIndex(x => x.RecipientUserId);
            entity.HasIndex(x => new { x.RecipientUserId, x.Status });
            entity.HasIndex(x => new { x.RecipientUserId, x.CreatedAt });
        });

        modelBuilder.Entity<NotificationQueueEntity>(entity =>
        {
            entity.ToTable("notification_queue");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.Channel).HasColumnName("channel").HasMaxLength(64);
            entity.Property(x => x.TargetRef).HasColumnName("target_ref").HasMaxLength(512);
            entity.Property(x => x.PayloadRef).HasColumnName("payload_ref").HasMaxLength(512);
            entity.Property(x => x.QueuedAt).HasColumnName("queued_at");
            entity.Property(x => x.Status).HasColumnName("status").HasMaxLength(32);
            entity.Property(x => x.RetryCount).HasColumnName("retry_count");
            entity.Property(x => x.LastError).HasColumnName("last_error").HasMaxLength(2000);
            entity.Property(x => x.LastRetriedAt).HasColumnName("last_retried_at");
            entity.HasIndex(x => new { x.Status, x.QueuedAt });
            entity.HasIndex(x => new { x.Channel, x.Status });
        });

        modelBuilder.Entity<CapaRecordEntity>(entity =>
        {
            entity.ToTable("capa_records");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.SourceType).HasColumnName("source_type").HasMaxLength(64);
            entity.Property(x => x.SourceRef).HasColumnName("source_ref").HasMaxLength(256);
            entity.Property(x => x.Title).HasColumnName("title").HasMaxLength(512);
            entity.Property(x => x.OwnerUserId).HasColumnName("owner_user_id").HasMaxLength(128);
            entity.Property(x => x.RootCauseSummary).HasColumnName("root_cause_summary").HasMaxLength(4000);
            entity.Property(x => x.Status).HasColumnName("status").HasMaxLength(32);
            entity.Property(x => x.CreatedAt).HasColumnName("created_at");
            entity.Property(x => x.UpdatedAt).HasColumnName("updated_at");
            entity.Property(x => x.VerifiedAt).HasColumnName("verified_at");
            entity.Property(x => x.VerifiedBy).HasColumnName("verified_by").HasMaxLength(128);
            entity.Property(x => x.ClosedAt).HasColumnName("closed_at");
            entity.Property(x => x.ClosedBy).HasColumnName("closed_by").HasMaxLength(128);
            entity.HasIndex(x => new { x.SourceType, x.Status });
            entity.HasIndex(x => new { x.OwnerUserId, x.Status });
        });

        modelBuilder.Entity<CapaActionEntity>(entity =>
        {
            entity.ToTable("capa_actions");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.CapaRecordId).HasColumnName("capa_record_id");
            entity.Property(x => x.ActionDescription).HasColumnName("action_description").HasMaxLength(2000);
            entity.Property(x => x.AssignedTo).HasColumnName("assigned_to").HasMaxLength(128);
            entity.Property(x => x.DueDate).HasColumnName("due_date");
            entity.Property(x => x.Status).HasColumnName("status").HasMaxLength(32);
            entity.Property(x => x.CreatedAt).HasColumnName("created_at");
            entity.Property(x => x.UpdatedAt).HasColumnName("updated_at");
            entity.HasOne<CapaRecordEntity>().WithMany().HasForeignKey(x => x.CapaRecordId).OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(x => new { x.CapaRecordId, x.Status });
            entity.HasIndex(x => new { x.AssignedTo, x.DueDate });
        });

        modelBuilder.Entity<EscalationEventEntity>(entity =>
        {
            entity.ToTable("escalation_events");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.ScopeType).HasColumnName("scope_type").HasMaxLength(64);
            entity.Property(x => x.ScopeRef).HasColumnName("scope_ref").HasMaxLength(256);
            entity.Property(x => x.TriggeredAt).HasColumnName("triggered_at");
            entity.Property(x => x.TriggerReason).HasColumnName("trigger_reason").HasMaxLength(2000);
            entity.Property(x => x.EscalatedTo).HasColumnName("escalated_to").HasMaxLength(128);
            entity.Property(x => x.Status).HasColumnName("status").HasMaxLength(32);
            entity.Property(x => x.CreatedAt).HasColumnName("created_at");
            entity.Property(x => x.UpdatedAt).HasColumnName("updated_at");
            entity.HasIndex(x => new { x.ScopeType, x.Status });
            entity.HasIndex(x => new { x.EscalatedTo, x.TriggeredAt });
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

        modelBuilder.Entity<ProcessAssetEntity>(entity =>
        {
            entity.ToTable("process_assets");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.Code).HasColumnName("code").HasMaxLength(128);
            entity.Property(x => x.Name).HasColumnName("name").HasMaxLength(256);
            entity.Property(x => x.Category).HasColumnName("category").HasMaxLength(128);
            entity.Property(x => x.Status).HasColumnName("status").HasMaxLength(32);
            entity.Property(x => x.OwnerUserId).HasColumnName("owner_user_id").HasMaxLength(64);
            entity.Property(x => x.EffectiveFrom).HasColumnName("effective_from");
            entity.Property(x => x.EffectiveTo).HasColumnName("effective_to");
            entity.Property(x => x.CurrentVersionId).HasColumnName("current_version_id");
            entity.Property(x => x.CreatedAt).HasColumnName("created_at");
            entity.Property(x => x.UpdatedAt).HasColumnName("updated_at");
            entity.HasIndex(x => x.Code).IsUnique();
            entity.HasIndex(x => new { x.Category, x.Status });
            entity.HasIndex(x => x.OwnerUserId);
        });

        modelBuilder.Entity<ProcessAssetVersionEntity>(entity =>
        {
            entity.ToTable("process_asset_versions");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.ProcessAssetId).HasColumnName("process_asset_id");
            entity.Property(x => x.VersionNumber).HasColumnName("version_number");
            entity.Property(x => x.Title).HasColumnName("title").HasMaxLength(256);
            entity.Property(x => x.Summary).HasColumnName("summary").HasMaxLength(4000);
            entity.Property(x => x.ContentRef).HasColumnName("content_ref").HasMaxLength(512);
            entity.Property(x => x.Status).HasColumnName("status").HasMaxLength(32);
            entity.Property(x => x.ChangeSummary).HasColumnName("change_summary").HasMaxLength(2000);
            entity.Property(x => x.ApprovedBy).HasColumnName("approved_by").HasMaxLength(128);
            entity.Property(x => x.ApprovedAt).HasColumnName("approved_at");
            entity.Property(x => x.CreatedAt).HasColumnName("created_at");
            entity.Property(x => x.UpdatedAt).HasColumnName("updated_at");
            entity.HasOne<ProcessAssetEntity>().WithMany().HasForeignKey(x => x.ProcessAssetId).OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(x => new { x.ProcessAssetId, x.VersionNumber }).IsUnique();
            entity.HasIndex(x => new { x.ProcessAssetId, x.Status });
        });

        modelBuilder.Entity<QaChecklistEntity>(entity =>
        {
            entity.ToTable("qa_checklists");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.Code).HasColumnName("code").HasMaxLength(128);
            entity.Property(x => x.Name).HasColumnName("name").HasMaxLength(256);
            entity.Property(x => x.Scope).HasColumnName("scope").HasMaxLength(256);
            entity.Property(x => x.Status).HasColumnName("status").HasMaxLength(32);
            entity.Property(x => x.OwnerUserId).HasColumnName("owner_user_id").HasMaxLength(64);
            entity.Property(x => x.ItemsJson).HasColumnName("items_json").HasColumnType("jsonb");
            entity.Property(x => x.CreatedAt).HasColumnName("created_at");
            entity.Property(x => x.UpdatedAt).HasColumnName("updated_at");
            entity.HasIndex(x => x.Code).IsUnique();
            entity.HasIndex(x => new { x.Scope, x.Status });
        });

        modelBuilder.Entity<ProjectPlanEntity>(entity =>
        {
            entity.ToTable("project_plans");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.ProjectId).HasColumnName("project_id");
            entity.Property(x => x.Name).HasColumnName("name").HasMaxLength(256);
            entity.Property(x => x.ScopeSummary).HasColumnName("scope_summary").HasMaxLength(4000);
            entity.Property(x => x.LifecycleModel).HasColumnName("lifecycle_model").HasMaxLength(128);
            entity.Property(x => x.StartDate).HasColumnName("start_date");
            entity.Property(x => x.TargetEndDate).HasColumnName("target_end_date");
            entity.Property(x => x.OwnerUserId).HasColumnName("owner_user_id").HasMaxLength(64);
            entity.Property(x => x.Status).HasColumnName("status").HasMaxLength(32);
            entity.Property(x => x.MilestonesJson).HasColumnName("milestones_json").HasColumnType("jsonb");
            entity.Property(x => x.RolesJson).HasColumnName("roles_json").HasColumnType("jsonb");
            entity.Property(x => x.RiskApproach).HasColumnName("risk_approach").HasMaxLength(4000);
            entity.Property(x => x.QualityApproach).HasColumnName("quality_approach").HasMaxLength(4000);
            entity.Property(x => x.ApprovalReason).HasColumnName("approval_reason").HasMaxLength(2000);
            entity.Property(x => x.ApprovedBy).HasColumnName("approved_by").HasMaxLength(128);
            entity.Property(x => x.ApprovedAt).HasColumnName("approved_at");
            entity.Property(x => x.CreatedAt).HasColumnName("created_at");
            entity.Property(x => x.UpdatedAt).HasColumnName("updated_at");
            entity.HasOne<ProjectEntity>().WithMany().HasForeignKey(x => x.ProjectId).OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(x => new { x.ProjectId, x.Status });
            entity.HasIndex(x => new { x.Status, x.OwnerUserId });
        });

        modelBuilder.Entity<StakeholderEntity>(entity =>
        {
            entity.ToTable("stakeholders");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.ProjectId).HasColumnName("project_id");
            entity.Property(x => x.Name).HasColumnName("name").HasMaxLength(256);
            entity.Property(x => x.RoleName).HasColumnName("role_name").HasMaxLength(128);
            entity.Property(x => x.InfluenceLevel).HasColumnName("influence_level").HasMaxLength(64);
            entity.Property(x => x.ContactChannel).HasColumnName("contact_channel").HasMaxLength(256);
            entity.Property(x => x.Status).HasColumnName("status").HasMaxLength(32);
            entity.Property(x => x.CreatedAt).HasColumnName("created_at");
            entity.Property(x => x.UpdatedAt).HasColumnName("updated_at");
            entity.HasOne<ProjectEntity>().WithMany().HasForeignKey(x => x.ProjectId).OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(x => new { x.ProjectId, x.Status });
        });

        modelBuilder.Entity<TailoringRecordEntity>(entity =>
        {
            entity.ToTable("tailoring_records");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.ProjectId).HasColumnName("project_id");
            entity.Property(x => x.RequesterUserId).HasColumnName("requester_user_id").HasMaxLength(64);
            entity.Property(x => x.RequestedChange).HasColumnName("requested_change").HasMaxLength(4000);
            entity.Property(x => x.Reason).HasColumnName("reason").HasMaxLength(2000);
            entity.Property(x => x.ImpactSummary).HasColumnName("impact_summary").HasMaxLength(4000);
            entity.Property(x => x.Status).HasColumnName("status").HasMaxLength(32);
            entity.Property(x => x.ApproverUserId).HasColumnName("approver_user_id").HasMaxLength(128);
            entity.Property(x => x.ApprovedAt).HasColumnName("approved_at");
            entity.Property(x => x.ImpactedProcessAssetId).HasColumnName("impacted_process_asset_id");
            entity.Property(x => x.ApprovalRationale).HasColumnName("approval_rationale").HasMaxLength(2000);
            entity.Property(x => x.CreatedAt).HasColumnName("created_at");
            entity.Property(x => x.UpdatedAt).HasColumnName("updated_at");
            entity.HasOne<ProjectEntity>().WithMany().HasForeignKey(x => x.ProjectId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne<ProcessAssetEntity>().WithMany().HasForeignKey(x => x.ImpactedProcessAssetId).OnDelete(DeleteBehavior.SetNull);
            entity.HasIndex(x => new { x.ProjectId, x.Status });
            entity.HasIndex(x => new { x.RequesterUserId, x.Status });
        });

        modelBuilder.Entity<RaciMapEntity>(entity =>
        {
            entity.ToTable("raci_maps");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.ProcessCode).HasColumnName("process_code").HasMaxLength(128);
            entity.Property(x => x.RoleName).HasColumnName("role_name").HasMaxLength(256);
            entity.Property(x => x.ResponsibilityType).HasColumnName("responsibility_type").HasMaxLength(8);
            entity.Property(x => x.Status).HasColumnName("status").HasMaxLength(32);
            entity.Property(x => x.CreatedAt).HasColumnName("created_at");
            entity.Property(x => x.UpdatedAt).HasColumnName("updated_at");
            entity.HasIndex(x => new { x.ProcessCode, x.RoleName, x.ResponsibilityType }).IsUnique();
            entity.HasIndex(x => new { x.ProcessCode, x.Status });
        });

        modelBuilder.Entity<ApprovalEvidenceLogEntity>(entity =>
        {
            entity.ToTable("approval_evidence_logs");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.EntityType).HasColumnName("entity_type").HasMaxLength(64);
            entity.Property(x => x.EntityId).HasColumnName("entity_id").HasMaxLength(64);
            entity.Property(x => x.ApproverUserId).HasColumnName("approver_user_id").HasMaxLength(128);
            entity.Property(x => x.ApprovedAt).HasColumnName("approved_at");
            entity.Property(x => x.Reason).HasColumnName("reason").HasMaxLength(2000);
            entity.Property(x => x.Outcome).HasColumnName("outcome").HasMaxLength(32);
            entity.HasIndex(x => new { x.EntityType, x.Outcome, x.ApprovedAt });
            entity.HasIndex(x => new { x.ApproverUserId, x.ApprovedAt });
        });

        modelBuilder.Entity<WorkflowOverrideLogEntity>(entity =>
        {
            entity.ToTable("workflow_override_logs");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.EntityType).HasColumnName("entity_type").HasMaxLength(64);
            entity.Property(x => x.EntityId).HasColumnName("entity_id").HasMaxLength(64);
            entity.Property(x => x.RequestedBy).HasColumnName("requested_by").HasMaxLength(128);
            entity.Property(x => x.ApprovedBy).HasColumnName("approved_by").HasMaxLength(128);
            entity.Property(x => x.Reason).HasColumnName("reason").HasMaxLength(2000);
            entity.Property(x => x.OccurredAt).HasColumnName("occurred_at");
            entity.HasIndex(x => new { x.EntityType, x.OccurredAt });
            entity.HasIndex(x => new { x.RequestedBy, x.OccurredAt });
        });

        modelBuilder.Entity<SlaRuleEntity>(entity =>
        {
            entity.ToTable("sla_rules");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.ScopeType).HasColumnName("scope_type").HasMaxLength(64);
            entity.Property(x => x.ScopeRef).HasColumnName("scope_ref").HasMaxLength(256);
            entity.Property(x => x.TargetDurationHours).HasColumnName("target_duration_hours");
            entity.Property(x => x.EscalationPolicyId).HasColumnName("escalation_policy_id").HasMaxLength(128);
            entity.Property(x => x.Status).HasColumnName("status").HasMaxLength(32);
            entity.Property(x => x.CreatedAt).HasColumnName("created_at");
            entity.Property(x => x.UpdatedAt).HasColumnName("updated_at");
            entity.HasIndex(x => new { x.ScopeType, x.Status });
        });

        modelBuilder.Entity<RetentionPolicyEntity>(entity =>
        {
            entity.ToTable("retention_policies");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.PolicyCode).HasColumnName("policy_code").HasMaxLength(128);
            entity.Property(x => x.AppliesTo).HasColumnName("applies_to").HasMaxLength(128);
            entity.Property(x => x.RetentionPeriodDays).HasColumnName("retention_period_days");
            entity.Property(x => x.ArchiveRule).HasColumnName("archive_rule").HasMaxLength(512);
            entity.Property(x => x.Status).HasColumnName("status").HasMaxLength(32);
            entity.Property(x => x.CreatedAt).HasColumnName("created_at");
            entity.Property(x => x.UpdatedAt).HasColumnName("updated_at");
            entity.HasIndex(x => x.PolicyCode).IsUnique();
            entity.HasIndex(x => new { x.AppliesTo, x.Status });
        });

        modelBuilder.Entity<ArchitectureRecordEntity>(entity =>
        {
            entity.ToTable("architecture_records");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.ProjectId).HasColumnName("project_id");
            entity.Property(x => x.Title).HasColumnName("title").HasMaxLength(512);
            entity.Property(x => x.ArchitectureType).HasColumnName("architecture_type").HasMaxLength(128);
            entity.Property(x => x.OwnerUserId).HasColumnName("owner_user_id").HasMaxLength(128);
            entity.Property(x => x.Status).HasColumnName("status").HasMaxLength(32);
            entity.Property(x => x.CurrentVersionId).HasColumnName("current_version_id").HasMaxLength(128);
            entity.Property(x => x.Summary).HasColumnName("summary").HasMaxLength(4000);
            entity.Property(x => x.SecurityImpact).HasColumnName("security_impact").HasMaxLength(4000);
            entity.Property(x => x.EvidenceRef).HasColumnName("evidence_ref").HasMaxLength(512);
            entity.Property(x => x.ApprovedBy).HasColumnName("approved_by").HasMaxLength(128);
            entity.Property(x => x.ApprovedAt).HasColumnName("approved_at");
            entity.Property(x => x.CreatedAt).HasColumnName("created_at");
            entity.Property(x => x.UpdatedAt).HasColumnName("updated_at");
            entity.HasOne<ProjectEntity>().WithMany().HasForeignKey(x => x.ProjectId).OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(x => new { x.ProjectId, x.ArchitectureType, x.Status });
            entity.HasIndex(x => x.OwnerUserId);
        });

        modelBuilder.Entity<DesignReviewEntity>(entity =>
        {
            entity.ToTable("design_reviews");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.ArchitectureRecordId).HasColumnName("architecture_record_id");
            entity.Property(x => x.ReviewType).HasColumnName("review_type").HasMaxLength(128);
            entity.Property(x => x.ReviewedBy).HasColumnName("reviewed_by").HasMaxLength(128);
            entity.Property(x => x.Status).HasColumnName("status").HasMaxLength(32);
            entity.Property(x => x.DecisionReason).HasColumnName("decision_reason").HasMaxLength(2000);
            entity.Property(x => x.DesignSummary).HasColumnName("design_summary").HasMaxLength(4000);
            entity.Property(x => x.Concerns).HasColumnName("concerns").HasMaxLength(4000);
            entity.Property(x => x.EvidenceRef).HasColumnName("evidence_ref").HasMaxLength(512);
            entity.Property(x => x.DecidedAt).HasColumnName("decided_at");
            entity.Property(x => x.CreatedAt).HasColumnName("created_at");
            entity.Property(x => x.UpdatedAt).HasColumnName("updated_at");
            entity.HasOne<ArchitectureRecordEntity>().WithMany().HasForeignKey(x => x.ArchitectureRecordId).OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(x => new { x.ArchitectureRecordId, x.Status });
            entity.HasIndex(x => x.ReviewedBy);
        });

        modelBuilder.Entity<IntegrationReviewEntity>(entity =>
        {
            entity.ToTable("integration_reviews");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.ScopeRef).HasColumnName("scope_ref").HasMaxLength(256);
            entity.Property(x => x.IntegrationType).HasColumnName("integration_type").HasMaxLength(128);
            entity.Property(x => x.ReviewedBy).HasColumnName("reviewed_by").HasMaxLength(128);
            entity.Property(x => x.Status).HasColumnName("status").HasMaxLength(32);
            entity.Property(x => x.DecisionReason).HasColumnName("decision_reason").HasMaxLength(2000);
            entity.Property(x => x.Risks).HasColumnName("risks").HasMaxLength(4000);
            entity.Property(x => x.DependencyImpact).HasColumnName("dependency_impact").HasMaxLength(4000);
            entity.Property(x => x.EvidenceRef).HasColumnName("evidence_ref").HasMaxLength(512);
            entity.Property(x => x.DecidedAt).HasColumnName("decided_at");
            entity.Property(x => x.AppliedAt).HasColumnName("applied_at");
            entity.Property(x => x.CreatedAt).HasColumnName("created_at");
            entity.Property(x => x.UpdatedAt).HasColumnName("updated_at");
            entity.HasIndex(x => new { x.ScopeRef, x.IntegrationType, x.Status });
            entity.HasIndex(x => x.ReviewedBy);
        });

        modelBuilder.Entity<RequirementEntity>(entity =>
        {
            entity.ToTable("requirements");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.ProjectId).HasColumnName("project_id");
            entity.Property(x => x.Code).HasColumnName("code").HasMaxLength(128);
            entity.Property(x => x.Title).HasColumnName("title").HasMaxLength(512);
            entity.Property(x => x.Description).HasColumnName("description").HasMaxLength(8000);
            entity.Property(x => x.Priority).HasColumnName("priority").HasMaxLength(32);
            entity.Property(x => x.OwnerUserId).HasColumnName("owner_user_id").HasMaxLength(64);
            entity.Property(x => x.Status).HasColumnName("status").HasMaxLength(32);
            entity.Property(x => x.CurrentVersionId).HasColumnName("current_version_id");
            entity.Property(x => x.CreatedAt).HasColumnName("created_at");
            entity.Property(x => x.UpdatedAt).HasColumnName("updated_at");
            entity.HasOne<ProjectEntity>().WithMany().HasForeignKey(x => x.ProjectId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne<RequirementVersionEntity>().WithMany().HasForeignKey(x => x.CurrentVersionId).OnDelete(DeleteBehavior.SetNull);
            entity.HasIndex(x => new { x.ProjectId, x.Code }).IsUnique();
            entity.HasIndex(x => new { x.ProjectId, x.Status, x.Priority });
            entity.HasIndex(x => x.OwnerUserId);
            entity.HasIndex(x => x.CurrentVersionId);
        });

        modelBuilder.Entity<RequirementVersionEntity>(entity =>
        {
            entity.ToTable("requirement_versions");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.RequirementId).HasColumnName("requirement_id");
            entity.Property(x => x.VersionNumber).HasColumnName("version_number");
            entity.Property(x => x.BusinessReason).HasColumnName("business_reason").HasMaxLength(4000);
            entity.Property(x => x.AcceptanceCriteria).HasColumnName("acceptance_criteria").HasMaxLength(8000);
            entity.Property(x => x.SecurityImpact).HasColumnName("security_impact").HasMaxLength(4000);
            entity.Property(x => x.PerformanceImpact).HasColumnName("performance_impact").HasMaxLength(4000);
            entity.Property(x => x.Status).HasColumnName("status").HasMaxLength(32);
            entity.Property(x => x.CreatedAt).HasColumnName("created_at");
            entity.HasOne<RequirementEntity>().WithMany().HasForeignKey(x => x.RequirementId).OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(x => new { x.RequirementId, x.VersionNumber }).IsUnique();
        });

        modelBuilder.Entity<RequirementBaselineEntity>(entity =>
        {
            entity.ToTable("requirement_baselines");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.ProjectId).HasColumnName("project_id");
            entity.Property(x => x.BaselineName).HasColumnName("baseline_name").HasMaxLength(256);
            entity.Property(x => x.RequirementIdsJson).HasColumnName("requirement_ids_json").HasColumnType("jsonb");
            entity.Property(x => x.Reason).HasColumnName("reason").HasMaxLength(2000);
            entity.Property(x => x.ApprovedBy).HasColumnName("approved_by").HasMaxLength(128);
            entity.Property(x => x.ApprovedAt).HasColumnName("approved_at");
            entity.Property(x => x.Status).HasColumnName("status").HasMaxLength(32);
            entity.HasOne<ProjectEntity>().WithMany().HasForeignKey(x => x.ProjectId).OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(x => new { x.ProjectId, x.Status });
        });

        modelBuilder.Entity<TraceabilityLinkEntity>(entity =>
        {
            entity.ToTable("traceability_links");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.SourceType).HasColumnName("source_type").HasMaxLength(64);
            entity.Property(x => x.SourceId).HasColumnName("source_id").HasMaxLength(128);
            entity.Property(x => x.TargetType).HasColumnName("target_type").HasMaxLength(64);
            entity.Property(x => x.TargetId).HasColumnName("target_id").HasMaxLength(128);
            entity.Property(x => x.LinkRule).HasColumnName("link_rule").HasMaxLength(128);
            entity.Property(x => x.Status).HasColumnName("status").HasMaxLength(32);
            entity.Property(x => x.CreatedBy).HasColumnName("created_by").HasMaxLength(128);
            entity.Property(x => x.CreatedAt).HasColumnName("created_at");
            entity.HasIndex(x => new { x.SourceType, x.SourceId, x.TargetType, x.TargetId, x.LinkRule }).IsUnique();
            entity.HasIndex(x => new { x.Status, x.SourceType, x.TargetType });
            entity.HasIndex(x => x.SourceId);
            entity.HasIndex(x => x.TargetId);
        });

        modelBuilder.Entity<DefectEntity>(entity =>
        {
            entity.ToTable("defects");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.ProjectId).HasColumnName("project_id");
            entity.Property(x => x.Code).HasColumnName("code").HasMaxLength(128);
            entity.Property(x => x.Title).HasColumnName("title").HasMaxLength(512);
            entity.Property(x => x.Description).HasColumnName("description").HasMaxLength(4000);
            entity.Property(x => x.Severity).HasColumnName("severity").HasMaxLength(32);
            entity.Property(x => x.OwnerUserId).HasColumnName("owner_user_id").HasMaxLength(64);
            entity.Property(x => x.Status).HasColumnName("status").HasMaxLength(32);
            entity.Property(x => x.DetectedInPhase).HasColumnName("detected_in_phase").HasMaxLength(64);
            entity.Property(x => x.ResolutionSummary).HasColumnName("resolution_summary").HasMaxLength(4000);
            entity.Property(x => x.CorrectiveActionRef).HasColumnName("corrective_action_ref").HasMaxLength(256);
            entity.Property(x => x.AffectedArtifactRefsJson).HasColumnName("affected_artifact_refs_json").HasColumnType("jsonb");
            entity.Property(x => x.CreatedAt).HasColumnName("created_at");
            entity.Property(x => x.UpdatedAt).HasColumnName("updated_at");
            entity.HasOne<ProjectEntity>().WithMany().HasForeignKey(x => x.ProjectId).OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(x => new { x.ProjectId, x.Code }).IsUnique();
            entity.HasIndex(x => new { x.ProjectId, x.Severity, x.Status });
            entity.HasIndex(x => x.OwnerUserId);
        });

        modelBuilder.Entity<NonConformanceEntity>(entity =>
        {
            entity.ToTable("non_conformances");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.ProjectId).HasColumnName("project_id");
            entity.Property(x => x.Code).HasColumnName("code").HasMaxLength(128);
            entity.Property(x => x.Title).HasColumnName("title").HasMaxLength(512);
            entity.Property(x => x.Description).HasColumnName("description").HasMaxLength(4000);
            entity.Property(x => x.SourceType).HasColumnName("source_type").HasMaxLength(128);
            entity.Property(x => x.OwnerUserId).HasColumnName("owner_user_id").HasMaxLength(64);
            entity.Property(x => x.Status).HasColumnName("status").HasMaxLength(32);
            entity.Property(x => x.CorrectiveActionRef).HasColumnName("corrective_action_ref").HasMaxLength(256);
            entity.Property(x => x.RootCause).HasColumnName("root_cause").HasMaxLength(4000);
            entity.Property(x => x.ResolutionSummary).HasColumnName("resolution_summary").HasMaxLength(4000);
            entity.Property(x => x.AcceptedDisposition).HasColumnName("accepted_disposition").HasMaxLength(2000);
            entity.Property(x => x.LinkedFindingRefsJson).HasColumnName("linked_finding_refs_json").HasColumnType("jsonb");
            entity.Property(x => x.CreatedAt).HasColumnName("created_at");
            entity.Property(x => x.UpdatedAt).HasColumnName("updated_at");
            entity.HasOne<ProjectEntity>().WithMany().HasForeignKey(x => x.ProjectId).OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(x => new { x.ProjectId, x.Code }).IsUnique();
            entity.HasIndex(x => new { x.ProjectId, x.Status });
            entity.HasIndex(x => x.OwnerUserId);
        });

        modelBuilder.Entity<RiskEntity>(entity =>
        {
            entity.ToTable("risks");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.ProjectId).HasColumnName("project_id");
            entity.Property(x => x.Code).HasColumnName("code").HasMaxLength(128);
            entity.Property(x => x.Title).HasColumnName("title").HasMaxLength(512);
            entity.Property(x => x.Description).HasColumnName("description").HasMaxLength(4000);
            entity.Property(x => x.Probability).HasColumnName("probability");
            entity.Property(x => x.Impact).HasColumnName("impact");
            entity.Property(x => x.OwnerUserId).HasColumnName("owner_user_id").HasMaxLength(64);
            entity.Property(x => x.MitigationPlan).HasColumnName("mitigation_plan").HasMaxLength(4000);
            entity.Property(x => x.Cause).HasColumnName("cause").HasMaxLength(2000);
            entity.Property(x => x.Effect).HasColumnName("effect").HasMaxLength(2000);
            entity.Property(x => x.ContingencyPlan).HasColumnName("contingency_plan").HasMaxLength(4000);
            entity.Property(x => x.Status).HasColumnName("status").HasMaxLength(32);
            entity.Property(x => x.NextReviewAt).HasColumnName("next_review_at");
            entity.Property(x => x.CreatedAt).HasColumnName("created_at");
            entity.Property(x => x.UpdatedAt).HasColumnName("updated_at");
            entity.HasOne<ProjectEntity>().WithMany().HasForeignKey(x => x.ProjectId).OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(x => new { x.ProjectId, x.Code }).IsUnique();
            entity.HasIndex(x => new { x.ProjectId, x.Status, x.NextReviewAt });
            entity.HasIndex(x => x.OwnerUserId);
        });

        modelBuilder.Entity<RiskReviewEntity>(entity =>
        {
            entity.ToTable("risk_reviews");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.RiskId).HasColumnName("risk_id");
            entity.Property(x => x.ReviewedBy).HasColumnName("reviewed_by").HasMaxLength(128);
            entity.Property(x => x.ReviewedAt).HasColumnName("reviewed_at");
            entity.Property(x => x.Decision).HasColumnName("decision").HasMaxLength(64);
            entity.Property(x => x.Notes).HasColumnName("notes").HasMaxLength(2000);
            entity.HasOne<RiskEntity>().WithMany().HasForeignKey(x => x.RiskId).OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(x => new { x.RiskId, x.ReviewedAt });
        });

        modelBuilder.Entity<IssueEntity>(entity =>
        {
            entity.ToTable("issues");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.ProjectId).HasColumnName("project_id");
            entity.Property(x => x.Code).HasColumnName("code").HasMaxLength(128);
            entity.Property(x => x.Title).HasColumnName("title").HasMaxLength(512);
            entity.Property(x => x.Description).HasColumnName("description").HasMaxLength(4000);
            entity.Property(x => x.OwnerUserId).HasColumnName("owner_user_id").HasMaxLength(64);
            entity.Property(x => x.DueDate).HasColumnName("due_date");
            entity.Property(x => x.Status).HasColumnName("status").HasMaxLength(32);
            entity.Property(x => x.Severity).HasColumnName("severity").HasMaxLength(32);
            entity.Property(x => x.RootIssue).HasColumnName("root_issue").HasMaxLength(2000);
            entity.Property(x => x.Dependencies).HasColumnName("dependencies").HasMaxLength(4000);
            entity.Property(x => x.ResolutionSummary).HasColumnName("resolution_summary").HasMaxLength(4000);
            entity.Property(x => x.IsSensitive).HasColumnName("is_sensitive");
            entity.Property(x => x.SensitiveContext).HasColumnName("sensitive_context").HasMaxLength(256);
            entity.Property(x => x.CreatedAt).HasColumnName("created_at");
            entity.Property(x => x.UpdatedAt).HasColumnName("updated_at");
            entity.HasOne<ProjectEntity>().WithMany().HasForeignKey(x => x.ProjectId).OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(x => new { x.ProjectId, x.Code }).IsUnique();
            entity.HasIndex(x => new { x.ProjectId, x.Status, x.Severity });
            entity.HasIndex(x => x.OwnerUserId);
            entity.HasIndex(x => x.DueDate);
            entity.HasIndex(x => x.IsSensitive);
        });

        modelBuilder.Entity<IssueActionEntity>(entity =>
        {
            entity.ToTable("issue_actions");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.IssueId).HasColumnName("issue_id");
            entity.Property(x => x.ActionDescription).HasColumnName("action_description").HasMaxLength(4000);
            entity.Property(x => x.AssignedTo).HasColumnName("assigned_to").HasMaxLength(128);
            entity.Property(x => x.DueDate).HasColumnName("due_date");
            entity.Property(x => x.Status).HasColumnName("status").HasMaxLength(32);
            entity.Property(x => x.VerificationNote).HasColumnName("verification_note").HasMaxLength(2000);
            entity.Property(x => x.CreatedAt).HasColumnName("created_at");
            entity.Property(x => x.UpdatedAt).HasColumnName("updated_at");
            entity.HasOne<IssueEntity>().WithMany().HasForeignKey(x => x.IssueId).OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(x => new { x.IssueId, x.Status });
            entity.HasIndex(x => new { x.AssignedTo, x.Status });
        });

        modelBuilder.Entity<MeetingRecordEntity>(entity =>
        {
            entity.ToTable("meeting_records");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.ProjectId).HasColumnName("project_id");
            entity.Property(x => x.MeetingType).HasColumnName("meeting_type").HasMaxLength(128);
            entity.Property(x => x.Title).HasColumnName("title").HasMaxLength(512);
            entity.Property(x => x.MeetingAt).HasColumnName("meeting_at");
            entity.Property(x => x.FacilitatorUserId).HasColumnName("facilitator_user_id").HasMaxLength(128);
            entity.Property(x => x.Status).HasColumnName("status").HasMaxLength(32);
            entity.Property(x => x.Agenda).HasColumnName("agenda").HasMaxLength(4000);
            entity.Property(x => x.DiscussionSummary).HasColumnName("discussion_summary").HasMaxLength(4000);
            entity.Property(x => x.IsRestricted).HasColumnName("is_restricted");
            entity.Property(x => x.Classification).HasColumnName("classification").HasMaxLength(64);
            entity.Property(x => x.CreatedAt).HasColumnName("created_at");
            entity.Property(x => x.UpdatedAt).HasColumnName("updated_at");
            entity.HasOne<ProjectEntity>().WithMany().HasForeignKey(x => x.ProjectId).OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(x => new { x.ProjectId, x.MeetingType, x.MeetingAt });
            entity.HasIndex(x => new { x.ProjectId, x.Status });
            entity.HasIndex(x => x.IsRestricted);
        });

        modelBuilder.Entity<MeetingMinutesEntity>(entity =>
        {
            entity.ToTable("meeting_minutes");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.MeetingRecordId).HasColumnName("meeting_record_id");
            entity.Property(x => x.Summary).HasColumnName("summary").HasMaxLength(4000);
            entity.Property(x => x.DecisionsSummary).HasColumnName("decisions_summary").HasMaxLength(4000);
            entity.Property(x => x.ActionsSummary).HasColumnName("actions_summary").HasMaxLength(4000);
            entity.Property(x => x.Status).HasColumnName("status").HasMaxLength(32);
            entity.Property(x => x.UpdatedAt).HasColumnName("updated_at");
            entity.HasOne<MeetingRecordEntity>().WithMany().HasForeignKey(x => x.MeetingRecordId).OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(x => x.MeetingRecordId).IsUnique();
        });

        modelBuilder.Entity<MeetingAttendeeEntity>(entity =>
        {
            entity.ToTable("meeting_attendees");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.MeetingRecordId).HasColumnName("meeting_record_id");
            entity.Property(x => x.UserId).HasColumnName("user_id").HasMaxLength(128);
            entity.Property(x => x.AttendanceStatus).HasColumnName("attendance_status").HasMaxLength(32);
            entity.HasOne<MeetingRecordEntity>().WithMany().HasForeignKey(x => x.MeetingRecordId).OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(x => new { x.MeetingRecordId, x.UserId }).IsUnique();
        });

        modelBuilder.Entity<DecisionEntity>(entity =>
        {
            entity.ToTable("decisions");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.ProjectId).HasColumnName("project_id");
            entity.Property(x => x.MeetingId).HasColumnName("meeting_id");
            entity.Property(x => x.Code).HasColumnName("code").HasMaxLength(128);
            entity.Property(x => x.Title).HasColumnName("title").HasMaxLength(512);
            entity.Property(x => x.DecisionType).HasColumnName("decision_type").HasMaxLength(128);
            entity.Property(x => x.Rationale).HasColumnName("rationale").HasMaxLength(4000);
            entity.Property(x => x.AlternativesConsidered).HasColumnName("alternatives_considered").HasMaxLength(4000);
            entity.Property(x => x.ImpactedArtifactsJson).HasColumnName("impacted_artifacts_json").HasColumnType("jsonb");
            entity.Property(x => x.ApprovedBy).HasColumnName("approved_by").HasMaxLength(128);
            entity.Property(x => x.ApprovedAt).HasColumnName("approved_at");
            entity.Property(x => x.Status).HasColumnName("status").HasMaxLength(32);
            entity.Property(x => x.IsRestricted).HasColumnName("is_restricted");
            entity.Property(x => x.Classification).HasColumnName("classification").HasMaxLength(64);
            entity.Property(x => x.CreatedAt).HasColumnName("created_at");
            entity.Property(x => x.UpdatedAt).HasColumnName("updated_at");
            entity.HasOne<ProjectEntity>().WithMany().HasForeignKey(x => x.ProjectId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne<MeetingRecordEntity>().WithMany().HasForeignKey(x => x.MeetingId).OnDelete(DeleteBehavior.SetNull);
            entity.HasIndex(x => new { x.ProjectId, x.Code }).IsUnique();
            entity.HasIndex(x => new { x.ProjectId, x.Status, x.DecisionType });
            entity.HasIndex(x => x.MeetingId);
            entity.HasIndex(x => x.IsRestricted);
        });

        modelBuilder.Entity<TestPlanEntity>(entity =>
        {
            entity.ToTable("test_plans");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.ProjectId).HasColumnName("project_id");
            entity.Property(x => x.Code).HasColumnName("code").HasMaxLength(128);
            entity.Property(x => x.Title).HasColumnName("title").HasMaxLength(512);
            entity.Property(x => x.ScopeSummary).HasColumnName("scope_summary").HasMaxLength(4000);
            entity.Property(x => x.OwnerUserId).HasColumnName("owner_user_id").HasMaxLength(128);
            entity.Property(x => x.Status).HasColumnName("status").HasMaxLength(32);
            entity.Property(x => x.EntryCriteria).HasColumnName("entry_criteria").HasMaxLength(4000);
            entity.Property(x => x.ExitCriteria).HasColumnName("exit_criteria").HasMaxLength(4000);
            entity.Property(x => x.LinkedRequirementIdsJson).HasColumnName("linked_requirement_ids_json").HasColumnType("jsonb");
            entity.Property(x => x.ApprovalReason).HasColumnName("approval_reason").HasMaxLength(2000);
            entity.Property(x => x.ApprovedBy).HasColumnName("approved_by").HasMaxLength(128);
            entity.Property(x => x.ApprovedAt).HasColumnName("approved_at");
            entity.Property(x => x.BaselinedAt).HasColumnName("baselined_at");
            entity.Property(x => x.CreatedAt).HasColumnName("created_at");
            entity.Property(x => x.UpdatedAt).HasColumnName("updated_at");
            entity.HasOne<ProjectEntity>().WithMany().HasForeignKey(x => x.ProjectId).OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(x => new { x.ProjectId, x.Code }).IsUnique();
            entity.HasIndex(x => new { x.ProjectId, x.Status });
            entity.HasIndex(x => x.OwnerUserId);
        });

        modelBuilder.Entity<TestCaseEntity>(entity =>
        {
            entity.ToTable("test_cases");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.TestPlanId).HasColumnName("test_plan_id");
            entity.Property(x => x.Code).HasColumnName("code").HasMaxLength(128);
            entity.Property(x => x.Title).HasColumnName("title").HasMaxLength(512);
            entity.Property(x => x.Preconditions).HasColumnName("preconditions").HasMaxLength(4000);
            entity.Property(x => x.StepsJson).HasColumnName("steps_json").HasColumnType("jsonb");
            entity.Property(x => x.ExpectedResult).HasColumnName("expected_result").HasMaxLength(4000);
            entity.Property(x => x.RequirementId).HasColumnName("requirement_id");
            entity.Property(x => x.Status).HasColumnName("status").HasMaxLength(32);
            entity.Property(x => x.CreatedAt).HasColumnName("created_at");
            entity.Property(x => x.UpdatedAt).HasColumnName("updated_at");
            entity.HasOne<TestPlanEntity>().WithMany().HasForeignKey(x => x.TestPlanId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne<RequirementEntity>().WithMany().HasForeignKey(x => x.RequirementId).OnDelete(DeleteBehavior.SetNull);
            entity.HasIndex(x => new { x.TestPlanId, x.Code }).IsUnique();
            entity.HasIndex(x => new { x.TestPlanId, x.Status });
            entity.HasIndex(x => x.RequirementId);
        });

        modelBuilder.Entity<TestExecutionEntity>(entity =>
        {
            entity.ToTable("test_executions");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.TestCaseId).HasColumnName("test_case_id");
            entity.Property(x => x.ExecutedBy).HasColumnName("executed_by").HasMaxLength(128);
            entity.Property(x => x.ExecutedAt).HasColumnName("executed_at");
            entity.Property(x => x.Result).HasColumnName("result").HasMaxLength(32);
            entity.Property(x => x.EvidenceRef).HasColumnName("evidence_ref").HasMaxLength(512);
            entity.Property(x => x.Notes).HasColumnName("notes").HasMaxLength(4000);
            entity.Property(x => x.IsSensitiveEvidence).HasColumnName("is_sensitive_evidence");
            entity.Property(x => x.EvidenceClassification).HasColumnName("evidence_classification").HasMaxLength(64);
            entity.HasOne<TestCaseEntity>().WithMany().HasForeignKey(x => x.TestCaseId).OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(x => new { x.TestCaseId, x.ExecutedAt });
            entity.HasIndex(x => new { x.Result, x.ExecutedAt });
            entity.HasIndex(x => x.ExecutedBy);
            entity.HasIndex(x => x.IsSensitiveEvidence);
        });

        modelBuilder.Entity<UatSignoffEntity>(entity =>
        {
            entity.ToTable("uat_signoffs");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.ProjectId).HasColumnName("project_id");
            entity.Property(x => x.ReleaseId).HasColumnName("release_id").HasMaxLength(128);
            entity.Property(x => x.ScopeSummary).HasColumnName("scope_summary").HasMaxLength(4000);
            entity.Property(x => x.SubmittedBy).HasColumnName("submitted_by").HasMaxLength(128);
            entity.Property(x => x.SubmittedAt).HasColumnName("submitted_at");
            entity.Property(x => x.ApprovedBy).HasColumnName("approved_by").HasMaxLength(128);
            entity.Property(x => x.ApprovedAt).HasColumnName("approved_at");
            entity.Property(x => x.Status).HasColumnName("status").HasMaxLength(32);
            entity.Property(x => x.DecisionReason).HasColumnName("decision_reason").HasMaxLength(2000);
            entity.Property(x => x.EvidenceRefsJson).HasColumnName("evidence_refs_json").HasColumnType("jsonb");
            entity.Property(x => x.CreatedAt).HasColumnName("created_at");
            entity.Property(x => x.UpdatedAt).HasColumnName("updated_at");
            entity.HasOne<ProjectEntity>().WithMany().HasForeignKey(x => x.ProjectId).OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(x => new { x.ProjectId, x.Status });
            entity.HasIndex(x => x.ReleaseId);
            entity.HasIndex(x => x.SubmittedBy);
        });

        modelBuilder.Entity<ChangeRequestEntity>(entity =>
        {
            entity.ToTable("change_requests");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.ProjectId).HasColumnName("project_id");
            entity.Property(x => x.Code).HasColumnName("code").HasMaxLength(128);
            entity.Property(x => x.Title).HasColumnName("title").HasMaxLength(512);
            entity.Property(x => x.RequestedBy).HasColumnName("requested_by").HasMaxLength(128);
            entity.Property(x => x.Reason).HasColumnName("reason").HasMaxLength(4000);
            entity.Property(x => x.Status).HasColumnName("status").HasMaxLength(32);
            entity.Property(x => x.Priority).HasColumnName("priority").HasMaxLength(32);
            entity.Property(x => x.TargetBaselineId).HasColumnName("target_baseline_id");
            entity.Property(x => x.LinkedRequirementIdsJson).HasColumnName("linked_requirement_ids_json").HasColumnType("jsonb");
            entity.Property(x => x.LinkedConfigurationItemIdsJson).HasColumnName("linked_configuration_item_ids_json").HasColumnType("jsonb");
            entity.Property(x => x.DecisionRationale).HasColumnName("decision_rationale").HasMaxLength(2000);
            entity.Property(x => x.ImplementationSummary).HasColumnName("implementation_summary").HasMaxLength(2000);
            entity.Property(x => x.ApprovedBy).HasColumnName("approved_by").HasMaxLength(128);
            entity.Property(x => x.ApprovedAt).HasColumnName("approved_at");
            entity.Property(x => x.CreatedAt).HasColumnName("created_at");
            entity.Property(x => x.UpdatedAt).HasColumnName("updated_at");
            entity.HasOne<ProjectEntity>().WithMany().HasForeignKey(x => x.ProjectId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne<BaselineRegistryEntity>().WithMany().HasForeignKey(x => x.TargetBaselineId).OnDelete(DeleteBehavior.SetNull);
            entity.HasIndex(x => new { x.ProjectId, x.Code }).IsUnique();
            entity.HasIndex(x => new { x.ProjectId, x.Status, x.Priority });
            entity.HasIndex(x => x.TargetBaselineId);
        });

        modelBuilder.Entity<ChangeImpactEntity>(entity =>
        {
            entity.ToTable("change_impacts");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.ChangeRequestId).HasColumnName("change_request_id");
            entity.Property(x => x.ScopeImpact).HasColumnName("scope_impact").HasMaxLength(4000);
            entity.Property(x => x.ScheduleImpact).HasColumnName("schedule_impact").HasMaxLength(4000);
            entity.Property(x => x.QualityImpact).HasColumnName("quality_impact").HasMaxLength(4000);
            entity.Property(x => x.SecurityImpact).HasColumnName("security_impact").HasMaxLength(4000);
            entity.Property(x => x.PerformanceImpact).HasColumnName("performance_impact").HasMaxLength(4000);
            entity.Property(x => x.RiskImpact).HasColumnName("risk_impact").HasMaxLength(4000);
            entity.HasOne<ChangeRequestEntity>().WithMany().HasForeignKey(x => x.ChangeRequestId).OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(x => x.ChangeRequestId).IsUnique();
        });

        modelBuilder.Entity<ConfigurationItemEntity>(entity =>
        {
            entity.ToTable("configuration_items");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.ProjectId).HasColumnName("project_id");
            entity.Property(x => x.Code).HasColumnName("code").HasMaxLength(128);
            entity.Property(x => x.Name).HasColumnName("name").HasMaxLength(256);
            entity.Property(x => x.ItemType).HasColumnName("item_type").HasMaxLength(128);
            entity.Property(x => x.OwnerModule).HasColumnName("owner_module").HasMaxLength(128);
            entity.Property(x => x.Status).HasColumnName("status").HasMaxLength(32);
            entity.Property(x => x.BaselineRef).HasColumnName("baseline_ref").HasMaxLength(256);
            entity.Property(x => x.CreatedAt).HasColumnName("created_at");
            entity.Property(x => x.UpdatedAt).HasColumnName("updated_at");
            entity.HasOne<ProjectEntity>().WithMany().HasForeignKey(x => x.ProjectId).OnDelete(DeleteBehavior.Cascade);
            entity.HasIndex(x => new { x.ProjectId, x.Code }).IsUnique();
            entity.HasIndex(x => new { x.ProjectId, x.ItemType, x.Status });
        });

        modelBuilder.Entity<BaselineRegistryEntity>(entity =>
        {
            entity.ToTable("baseline_registry");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Id).HasColumnName("id");
            entity.Property(x => x.ProjectId).HasColumnName("project_id");
            entity.Property(x => x.BaselineName).HasColumnName("baseline_name").HasMaxLength(256);
            entity.Property(x => x.BaselineType).HasColumnName("baseline_type").HasMaxLength(64);
            entity.Property(x => x.SourceEntityType).HasColumnName("source_entity_type").HasMaxLength(64);
            entity.Property(x => x.SourceEntityId).HasColumnName("source_entity_id").HasMaxLength(128);
            entity.Property(x => x.Status).HasColumnName("status").HasMaxLength(32);
            entity.Property(x => x.ApprovedBy).HasColumnName("approved_by").HasMaxLength(128);
            entity.Property(x => x.ApprovedAt).HasColumnName("approved_at");
            entity.Property(x => x.ChangeRequestId).HasColumnName("change_request_id");
            entity.Property(x => x.SupersededByBaselineId).HasColumnName("superseded_by_baseline_id");
            entity.Property(x => x.OverrideReason).HasColumnName("override_reason").HasMaxLength(2000);
            entity.Property(x => x.CreatedAt).HasColumnName("created_at");
            entity.Property(x => x.UpdatedAt).HasColumnName("updated_at");
            entity.HasOne<ProjectEntity>().WithMany().HasForeignKey(x => x.ProjectId).OnDelete(DeleteBehavior.Cascade);
            entity.HasOne<ChangeRequestEntity>().WithMany().HasForeignKey(x => x.ChangeRequestId).OnDelete(DeleteBehavior.SetNull);
            entity.HasOne<BaselineRegistryEntity>().WithMany().HasForeignKey(x => x.SupersededByBaselineId).OnDelete(DeleteBehavior.SetNull);
            entity.HasIndex(x => new { x.ProjectId, x.BaselineType, x.Status });
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
