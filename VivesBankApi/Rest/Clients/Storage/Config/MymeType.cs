namespace VivesBankApi.Rest.Clients.storage.Config
{
    /// <summary>
    /// Clase estática para obtener el tipo MIME correspondiente a una extensión de archivo.
    /// </summary>
    /// <author>Raul Fernandez, Javier Hernandez, Samuel Cortes, Alvaro Herrero, German, Tomas</author>
    /// <version>1.0</version>
    public static class MimeTypes
    {
        /// <summary>
        /// Diccionario que mapea extensiones de archivo a sus tipos MIME correspondientes.
        /// </summary>
        private static readonly Dictionary<string, string> MimeTypeMappings = new(StringComparer.InvariantCultureIgnoreCase)
        {
            { ".txt", "text/plain" },
            { ".pdf", "application/pdf" },
            { ".doc", "application/msword" },
            { ".docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document" },
            { ".xls", "application/vnd.ms-excel" },
            { ".xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" },
            { ".png", "image/png" },
            { ".jpg", "image/jpeg" },
            { ".jpeg", "image/jpeg" },
            { ".gif", "image/gif" }
        };

        /// <summary>
        /// Obtiene el tipo MIME para una extensión de archivo dada.
        /// Si la extensión no está registrada, devuelve "application/octet-stream".
        /// </summary>
        /// <param name="extension">La extensión del archivo (por ejemplo, ".jpg", ".txt").</param>
        /// <returns>El tipo MIME correspondiente o "application/octet-stream" si la extensión no está registrada.</returns>
        public static string GetMimeType(string extension)
        {
            // Verifica si la extensión existe en el diccionario y obtiene el tipo MIME correspondiente
            if (MimeTypeMappings.TryGetValue(extension, out var mimeType))
            {
                return mimeType;
            }

            // Devuelve un tipo MIME genérico si la extensión no está registrada
            return "application/octet-stream"; 
        }
    }
}
