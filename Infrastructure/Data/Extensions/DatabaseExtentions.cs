global using Infrastructure.Data.DataBaseContext;
global using Microsoft.AspNetCore.Builder;
global using Microsoft.Extensions.DependencyInjection;

namespace Infrastructure.Data.Extensions;

/// <summary>
/// Расширения для работы с базой данных
/// </summary>
public static class DatabaseExtentions
{
    public static async Task InitializeDatabaseAsync(this WebApplication app)
    {
        using IServiceScope scope = app.Services.CreateScope();
        var dbContext = scope
            .ServiceProvider
            .GetRequiredService<ApplicationDbContext>();
        
        dbContext.Database.MigrateAsync().GetAwaiter().GetResult();

        await SeedData(dbContext);
    }

    private static async Task SeedData(ApplicationDbContext dbContext)
    {
        await SeedTopicsAsync(dbContext);
    }

    private static async Task SeedTopicsAsync(ApplicationDbContext dbContext)
    {
        if(!await dbContext.Topics.AnyAsync())
        {
            
            await dbContext.Topics.AddRangeAsync(InitialData.Topics);
            await dbContext.SaveChangesAsync();
        }
    }
}
