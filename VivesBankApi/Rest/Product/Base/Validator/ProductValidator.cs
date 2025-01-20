using VivesBankApi.Rest.Product.Base.Dto;

namespace VivesBankApi.Rest.Product.Base.Validators;

public class ProductValidator
{
    public static bool isValidProduct(ProductCreateRequest productRequest)
    {
        if (string.IsNullOrWhiteSpace(productRequest.Name))
        {
            return false;
        }

        var validTypes = new[] { "BankAccount", "CreditCard" };

        if (!validTypes.Any(validType => string.Equals(validType, productRequest.Type, StringComparison.OrdinalIgnoreCase)))
        {
            return false;
        }

        return true;
    }
}