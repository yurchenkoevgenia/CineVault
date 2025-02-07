namespace CineVault.API.Controllers.Requests;

public class ApiRequest<TRequestData>
{
    /// <summary>
    /// Дані запиту
    /// </summary>
    public TRequestData Data { get; set; }

    /// <summary>
    /// Допоміжні метадані (наприклад, CorrelationId)
    /// </summary>
    public Dictionary<string, string> Meta { get; set; }

    public ApiRequest()
    {
        Meta = new Dictionary<string, string>();
    }
}
