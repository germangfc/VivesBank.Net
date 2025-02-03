/// <summary>
/// Clase que representa una solicitud de backup.
/// Contiene la ruta del archivo ZIP utilizado para la exportacion o importacion del backup.
/// </summary>
/// <author>Raul Fernandez, Samuel Cortes, Javier Hernandez, Alvaro Herrero, German, Tomas</author>
namespace VivesBankApi.Backup;

using System.ComponentModel.DataAnnotations;

public class BackUpRequest
{
    /// <summary>
    /// Ruta del archivo ZIP.
    /// Este campo es obligatorio.
    /// </summary>
    [Required]
    public string FilePath { get; set; }
}