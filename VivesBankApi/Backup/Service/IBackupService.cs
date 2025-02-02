namespace VivesBankApi.Backup.Service;

public interface IBackupService
{
    Task ImportFromZip(BackUpRequest zipFilePath);
    Task ExportToZip(BackUpRequest zipFilePath);
}