using Operis_API.Modules.Knowledge.Contracts;

namespace Operis_API.Modules.Knowledge.Application;

public interface IKnowledgeCommands
{
    Task<KnowledgeCommandResult<LessonLearnedItem>> CreateLessonLearnedAsync(CreateLessonLearnedRequest request, string? actorUserId, CancellationToken cancellationToken);
    Task<KnowledgeCommandResult<LessonLearnedItem>> UpdateLessonLearnedAsync(Guid lessonId, UpdateLessonLearnedRequest request, string? actorUserId, CancellationToken cancellationToken);
    Task<KnowledgeCommandResult<LessonLearnedItem>> PublishLessonLearnedAsync(Guid lessonId, PublishLessonLearnedRequest request, string? actorUserId, CancellationToken cancellationToken);
}
