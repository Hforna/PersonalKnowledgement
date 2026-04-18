using PersonalKnowledge.Domain.Exceptions;
using ApplicationException = PersonalKnowledge.Domain.Exceptions.ApplicationException;

namespace PersonalKnowledge.Middleware;

public class GlobalExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;

    public GlobalExceptionHandlerMiddleware(RequestDelegate next, ILogger<GlobalExceptionHandlerMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred: {ExceptionMessage}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private static Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";

        var response = exception switch
        {
            InvalidAssetFormatException ex => HandleInvalidAssetFormat(context, ex),
            AssetHandlerNotFoundException ex => HandleAssetHandlerNotFound(context, ex),
            FileProcessingException ex => HandleFileProcessing(context, ex),
            AssetChunkingException ex => HandleAssetChunking(context, ex),
            AssetException ex => HandleAssetException(context, ex),
            
            EntityNotFoundException ex => HandleEntityNotFound(context, ex),
            RepositoryException ex => HandleRepositoryException(context, ex),
            TransactionException ex => HandleTransactionException(context, ex),
            
            UserNotFoundException ex => HandleUserNotFound(context, ex),
            AuthenticationException ex => HandleAuthenticationException(context, ex),
            AuthorizationException ex => HandleAuthorizationException(context, ex),
            UserException ex => HandleUserException(context, ex),
            
            ValidationException ex => HandleValidationException(context, ex),
            ConversationException ex => HandleConversationException(context, ex),
            StorageException ex => HandleStorageException(context, ex),
            OperationException ex => HandleOperationException(context, ex),
            
            ApplicationException ex => HandleApplicationException(context, ex),
            _ => HandleGenericException(context, exception)
        };

        return response;
    }

    private static Task HandleInvalidAssetFormat(HttpContext context, InvalidAssetFormatException ex)
    {
        context.Response.StatusCode = StatusCodes.Status400BadRequest;
        var response = new
        {
            isSuccess = false,
            message = "Invalid asset format provided",
            error = ex.Message,
            statusCode = StatusCodes.Status400BadRequest,
            timestamp = DateTime.UtcNow
        };
        return context.Response.WriteAsJsonAsync(response);
    }

    private static Task HandleAssetHandlerNotFound(HttpContext context, AssetHandlerNotFoundException ex)
    {
        context.Response.StatusCode = StatusCodes.Status400BadRequest;
        var response = new
        {
            isSuccess = false,
            message = "Asset handler not found",
            error = ex.Message,
            fileExtension = ex.FileExtension.ToString(),
            statusCode = StatusCodes.Status400BadRequest,
            timestamp = DateTime.UtcNow
        };
        return context.Response.WriteAsJsonAsync(response);
    }

    private static Task HandleFileProcessing(HttpContext context, FileProcessingException ex)
    {
        context.Response.StatusCode = StatusCodes.Status400BadRequest;
        var response = new
        {
            isSuccess = false,
            message = "Error processing file",
            error = ex.Message,
            fileName = ex.FileName,
            statusCode = StatusCodes.Status400BadRequest,
            timestamp = DateTime.UtcNow
        };
        return context.Response.WriteAsJsonAsync(response);
    }

    private static Task HandleAssetChunking(HttpContext context, AssetChunkingException ex)
    {
        context.Response.StatusCode = StatusCodes.Status400BadRequest;
        var response = new
        {
            isSuccess = false,
            message = "Error during asset chunking",
            error = ex.Message,
            statusCode = StatusCodes.Status400BadRequest,
            timestamp = DateTime.UtcNow
        };
        return context.Response.WriteAsJsonAsync(response);
    }

    private static Task HandleAssetException(HttpContext context, AssetException ex)
    {
        context.Response.StatusCode = StatusCodes.Status400BadRequest;
        var response = new
        {
            isSuccess = false,
            message = "Asset operation failed",
            error = ex.Message,
            statusCode = StatusCodes.Status400BadRequest,
            timestamp = DateTime.UtcNow
        };
        return context.Response.WriteAsJsonAsync(response);
    }

    private static Task HandleEntityNotFound(HttpContext context, EntityNotFoundException ex)
    {
        context.Response.StatusCode = StatusCodes.Status404NotFound;
        var response = new
        {
            isSuccess = false,
            message = "Entity not found",
            error = ex.Message,
            entityType = ex.EntityType,
            entityId = ex.EntityId,
            statusCode = StatusCodes.Status404NotFound,
            timestamp = DateTime.UtcNow
        };
        return context.Response.WriteAsJsonAsync(response);
    }

    private static Task HandleRepositoryException(HttpContext context, RepositoryException ex)
    {
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        var response = new
        {
            isSuccess = false,
            message = "A repository error occurred",
            error = ex.Message,
            entityType = ex.EntityType,
            statusCode = StatusCodes.Status500InternalServerError,
            timestamp = DateTime.UtcNow
        };
        return context.Response.WriteAsJsonAsync(response);
    }

    private static Task HandleTransactionException(HttpContext context, TransactionException ex)
    {
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        var response = new
        {
            isSuccess = false,
            message = "Database transaction failed",
            error = ex.Message,
            statusCode = StatusCodes.Status500InternalServerError,
            timestamp = DateTime.UtcNow
        };
        return context.Response.WriteAsJsonAsync(response);
    }

    private static Task HandleUserNotFound(HttpContext context, UserNotFoundException ex)
    {
        context.Response.StatusCode = StatusCodes.Status404NotFound;
        var response = new
        {
            isSuccess = false,
            message = "User not found",
            error = ex.Message,
            statusCode = StatusCodes.Status404NotFound,
            timestamp = DateTime.UtcNow
        };
        return context.Response.WriteAsJsonAsync(response);
    }

    private static Task HandleAuthenticationException(HttpContext context, AuthenticationException ex)
    {
        context.Response.StatusCode = StatusCodes.Status401Unauthorized;
        var response = new
        {
            isSuccess = false,
            message = "Authentication failed",
            error = ex.Message,
            statusCode = StatusCodes.Status401Unauthorized,
            timestamp = DateTime.UtcNow
        };
        return context.Response.WriteAsJsonAsync(response);
    }

    private static Task HandleAuthorizationException(HttpContext context, AuthorizationException ex)
    {
        context.Response.StatusCode = StatusCodes.Status403Forbidden;
        var response = new
        {
            isSuccess = false,
            message = "Access denied",
            error = ex.Message,
            resource = ex.Resource,
            statusCode = StatusCodes.Status403Forbidden,
            timestamp = DateTime.UtcNow
        };
        return context.Response.WriteAsJsonAsync(response);
    }

    private static Task HandleUserException(HttpContext context, UserException ex)
    {
        context.Response.StatusCode = StatusCodes.Status400BadRequest;
        var response = new
        {
            isSuccess = false,
            message = "User operation failed",
            error = ex.Message,
            statusCode = StatusCodes.Status400BadRequest,
            timestamp = DateTime.UtcNow
        };
        return context.Response.WriteAsJsonAsync(response);
    }

    private static Task HandleValidationException(HttpContext context, ValidationException ex)
    {
        context.Response.StatusCode = StatusCodes.Status400BadRequest;
        var errors = new Dictionary<string, object>();
        
        if (ex.Errors.Count > 0)
        {
            foreach (var error in ex.Errors)
                errors[error.Key] = error.Value;
        }
        else
        {
            errors["general"] = ex.Message;
        }
        
        var response = new
        {
            isSuccess = false,
            message = "Validation failed",
            errors = errors,
            statusCode = StatusCodes.Status400BadRequest,
            timestamp = DateTime.UtcNow
        };
        return context.Response.WriteAsJsonAsync(response);
    }

    private static Task HandleConversationException(HttpContext context, ConversationException ex)
    {
        context.Response.StatusCode = StatusCodes.Status400BadRequest;
        var response = new
        {
            isSuccess = false,
            message = "Conversation operation failed",
            error = ex.Message,
            statusCode = StatusCodes.Status400BadRequest,
            timestamp = DateTime.UtcNow
        };
        return context.Response.WriteAsJsonAsync(response);
    }

    private static Task HandleStorageException(HttpContext context, StorageException ex)
    {
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        var response = new
        {
            isSuccess = false,
            message = "Storage operation failed",
            error = ex.Message,
            filePath = ex.FilePath,
            statusCode = StatusCodes.Status500InternalServerError,
            timestamp = DateTime.UtcNow
        };
        return context.Response.WriteAsJsonAsync(response);
    }

    private static Task HandleOperationException(HttpContext context, OperationException ex)
    {
        context.Response.StatusCode = StatusCodes.Status400BadRequest;
        var response = new
        {
            isSuccess = false,
            message = ex.OperationName + " operation failed",
            error = ex.Message,
            statusCode = StatusCodes.Status400BadRequest,
            timestamp = DateTime.UtcNow
        };
        return context.Response.WriteAsJsonAsync(response);
    }

    private static Task HandleApplicationException(HttpContext context, ApplicationException ex)
    {
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        var response = new
        {
            isSuccess = false,
            message = "An application error occurred",
            error = ex.Message,
            statusCode = StatusCodes.Status500InternalServerError,
            timestamp = DateTime.UtcNow
        };
        return context.Response.WriteAsJsonAsync(response);
    }

    private static Task HandleGenericException(HttpContext context, Exception ex)
    {
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;
        var response = new
        {
            isSuccess = false,
            message = "An unexpected error occurred",
            error = "",
            statusCode = StatusCodes.Status500InternalServerError,
            timestamp = DateTime.UtcNow
        };
        return context.Response.WriteAsJsonAsync(response);
    }
}




