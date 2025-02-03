using Microsoft.AspNetCore.Mvc;
using VivesBankApi.Utils.Backup;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using VivesBankApi.Backup;
using VivesBankApi.Backup.Service;
using Path = System.IO.Path;

/// <summary>
/// Controlador para la gestion de backups en la aplicacion.
/// Proporciona endpoints para exportar e importar backups en formato ZIP.
/// </summary>
/// <author>Raul Fernandez, Samuel Cortes, Javier Hernandez, Alvaro Herrrero, German, Tomas</author>
/// <version>1.0</version>
namespace VivesBankApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BackupController : ControllerBase
    {
        private readonly ILogger<BackupController> _logger;
        private readonly IBackupService _backupService;

        /// <summary>
        /// Constructor del controlador BackupController.
        /// </summary>
        /// <param name="backupService">Servicio de backup inyectado</param>
        public BackupController(IBackupService backupService)
        {
            _backupService = backupService;
        }

        /// <summary>
        /// Exporta un backup a un archivo ZIP.
        /// </summary>
        /// <param name="zipRequest">Datos requeridos para generar el backup</param>
        /// <returns>Ruta del archivo ZIP generado</returns>
        [HttpPost("export")]
        [Authorize("AdminPolicy")]
        public async Task<IActionResult> ExportToZip([FromBody] BackUpRequest zipRequest)
        {
            var zipFilePath = await _backupService.ExportToZip(zipRequest);

            if (!System.IO.File.Exists(zipFilePath))
            {
                return NotFound(new { Message = "No se pudo generar el archivo ZIP." });
            }

            return Ok(new { Message = "Backup exportado correctamente.", FilePath = zipFilePath });
        }

        /// <summary>
        /// Importa un backup desde un archivo ZIP.
        /// </summary>
        /// <param name="file">Archivo ZIP que contiene el backup</param>
        /// <returns>Resultado de la operacion de importacion</returns>
        [HttpPost("import")]
        [Authorize("AdminPolicy")]
        public async Task<IActionResult> ImportFromZip([FromForm] IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(new { Message = "Debe proporcionar un archivo ZIP valido." });
            }

            var tempFilePath = Path.Combine(Path.GetTempPath(), file.FileName);

            using (var stream = new FileStream(tempFilePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            await _backupService.ImportFromZip(new BackUpRequest { FilePath = tempFilePath });

            System.IO.File.Delete(tempFilePath);

            return Ok(new { Message = "Backup importado correctamente." });
        }
    }
}
