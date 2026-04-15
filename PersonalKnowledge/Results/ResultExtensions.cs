using Microsoft.AspNetCore.Mvc;

namespace PersonalKnowledge.Results;

public static class ResultExtensions
{
    public static ActionResult<ApiResult> OkResult(this ControllerBase controller, string message = "Operation completed successfully")
    {
        return controller.Ok(ApiResult.Success(message));
    }

    public static ActionResult<ApiResult<T>> OkResult<T>(this ControllerBase controller, T data, string message = "Operation completed successfully")
    {
        return controller.Ok(ApiResult<T>.Success(data, message));
    }

    public static ActionResult<ApiResult> CreatedResult(this ControllerBase controller, string location, string message = "Resource created successfully")
    {
        return controller.Created(location, ApiResult.Success(message, StatusCodes.Status201Created));
    }

    public static ActionResult<ApiResult<T>> CreatedResult<T>(this ControllerBase controller, string location, T data, string message = "Resource created successfully")
    {
        return controller.Created(location, ApiResult<T>.Success(data, message, StatusCodes.Status201Created));
    }

    public static ActionResult<ApiResult> BadRequestResult(this ControllerBase controller, string message, Dictionary<string, object>? errors = null)
    {
        return controller.BadRequest(ApiResult.Failure(message, StatusCodes.Status400BadRequest, errors));
    }

    public static ActionResult<ApiResult> NotFoundResult(this ControllerBase controller, string message)
    {
        return controller.NotFound(ApiResult.Failure(message, StatusCodes.Status404NotFound));
    }

    public static ActionResult<ApiResult> UnauthorizedResult(this ControllerBase controller, string message = "Unauthorized")
    {
        return controller.Unauthorized(ApiResult.Failure(message, StatusCodes.Status401Unauthorized));
    }

    public static ActionResult<ApiResult> ForbiddenResult(this ControllerBase controller, string message = "Forbidden")
    {
        return controller.Forbid();
    }

    public static ActionResult<ApiResult> ConflictResult(this ControllerBase controller, string message)
    {
        return controller.Conflict(ApiResult.Failure(message, StatusCodes.Status409Conflict));
    }

    public static ActionResult<ApiResult> InternalServerErrorResult(this ControllerBase controller, string message = "An internal server error occurred")
    {
        return controller.StatusCode(StatusCodes.Status500InternalServerError, ApiResult.Failure(message));
    }
}




