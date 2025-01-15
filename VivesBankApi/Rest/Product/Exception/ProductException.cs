namespace VivesBankApi.Rest.Product.Exception;

public class ProductException(string message) : System.Exception(message)
{
    public class ProductNotFoundException(string id)
        : ProductException($"The product with the ID {id} was not found");

    public class ProductInvalidTypeException(string invalidType)
        : ProductException($"Invalid product type: {invalidType}");
}