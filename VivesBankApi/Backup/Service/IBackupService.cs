namespace VivesBankApi.Backup.Service;

/// <summary>
/// Interfaz para la gestion de backups.
/// Define metodos para importar y exportar backups en formato ZIP.
/// </summary>
/// <author>Raul Fernandez, Samuel Cortes, Javier Hernandez, Alvaro Herrero, German, Tomas</author>
public interface IBackupService
{
    /// <summary>
    /// Importa un backup desde un archivo ZIP.
    /// </summary>
    /// <param name="zipFilePath">Solicitud de backup con la ruta del archivo ZIP.</param>
    Task ImportFromZip(BackUpRequest zipFilePath);

    /// <summary>
    /// Exporta un backup a un archivo ZIP.
    /// </summary>
    /// <param name="zipRequest">Solicitud de backup con los datos requeridos.</param>
    /// <returns>Ruta del archivo ZIP generado.</returns>
    Task<string> ExportToZip(BackUpRequest zipRequest);
}