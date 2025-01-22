public class FileStorageExceptions : Exception
{
    public FileStorageExceptions(string message) : base(message)
    {
    }

    public FileStorageExceptions(string message, Exception innerException) : base(message, innerException)
    {
    }
}