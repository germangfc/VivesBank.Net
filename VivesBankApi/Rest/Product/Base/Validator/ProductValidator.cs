using VivesBankApi.Rest.Product.Base.Dto;

namespace VivesBankApi.Rest.Product.Base.Validators;

/// <summary>
/// Valida un producto para asegurar que cumpla con las reglas definidas para su creación.
/// </summary>
/// <author>Raul Fernandez, Javier Hernandez, Samuel Cortes, German, Alvaro Herrero, Tomas</author>
public class ProductValidator
{
    /// <summary>
    /// Valida si la solicitud de creación de producto es válida.
    /// Las reglas de validación son:
    /// 1. El nombre del producto no puede ser nulo ni estar vacío.
    /// 2. El tipo del producto debe ser uno de los siguientes: "BankAccount" o "CreditCard".
    /// </summary>
    /// <param name="productRequest">La solicitud de creación del producto a validar.</param>
    /// <returns>Devuelve `true` si el producto es válido, de lo contrario devuelve `false`.</returns>
    public static bool isValidProduct(ProductCreateRequest productRequest)
    {
        // Verifica que el nombre no esté vacío ni nulo
        if (string.IsNullOrWhiteSpace(productRequest.Name))
        {
            return false;
        }

        // Tipos válidos de producto
        var validTypes = new[] { "BankAccount", "CreditCard" };

        // Verifica si el tipo del producto es uno de los tipos válidos
        if (!validTypes.Any(validType => string.Equals(validType, productRequest.Type, StringComparison.OrdinalIgnoreCase)))
        {
            return false;
        }

        return true;
    }
}