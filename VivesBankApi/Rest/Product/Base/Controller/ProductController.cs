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

namespace VivesBankApi.Rest.Product.Base.Controller
{
    /// <summary>
    /// Controlador para gestionar los productos en la API.
    /// </summary>
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

        /// <summary>
        /// Obtiene todos los productos.
        /// </summary>
        /// <returns>Lista de productos.</returns>
        [HttpGet]
        public async Task<ActionResult<List<ProductResponse>>> GetAllProductsAsync()
        {
            _logger.LogInformation("Obteniendo todos los productos");
            var products = await _productService.GetAllProductsAsync();
            return Ok(products);
        }
        
        /// <summary>
        /// Obtiene un producto por su ID.
        /// </summary>
        /// <param name="productId">ID del producto.</param>
        /// <returns>Producto solicitado.</returns>
        [HttpGet("{productId}")]
        [Authorize("AdminPolicy")]
        public async Task<ActionResult<ProductResponse>> GetProductByIdAsync(string productId)
        {
            _logger.LogInformation($"Obteniendo producto con ID {productId}");

            var result = await _productService.GetProductByIdAsync(productId);

            if (result == null)
            {
                _logger.LogWarning($"Producto con ID {productId} no encontrado.");
                return NotFound(); 
            }

            return Ok(result);
        }

        /// <summary>
        /// Crea un nuevo producto.
        /// </summary>
        /// <param name="request">Detalles del producto a crear.</param>
        /// <returns>Producto creado.</returns>
        [HttpPost]
        [Authorize("AdminPolicy")]
        public async Task<ActionResult<ProductResponse>> CreateProductAsync(ProductCreateRequest request)
        {
            _logger.LogInformation("Creando un nuevo producto");
            var producto = await _productService.CreateProductAsync(request);
            return CreatedAtAction(nameof(GetProductByIdAsync), new { id = producto.Id }, producto);
        }

        /// <summary>
        /// Actualiza un producto existente.
        /// </summary>
        /// <param name="productId">ID del producto a actualizar.</param>
        /// <param name="request">Detalles actualizados del producto.</param>
        /// <returns>Producto actualizado.</returns>
        [HttpPut("{productId}")]
        [Authorize("AdminPolicy")]
        public async Task<ActionResult<ProductResponse>> UpdateProductAsync(string productId, ProductUpdateRequest request)
        {
            _logger.LogInformation($"Actualizando producto con ID {productId}");
        
            var product = await _productService.UpdateProductAsync(productId, request);
            if (product == null)
            {
                _logger.LogWarning($"Producto con ID {productId} no encontrado");
                return NotFound();
            }

            return CreatedAtAction(nameof(GetProductByIdAsync), new { id = product.Id }, product);
        }

        /// <summary>
        /// Elimina un producto por su ID.
        /// </summary>
        /// <param name="productId">ID del producto a eliminar.</param>
        /// <returns>Resultado de la eliminación del producto.</returns>
        [HttpDelete("{productId}")]
        [Authorize("AdminPolicy")]
        public async Task<IActionResult> DeleteProductAsync(string productId)
        {
            _logger.LogInformation($"Eliminando producto con ID {productId}");

            var wasDeleted = await _productService.DeleteProductAsync(productId); 
            if (!wasDeleted) 
            {
                _logger.LogWarning($"Producto con ID {productId} no encontrado");
                return NotFound();
            }

            return NoContent();
        }
        
        /// <summary>
        /// Carga un archivo CSV con productos.
        /// </summary>
        /// <param name="file">Archivo CSV con productos.</param>
        /// <returns>Productos cargados.</returns>
        [HttpPost("csv")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> LoadProduct([Required] IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("No se ha subido ningún archivo.");
            }

            using (var stream = file.OpenReadStream())
            {
                var products = _productService.LoadCsv(stream);
                return Ok(products);
            }
        }
        
        /// <summary>
        /// Importa productos desde un archivo JSON.
        /// </summary>
        /// <param name="file">Archivo JSON con productos.</param>
        /// <returns>Productos importados.</returns>
        [HttpPost("import")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> ImportProductsFromJson([Required] IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("No se ha subido ningún archivo.");
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

        /// <summary>
        /// Exporta los productos a un archivo JSON.
        /// </summary>
        /// <returns>Archivo JSON con productos exportados.</returns>
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
                _logger.LogError($"Error al exportar productos: {ex.Message}");
                return StatusCode(500, new { message = "Error al exportar productos", details = ex.Message });
            }
        }
    }
}
