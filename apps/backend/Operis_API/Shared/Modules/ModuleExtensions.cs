using System.Reflection;

namespace Operis_API.Shared.Modules;

public static class ModuleExtensions
{
    public static IServiceCollection AddModules(this IServiceCollection services, IConfiguration configuration)
    {
        var modules = DiscoverModules();
        services.AddSingleton<IReadOnlyCollection<IModule>>(modules);

        foreach (var module in modules)
        {
            module.RegisterServices(services, configuration);
        }

        return services;
    }

    public static IEndpointRouteBuilder MapModules(this IEndpointRouteBuilder app)
    {
        var modules = app.ServiceProvider.GetRequiredService<IReadOnlyCollection<IModule>>();

        foreach (var module in modules)
        {
            module.MapEndpoints(app);
        }

        return app;
    }

    private static IReadOnlyCollection<IModule> DiscoverModules()
    {
        var moduleType = typeof(IModule);

        return Assembly.GetExecutingAssembly()
            .GetTypes()
            .Where(type => moduleType.IsAssignableFrom(type) && type is { IsClass: true, IsAbstract: false })
            .Select(type => (IModule)Activator.CreateInstance(type)!)
            .ToArray();
    }
}
