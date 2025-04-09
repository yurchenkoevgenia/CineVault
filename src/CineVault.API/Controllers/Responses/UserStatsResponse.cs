namespace CineVault.API.Controllers.Responses;

// TODO 9 Додати такі нові методи в API
public sealed class UserStatsResponse
{
    public int TotalReviews { get; set; }
    public double AverageRating { get; set; }
    public DateTime? LastActivity { get; set; }
    public List<GenreStat> GenreStats { get; set; }

    public sealed class GenreStat
    {
        public string Genre { get; set; }
        public int TotalReviews { get; set; }
        public double AverageRating { get; set; }
    }
}