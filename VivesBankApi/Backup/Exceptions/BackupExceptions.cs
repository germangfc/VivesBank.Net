namespace VivesBankApi.Backup.Exceptions;
public abstract class BackupException : Exception
    {
        protected BackupException(string message) : base(message)
        {
        }

        public class BackupFileNotFoundException : BackupException
        {
            public BackupFileNotFoundException(string zipFilePath)
                : base($"The file {zipFilePath} was not found.") 
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

        public class BackupDirectoryNotFoundException : BackupException
        {
            public BackupDirectoryNotFoundException(string directoryPath)
                : base($"The directory {directoryPath} does not exist.")
            {
            }
        }
        
    }
