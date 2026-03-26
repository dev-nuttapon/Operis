using Operis_API.Modules.ChangeControl.Contracts;

namespace Operis_API.Modules.ChangeControl.Application;

public interface IChangeControlCommands
{
    Task<ChangeControlCommandResult<ChangeRequestResponse>> CreateChangeRequestAsync(CreateChangeRequestRequest request, string? actorUserId, CancellationToken cancellationToken);
    Task<ChangeControlCommandResult<ChangeRequestResponse>> UpdateChangeRequestAsync(Guid changeRequestId, UpdateChangeRequestRequest request, string? actorUserId, CancellationToken cancellationToken);
    Task<ChangeControlCommandResult<ChangeRequestResponse>> SubmitChangeRequestAsync(Guid changeRequestId, string? actorUserId, CancellationToken cancellationToken);
    Task<ChangeControlCommandResult<ChangeRequestResponse>> ApproveChangeRequestAsync(Guid changeRequestId, ChangeDecisionRequest request, string? actorUserId, CancellationToken cancellationToken);
    Task<ChangeControlCommandResult<ChangeRequestResponse>> RejectChangeRequestAsync(Guid changeRequestId, ChangeDecisionRequest request, string? actorUserId, CancellationToken cancellationToken);
    Task<ChangeControlCommandResult<ChangeRequestResponse>> ImplementChangeRequestAsync(Guid changeRequestId, ChangeImplementationRequest request, string? actorUserId, CancellationToken cancellationToken);
    Task<ChangeControlCommandResult<ChangeRequestResponse>> CloseChangeRequestAsync(Guid changeRequestId, ChangeImplementationRequest request, string? actorUserId, CancellationToken cancellationToken);
    Task<ChangeControlCommandResult<ConfigurationItemResponse>> CreateConfigurationItemAsync(CreateConfigurationItemRequest request, string? actorUserId, CancellationToken cancellationToken);
    Task<ChangeControlCommandResult<ConfigurationItemResponse>> UpdateConfigurationItemAsync(Guid configurationItemId, UpdateConfigurationItemRequest request, string? actorUserId, CancellationToken cancellationToken);
    Task<ChangeControlCommandResult<ConfigurationItemResponse>> ApproveConfigurationItemAsync(Guid configurationItemId, string? actorUserId, CancellationToken cancellationToken);
    Task<ChangeControlCommandResult<BaselineRegistryResponse>> CreateBaselineRegistryAsync(CreateBaselineRegistryRequest request, string? actorUserId, CancellationToken cancellationToken);
    Task<ChangeControlCommandResult<BaselineRegistryResponse>> ApproveBaselineRegistryAsync(Guid baselineRegistryId, string? actorUserId, CancellationToken cancellationToken);
    Task<ChangeControlCommandResult<BaselineRegistryResponse>> SupersedeBaselineRegistryAsync(Guid baselineRegistryId, BaselineOverrideRequest request, string? actorUserId, bool canEmergencyOverride, CancellationToken cancellationToken);
}
