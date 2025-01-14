using VivesBankApi.Rest.Product.Dto;
using VivesBankApi.Rest.Product.Exception;
using VivesBankApi.Rest.Product.Models;


public static class ProductMapper
{
    public static ProductResponse ToDtoResponse(this Product product)
    {
        return new ProductResponse
        {
            Name = product.Name,
            Type = product.ProductType.ToString(),
            CreatedAt = product.CreatedAt.ToString(),
            UpdatedAt = product.UpdatedAt.ToString()
        };
    }
    
    public static Product FromDtoRequest(this ProductCreateRequest createRequest)
    {
        if (Enum.TryParse<Product.Type>(createRequest.Type, true, out var tipoProduct))
        {
            return new Product(
                createRequest.Name,
                tipoProduct);
        }
        else
        {
            throw new ProductException.ProductInvalidTypeException("Invalid Type");
        }
    }

}