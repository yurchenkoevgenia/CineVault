namespace CineVault.API.Controllers.Responses;

public class ApiResponse<TResponseData>
{
    /// <summary>
    /// Статус відповіді (наприклад, 200, 400, 500)
    /// </summary>
    public int StatusCode { get; set; }

    /// <summary>
    /// Повідомлення про статус (успіх, помилка)
    /// </summary>
    public string Message { get; set; }

    /// <summary>
    /// Основні дані відповіді
    /// </summary>
    public TResponseData Data { get; set; }

    /// <summary>
    /// Чи успішний запит
    /// </summary>
    public bool IsSuccess => StatusCode >= 200 && StatusCode < 300;

    /// <summary>
    /// Додаткові метадані відповіді
    /// </summary>
    public Dictionary<string, string> Meta { get; set; }

    public ApiResponse()
    {
        Meta = new Dictionary<string, string>();
    }

    /// <summary>
    /// Формування успішної відповіді
    /// </summary>
    public static ApiResponse<TResponseData> Success(TResponseData data, string message = "Success", int statusCode = 200)
    {
        return new ApiResponse<TResponseData>
        {
            StatusCode = statusCode,
            Message = message,
            Data = data
        };
    }

    /// <summary>
    /// Формування відповіді про помилку 400
    /// </summary>
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
