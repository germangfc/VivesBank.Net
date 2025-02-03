using Microsoft.EntityFrameworkCore;
using VivesBankApi.Database;


/// <summary>
/// Extensiones para aplicar las migraciones de base de datos al iniciar la aplicación.
/// </summary>
public static class MigrationExtensions
{
    /// <summary>
    /// Aplica las migraciones de base de datos al iniciar la aplicación, si existen.
    /// </summary>
    /// <param name="context">El contexto de la aplicación, que proporciona acceso a los servicios de la aplicación.</param>
    /// <remarks>
    /// Este método crea un ámbito de servicio, obtiene el contexto de base de datos e intenta aplicar las migraciones
    /// utilizando el método <see cref="BancoDbContext.Database.Migrate"/>. 
    /// Asegúrate de que el contexto de base de datos esté configurado correctamente en los servicios de la aplicación.
    /// </remarks>
    public static void ApplyMigrations(this IApplicationBuilder context)
    {
        using IServiceScope scope = context.ApplicationServices.CreateScope();
        using BancoDbContext dbContext = scope.ServiceProvider.GetRequiredService<BancoDbContext>();
        dbContext.Database.Migrate(); // Aplica las migraciones si existen
    }
}