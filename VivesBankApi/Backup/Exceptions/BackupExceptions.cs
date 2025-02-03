namespace VivesBankApi.Backup.Exceptions
{
    public abstract class BackupException : Exception
    {
        protected BackupException(string message) : base(message)
        {
        }

        public class BackupFileNotFoundException : BackupException
        {
            public BackupFileNotFoundException(string zipFilePath)
                : base($"{zipFilePath}")
            {
            }
        }

        public class BackupPermissionException : BackupException
        {
            public BackupPermissionException(string message, Exception innerException)
                : base("message, innerException")
            {
            }
        }
        
    }
}