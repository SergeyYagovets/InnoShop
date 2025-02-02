using ProductManagement.Domain.Entities;

namespace ProductManagement.Infrastructure.Interfaces;

public interface IProductRepository
{
    Task<IEnumerable<Product>> GetProductsAsync();
    Task<Product?> GetProductByIdAsync(int id);
    
    Task<Product?> GetAvailableProductsByIdAsync(int id);
    
    Task<Product?> GetUserProductByIdAsync(int id, int userId);
    Task<IEnumerable<Product>> SearchProductsAsync(string? name, decimal? minPrice, decimal? maxPrice, int? userId);
    
    Task<IEnumerable<Product>> SearchAvailableProductsAsync(string? name, decimal? minPrice, decimal? maxPrice);
    Task AddProductAsync(Product product);
    Task UpdateProductAsync(Product product);
    Task DeleteProductAsync(Product product);
    
    Task<bool> SoftDeleteByUserIdAsync(int userId, bool isDeleted);
}