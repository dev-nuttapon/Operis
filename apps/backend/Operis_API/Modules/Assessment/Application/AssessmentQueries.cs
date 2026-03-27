using System.Text.Json;
using Microsoft.EntityFrameworkCore;
using Operis_API.Infrastructure.Persistence;
using Operis_API.Modules.Assessment.Contracts;
using Operis_API.Modules.Assessment.Infrastructure;
using Operis_API.Modules.Users.Infrastructure;
using Operis_API.Shared.Contracts;

namespace Operis_API.Modules.Assessment.Application;

public sealed class AssessmentQueries(OperisDbContext dbContext) : IAssessmentQueries
{
    private static readonly JsonSerializerOptions SerializerOptions = new(JsonSerializerDefaults.Web);

    public async Task<PagedResult<AssessmentPackageListItemResponse>> ListPackagesAsync(AssessmentPackageListQuery query, CancellationToken cancellationToken)
    {
        var source =
            from package in dbContext.AssessmentPackages.AsNoTracking()
            join project in dbContext.Projects.AsNoTracking() on package.ProjectId equals project.Id into projectJoin
            from project in projectJoin.DefaultIfEmpty()
            select new { Package = package, Project = project };

        if (query.ProjectId.HasValue)
        {
            source = source.Where(x => x.Package.ProjectId == query.ProjectId.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.ProcessArea))
        {
            var processArea = query.ProcessArea.Trim().ToLowerInvariant();
            source = source.Where(x => x.Package.ProcessArea == processArea);
        }

        if (!string.IsNullOrWhiteSpace(query.Status))
        {
            var status = query.Status.Trim().ToLowerInvariant();
            source = source.Where(x => x.Package.Status == status);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var pattern = $"%{query.Search.Trim()}%";
            source = source.Where(x =>
                EF.Functions.ILike(x.Package.PackageCode, pattern) ||
                EF.Functions.ILike(x.Package.ScopeSummary, pattern) ||
                (x.Project != null && EF.Functions.ILike(x.Project.Name, pattern)));
        }

        var packageIds = await source.Select(x => x.Package.Id).ToListAsync(cancellationToken);
        var openFindingCounts = await dbContext.AssessmentFindings.AsNoTracking()
            .Where(x => packageIds.Contains(x.PackageId) && x.Status != "closed")
            .GroupBy(x => x.PackageId)
            .Select(group => new { group.Key, Count = group.Count() })
            .ToDictionaryAsync(x => x.Key, x => x.Count, cancellationToken);

        return await PageAsync(
            source.OrderByDescending(x => x.Package.UpdatedAt)
                .Select(x => new AssessmentPackageListItemResponse(
                    x.Package.Id,
                    x.Package.PackageCode,
                    x.Package.ProjectId,
                    x.Project == null ? null : x.Project.Name,
                    x.Project == null ? null : x.Project.Code,
                    x.Package.ProcessArea,
                    x.Package.ScopeSummary,
                    x.Package.Status,
                    DeserializeEvidenceRefs(x.Package.EvidenceReferencesJson).Count,
                    openFindingCounts.GetValueOrDefault(x.Package.Id),
                    x.Package.UpdatedAt)),
            query.Page,
            query.PageSize,
            cancellationToken);
    }

    public async Task<AssessmentPackageDetailResponse?> GetPackageAsync(Guid packageId, CancellationToken cancellationToken)
    {
        var package = await (
            from entity in dbContext.AssessmentPackages.AsNoTracking()
            where entity.Id == packageId
            join project in dbContext.Projects.AsNoTracking() on entity.ProjectId equals project.Id into projectJoin
            from project in projectJoin.DefaultIfEmpty()
            select new { Package = entity, Project = project })
            .SingleOrDefaultAsync(cancellationToken);

        if (package is null)
        {
            return null;
        }

        var notes = await dbContext.AssessmentNotes.AsNoTracking()
            .Where(x => x.PackageId == packageId)
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => new AssessmentPackageNoteResponse(x.Id, x.NoteType, x.Note, x.CreatedByUserId, x.CreatedAt))
            .ToListAsync(cancellationToken);

