namespace VivesBankApi.Rest.Clients.storage.Config;

public class FileStorageRemoteConfig
{
    public long MaxFileSize { get; set; }
    public string[] AllowedFileTypes { get; set; }
    public string FtpHost { get; set; }
    public int FtpPort { get; set; }
    public string FtpUsername { get; set; }
    public string FtpPassword { get; set; }
    public string FtpDirectory { get; set; }
}