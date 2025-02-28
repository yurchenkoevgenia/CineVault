namespace CineVault.API.Controllers.Responses;

public class ApiResponse
{
    public int StatusCode { get; set; }
    public string Message { get; set; }
    public bool IsSuccess => StatusCode >= 200 && StatusCode < 300;
    public Dictionary<string, string> Meta { get; set; }

    public static ApiResponse<T> Success<T>(T data, string message = "Success", int statusCode = 200)
    {
        return new ApiResponse<T>
        {
            StatusCode = statusCode,
            Message = message,
            Data = data
        };
    }

    public static ApiResponse Success(string message = "Success", int statusCode = 200)
    {
        return new ApiResponse
        {
            StatusCode = statusCode,
            Message = message,
        };
    }

    public static ApiResponse Failure(string message, int statusCode = 400)
    {
        return new ApiResponse
        {
            StatusCode = statusCode,
            Message = message
        };
    }
}

public class ApiResponse<TResponseData> : ApiResponse
{
    public TResponseData Data { get; set; }
}
