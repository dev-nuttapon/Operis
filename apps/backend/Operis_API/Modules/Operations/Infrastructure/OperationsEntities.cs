namespace Operis_API.Modules.Operations.Infrastructure;

public sealed class AccessReviewEntity
{
    public Guid Id { get; init; }
    public string ScopeType { get; set; } = string.Empty;
    public string ScopeRef { get; set; } = string.Empty;
    public string ReviewCycle { get; set; } = string.Empty;
    public string? ReviewedBy { get; set; }
    public string Status { get; set; } = "Scheduled";
    public string? Decision { get; set; }
    public string? DecisionRationale { get; set; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset? UpdatedAt { get; set; }
}

public sealed class SecurityReviewEntity
{
    public Guid Id { get; init; }
    public string ScopeType { get; set; } = string.Empty;
    public string ScopeRef { get; set; } = string.Empty;
    public string ControlsReviewed { get; set; } = string.Empty;
    public string? FindingsSummary { get; set; }
    public string Status { get; set; } = "Planned";
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset? UpdatedAt { get; set; }
}

public sealed class ExternalDependencyEntity
{
    public Guid Id { get; init; }
    public string Name { get; set; } = string.Empty;
    public string DependencyType { get; set; } = string.Empty;
    public Guid? SupplierId { get; set; }
    public string OwnerUserId { get; set; } = string.Empty;
    public string Criticality { get; set; } = string.Empty;
    public string Status { get; set; } = "Active";
    public DateTimeOffset? ReviewDueAt { get; set; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset? UpdatedAt { get; set; }
}

public sealed class SupplierEntity
{
    public Guid Id { get; init; }
    public string Name { get; set; } = string.Empty;
    public string SupplierType { get; set; } = string.Empty;
    public string OwnerUserId { get; set; } = string.Empty;
    public string Status { get; set; } = "Active";
    public string Criticality { get; set; } = string.Empty;
    public DateTimeOffset? ReviewDueAt { get; set; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset? UpdatedAt { get; set; }
}

public sealed class SupplierAgreementEntity
{
    public Guid Id { get; init; }
    public Guid SupplierId { get; set; }
    public string AgreementType { get; set; } = string.Empty;
    public DateOnly EffectiveFrom { get; set; }
    public DateOnly? EffectiveTo { get; set; }
    public string SlaTerms { get; set; } = string.Empty;
    public string EvidenceRef { get; set; } = string.Empty;
    public string Status { get; set; } = "Draft";
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset? UpdatedAt { get; set; }
}

public sealed class ConfigurationAuditEntity
{
    public Guid Id { get; init; }
    public string ScopeRef { get; set; } = string.Empty;
    public DateTimeOffset PlannedAt { get; set; }
    public string Status { get; set; } = "Planned";
    public int FindingCount { get; set; }
    public DateTimeOffset CreatedAt { get; init; }
    public DateTimeOffset? UpdatedAt { get; set; }
}
