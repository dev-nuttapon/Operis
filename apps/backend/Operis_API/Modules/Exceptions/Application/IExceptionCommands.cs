using Operis_API.Modules.Exceptions.Contracts;

namespace Operis_API.Modules.Exceptions.Application;

public interface IExceptionCommands
{
    Task<ExceptionCommandResult<WaiverDetailResponse>> CreateWaiverAsync(CreateWaiverRequest request, string? actorUserId, CancellationToken cancellationToken);
    Task<ExceptionCommandResult<WaiverDetailResponse>> UpdateWaiverAsync(Guid waiverId, UpdateWaiverRequest request, string? actorUserId, CancellationToken cancellationToken);
    Task<ExceptionCommandResult<WaiverDetailResponse>> TransitionWaiverAsync(Guid waiverId, TransitionWaiverRequest request, string? actorUserId, CancellationToken cancellationToken);
}
