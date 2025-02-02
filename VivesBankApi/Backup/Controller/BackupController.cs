using Microsoft.AspNetCore.Mvc;
using VivesBankApi.Utils.Backup;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using VivesBankApi.Backup;
using VivesBankApi.Backup.Service;

namespace VivesBankApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    
    public class BackupController : ControllerBase
    {
        private readonly IBackupService _backupService;
        public BackupController(IBackupService backupService)
        {
            _backupService = backupService;
        }

        [HttpPost("export")]
        [Authorize("AdminPolicy")]

        public async Task<IActionResult> ExportToZip([FromBody] BackUpRequest zipFilePath)
        {
            await _backupService.ExportToZip(zipFilePath);
            return Ok(new { Message = "Backup exported successfully." });
        }

        [HttpPost("import")]
        [Authorize("AdminPolicy")]
        public async Task<IActionResult> ImportFromZip([FromBody] BackUpRequest zipFilePath)
        {
            await _backupService.ImportFromZip(zipFilePath);
            return Ok(new { Message = "Backup imported successfully." });
        }
    }
}