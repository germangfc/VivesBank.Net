using System.Text;
using ApiFunkosCS.Utils.DevApplyMigrations;
using ApiFunkosCS.Utils.ExceptionMiddleware;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;
using Serilog;
using Serilog.Core;
using VivesBankApi.Database;
using VivesBankApi.Rest.Movimientos.Config;

Console.OutputEncoding = Encoding.UTF8; // Configura la codificación de salida de la consola a UTF-8 para mostrar caracteres especiales.

var environment = InitLocalEnvironment(); // Inicializa y obtiene el entorno de ejecución actual de la aplicación.

var configuration = InitConfiguration(); // Construye y obtiene la configuración de la aplicación desde archivos JSON.

var logger = InitLogConfig(); // Inicializa y configura el logger de Serilog para registrar eventos y mensajes.

var builder = InitServices(); // Configura y obtiene un WebApplicationBuilder con servicios necesarios.

builder.Services.AddControllers(); // Agrega soporte para controladores, permitiendo manejar solicitudes HTTP.

builder.Services.AddEndpointsApiExplorer(); // Agrega servicios para explorar los endpoints de la API, necesario para Swagger.

var app = builder.Build(); // Construye la aplicación web a partir del WebApplicationBuilder.

if (app.Environment.IsDevelopment()) // Verifica si el entorno es de desarrollo.
{
    app.UseSwagger(); // Habilita Swagger para generar documentación de la API.
    app.UseSwaggerUI(); // Habilita Swagger UI para explorar y probar la API visualmente.
}

app.ApplyMigrations(); // Aplica las migraciones de la base de datos si es necesario.

//StorageInit(); // Inicializa el almacenamiento de archivos

app.UseMiddleware<GlobalExceptionMiddleware>(); // Agrega el middleware de manejo de excepciones globales para loguear y manejar errores.

app.MapGraphQL(); // Habilita GraphQL para permitir la ejecución de consultas y mutaciones GraphQL.

app.UseHttpsRedirection(); // Redirige automáticamente las solicitudes HTTP a HTTPS para mejorar la seguridad.

app.UseRouting(); // Habilita el enrutamiento para dirigir las solicitudes HTTP a los controladores correspondientes.

app.UseAuthorization(); // Habilita la autorización para asegurar el acceso a recursos protegidos.

app.MapControllers(); // Mapea las rutas de los controladores a los endpoints de la aplicación.

logger.Information("🚀 Banco API started 🟢"); // Registra un mensaje informativo indicando que la API ha iniciado.
Console.WriteLine("🚀 Banco API started 🟢"); // Muestra un mensaje en la consola indicando que la API ha iniciado.

app.Run(); // Inicia la aplicación y comienza a escuchar las solicitudes HTTP entrantes.

string InitLocalEnvironment()
{
    Console.OutputEncoding = Encoding.UTF8; // Necesario para mostrar emojis
    var myEnvironment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "";
    Console.WriteLine($"Environment: {myEnvironment}");
    return myEnvironment;
}

IConfiguration InitConfiguration()
{
    var myConfiguration = new ConfigurationBuilder()
        .SetBasePath(Directory.GetCurrentDirectory())
        .AddJsonFile("appsettings.json", false, true)
        .AddJsonFile($"appsettings.{environment}.json", true)
        .Build();
    return myConfiguration;
}

Logger InitLogConfig()
{
    // Creamos un logger con la configuración de Serilog
    return new LoggerConfiguration()
        .ReadFrom.Configuration(configuration)
        .CreateLogger();
}

WebApplicationBuilder InitServices()
{
    
    var myBuilder = WebApplication.CreateBuilder(args);
    
    
    myBuilder.Services.AddLogging(logging =>
    {
        logging.ClearProviders(); // Limpia los proveedores de log por defecto
        logging.AddSerilog(logger, true); // Añade Serilog como un proveedor de log
    });
    logger.Debug("Serilog added as default logger");


    myBuilder.Services.AddMemoryCache(
        options => options.ExpirationScanFrequency = TimeSpan.FromSeconds(30)
        );

    
    /**************** BANCO POSTGRESQL DATABASE SETTINGS **************/
    myBuilder.Services.AddDbContext<BancoDbContext>(options =>
    {
        var connectionString = configuration.GetSection("PostgreSQLDataBase:ConnectionString")?.Value 
                               ?? throw new InvalidOperationException("Database connection string not found");
        options.UseNpgsql(connectionString)
            .EnableSensitiveDataLogging(); // Habilita el registro de datos sensibles
        Console.WriteLine("PostgreSQL database connected 🟢");
    });

    /*********************************************************/
    
    /**************** MONGO MOVIMIENTOS DATABASE SETTINGS **************/
     myBuilder.Services.Configure<MongoDatabaseConfig>(
         myBuilder.Configuration.GetSection("MongoDataBase"));
    /*********************************************************/
    


/**************** INYECCION DE DEPENDENCIAS **************/
// REPOSITORIO Y SERVICIOS

// // FUNKO
//     myBuilder.Services.AddScoped<IFunkoRepository, FunkoRepository>(); 
//     myBuilder.Services.AddScoped<IFunkoService, FunkoService>();
//
// // CATEGORIA
//     myBuilder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
//     myBuilder.Services.AddScoped<ICategoryService, CategoryService>();
//     
// // LOCAL STORAGE
//     var storageConfig = myBuilder.Configuration
//         .GetSection("FileStorage")
//         .Get<StorageConfig>();
//
//     myBuilder.Services.AddSingleton(storageConfig);
//     myBuilder.Services.AddScoped<IStorageService, LocalStorageService>();
//     
// // CSV CATEGORY STORAGE
//     myBuilder.Services.AddScoped<ICategoryStorageImportCsv, CategoryStorageImportCsv>();
//     
// // JSON CATEGORY STORAGE
//     myBuilder.Services.AddScoped<ICategoryStorageImportJson, CategoryStorageImportJson>();
//     
// // CSV FUNKO STORAGE
//     myBuilder.Services.AddScoped<IFunkoStorageImportCsv, FunkoStorageImportCsv>();
/*********************************************************/

/****************  DOCUMENTACION DE SWAGGER **************/
    myBuilder.Services.AddSwaggerGen(c =>
    {
        c.EnableAnnotations();
        // Otros metadatos de la API
        c.SwaggerDoc("v1", new OpenApiInfo
        {
            Version = "v1",
            Title = "Banco API",
            Description = "An api where you can have all the basic functionality of a bank",
            Contact = new OpenApiContact
            {
                Name = "Álvaro Herrero, Javier Hernández, Raúl Fernandez, Samuel Cortés, German Fernández, Diego",
                Url = new Uri("https://github.com/Javierhvicente/VivesBank.Net")
            },
        });
    }); 
/*********************************************************/

/*************************** GRAPHQL SETTINGS **************/

    // myBuilder.Services
    //     .AddGraphQLServer()
    //     .AddQueryType<QueryFunko>()
    //     .AddQueryType<CategoryQuery>()
    //     .AddFiltering()
    //     .AddSorting();

return myBuilder;
}

// void StorageInit()
// {
//     logger.Debug("Initializing file storage");
//     var fileStorageConfig = configuration.GetSection("FileStorage").Get<StorageConfig>();
//     Directory.CreateDirectory(fileStorageConfig.UploadDirectory);
//     if (fileStorageConfig.RemoveAll)
//     {
//         logger.Debug("Removing all files in the storage directory");
//         foreach (var file in Directory.GetFiles(fileStorageConfig.UploadDirectory))
//             File.Delete(file);
//     }
//
//     logger.Information("🟢 File storage initialized successfully!");
// }