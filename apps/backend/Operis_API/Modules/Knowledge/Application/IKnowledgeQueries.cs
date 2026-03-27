using Operis_API.Modules.Knowledge.Contracts;
using Operis_API.Shared.Contracts;

namespace Operis_API.Modules.Knowledge.Application;

public interface IKnowledgeQueries
{
    Task<PagedResult<LessonLearnedItem>> ListLessonsLearnedAsync(LessonLearnedListQuery query, CancellationToken cancellationToken);
    Task<LessonLearnedItem?> GetLessonLearnedAsync(Guid lessonId, CancellationToken cancellationToken);
}
