namespace VivesBankApi.Rest.Clients.storage.Config;

public class FileStorageConfig
{
    public string UploadDirectory { get; set; } = "uploads";
    public long MaxFileSize { get; set; } = 10 * 1024 * 1024;
    public List<string> AllowedFileTypes { get; set; } = [".jpg", ".jpeg", ".png"];
    public bool RemoveAll { get; set; } = false;
    public string SomeProperty { get; set; }
}