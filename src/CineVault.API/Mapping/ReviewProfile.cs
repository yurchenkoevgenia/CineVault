using Mapster;

namespace CineVault.API.Mapping;

public class ReviewProfile : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<Review, ReviewResponse>()
            .Map(dest => dest.MovieTitle, src => src.Movie != null ? src.Movie.Title : string.Empty)
            .Map(dest => dest.Username, src => src.User != null ? src.User.Username : string.Empty);
    }
}
