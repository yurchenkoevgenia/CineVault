namespace CineVault.API.Controllers.Requests;

// TODO 5 Нова сутність Actor
public sealed class ActorRequest
{
    public required string FullName { get; init; }
    public DateOnly BirthDate { get; init; }
    public string? Biography { get; init; }
}