using VivesBankApi.Rest.Product.Base.Dto;
using VivesBankApi.Rest.Product.Base.Exception;
namespace VivesBankApi.Rest.Product.Base.Mapper;
using Models;

/// <summary>
/// Mapper for converting between Product and its DTO representations.
/// </summary>
/// <author>Raul Fernandez, Javier Hernandez, Samuel Cortes, German, Alvaro Herrero, Tomas</author>
public static class ProductMapper
{
    /// <summary>
    /// Converts a Product entity to a ProductResponse DTO.
    /// </summary>
    /// <param name="product">The product entity to convert.</param>
    /// <returns>A ProductResponse DTO representing the product.</returns>
    /// <exception cref="ProductException.ProductNotFoundException">Thrown if the product cannot be found.</exception>
    /// <author>Raul Fernandez, Javier Hernandez, Samuel Cortes, German, Alvaro Herrero, Tomas</author>
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

    /// <summary>
    /// Converts a ProductCreateRequest DTO to a Product entity.
    /// </summary>
    /// <param name="createRequest">The ProductCreateRequest DTO to convert.</param>
    /// <returns>A Product entity created based on the provided DTO.</returns>
    /// <exception cref="ProductException.ProductInvalidTypeException">Thrown if the product type is invalid or not found.</exception>
    /// <author>Raul Fernandez, Javier Hernandez, Samuel Cortes, German, Alvaro Herrero, Tomas</author>
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

    /// <summary>
    /// Converts a ProductResponse DTO back to a Product entity.
    /// </summary>
    /// <param name="productResponse">The ProductResponse DTO to convert.</param>
    /// <returns>A Product entity created from the provided DTO.</returns>
    /// <exception cref="ProductException.ProductInvalidTypeException">Thrown if the product type in the response is invalid.</exception>
    /// <author>Raul Fernandez, Javier Hernandez, Samuel Cortes, German, Alvaro Herrero, Tomas</author>
    public static Product FromDtoResponse(this ProductResponse productResponse)
    {
        if (Enum.TryParse<Product.Type>(productResponse.Type, true, out var productType))
        {
            return new Product(productResponse.Name, productType)
            {
                Id = productResponse.Id,
                CreatedAt = DateTime.Parse(productResponse.CreatedAt),
                UpdatedAt = DateTime.Parse(productResponse.UpdatedAt),
                IsDeleted = false
            };
        }
        else
        {
            throw new ProductException.ProductInvalidTypeException(
                $"Invalid Type: {productResponse.Type}. Valid values are: {string.Join(", ", Enum.GetNames(typeof(Product.Type)))}"
            );
        }
    }
}

