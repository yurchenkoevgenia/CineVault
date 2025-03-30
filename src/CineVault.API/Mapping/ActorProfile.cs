using Mapster;

namespace CineVault.API.Mapping;

public class ActorProfile : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<ActorRequest, Actor>();
        config.NewConfig<Actor, ActorResponse>();
    }
}