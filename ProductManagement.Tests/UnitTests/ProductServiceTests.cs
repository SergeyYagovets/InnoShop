using System.Security.Claims;
using Mapster;
using Moq;
using ProductManagement.Application.DTOs;
using ProductManagement.Application.Services;
using ProductManagement.Domain.Entities;
using ProductManagement.Domain.Exceptions;
using ProductManagement.Infrastructure.Interfaces;

namespace ProductManagement.Tests.UnitTests;

public class ProductServiceTests
{
    private readonly Mock<IProductRepository> _mockRepository;
    private readonly ProductService _productService;

    public ProductServiceTests()
    {
        _mockRepository = new Mock<IProductRepository>();
        _productService = new ProductService(_mockRepository.Object);
    }

    [Fact]
    public async Task CreateProductAsync_ValidData_ReturnsProductDto()
    {
        // Arrange
        var createDto = new ProductCreateDto("Test product", "Test description", 100, true);

        var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "1")
        }));

        var product = createDto.Adapt<Product>();
        product.UserId = 1;

        _mockRepository.Setup(repo => repo.AddProductAsync(It.IsAny<Product>()))
            .Returns(Task.CompletedTask);

        // Act
        var result = await _productService.CreateProductAsync(createDto, user);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(createDto.Name, result.Name);
        Assert.Equal(createDto.Price, result.Price);
        Assert.Equal(createDto.Description, result.Description);
        Assert.Equal(createDto.IsAvailable, result.IsAvailable);
        _mockRepository.Verify(repo => repo.AddProductAsync(It.IsAny<Product>()), Times.Once);
    }

    [Fact]
    public async Task GetAllAsync_AdminUser_ReturnsAllProducts()
    {
        // Arrange
        var filter = new ProductFilterDto("Test", 50, 200);

        var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.Role, "Admin")
        }));

        var products = new List<Product>
        {
            new Product { Id = 1, Name = "Test Product 1", Price = 100, UserId = 1 },
            new Product { Id = 2, Name = "Test Product 2", Price = 150, UserId = 2 }
        };

        _mockRepository.Setup(repo => repo.SearchProductsAsync(filter.Name, filter.MinPrice, filter.MaxPrice, null))
            .ReturnsAsync(products);

        // Act
        var result = await _productService.GetAllAsync(filter, user);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task GetAllAsync_NonAdminUser_ReturnsUserProducts()
    {
        // Arrange
        var filter = new ProductFilterDto("Test", 50, 200);
        
        var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "1")
        }));

        var products = new List<Product>
        {
            new Product { Id = 1, Name = "Test Product 1", Price = 100, UserId = 1 }
        };

        _mockRepository.Setup(repo => repo.SearchProductsAsync(filter.Name, filter.MinPrice, filter.MaxPrice, 1))
            .ReturnsAsync(products);

        // Act
        var result = await _productService.GetAllAsync(filter, user);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
    }

    [Fact]
    public async Task GetAllAvailableProductsAsync_ReturnsAvailableProducts()
    {
        // Arrange
        var filter = new ProductFilterDto("Test", 50, 200);

        var products = new List<Product>
        {
            new Product { Id = 1, Name = "Test Product 1", Price = 100 },
            new Product { Id = 2, Name = "Test Product 2", Price = 150 }
        };

        _mockRepository.Setup(repo => repo.SearchAvailableProductsAsync(filter.Name, filter.MinPrice, filter.MaxPrice))
            .ReturnsAsync(products);

        // Act
        var result = await _productService.GetAllAvailableProductsAsync(filter);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task GetProductByIdAsync_AdminUser_ReturnsProduct()
    {
        // Arrange
        var productId = 1;
        var product = new Product { Id = productId, Name = "Test Product", Price = 100, UserId = 1 };

        var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.Role, "Admin")
        }));

        _mockRepository.Setup(repo => repo.GetProductByIdAsync(productId))
            .ReturnsAsync(product);

        // Act
        var result = await _productService.GetProductByIdAsync(productId, user);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(productId, result.Id);
    }

    [Fact]
    public async Task GetProductByIdAsync_NonAdminUser_ReturnsUserProduct()
    {
        // Arrange
        var productId = 1;
        var product = new Product { Id = productId, Name = "Test Product", Price = 100, UserId = 1 };

        var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "1")
        }));

        _mockRepository.Setup(repo => repo.GetUserProductByIdAsync(productId, 1))
            .ReturnsAsync(product);

        // Act
        var result = await _productService.GetProductByIdAsync(productId, user);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(productId, result.Id);
    }

    [Fact]
    public async Task GetProductByIdAsync_ProductNotFound_ThrowsNotFoundException()
    {
        // Arrange
        var productId = 1;

        var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "1")
        }));

        _mockRepository.Setup(repo => repo.GetUserProductByIdAsync(productId, 1))
            .ReturnsAsync((Product)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _productService.GetProductByIdAsync(productId, user));
    }

    [Fact]
    public async Task GetAvailableProductsByIdAsync_ReturnsProduct()
    {
        // Arrange
        var productId = 1;
        var product = new Product { Id = productId, Name = "Test Product", Price = 100 };

        _mockRepository.Setup(repo => repo.GetAvailableProductsByIdAsync(productId))
            .ReturnsAsync(product);

        // Act
        var result = await _productService.GetAvailableProductsByIdAsync(productId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(productId, result.Id);
    }

    [Fact]
    public async Task GetAvailableProductsByIdAsync_ProductNotFound_ThrowsNotFoundException()
    {
        // Arrange
        var productId = 1;

        _mockRepository.Setup(repo => repo.GetAvailableProductsByIdAsync(productId))
            .ReturnsAsync((Product)null);

        // Act & Assert
        await Assert.ThrowsAsync<NotFoundException>(() => _productService.GetAvailableProductsByIdAsync(productId));
    }

    [Fact]
    public async Task UpdateProductAsync_AdminUser_UpdatesProduct()
    {
        // Arrange
        var productId = 1;
        var updateDto = new ProductUpdateDto("Updated Product", "Updated Description", 150, false);

        var product = new Product { Id = productId, Name = "Test Product", Price = 100.0m, UserId = 1, IsAvailable = true};

        var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.Role, "Admin")
        }));

        _mockRepository.Setup(repo => repo.GetProductByIdAsync(productId))
            .ReturnsAsync(product);

        // Act
        var result = await _productService.UpdateProductAsync(productId, updateDto, user);

        // Assert
        Assert.True(result);
        _mockRepository.Verify(repo => repo.UpdateProductAsync(It.IsAny<Product>()), Times.Once);
    }

    [Fact]
    public async Task UpdateProductAsync_NonAdminUser_UnauthorizedAccessException()
    {
        // Arrange
        var productId = 1;
        var updateDto = new ProductUpdateDto("Updated Product", "Updated Description", 150, false);

        var product = new Product { Id = productId, Name = "Test Product", Price = 100, UserId = 2, IsAvailable = true};

        var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "1")
        }));

        _mockRepository.Setup(repo => repo.GetProductByIdAsync(productId))
            .ReturnsAsync(product);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _productService.UpdateProductAsync(productId, updateDto, user));
    }

    [Fact]
    public async Task DeleteProductAsync_AdminUser_DeletesProduct()
    {
        // Arrange
        var productId = 1;
        var product = new Product { Id = productId, Name = "Test Product", Price = 100, UserId = 1 };

        var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.Role, "Admin")
        }));

        _mockRepository.Setup(repo => repo.GetProductByIdAsync(productId))
            .ReturnsAsync(product);

        // Act
        var result = await _productService.DeleteProductAsync(productId, user);

        // Assert
        Assert.True(result);
        _mockRepository.Verify(repo => repo.DeleteProductAsync(It.IsAny<Product>()), Times.Once);
    }

    [Fact]
    public async Task DeleteProductAsync_NonAdminUser_UnauthorizedAccessException()
    {
        // Arrange
        var productId = 1;
        var product = new Product { Id = productId, Name = "Test Product", Price = 100, UserId = 2 };

        var user = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.NameIdentifier, "1")
        }));

        _mockRepository.Setup(repo => repo.GetProductByIdAsync(productId))
            .ReturnsAsync(product);

        // Act & Assert
        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _productService.DeleteProductAsync(productId, user));
    }
}