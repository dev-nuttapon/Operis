using Operis_API.Modules.Learning.Contracts;

namespace Operis_API.Modules.Learning.Application;

public interface ILearningCommands
{
    Task<LearningCommandResult<TrainingCourseResponse>> CreateTrainingCourseAsync(CreateTrainingCourseRequest request, string? actor, CancellationToken cancellationToken);
    Task<LearningCommandResult<TrainingCourseResponse>> UpdateTrainingCourseAsync(Guid courseId, UpdateTrainingCourseRequest request, string? actor, CancellationToken cancellationToken);
    Task<LearningCommandResult<TrainingCourseResponse>> TransitionTrainingCourseAsync(Guid courseId, TransitionTrainingCourseRequest request, string? actor, CancellationToken cancellationToken);
    Task<LearningCommandResult<RoleTrainingRequirementResponse>> CreateRoleTrainingRequirementAsync(CreateRoleTrainingRequirementRequest request, string? actor, CancellationToken cancellationToken);
    Task<LearningCommandResult<RoleTrainingRequirementResponse>> UpdateRoleTrainingRequirementAsync(Guid requirementId, UpdateRoleTrainingRequirementRequest request, string? actor, CancellationToken cancellationToken);
    Task<LearningCommandResult<TrainingCompletionResponse>> RecordTrainingCompletionAsync(RecordTrainingCompletionRequest request, string? actor, CancellationToken cancellationToken);
    Task<LearningCommandResult<TrainingCompletionResponse>> UpdateTrainingCompletionAsync(Guid completionId, UpdateTrainingCompletionRequest request, string? actor, CancellationToken cancellationToken);
    Task<LearningCommandResult<CompetencyReviewResponse>> CreateCompetencyReviewAsync(CreateCompetencyReviewRequest request, string? actor, CancellationToken cancellationToken);
    Task<LearningCommandResult<CompetencyReviewResponse>> UpdateCompetencyReviewAsync(Guid reviewId, UpdateCompetencyReviewRequest request, string? actor, CancellationToken cancellationToken);
}
