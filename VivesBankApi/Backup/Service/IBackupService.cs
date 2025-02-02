namespace VivesBankApi.Backup.Service;

public interface IBackupService
{
    Task ImportFromZip(string zipFilePath);
    Task ExportToZip(string zipFilePath);
}