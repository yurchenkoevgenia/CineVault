namespace CineVault.API.Controllers.Requests;

// TODO 9 Додати такі нові методи в API
public sealed class MovieSearchRequest
{
    public string? SearchText { get; set; }
    public string? Genre { get; set; }
    public double? MinRating { get; set; }
    public DateOnly? ReleaseDateFrom { get; set; }
    public DateOnly? ReleaseDateTo { get; set; }
}