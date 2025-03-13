namespace CineVault.API.Controllers.Requests;

public sealed class BulkMoviesRequest
{
    public required List<MovieRequest> Movies { get; set; }
}
