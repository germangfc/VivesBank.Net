using Microsoft.AspNetCore.Mvc;
using VivesBankApi.Utils.Backup;
using System.Threading.Tasks;

namespace VivesBankApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BackupController : ControllerBase
    {
        private readonly BackupService _backupService;

        public BackupController(BackupService backupService)
        {
            _backupService = backupService;
        }

        [HttpPost("export")]
        public async Task<IActionResult> ExportToZip([FromBody] string zipFilePath)
        {
            await _backupService.ExportToZip(zipFilePath);
            return Ok(new { Message = "Backup exported successfully." });
        }

        [HttpPost("import")]
        public async Task<IActionResult> ImportFromZip([FromBody] string zipFilePath)
        {
            await _backupService.ImportFromZip(zipFilePath);
            return Ok(new { Message = "Backup imported successfully." });
        }
    }
}