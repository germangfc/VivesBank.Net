namespace VivesBankApi.Backup.Service;

public interface IBackupService
{
    Task ImportFromZip(BackUpRequest zipFilePath);
    Task<string> ExportToZip(BackUpRequest zipRequest);
}