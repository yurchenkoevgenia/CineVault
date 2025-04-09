namespace CineVault.API.Entities;

public sealed class Movie
{
    public int Id { get; set; }
    public required string Title { get; set; }
    public string? Description { get; set; }
    public DateOnly? ReleaseDate { get; set; }
    public string? Genre { get; set; }
    public string? Director { get; set; }
    // TODO 6 Додавання зв'язків між таблицями
    public ICollection<Review> Reviews { get; set; } = [];
    // TODO 5 Нова сутність Actor
    // TODO 6 Додавання зв'язків між таблицями
    public ICollection<Actor> Actors { get; set; } = [];
    // TODO 10 Реалізувати Soft Delete
    public bool IsDeleted { get; set; }
}