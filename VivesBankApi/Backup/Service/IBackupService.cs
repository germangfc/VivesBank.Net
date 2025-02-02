namespace VivesBankApi.Backup.Service;

public interface IBackupService {
    Task ImportFromZipAsync(FileInfo zipFile);
    Task ExportToZipAsync(FileInfo zipFile);
}