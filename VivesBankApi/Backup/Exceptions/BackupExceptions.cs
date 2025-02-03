
﻿namespace VivesBankApi.Backup.Exceptions;
/// <summary>
/// Clase base para excepciones relacionadas con el proceso de backup.
/// Define excepciones especificas para errores comunes en la gestion de backups.
/// </summary>
/// <author>Raul Fernandez, Samuel Cortes, Javier Hernandez, Alvaro Herrero, German, Tomas</author>
public abstract class BackupException : Exception
    {
        /// <summary>
        /// Constructor protegido para inicializar la excepcion con un mensaje especifico.
        /// </summary>
        /// <param name="message">Mensaje descriptivo del error.</param>
        protected BackupException(string message) : base(message)
        {
        }

        /// <summary>
        /// Excepcion que se lanza cuando el archivo de backup no se encuentra.
        /// </summary>
        public class BackupFileNotFoundException : BackupException
        {
            /// <summary>
            /// Constructor que inicializa la excepcion con el nombre del archivo que no se encontro.
            /// </summary>
            /// <param name="zipFilePath">Ruta del archivo ZIP no encontrado.</param>
            public BackupFileNotFoundException(string zipFilePath)
                : base($"{zipFilePath}")
            {
            }
        }

        /// <summary>
        /// Excepcion que se lanza cuando hay un problema de permisos durante el proceso de backup.
        /// </summary>
        public class BackupPermissionException : BackupException
        {
            /// <summary>
            /// Constructor que inicializa la excepcion con un mensaje y una excepcion interna.
            /// </summary>
            /// <param name="message">Mensaje descriptivo del error.</param>
            /// <param name="innerException">Excepcion interna que causo el error.</param>
            public BackupPermissionException(string message, Exception innerException)
                : base("message, innerException")
                : base("message, innerException") 
            {
            }
        }

        /// <summary>
        /// Excepcion que se lanza cuando el directorio de backup no existe.
        /// </summary>
        public class BackupDirectoryNotFoundException : BackupException
        {
            /// <summary>
            /// Constructor que inicializa la excepcion con el nombre del directorio que no se encontro.
            /// </summary>
            /// <param name="directoryPath">Ruta del directorio no encontrado.</param>
            public BackupDirectoryNotFoundException(string directoryPath)
                : base($"The directory {directoryPath} does not exist.")
            {
            }
        }
        
    }
}