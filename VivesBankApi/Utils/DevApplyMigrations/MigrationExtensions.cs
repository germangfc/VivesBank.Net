using Microsoft.EntityFrameworkCore;
using VivesBankApi.Database;

namespace ApiFunkosCS.Utils.DevApplyMigrations;

public static class MigrationExtensions
{
    public static void ApplyMigrations(this IApplicationBuilder context)
    {
        using IServiceScope scope = context.ApplicationServices.CreateScope();
        using BancoDbContext dbContext = scope.ServiceProvider.GetRequiredService<BancoDbContext>();
        dbContext.Database.Migrate(); // Aplica las migraciones si existen
    }
}