using Api.Exceptions.Handler;

namespace Api;

/// <summary>
/// Dependency Injection для Api
/// </summary>
public static class DependencyInjection
{
    public static IServiceCollection AddApiServices(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddExceptionHandler<CustomExceptionHandler>();
        services.AddControllers();
        services.AddOpenApi();

        services.AddCors(options =>
        {
            options.AddPolicy("react-policy", policy =>
            {
                policy.AllowAnyHeader()
                    .AllowAnyMethod()
                    .WithOrigins("http://localhost:3000");
            });
        });

        return services;
    }

    /// <summary>
    /// Добавление DI для использования сервисов Api
    /// </summary>
    /// <param name="app"></param>
    /// <returns></returns>
    public static WebApplication UseApiServices(this WebApplication app)
    {
        app.UseCors("react-policy");

        if (app.Environment.IsDevelopment())
        {
            app.MapOpenApi();
        }

        app.UseExceptionHandler(options => { });

        app.UseHttpsRedirection();

        app.UseAuthorization();

        app.MapControllers();

        return app;
    }
}
