namespace CineVault.API.Entities;

// TODO 5 Підтримка лайків для відгуків-коментарів з оцінкою
public sealed class ReviewLike
{
    public int Id { get; set; }
    public required int ReviewId { get; set; }
    public required int UserId { get; set; }

    public Review Review { get; set; } = null!;
    public User User { get; set; } = null!;
}
