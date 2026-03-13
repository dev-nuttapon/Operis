using Microsoft.AspNetCore.Mvc;

namespace Operis_API.Shared.Contracts;

public static class ApiProblemDetailsFactory
{
    public static ProblemDetails Create(int status, string code, string title, string? detail = null)
    {
        var problem = new ProblemDetails
        {
            Status = status,
            Title = title,
            Detail = detail
        };

        problem.Extensions["code"] = code;
        return problem;
    }
}
