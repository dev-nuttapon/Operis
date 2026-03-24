using Microsoft.EntityFrameworkCore;
using Operis_API.Infrastructure.Persistence;
using Operis_API.Modules.Users.Infrastructure;
using Operis_API.Modules.Workflows.Infrastructure;
using Operis_API.Shared.Contracts;

namespace Operis_API.Modules.Workflows;

public sealed class WorkflowTaskQueries(OperisDbContext dbContext) : IWorkflowTaskQueries
{
    public async Task<PagedResult<WorkflowTaskListItem>> ListTasksAsync(
        WorkflowTaskListQuery query,
        string? currentUserId,
        CancellationToken cancellationToken)
    {
        var page = Math.Max(query.Page, 1);
        var pageSize = query.PageSize <= 0 ? 10 : Math.Min(query.PageSize, 100);
        var now = DateTimeOffset.UtcNow;

        var baseQuery =
            from instance in dbContext.WorkflowInstances.AsNoTracking()
            join instanceStep in dbContext.WorkflowInstanceSteps.AsNoTracking()
                on instance.Id equals instanceStep.WorkflowInstanceId
            join workflowStep in dbContext.WorkflowSteps.AsNoTracking()
                on instanceStep.WorkflowStepId equals workflowStep.Id
            join project in dbContext.Projects.AsNoTracking()
                on instance.ProjectId equals project.Id
            join document in dbContext.Documents.AsNoTracking()
                on instance.DocumentId equals document.Id
            select new
            {
                InstanceId = instance.Id,
                instance.ProjectId,
                ProjectName = project.Name,
                instance.DocumentId,
                DocumentName = document.DocumentName,
                InstanceStatus = instance.Status,
                InstanceStartedAt = instance.StartedAt,
                InstanceStepId = instanceStep.Id,
                instanceStep.WorkflowStepId,
                instanceStep.StepType,
                instanceStep.Status,
                instanceStep.DisplayOrder,
                workflowStep.Name
            };

        if (query.ProjectId.HasValue)
        {
            baseQuery = baseQuery.Where(x => x.ProjectId == query.ProjectId.Value);
        }

        baseQuery = baseQuery
            .Where(x => x.InstanceStatus == "in_progress" && x.Status == "in_progress");

        if (!string.IsNullOrWhiteSpace(currentUserId))
        {
            baseQuery = baseQuery.Where(item =>
                dbContext.WorkflowStepRoles.Any(stepRole =>
                    stepRole.WorkflowStepId == item.WorkflowStepId &&
                    dbContext.UserProjectAssignments.Any(assignment =>
                        assignment.UserId == currentUserId &&
                        assignment.ProjectId == item.ProjectId &&
                        assignment.ProjectRoleId == stepRole.ProjectRoleId &&
                        assignment.Status == "Active" &&
                        (assignment.EndAt == null || assignment.EndAt > now))));
        }

        var total = await baseQuery.CountAsync(cancellationToken);

        var pageItems = await baseQuery
            .OrderByDescending(x => x.InstanceStartedAt)
            .ThenBy(x => x.DisplayOrder)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        if (pageItems.Count == 0)
        {
            return new PagedResult<WorkflowTaskListItem>(Array.Empty<WorkflowTaskListItem>(), total, page, pageSize);
        }

        var stepIds = pageItems.Select(x => x.WorkflowStepId).Distinct().ToList();
        var projectIds = pageItems.Select(x => x.ProjectId).Distinct().ToList();

        var roleLookup = await dbContext.WorkflowStepRoles
            .AsNoTracking()
            .Where(x => stepIds.Contains(x.WorkflowStepId))
            .Join(dbContext.ProjectRoles.AsNoTracking(),
                stepRole => stepRole.ProjectRoleId,
                projectRole => projectRole.Id,
                (stepRole, projectRole) => new
                {
                    stepRole.WorkflowStepId,
                    stepRole.ProjectRoleId,
                    projectRole.Name
                })
            .ToListAsync(cancellationToken);

        var roleNamesByStepId = roleLookup
            .GroupBy(x => x.WorkflowStepId)
            .ToDictionary(group => group.Key, group => string.Join(", ", group.Select(x => x.Name)));

        var roleIdsByStepId = roleLookup
            .GroupBy(x => x.WorkflowStepId)
            .ToDictionary(group => group.Key, group => group.Select(x => x.ProjectRoleId).Distinct().ToList());

        var assignments = string.IsNullOrWhiteSpace(currentUserId)
            ? new List<UserProjectAssignmentEntity>()
            : await dbContext.UserProjectAssignments
                .AsNoTracking()
                .Where(x =>
                    x.UserId == currentUserId &&
                    projectIds.Contains(x.ProjectId) &&
                    x.Status == "Active" &&
                    (x.EndAt == null || x.EndAt > now))
                .ToListAsync(cancellationToken);

        var assignmentRolesByProject = assignments
            .GroupBy(x => x.ProjectId)
            .ToDictionary(group => group.Key, group => group.Select(x => x.ProjectRoleId).Distinct().ToList());

        var items = pageItems.Select(item =>
        {
            roleNamesByStepId.TryGetValue(item.WorkflowStepId, out var roleNames);
            roleIdsByStepId.TryGetValue(item.WorkflowStepId, out var roleIds);
            assignmentRolesByProject.TryGetValue(item.ProjectId, out var assignedRoles);

            var canAct = false;
            if (!string.IsNullOrWhiteSpace(currentUserId) &&
                string.Equals(item.InstanceStatus, "in_progress", StringComparison.OrdinalIgnoreCase) &&
                string.Equals(item.Status, "in_progress", StringComparison.OrdinalIgnoreCase) &&
                roleIds is not null && assignedRoles is not null)
            {
                canAct = assignedRoles.Intersect(roleIds).Any();
            }

            var relevantRoleNames = roleNames;
            if (assignedRoles is not null && assignedRoles.Count > 0)
            {
                var names = roleLookup
                    .Where(x => x.WorkflowStepId == item.WorkflowStepId && assignedRoles.Contains(x.ProjectRoleId))
                    .Select(x => x.Name)
                    .Distinct()
                    .ToArray();
                if (names.Length > 0)
                {
                    relevantRoleNames = string.Join(", ", names);
                }
            }

            return new WorkflowTaskListItem(
                item.InstanceId,
                item.InstanceStepId,
                item.ProjectId,
                item.ProjectName ?? "-",
                item.DocumentId,
                item.DocumentName ?? "-",
                item.Name ?? "-",
                item.StepType ?? "-",
                relevantRoleNames ?? "-",
                item.Status ?? "-",
                null,
                canAct);
        }).ToList();

        return new PagedResult<WorkflowTaskListItem>(items, total, page, pageSize);
    }
}
