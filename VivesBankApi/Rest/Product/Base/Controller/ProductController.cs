using Microsoft.AspNetCore.Mvc;
using VivesBankApi.Rest.Product.Base.Dto;
using VivesBankApi.Rest.Product.Base.Exception;
using VivesBankApi.Rest.Product.Service;

namespace VivesBankApi.Rest.Product.Base.Controller;

[ApiController]
[Route("api/[controller]")]
public class ProductController : ControllerBase
{
    private readonly IProductService _productService;
    private readonly ILogger _logger;
    
    public ProductController(IProductService productService, ILogger<ProductController> logger)
    {
        _productService = productService;
        _logger = logger;
    }

    [HttpGet]
    public async Task<ActionResult<List<ProductResponse>>> GetAllProductsAsync()
    {
        _logger.LogInformation("Getting all products");
        var products = await _productService.GetAllProductsAsync();
        return Ok(products);
    }
    
    [HttpGet("{productId}")]
    public async Task<ActionResult<ProductResponse>> GetProductByIdAsync(string productId)
    {
        _logger.LogInformation($"Getting product with id {productId}");

        var result = await _productService.GetProductByIdAsync(productId);

        if (result == null)
        {
            _logger.LogWarning($"Product with id {productId} was not found.");
            return NotFound(); 
        }

        return Ok(result);
    }

    [HttpPost]
    public async Task<ActionResult<ProductResponse>> CreateProductAsync(ProductCreateRequest request)
    {
        _logger.LogInformation("Creating a new product");
        var producto = await _productService.CreateProductAsync(request);
        return CreatedAtAction(nameof(GetProductByIdAsync) , new { id = producto.Id }, producto);
    }

    [HttpPut("{productId}")]
    public async Task<ActionResult<ProductResponse>> UpdateProductAsync(string productId, ProductUpdateRequest request)
    {
        _logger.LogInformation($"Updating product with id {productId}");
    
        var product = await _productService.UpdateProductAsync(productId, request);
        if (product == null)
        {
            _logger.LogWarning($"Product with id {productId} not found");
            return NotFound();
        }

        return CreatedAtAction(nameof(GetProductByIdAsync), new { id = product.Id }, product);
    }

    [HttpDelete("{productId}")]
    public async Task<IActionResult> DeleteProductAsync(string productId)
    {
        _logger.LogInformation($"Deleting product with id {productId}");

        var wasDeleted = await _productService.DeleteProductAsync(productId); 
        if (!wasDeleted) 
        {
            _logger.LogWarning($"Product with id {productId} not found");
            return NotFound();
        }

        return NoContent();
    }
    
    

}