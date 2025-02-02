namespace VivesBankApi.Backup.Service;

public interface IBackupService {
    Task importFromZip(IFormFile zipFile);
    Task exportToZip(FileInfo zipFile);
}