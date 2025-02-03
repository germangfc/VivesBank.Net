namespace VivesBankApi.Rest.Movimientos.Config
{
    /// <summary>
    /// Configuración para la conexión a una base de datos MongoDB.
    /// </summary>
    /// <remarks>
    /// Esta clase encapsula la configuración necesaria para conectarse a una base de datos MongoDB,
    /// especificando la cadena de conexión, el nombre de la base de datos y los nombres de las colecciones.
    /// </remarks>
    public class MongoDatabaseConfig
    {
        /// <summary>
        /// Cadena de conexión a la base de datos MongoDB.
        /// </summary>
        /// <remarks>
        /// Este valor se utiliza para conectar con la base de datos MongoDB y debe seguir el formato estándar de conexión
        /// de MongoDB, como: "mongodb://<usuario>:<contraseña>@<servidor>:<puerto>/<base_de_datos>".
        /// </remarks>
        public string ConnectionString { get; set; } = null!;

        /// <summary>
        /// Nombre de la base de datos MongoDB.
        /// </summary>
        /// <remarks>
        /// Especifica el nombre de la base de datos dentro del servidor MongoDB con la que interactuará la aplicación.
        /// </remarks>
        public string DatabaseName { get; set; } = null!;

        /// <summary>
        /// Nombre de la colección de domiciliaciones en MongoDB.
        /// </summary>
        /// <remarks>
        /// Define el nombre de la colección que se usará para almacenar la información de domiciliaciones
        /// dentro de la base de datos MongoDB.
        /// </remarks>
        public string DomiciliacionCollectionName { get; set; } = null!;

        /// <summary>
        /// Nombre de la colección de movimientos en MongoDB.
        /// </summary>
        /// <remarks>
        /// Define el nombre de la colección que se usará para almacenar los movimientos dentro de la base de datos MongoDB.
        /// </remarks>
        public string MovimientosCollectionName { get; set; } = null!;
    }
}
