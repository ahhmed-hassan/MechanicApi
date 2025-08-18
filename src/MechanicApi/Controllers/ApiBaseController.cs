using ErrorOr;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace MechanicApi.Controllers;

[ApiController]

public class ApiBaseController : ControllerBase
{
    protected ActionResult Problem(List<Error> errors)
    {
        if (errors.Count() is 0)
            return Problem();

        if (errors.All(e => e.Type == ErrorType.Validation))
        {
            return ValidationProblem(errors);
        }
        return Problem(errors.First()); 
    }

    private ObjectResult Problem(Error? error)
    {
        var statusCode = error?.Type switch
        {
            ErrorType.Conflict => StatusCodes.Status409Conflict,
            ErrorType.Validation => StatusCodes.Status400BadRequest,
            ErrorType.NotFound => StatusCodes.Status404NotFound,
            ErrorType.Unauthorized => StatusCodes.Status403Forbidden,
            _ => StatusCodes.Status500InternalServerError,
        };

        return Problem(statusCode: statusCode, title: error?.Description);
    }
    private ActionResult ValidationProblem(List<Error> errors)
    {
        throw new NotImplementedException(); 
    }

}
