using System.Data;
using Microsoft.EntityFrameworkCore;

namespace VivesBankApi.Database
{
    /// <summary>
    /// Clase responsable de la inicialización de la base de datos.
    /// Aplica migraciones y ejecuta scripts SQL adicionales al inicializar la base de datos.
    /// </summary>
    /// <author>Raul Fernandez, Javier Hernandez, Samuel Cortes, Alvaro Herrero, German, Tomas</author>
    /// <version>1.0</version>
    public static class DatabseInitializer
    {
        /// <summary>
        /// Inicializa la base de datos aplicando migraciones y ejecutando un script SQL si existe.
        /// </summary>
        /// <param name="serviceProvider">Proveedor de servicios para obtener el contexto de la base de datos.</param>
        /// <param name="scriptFilePath">Ruta del archivo de script SQL a ejecutar.</param>
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

        /// <summary>
        /// Ejecuta un script SQL sobre la base de datos.
        /// </summary>
        /// <param name="context">Contexto de la base de datos.</param>
        /// <param name="sqlScript">Script SQL que se va a ejecutar.</param>
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
}
