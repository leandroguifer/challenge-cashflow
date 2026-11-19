using Microsoft.AspNetCore.Http;

namespace Cashflow.Core.Results;

public static class ResultHttpExtensions
{
    public static IResult ToHttpResult<T>(this Result<T> result)
    {
        if (result.IsSuccess)
        {
            return TypedResults.Ok(result.Value);
        }

        var error = result.Error!;

        var status = error.Kind switch
        {
            ResultErrorKind.Validation => StatusCodes.Status400BadRequest,
            ResultErrorKind.NotFound => StatusCodes.Status404NotFound,
            ResultErrorKind.Conflict => StatusCodes.Status409Conflict,
            ResultErrorKind.Forbidden => StatusCodes.Status403Forbidden,
            ResultErrorKind.Unauthorized => StatusCodes.Status401Unauthorized,
            _ => StatusCodes.Status500InternalServerError
        };

        return TypedResults.Problem(
            statusCode: status, 
            title: error.Message, 
            detail: error.Code, 
            extensions: new Dictionary<string, object?> { ["code"] = error.Code }
        );
    }
}