namespace CineVault.API.Controllers.Responses;

// TODO 5 Нова сутність Actor
public sealed class ActorResponse
{
    public int Id { get; set; }
    public required string FullName { get; set; }
    public DateOnly BirthDate { get; set; }
    public string? Biography { get; set; }
}