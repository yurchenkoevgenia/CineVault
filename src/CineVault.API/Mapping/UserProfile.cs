using Mapster;

namespace CineVault.API.Mapping;

public class UserProfile : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<User, UserResponse>();
    }
}
