namespace CineVault.API.Entities;

public sealed class Review
{
    public int Id { get; set; }
    public required int MovieId { get; set; }
    public required int UserId { get; set; }
    public required int Rating { get; set; }
    public string? Comment { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    // TODO 6 Додавання зв'язків між таблицями
    public Movie? Movie { get; set; }
    // TODO 6 Додавання зв'язків між таблицями
    public User? User { get; set; }
    // TODO 6 Додавання зв'язків між таблицями
    public ICollection<ReviewLike> Likes { get; set; } = [];
    // TODO 10 Реалізувати Soft Delete
    public bool IsDeleted { get; set; }
}