namespace VivesBankApi.Rest.Clients.storage.Config;

public class FileStorageRemoteConfig
{
    public string FtpHost { get; set; }
    
    public int FtpPort { get; set; }
    public string FtpUsername { get; set; }
    public string FtpPassword { get; set; }
    public string FtpDirectory { get; set; }
    public string[] AllowedFileTypes { get; set; }
    public long MaxFileSize { get; set; }
    
}