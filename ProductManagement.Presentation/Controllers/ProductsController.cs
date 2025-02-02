using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProductManagement.Application.DTOs;
using ProductManagement.Application.Interfaces;

namespace ProductManagement.Presentation.Controllers;

[Route("api/products")]
[ApiController]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;

    public ProductsController(IProductService productService)
    {
        _productService = productService;
    }
    
    [HttpPost("create-product")]
    [Authorize]
    public async Task<IActionResult> CreateProduct([FromBody] ProductCreateDto createDto)
    {
        var product = await _productService.CreateProductAsync(createDto, User);
        return CreatedAtAction(nameof(GetProductById), new { id = product.Id }, product);
    }
    
    [HttpGet("public/get-available-products")]
    public async Task<ActionResult<IEnumerable<ProductDto>>> GetAvailableProducts([FromQuery] ProductFilterDto filter)
    {
        var products = await _productService.GetAllAvailableProductsAsync(filter);
        return Ok(products);
    }
    
    [HttpGet("public/get-available-product{id}")]
    public async Task<ActionResult<ProductDto>> GetAvailableProductById(int id)
    {
        var product = await _productService.GetAvailableProductsByIdAsync(id);
        return Ok(product);
    }
    
    [HttpGet("get-current-user-products")]
    [Authorize]
    public async Task<ActionResult<IEnumerable<ProductDto>>> GetProducts([FromQuery] ProductFilterDto filter)
    {
        var products = await _productService.GetAllAsync(filter, User);
        return Ok(products);
    }
    
    [HttpGet("get-current-user-product{id}")]
    [Authorize]
    public async Task<ActionResult<ProductDto>> GetProductById(int id)
    {
        var product = await _productService.GetProductByIdAsync(id, User);
        return Ok(product);
    }
    
    [HttpPut("update-product{id}")]
    [Authorize]
    public async Task<IActionResult> UpdateProduct(int id, [FromBody] ProductUpdateDto updateDto)
    {
        await _productService.UpdateProductAsync(id, updateDto, User);
        return Ok("Product updated successfully");
    }
    
    [HttpDelete("delete-product{id}")]
    [Authorize]
    public async Task<IActionResult> DeleteProduct(int id)
    {
        await _productService.DeleteProductAsync(id, User);
        return Ok("Product deleted successfully");
    }

    [HttpPatch("soft-delete/{userId}")]
    [Authorize(Policy = "AdminOnly")]
    [ApiExplorerSettings(IgnoreApi = true)]
    public async Task<IActionResult> SoftDeleteByUser(int userId, [FromQuery] bool isDeleted)
    {
        await _productService.SoftDeleteByUserIdAsync(userId, isDeleted);
        return NoContent();
    }
}