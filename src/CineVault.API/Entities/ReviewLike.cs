namespace CineVault.API.Entities;

public sealed class ReviewLike
{
    public int Id { get; set; }
    public required int ReviewId { get; set; }
    public required int UserId { get; set; }
    // TODO 6 Додавання зв'язків між таблицями
    public Review Review { get; set; } = null!;
    // TODO 6 Додавання зв'язків між таблицями
    public User User { get; set; } = null!;
    // TODO 10 Реалізувати Soft Delete
    public bool IsDeleted { get; set; }
}