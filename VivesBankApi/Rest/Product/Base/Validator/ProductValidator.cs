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
    
            var validTypes = new[] { "BANKACCOUNT", "CREDITCARD" };
            if (!validTypes.Contains(productRequest.Type))
            {
                return false;
            }
    
            return true;
        }
}
