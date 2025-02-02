using System.Security.Claims;
using ProductManagement.Application.DTOs;
using ProductManagement.Domain.Entities;

namespace ProductManagement.Application.Interfaces;

public interface IProductService
{
    Task<ProductDto> CreateProductAsync(ProductCreateDto createDto, ClaimsPrincipal user);
    
    Task<IEnumerable<ProductDto>> GetAllAsync(ProductFilterDto filter, ClaimsPrincipal user);
    
    Task<IEnumerable<ProductDto>> GetAllAvailableProductsAsync(ProductFilterDto filter);
    Task<ProductDto?> GetProductByIdAsync(int id, ClaimsPrincipal user);
    
    Task<ProductDto?> GetAvailableProductsByIdAsync(int id);
    Task<bool> UpdateProductAsync(int id, ProductUpdateDto updateDto, ClaimsPrincipal user);
    Task<bool> DeleteProductAsync(int id, ClaimsPrincipal user);
    
    Task<bool> SoftDeleteByUserIdAsync(int userId, bool isDeleted);
}