using System.ComponentModel.DataAnnotations;
using System.Reactive.Linq;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.FileSystemGlobbing.Abstractions;
using VivesBankApi.Rest.Product.Base.Dto;
using VivesBankApi.Rest.Product.Base.Exception;
using VivesBankApi.Rest.Product.Base.Mapper;
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
    [Authorize("AdminPolicy")]
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
    [Authorize("AdminPolicy")]
    public async Task<ActionResult<ProductResponse>> CreateProductAsync(ProductCreateRequest request)
    {
        _logger.LogInformation("Creating a new product");
        var producto = await _productService.CreateProductAsync(request);
        return CreatedAtAction(nameof(GetProductByIdAsync) , new { id = producto.Id }, producto);
    }

    [HttpPut("{productId}")]
    [Authorize("AdminPolicy")]
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
    [Authorize("AdminPolicy")]
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
    
    [HttpPost("csv")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> LoadProduct([Required] IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest("No file uploaded.");
        }

        using (var stream = file.OpenReadStream())
        {
            var products = _productService.LoadCsv(stream);
            return Ok(products);
        }
    }
    
    [HttpPost("import")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> ImportProductsFromJson([Required] IFormFile file)
    {
        if (file == null || file.Length == 0)
        {
            return BadRequest("No file uploaded.");
        }

        try
        {
            var products = await _productService.Import(file).ToList();
            return Ok(products);
        }
        catch (System.Exception ex)
        {
            return StatusCode(500, ex.Message);
        }
    }

    
    [HttpPost("export")]
    public async Task<IActionResult> ExportProductsToJson()
    {
        try
        {
            var productResponses = await _productService.GetAllProductsAsync();

            if (productResponses == null || !productResponses.Any())
            {
                return NoContent();
            }

            var products = productResponses.Select(pr => pr.FromDtoResponse()).ToList();

            var fileStream = await _productService.Export(products);

            return File(fileStream, "application/json", "products.json");
        }
        catch (System.Exception ex)
        {
            _logger.LogError($"Error exporting products: {ex.Message}");
            return StatusCode(500, new { message = "Error exporting products", details = ex.Message });
        }
    }
}