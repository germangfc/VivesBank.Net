namespace VivesBankApi.Rest.Clients.storage.Config
{
    /// <summary>
    /// Clase de configuración para el almacenamiento remoto de archivos a través de FTP.
    /// </summary>
    public class FileStorageRemoteConfig
    {
        /// <summary>
        /// Obtiene o establece el host FTP al que se conectará.
        /// </summary>
        public string FtpHost { get; set; }

        /// <summary>
        /// Obtiene o establece el puerto FTP para la conexión.
        /// </summary>
        public int FtpPort { get; set; }

        /// <summary>
        /// Obtiene o establece el nombre de usuario para autenticarse en el servidor FTP.
        /// </summary>
        public string FtpUsername { get; set; }

        /// <summary>
        /// Obtiene o establece la contraseña para la autenticación en el servidor FTP.
        /// </summary>
        public string FtpPassword { get; set; }

        /// <summary>
        /// Obtiene o establece el directorio remoto donde se almacenarán los archivos en el servidor FTP.
        /// </summary>
        public string FtpDirectory { get; set; }

        /// <summary>
        /// Obtiene o establece los tipos de archivo permitidos para su carga en el servidor FTP.
        /// </summary>
        public string[] AllowedFileTypes { get; set; }

        /// <summary>
        /// Obtiene o establece el tamaño máximo permitido para los archivos que se suban al servidor FTP, en bytes.
        /// </summary>
        public long MaxFileSize { get; set; }

        /// <summary>
        /// Constructor vacío para la configuración remota de almacenamiento FTP.
        /// </summary>
        public FileStorageRemoteConfig()
        {
            // Establece valores predeterminados, si es necesario
            FtpPort = 21; // Puerto por defecto para FTP
            AllowedFileTypes = new string[] { ".jpg", ".jpeg", ".png" }; // Tipos de archivo por defecto
            MaxFileSize = 10 * 1024 * 1024; // Tamaño máximo predeterminado: 10 MB
        }
    }
}
