namespace CineVault.API.Controllers.Responses;

// TODO 9 Додати такі нові методи в API
public sealed class MovieDetailsResponse
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public DateOnly? ReleaseDate { get; set; }
    public string Genre { get; set; }
    public string Director { get; set; }
    public double AverageRating { get; set; }
    public int ReviewCount { get; set; }
    public List<ReviewResponse> LatestReviews { get; set; }
}