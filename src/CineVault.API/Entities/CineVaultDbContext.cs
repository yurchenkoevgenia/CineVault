namespace CineVault.API.Entities;

public sealed class CineVaultDbContext : DbContext
{
    public DbSet<Movie> Movies { get; set; }
    public DbSet<Review> Reviews { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<ReviewLike> ReviewLikes { get; set; }
    public DbSet<Actor> Actors { get; set; }

    public CineVaultDbContext(DbContextOptions<CineVaultDbContext> options)
        : base(options)
    {
    }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        // TODO 4 Виконати міграцію
        builder.Entity<Review>().ToTable(t =>
            t.HasCheckConstraint("CK_Review_Rating", "Rating BETWEEN 1 AND 10"));

        // TODO 4 Виконати міграцію
        builder.Entity<Movie>(entity =>
        {
            entity.HasIndex(m => m.ReleaseDate);
        });

        // TODO 4 Виконати міграцію
        builder.Entity<User>(entity =>
        {
            entity.Property(u => u.Password).HasMaxLength(255);
        });

        // TODO 6 Додавання зв'язків між таблицями
        builder.Entity<Review>(entity =>
        {
            entity.HasOne(r => r.User)
                .WithMany(u => u.Reviews)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.NoAction);
        });

        // TODO 7 Забезпечення унікальних ключів
        builder.Entity<Movie>()
            .HasIndex(m => m.Title)
            .IsUnique();

        // TODO 7 Забезпечення унікальних ключів
        builder.Entity<User>()
            .HasIndex(u => u.Username)
            .IsUnique();

        // TODO 7 Забезпечення унікальних ключів
        builder.Entity<User>()
            .HasIndex(u => u.Email)
            .IsUnique();

        // TODO 7 Забезпечення унікальних ключів
        builder.Entity<ReviewLike>()
            .HasIndex(rl => new { rl.ReviewId, rl.UserId })
            .IsUnique();

        // TODO 8 Використання конфігурацій моделей
        builder.Entity<Movie>(entity =>
        {
            entity.Property(m => m.Title).HasMaxLength(255);
            entity.Property(m => m.Description).HasMaxLength(1000);
            entity.Property(m => m.Genre).HasMaxLength(255);
            entity.Property(m => m.Director).HasMaxLength(255);
        });

        // TODO 8 Використання конфігурацій моделей
        builder.Entity<Actor>(entity =>
        {
            entity.Property(a => a.FullName).HasMaxLength(255);
            entity.Property(a => a.Biography).HasMaxLength(2000);
        });

        // TODO 8 Використання конфігурацій моделей
        builder.Entity<User>(entity =>
        {
            entity.Property(u => u.Username).HasMaxLength(255);
            entity.Property(u => u.Email).HasMaxLength(255);
        });

        // TODO 8 Використання конфігурацій моделей
        builder.Entity<Review>(entity =>
        {
            entity.Property(r => r.Comment).HasMaxLength(1000);
        });

        // TODO 10 Реалізувати Soft Delete
        builder.Entity<Actor>().HasQueryFilter(a => !a.IsDeleted);
        builder.Entity<ReviewLike>().HasQueryFilter(l => !l.IsDeleted);
        builder.Entity<Movie>().HasQueryFilter(m => !m.IsDeleted);
        builder.Entity<Review>().HasQueryFilter(r => !r.IsDeleted);
        builder.Entity<User>().HasQueryFilter(u => !u.IsDeleted);
    }
}