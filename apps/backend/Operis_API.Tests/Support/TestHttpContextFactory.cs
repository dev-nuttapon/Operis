using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.DependencyInjection;

namespace Operis_API.Tests.Support;

internal static class TestHttpContextFactory
{
    public static HttpContext Create()
    {
        var services = new ServiceCollection()
            .AddLogging()
            .AddProblemDetails()
            .ConfigureHttpJsonOptions(_ => { })
            .BuildServiceProvider();

        return new DefaultHttpContext
        {
            RequestServices = services
        };
    }
}
