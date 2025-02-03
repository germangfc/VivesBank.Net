namespace ApiFranfurkt.Properties.Currency.Exceptions
{
    /// <summary>
    /// Clase base para las excepciones relacionadas con la API de divisas.
    /// </summary>
    /// <author>Raul Fernandez, Javier Hernandez, Samuel Cortes, Alvaro Herrero, German, Tomas</author>
    /// <version>1.0</version>
    public abstract class CurrencyException : System.Exception
    {
        protected CurrencyException(string message) : base(message) { }

        protected CurrencyException(string message, Exception innerException)
            : base(message, innerException) { }

        public override string ToString()
        {
            return Message ?? "ERROR";
        }
    }

    /// <summary>
    /// Clase de excepción para manejar respuestas vacías de la API.
    /// </summary>
    public class CurrencyEmptyResponseException : CurrencyException
    {
        public CurrencyEmptyResponseException() : base("API response is empty") { }
    }

    /// <summary>
    /// Clase de excepción para manejar los errores de conexión con la API.
    /// </summary>
    public class CurrencyConnectionException : CurrencyException
    {
        public CurrencyConnectionException() : base("API connection failed") { }

        public CurrencyConnectionException(string message) : base(message) { }
    }

    /// <summary>
    /// Clase de excepción para manejar los errores inesperados de la API.
    /// </summary>
    public class CurrencyUnexpectedException : CurrencyException
    {
        public CurrencyUnexpectedException() : base("API unexpected error") { }

        public CurrencyUnexpectedException(string message) : base(message) { }

        public CurrencyUnexpectedException(string message, Exception innerException)
            : base(message, innerException) { }
    }
}