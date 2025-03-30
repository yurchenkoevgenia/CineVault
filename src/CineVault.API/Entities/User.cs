namespace CineVault.API.Entities;

public sealed class User
{
    public int Id { get; set; }
    public required string Username { get; set; }
    public required string Email { get; set; }
    public required string Password { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    // TODO 6 Додавання зв'язків між таблицями
    public ICollection<Review> Reviews { get; set; } = [];
    // TODO 10 Реалізувати Soft Delete
    public bool IsDeleted { get; set; }
}