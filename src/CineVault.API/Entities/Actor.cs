namespace CineVault.API.Entities;

// TODO 5 Нова сутність Actor
public sealed class Actor
{
    public int Id { get; set; }
    public required string FullName { get; set; }
    public DateOnly BirthDate { get; set; }
    public string? Biography { get; set; }
    // TODO 6 Додавання зв'язків між таблицями
    public ICollection<Movie> Movies { get; set; } = [];
    // TODO 10 Реалізувати Soft Delete
    public bool IsDeleted { get; set; }
}