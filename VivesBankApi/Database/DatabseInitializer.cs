using System.Data;
using Microsoft.EntityFrameworkCore;

namespace VivesBankApi.Database;

public static class DatabseInitializer
{
    public static void InitializeDatabase(IServiceProvider serviceProvider, string scriptFilePath)
    {
        // Obtener el contexto de la base de datos
        using var scope = serviceProvider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<BancoDbContext>();

        // Aplicar migraciones
        context.Database.Migrate();

        // Verificar si el archivo de script existe
        if (File.Exists(scriptFilePath))
        {
            var sqlScript = File.ReadAllText(scriptFilePath);

            // Ejecutar el script SQL
            ExecuteSqlScript(context, sqlScript);
        }
        else
        {
            throw new FileNotFoundException($"El archivo de script SQL no se encuentra en la ruta: {scriptFilePath}");
        }
    }

    private static void ExecuteSqlScript(BancoDbContext context, string sqlScript)
    {
        using var connection = context.Database.GetDbConnection();
        if (connection.State != ConnectionState.Open)
        {
            connection.Open();
        }

        using var command = connection.CreateCommand();
        command.CommandText = sqlScript;
        command.ExecuteNonQuery();
    }
}