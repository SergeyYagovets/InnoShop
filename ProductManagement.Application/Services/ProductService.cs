using System.Security.Claims;
using Mapster;
using ProductManagement.Application.DTOs;
using ProductManagement.Application.Interfaces;
using ProductManagement.Domain.Entities;
using ProductManagement.Domain.Exceptions;
using ProductManagement.Infrastructure.Interfaces;

namespace ProductManagement.Application.Services;

public class ProductService : IProductService
{
    private readonly IProductRepository _repository;
    
    public ProductService(IProductRepository productRepository)
    {
        _repository = productRepository;
    }
    
    public async Task<ProductDto> CreateProductAsync(ProductCreateDto createDto, ClaimsPrincipal user)
    {
        var userId = GetUserId(user);
        var product = createDto.Adapt<Product>();
        product.UserId = userId;

        await _repository.AddProductAsync(product);
        return product.Adapt<ProductDto>();
    }

    public async Task<IEnumerable<ProductDto>> GetAllAsync(ProductFilterDto filter, ClaimsPrincipal user)
    {
        int? userId = IsAdmin(user) ? null : GetUserId(user);
        var products = await _repository.SearchProductsAsync(filter.Name, filter.MinPrice, filter.MaxPrice, userId);
        
        return products.Adapt<List<ProductDto>>();
    }

    public async Task<IEnumerable<ProductDto>> GetAllAvailableProductsAsync(ProductFilterDto filter)
    {
        var products = await _repository.SearchAvailableProductsAsync(filter.Name, filter.MinPrice, filter.MaxPrice);
        return products.Adapt<List<ProductDto>>();
    }

    public async Task<ProductDto?> GetProductByIdAsync(int id, ClaimsPrincipal user)
    {
        var product = IsAdmin(user)
            ? await _repository.GetProductByIdAsync(id)
            : await _repository.GetUserProductByIdAsync(id, GetUserId(user));

        if (product == null)
            throw new NotFoundException($"Product with id {id} not found");

        return product.Adapt<ProductDto>();
    }

    public async Task<ProductDto?> GetAvailableProductsByIdAsync(int id)
    {
        var product = await _repository.GetAvailableProductsByIdAsync(id)
                      ?? throw new NotFoundException($"Product with id {id} not found");

        return product.Adapt<ProductDto>();
    }

    public async Task<bool> UpdateProductAsync(int id, ProductUpdateDto updateDto, ClaimsPrincipal user)
    {
        var product = await _repository.GetProductByIdAsync(id)
                      ?? throw new NotFoundException($"Product with id {id} not found");

        if (!IsAdmin(user) && product.UserId != GetUserId(user))
            throw new UnauthorizedAccessException("You don't have permission to update this product");

        updateDto.Adapt(product);
        await _repository.UpdateProductAsync(product);
        return true;
    }

    public async Task<bool> DeleteProductAsync(int id, ClaimsPrincipal user)
    {
        var product = await _repository.GetProductByIdAsync(id)
                      ?? throw new NotFoundException($"Product with id {id} not found");

        if (!IsAdmin(user) && product.UserId != GetUserId(user))
            throw new UnauthorizedAccessException("You don't have permission to delete this product");

        await _repository.DeleteProductAsync(product);
        return true;
    }

    public Task<bool> SoftDeleteByUserIdAsync(int userId,bool isDeleted)
    {
        return _repository.SoftDeleteByUserIdAsync(userId, isDeleted);
    }
    
    private static int GetUserId(ClaimsPrincipal user)
    {
        if (int.TryParse(user.FindFirst(ClaimTypes.NameIdentifier)?.Value, out int userId))
            return userId;

        throw new UnauthorizedAccessException("Authentication error. Couldn't identify the user");
    }

    private static bool IsAdmin(ClaimsPrincipal user)
    {
        return user.FindFirst(ClaimTypes.Role)?.Value == "Admin";
    }
}