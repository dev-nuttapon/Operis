using Operis_API.Modules.Learning.Contracts;
using Operis_API.Shared.Contracts;

namespace Operis_API.Modules.Learning.Application;

public interface ILearningQueries
{
    Task<PagedResult<TrainingCourseResponse>> ListTrainingCoursesAsync(TrainingCourseListQuery query, CancellationToken cancellationToken);
    Task<PagedResult<RoleTrainingRequirementResponse>> ListRoleTrainingRequirementsAsync(RoleTrainingMatrixQuery query, CancellationToken cancellationToken);
    Task<PagedResult<TrainingCompletionResponse>> ListTrainingCompletionsAsync(TrainingCompletionListQuery query, CancellationToken cancellationToken);
    Task<PagedResult<CompetencyReviewResponse>> ListCompetencyReviewsAsync(CompetencyReviewListQuery query, CancellationToken cancellationToken);
    Task<IReadOnlyList<ProjectRoleOptionResponse>> ListProjectRoleOptionsAsync(Guid? projectId, CancellationToken cancellationToken);
}
