using VivesBankApi.Rest.Product.Base.Dto;
using VivesBankApi.Rest.Product.Base.Exception;

namespace VivesBankApi.Rest.Product.Base.Mapper;
using Models;

public static class ProductMapper
{
    public static ProductResponse ToDtoResponse(this Product product)
    {
        return new ProductResponse
        {
            Id = product.Id,
            Name = product.Name,
            Type = product.ProductType.ToString(),
            CreatedAt = product.CreatedAt.ToString(),
            UpdatedAt = product.UpdatedAt.ToString()
        };
    }
    
    public static Product FromDtoRequest(this ProductCreateRequest createRequest)
    {
        if (string.IsNullOrWhiteSpace(createRequest.Type))
        {
            throw new ProductException.ProductInvalidTypeException("The Type field is required and cannot be null or empty.");
        }

        if (Enum.TryParse<Product.Type>(createRequest.Type.Trim(), true, out var productType))
        {
            
            var product = new Product(createRequest.Name, productType)
            {
                CreatedAt = DateTime.UtcNow, 
                UpdatedAt = DateTime.UtcNow, 
                IsDeleted = false 
            };

            return product;
        }
        else
        {
            throw new ProductException.ProductInvalidTypeException(
                $"Invalid Type: {createRequest.Type}. Valid values are: {string.Join(", ", Enum.GetNames(typeof(Product.Type)))}"
            );
        }
    }

}
