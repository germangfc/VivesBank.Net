using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc;
using VivesBankApi.Utils.Backup;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using VivesBankApi.Backup;
using VivesBankApi.Backup.Service;
using Path = System.IO.Path;

namespace VivesBankApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BackupController : ControllerBase
    {
        
        private readonly ILogger<BackupController> _logger;
        private readonly IBackupService _backupService;
        public BackupController(IBackupService backupService)
        {
            _backupService = backupService;
        }

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

        
        [HttpPost("import")]
        [Authorize("AdminPolicy")]
        public async Task<IActionResult> ImportFromZip([Required] IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(new { Message = "Debe proporcionar un archivo ZIP válido." });
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