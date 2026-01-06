namespace Infrastructure;

/// <summary>
/// Dependency Injection для Infrastructure 
/// </summary>
public static class DependencyInjection
{
    /// <summary>
    /// Добавление DI для использования сервисов Infrastructure
    /// </summary>
    /// <param name="services"></param>
    /// <param name="configuration"></param>
    /// <returns></returns>
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("SqLiteConnection");
        services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseSqlite(connectionString);
        });

        return services;

    }
}
