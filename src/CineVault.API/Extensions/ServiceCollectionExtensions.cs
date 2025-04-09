namespace CineVault.API.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddCineVaultDbContext(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddDbContext<CineVaultDbContext>(options =>
        {
            string? connectionString = configuration.GetConnectionString("CineVaultDb");

            if (string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException("Connection string is not configured");
            }

            // TODO 1 Налаштувати EF Core для роботи з базою даних у проєкті
            options.UseSqlServer(connectionString);
        });

        return services;
    }
}
