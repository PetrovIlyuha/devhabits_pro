using DevHabit.Api.Database;
using Microsoft.EntityFrameworkCore;

namespace DevHabit.Api.Extensions;

public static class DatabaseExtensions
{
    public static async Task ApplyMigrationsAsync(this WebApplication app) 
    {  
        using IServiceScope scope = app.Services.CreateScope();
        await using ApplicationDbContext dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        try
        {
            await dbContext.Database.MigrateAsync();
            await dbContext.Database.EnsureCreatedAsync();
            app.Logger.LogInformation("Migrations applied successfully");
        }
        catch (Exception e)
        {
            app.Logger.LogError(e, "Migrations have failed");
            throw;
        }
    }
}
