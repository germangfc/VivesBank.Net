namespace VivesBankApi.Rest.Product.Base.Exception;

/// <summary>
/// Excepciones personalizadas para manejo de errores relacionados con productos.
/// </summary>
/// <author>Raul Fernandez, Javier Hernandez, Samuel Cortes, German, Alvaro Herrero, Tomas</author>
public class ProductException : System.Exception
{
    /// <summary>
    /// Constructor para la clase base de las excepciones de producto.
    /// </summary>
    /// <param name="message">El mensaje de error que se proporcionará con la excepción.</param>
    public ProductException(string message) : base(message)
    {
    }

    /// <summary>
    /// Excepción lanzada cuando no se encuentra un producto.
    /// </summary>
    /// <author>Raul Fernandez, Javier Hernandez, Samuel Cortes, German, Alvaro Herrero, Tomas</author>
    /// <param name="id">ID del producto que no se encontró.</param>
    /// <remarks>Se utiliza cuando un producto no existe en la base de datos.</remarks>
    public class ProductNotFoundException : ProductException
    {
        public ProductNotFoundException(string id)
            : base($"The product with the ID {id} was not found")
        {
        }
    }

    /// <summary>
    /// Excepción lanzada cuando el tipo de producto es inválido.
    /// </summary>
    /// <author>Raul Fernandez, Javier Hernandez, Samuel Cortes, German, Alvaro Herrero, Tomas</author>
    /// <param name="invalidType">Tipo de producto inválido.</param>
    /// <remarks>Se utiliza cuando el tipo proporcionado no es uno de los tipos permitidos.</remarks>
    public class ProductInvalidTypeException : ProductException
    {
        public ProductInvalidTypeException(string invalidType)
            : base($"Invalid product type: {invalidType}")
        {
        }
    }
}
