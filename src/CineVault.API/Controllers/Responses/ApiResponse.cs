namespace CineVault.API.Controllers.Responses;

public class ApiResponse<TResponseData>
{
    public int StatusCode { get; set; }
    public string Message { get; set; }
    public TResponseData Data { get; set; }
    public bool IsSuccess => StatusCode >= 200 && StatusCode < 300;
    public Dictionary<string, string> Meta { get; set; }
    public ApiResponse()
    {
        Meta = new Dictionary<string, string>();
    }
    public static ApiResponse<TResponseData> Success(TResponseData data, string message = "Success", int statusCode = 200)
    {
        return new ApiResponse<TResponseData>
        {
            StatusCode = statusCode,
            Message = message,
            Data = data
        };
    }

    public static ApiResponse<TResponseData> Failure(string message, int statusCode = 400)
    {
        return new ApiResponse<TResponseData>
        {
            StatusCode = statusCode,
            Message = message,
            Data = default
        };
    }
}
