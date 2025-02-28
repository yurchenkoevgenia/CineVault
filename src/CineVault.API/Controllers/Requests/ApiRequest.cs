namespace CineVault.API.Controllers.Requests;

public class ApiRequest
{
    public Dictionary<string, string> Meta { get; set; }

    public ApiRequest()
    {
        Meta = new Dictionary<string, string>();
    }
}

public class ApiRequest<TRequestData> : ApiRequest
{
    public TRequestData Data { get; set; }
}
