using Microsoft.EntityFrameworkCore;
using ProductManagement.Domain.Entities;
using ProductManagement.Infrastructure.Data;
using ProductManagement.Infrastructure.Interfaces;

namespace ProductManagement.Infrastructure.Repositories;

public class ProductRepository : IProductRepository
{
    private readonly ApplicationDbContext _context;
    
    public ProductRepository(ApplicationDbContext context)
    {
        _context = context;
    }
    
    public async Task<IEnumerable<Product>> GetProductsAsync() => 
        await _context.Products.ToListAsync();

    public async Task<Product?> GetProductByIdAsync(int id) =>
        await _context.Products.FirstOrDefaultAsync(p => p.Id == id);
    
    public async Task<Product?> GetAvailableProductsByIdAsync(int id) =>
        await _context.Products.FirstOrDefaultAsync(p => p.IsAvailable == true && p.IsDeleted == false && p.Id == id);

    public async Task<Product?> GetUserProductByIdAsync(int id, int userId) =>
        await _context.Products.FirstOrDefaultAsync(p => p.Id == id && p.UserId == userId);
    

    public async Task<IEnumerable<Product>> SearchProductsAsync(string? name, decimal? minPrice, decimal? maxPrice, int? userId)
    {
        var query = _context.Products.AsQueryable();

        if (userId.HasValue)
            query = query.Where(p => p.UserId == userId);

        if (!string.IsNullOrWhiteSpace(name))
            query = query.Where(p => p.Name.Contains(name));

        if (minPrice.HasValue)
            query = query.Where(p => p.Price >= minPrice);

        if (maxPrice.HasValue)
            query = query.Where(p => p.Price <= maxPrice);

        return await query.ToListAsync();
    }

    public async Task<IEnumerable<Product>> SearchAvailableProductsAsync(string? name, decimal? minPrice, decimal? maxPrice)
    {
        var query = _context.Products.Where(p => p.IsAvailable == true && p.IsDeleted == false).AsQueryable();

        if (!string.IsNullOrWhiteSpace(name))
            query = query.Where(p => p.Name.Contains(name));

        if (minPrice.HasValue)
            query = query.Where(p => p.Price >= minPrice);

        if (maxPrice.HasValue)
            query = query.Where(p => p.Price <= maxPrice);

        return await query.ToListAsync();
    }

    public async Task AddProductAsync(Product product)
    {
        await _context.Products.AddAsync(product);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateProductAsync(Product product)
    {
        _context.Products.Update(product);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteProductAsync(Product product)
    {
        _context.Products.Remove(product);
        await _context.SaveChangesAsync();
    }

    public async Task<bool> SoftDeleteByUserIdAsync(int userId, bool isDeleted)
    {
        var products = await _context.Products
            .Where(p => p.UserId == userId)
            .ToListAsync();

        if (!products.Any()) return false;

        foreach (var product in products)
        {
            product.IsDeleted = isDeleted;
        }

        await _context.SaveChangesAsync();
        return true;
    }
}