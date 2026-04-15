namespace PersonalKnowledge.Results;

public class ApiResult
{
    public bool IsSuccess { get; set; }
    public string Message { get; set; } = string.Empty;
    public Dictionary<string, object> Errors { get; set; } = new();
    public int StatusCode { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    public ApiResult() { }

    public ApiResult(bool isSuccess, string message, int statusCode)
    {
        IsSuccess = isSuccess;
        Message = message;
        StatusCode = statusCode;
    }

    public static ApiResult Success(string message = "Operation completed successfully", int statusCode = 200)
    {
        return new ApiResult(true, message, statusCode);
    }

    public static ApiResult Failure(string message, int statusCode = 500, Dictionary<string, object>? errors = null)
    {
        var result = new ApiResult(false, message, statusCode);
        if (errors != null)
            result.Errors = errors;
        return result;
    }
}

public class ApiResult<T> : ApiResult
{
    public T? Data { get; set; }

    public ApiResult() { }

    public ApiResult(T data, string message = "Operation completed successfully", int statusCode = 200)
    {
        IsSuccess = true;
        Message = message;
        StatusCode = statusCode;
        Data = data;
    }

    public static ApiResult<T> Success(T data, string message = "Operation completed successfully", int statusCode = 200)
    {
        return new ApiResult<T>(data, message, statusCode);
    }

    public static new ApiResult<T> Failure(string message, int statusCode = 500, Dictionary<string, object>? errors = null)
    {
        var result = new ApiResult<T>
        {
            IsSuccess = false,
            Message = message,
            StatusCode = statusCode,
            Data = default
        };
        if (errors != null)
            result.Errors = errors;
        return result;
    }
}




