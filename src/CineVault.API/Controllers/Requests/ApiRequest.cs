namespace CineVault.API.Controllers.Requests;

public class ApiRequest<TRequestData>
{
    public TRequestData Data { get; set; }
    public Dictionary<string, string> Meta { get; set; }

    public ApiRequest()
    {
        Meta = new Dictionary<string, string>();
    }
}
