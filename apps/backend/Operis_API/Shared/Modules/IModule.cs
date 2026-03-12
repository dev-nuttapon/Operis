namespace Operis_API.Shared.Modules;

public interface IModule
{
    IServiceCollection RegisterServices(IServiceCollection services, IConfiguration configuration);
    IEndpointRouteBuilder MapEndpoints(IEndpointRouteBuilder endpoints);
}
