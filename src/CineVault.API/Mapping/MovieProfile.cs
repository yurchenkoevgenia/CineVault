using Mapster;

namespace CineVault.API.Mapping;

public class MovieProfile
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<Movie, MovieResponse>()
            .Map(dest => dest.AverageRating, src => src.Reviews.Any() ? src.Reviews.Average(r => r.Rating) : 0)
            .Map(dest => dest.ReviewCount, src => src.Reviews.Count)
            .Map(dest => dest.Reviews, src => src.Reviews);

        config.NewConfig<MovieRequest, Movie>();
    }
}