        var findings = await dbContext.AssessmentFindings.AsNoTracking()
            .Where(x => x.PackageId == packageId)
            .OrderByDescending(x => x.UpdatedAt)
            .Select(x => new AssessmentFindingListItemResponse(
                x.Id,
                x.PackageId,
                package.Package.PackageCode,
                x.Title,
                x.Severity,
                x.Status,
                x.EvidenceEntityType,
                x.EvidenceEntityId,
                x.OwnerUserId,
                x.UpdatedAt))
            .ToListAsync(cancellationToken);

        return new AssessmentPackageDetailResponse(
            package.Package.Id,
            package.Package.PackageCode,
            package.Package.ProjectId,
            package.Project?.Name,
            package.Project?.Code,
            package.Package.ProcessArea,
            package.Package.ScopeSummary,
            package.Package.Status,
            package.Package.CreatedByUserId,
            package.Package.PreparedAt,
            package.Package.PreparedByUserId,
            package.Package.SharedAt,
            package.Package.SharedByUserId,
            package.Package.ArchivedAt,
            package.Package.ArchivedByUserId,
            DeserializeEvidenceRefs(package.Package.EvidenceReferencesJson),
            notes,
            findings,
            package.Package.CreatedAt,
            package.Package.UpdatedAt);
    }

    public async Task<PagedResult<AssessmentFindingListItemResponse>> ListFindingsAsync(AssessmentFindingListQuery query, CancellationToken cancellationToken)
    {
        var source =
            from finding in dbContext.AssessmentFindings.AsNoTracking()
            join package in dbContext.AssessmentPackages.AsNoTracking() on finding.PackageId equals package.Id
            select new { Finding = finding, Package = package };

        if (query.PackageId.HasValue)
        {
            source = source.Where(x => x.Finding.PackageId == query.PackageId.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.Status))
        {
            var status = query.Status.Trim().ToLowerInvariant();
            source = source.Where(x => x.Finding.Status == status);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var pattern = $"%{query.Search.Trim()}%";
            source = source.Where(x =>
                EF.Functions.ILike(x.Finding.Title, pattern) ||
                EF.Functions.ILike(x.Finding.Description, pattern) ||
                EF.Functions.ILike(x.Package.PackageCode, pattern));
        }

        return await PageAsync(
            source.OrderByDescending(x => x.Finding.UpdatedAt)
                .Select(x => new AssessmentFindingListItemResponse(
                    x.Finding.Id,
                    x.Finding.PackageId,
                    x.Package.PackageCode,
                    x.Finding.Title,
                    x.Finding.Severity,
                    x.Finding.Status,
                    x.Finding.EvidenceEntityType,
                    x.Finding.EvidenceEntityId,
                    x.Finding.OwnerUserId,
                    x.Finding.UpdatedAt)),
            query.Page,
            query.PageSize,
            cancellationToken);
    }

    public async Task<AssessmentFindingDetailResponse?> GetFindingAsync(Guid findingId, CancellationToken cancellationToken)
    {
        return await (
            from finding in dbContext.AssessmentFindings.AsNoTracking()
            where finding.Id == findingId
            join package in dbContext.AssessmentPackages.AsNoTracking() on finding.PackageId equals package.Id
            select new AssessmentFindingDetailResponse(
                finding.Id,
                finding.PackageId,
                package.PackageCode,
                finding.Title,
                finding.Description,
                finding.Severity,
                finding.Status,
                finding.EvidenceEntityType,
                finding.EvidenceEntityId,
                finding.EvidenceRoute,
                finding.OwnerUserId,
                finding.AcceptanceSummary,
                finding.ClosureSummary,
                finding.CreatedByUserId,
                finding.AcceptedAt,
                finding.AcceptedByUserId,
                finding.ClosedAt,
                finding.ClosedByUserId,
                finding.CreatedAt,
                finding.UpdatedAt))
            .SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<PagedResult<ControlCatalogItemResponse>> ListControlCatalogAsync(ControlCatalogListQuery query, CancellationToken cancellationToken)
    {
        var source =
            from control in dbContext.ControlCatalog.AsNoTracking()
            join project in dbContext.Projects.AsNoTracking() on control.ProjectId equals project.Id into projectJoin
            from project in projectJoin.DefaultIfEmpty()
            select new { Control = control, Project = project };

        if (query.ProjectId.HasValue)
        {
            source = source.Where(x => x.Control.ProjectId == query.ProjectId.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.ControlSet))
        {
            var controlSet = query.ControlSet.Trim().ToLowerInvariant();
            source = source.Where(x => x.Control.ControlSet == controlSet);
        }

        if (!string.IsNullOrWhiteSpace(query.ProcessArea))
        {
            var processArea = query.ProcessArea.Trim().ToLowerInvariant();
            source = source.Where(x => x.Control.ProcessArea == processArea);
        }

        if (!string.IsNullOrWhiteSpace(query.Status))
        {
            var status = query.Status.Trim().ToLowerInvariant();
            source = source.Where(x => x.Control.Status == status);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var pattern = $"%{query.Search.Trim()}%";
            source = source.Where(x =>
                EF.Functions.ILike(x.Control.ControlCode, pattern) ||
                EF.Functions.ILike(x.Control.Title, pattern) ||
                (x.Control.Description != null && EF.Functions.ILike(x.Control.Description, pattern)));
        }

        var controlIds = await source.Select(x => x.Control.Id).ToListAsync(cancellationToken);
        var mappingCounts = await dbContext.ControlMappings.AsNoTracking()
            .Where(x => controlIds.Contains(x.ControlId) && x.Status == "active")
            .GroupBy(x => x.ControlId)
            .Select(group => new { group.Key, Count = group.Count() })
            .ToDictionaryAsync(x => x.Key, x => x.Count, cancellationToken);

        return await PageAsync(
            source.OrderBy(x => x.Control.ControlCode)
                .Select(x => new ControlCatalogItemResponse(
                    x.Control.Id,
                    x.Control.ControlCode,
                    x.Control.Title,
                    x.Control.ControlSet,
                    x.Control.ProcessArea,
                    x.Control.Status,
                    x.Control.Description,
                    x.Control.ProjectId,
                    x.Project == null ? null : x.Project.Name,
                    mappingCounts.GetValueOrDefault(x.Control.Id),
                    x.Control.UpdatedAt)),
            query.Page,
            query.PageSize,
            cancellationToken);
    }

    public async Task<ControlCatalogItemResponse?> GetControlCatalogItemAsync(Guid controlId, CancellationToken cancellationToken)
    {
        var item = await (
            from control in dbContext.ControlCatalog.AsNoTracking()
            where control.Id == controlId
            join project in dbContext.Projects.AsNoTracking() on control.ProjectId equals project.Id into projectJoin
            from project in projectJoin.DefaultIfEmpty()
            select new
            {
                Control = control,
                ProjectName = project == null ? null : project.Name,
                ActiveMappingCount = dbContext.ControlMappings.Count(mapping => mapping.ControlId == control.Id && mapping.Status == "active")
            })
            .SingleOrDefaultAsync(cancellationToken);

        return item is null
            ? null
            : new ControlCatalogItemResponse(
                item.Control.Id,
                item.Control.ControlCode,
                item.Control.Title,
                item.Control.ControlSet,
                item.Control.ProcessArea,
                item.Control.Status,
                item.Control.Description,
                item.Control.ProjectId,
                item.ProjectName,
                item.ActiveMappingCount,
                item.Control.UpdatedAt);
    }

    public async Task<PagedResult<ControlMappingDetailResponse>> ListControlMappingsAsync(ControlMappingListQuery query, CancellationToken cancellationToken)
    {
        var source =
            from mapping in dbContext.ControlMappings.AsNoTracking()
            join control in dbContext.ControlCatalog.AsNoTracking() on mapping.ControlId equals control.Id
            join project in dbContext.Projects.AsNoTracking() on mapping.ProjectId equals project.Id into projectJoin
            from project in projectJoin.DefaultIfEmpty()
            select new { Mapping = mapping, Control = control, Project = project };

        if (query.ControlId.HasValue)
        {
            source = source.Where(x => x.Mapping.ControlId == query.ControlId.Value);
        }

        if (query.ProjectId.HasValue)
        {
            source = source.Where(x => x.Mapping.ProjectId == query.ProjectId.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.Status))
        {
            var status = query.Status.Trim().ToLowerInvariant();
            source = source.Where(x => x.Mapping.Status == status);
        }

        if (!string.IsNullOrWhiteSpace(query.TargetModule))
        {
            var targetModule = query.TargetModule.Trim().ToLowerInvariant();
            source = source.Where(x => x.Mapping.TargetModule == targetModule);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var pattern = $"%{query.Search.Trim()}%";
            source = source.Where(x =>
                EF.Functions.ILike(x.Control.ControlCode, pattern) ||
                EF.Functions.ILike(x.Control.Title, pattern) ||
                EF.Functions.ILike(x.Mapping.TargetEntityType, pattern) ||
                EF.Functions.ILike(x.Mapping.TargetEntityId, pattern));
        }

        return await PageAsync(
            source.OrderByDescending(x => x.Mapping.UpdatedAt)
                .Select(x => new ControlMappingDetailResponse(
                    x.Mapping.Id,
                    x.Mapping.ControlId,
                    x.Control.ControlCode,
                    x.Control.Title,
                    x.Mapping.ProjectId,
                    x.Project == null ? null : x.Project.Name,
                    x.Mapping.TargetModule,
                    x.Mapping.TargetEntityType,
                    x.Mapping.TargetEntityId,
                    x.Mapping.TargetRoute,
                    x.Mapping.EvidenceStatus,
                    x.Mapping.Status,
                    x.Mapping.Notes,
                    x.Mapping.CreatedAt,
                    x.Mapping.UpdatedAt)),
            query.Page,
            query.PageSize,
            cancellationToken);
    }

    public async Task<ControlMappingDetailResponse?> GetControlMappingAsync(Guid mappingId, CancellationToken cancellationToken)
    {
        return await (
            from mapping in dbContext.ControlMappings.AsNoTracking()
            where mapping.Id == mappingId
            join control in dbContext.ControlCatalog.AsNoTracking() on mapping.ControlId equals control.Id
            join project in dbContext.Projects.AsNoTracking() on mapping.ProjectId equals project.Id into projectJoin
            from project in projectJoin.DefaultIfEmpty()
            select new ControlMappingDetailResponse(
                mapping.Id,
                mapping.ControlId,
                control.ControlCode,
                control.Title,
                mapping.ProjectId,
                project == null ? null : project.Name,
                mapping.TargetModule,
                mapping.TargetEntityType,
                mapping.TargetEntityId,
                mapping.TargetRoute,
                mapping.EvidenceStatus,
                mapping.Status,
                mapping.Notes,
                mapping.CreatedAt,
                mapping.UpdatedAt))
            .SingleOrDefaultAsync(cancellationToken);
    }

    public async Task<PagedResult<ControlCoverageItemResponse>> ListControlCoverageAsync(ControlCoverageListQuery query, string? actorUserId, CancellationToken cancellationToken)
    {
        var controls = await (
            from control in dbContext.ControlCatalog.AsNoTracking()
            join project in dbContext.Projects.AsNoTracking() on control.ProjectId equals project.Id into projectJoin
            from project in projectJoin.DefaultIfEmpty()
            select new { Control = control, Project = project })
            .ToListAsync(cancellationToken);

        var mappings = await dbContext.ControlMappings.AsNoTracking().ToListAsync(cancellationToken);
        var generatedAt = DateTimeOffset.UtcNow;

        var rows = controls.Select(row =>
        {
            var activeMappings = mappings.Where(x => x.ControlId == row.Control.Id && x.Status == "active").ToList();
            var evidenceCount = activeMappings.Count(x => string.Equals(x.EvidenceStatus, "verified", StringComparison.OrdinalIgnoreCase) || string.Equals(x.EvidenceStatus, "referenced", StringComparison.OrdinalIgnoreCase));
            var gapCount = activeMappings.Count == 0 ? 1 : Math.Max(0, activeMappings.Count - evidenceCount);
            var coverageStatus = activeMappings.Count == 0 ? "gap" : evidenceCount == activeMappings.Count ? "sufficient" : "partial";

            return new ControlCoverageItemResponse(
                row.Control.Id,
                row.Control.ControlCode,
                row.Control.Title,
                row.Control.ControlSet,
                row.Control.ProcessArea,
                row.Control.ProjectId,
                row.Project?.Name,
                coverageStatus,
                activeMappings.Count,
                evidenceCount,
                gapCount,
                generatedAt);
        }).AsQueryable();

        if (query.ProjectId.HasValue)
        {
            rows = rows.Where(x => x.ProjectId == query.ProjectId.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.ControlSet))
        {
            var controlSet = query.ControlSet.Trim().ToLowerInvariant();
            rows = rows.Where(x => x.ControlSet == controlSet);
        }

        if (!string.IsNullOrWhiteSpace(query.ProcessArea))
        {
            var processArea = query.ProcessArea.Trim().ToLowerInvariant();
            rows = rows.Where(x => x.ProcessArea == processArea);
        }

        if (!string.IsNullOrWhiteSpace(query.CoverageStatus))
        {
            var coverageStatus = query.CoverageStatus.Trim().ToLowerInvariant();
            rows = rows.Where(x => x.CoverageStatus == coverageStatus);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = query.Search.Trim();
            rows = rows.Where(x => x.ControlCode.Contains(term, StringComparison.OrdinalIgnoreCase) || x.Title.Contains(term, StringComparison.OrdinalIgnoreCase));
        }

        var materialized = rows.OrderBy(x => x.ControlCode).ToList();
        await PersistCoverageSnapshotsAsync(materialized, cancellationToken);
        return new PagedResult<ControlCoverageItemResponse>(
            materialized.Skip((Math.Max(query.Page, 1) - 1) * Math.Min(Math.Max(query.PageSize, 1), 200)).Take(Math.Min(Math.Max(query.PageSize, 1), 200)).ToList(),
            materialized.Count,
            Math.Max(query.Page, 1),
            Math.Min(Math.Max(query.PageSize, 1), 200));
    }

    internal static IReadOnlyList<AssessmentEvidenceReferenceResponse> DeserializeEvidenceRefs(string? json)
    {
        if (string.IsNullOrWhiteSpace(json))
        {
            return [];
        }

        try
        {
            return JsonSerializer.Deserialize<List<AssessmentEvidenceReferenceResponse>>(json, SerializerOptions) ?? [];
        }
        catch
        {
            return [];
        }
    }

    private static async Task<PagedResult<T>> PageAsync<T>(IQueryable<T> query, int page, int pageSize, CancellationToken cancellationToken)
    {
        var safePage = page <= 0 ? 1 : page;
        var safePageSize = pageSize <= 0 ? 25 : Math.Min(pageSize, 200);
        var total = await query.CountAsync(cancellationToken);
        var items = await query.Skip((safePage - 1) * safePageSize).Take(safePageSize).ToListAsync(cancellationToken);
        return new PagedResult<T>(items, total, safePage, safePageSize);
    }

    private async Task PersistCoverageSnapshotsAsync(IReadOnlyList<ControlCoverageItemResponse> rows, CancellationToken cancellationToken)
    {
        var generatedAt = rows.Count > 0 ? rows[0].GeneratedAt : DateTimeOffset.UtcNow;
        dbContext.ControlCoverageSnapshots.AddRange(rows.Select(row => new ControlCoverageSnapshotEntity
        {
            Id = Guid.NewGuid(),
            ControlId = row.ControlId,
            ProjectId = row.ProjectId,
            CoverageStatus = row.CoverageStatus,
            ActiveMappingCount = row.ActiveMappingCount,
            EvidenceCount = row.EvidenceCount,
            GapCount = row.GapCount,
            GeneratedAt = generatedAt,
            CreatedAt = generatedAt
        }));

        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
