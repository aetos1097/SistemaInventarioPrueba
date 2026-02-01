using FluentAssertions;
using Moq;
using ProductsSales.Application.DTOs;
using ProductsSales.Application.Interfaces;
using ProductsSales.Application.Services;
using ProductsSales.Domain.Entities;
using Xunit;

namespace ProductsSales.Tests.Services;

public class ProductServiceTests
{
    private readonly Mock<IProductRepository> _mockProductRepo;
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly ProductService _service;

    public ProductServiceTests()
    {
        _mockProductRepo = new Mock<IProductRepository>();
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _service = new ProductService(_mockProductRepo.Object, _mockUnitOfWork.Object);
    }

    [Fact]
    public async Task GetAllProductsAsync_ShouldReturnAllProducts()
    {
        // Arrange
        var products = new List<Product>
        {
            CreateProduct(Guid.NewGuid(), "Product 1", 100, 10),
            CreateProduct(Guid.NewGuid(), "Product 2", 200, 20)
        };

        _mockProductRepo.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(products);

        // Act
        var result = await _service.GetAllProductsAsync();

        // Assert
        result.Should().HaveCount(2);
        result.Should().Contain(p => p.Name == "Product 1");
    }

    [Fact]
    public async Task GetProductByIdAsync_ExistingId_ShouldReturnProduct()
    {
        // Arrange
        var id = Guid.NewGuid();
        var product = CreateProduct(id, "Test Product", 100, 10);
        _mockProductRepo.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(product);

        // Act
        var result = await _service.GetProductByIdAsync(id);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Test Product");
        result.Price.Should().Be(100);
    }

    [Fact]
    public async Task GetProductByIdAsync_NonExistingId_ShouldReturnNull()
    {
        // Arrange
        var id = Guid.NewGuid();
        _mockProductRepo.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync((Product?)null);

        // Act
        var result = await _service.GetProductByIdAsync(id);

        // Assert
        result.Should().BeNull();
    }

    [Fact]
    public async Task CreateProductAsync_ValidDto_ShouldReturnCreatedProduct()
    {
        // Arrange
        var createDto = new CreateProductDto("Nuevo Producto", 150, 5, null);
        _mockProductRepo.Setup(r => r.CreateAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product p, CancellationToken _) => p);

        // Act
        var result = await _service.CreateProductAsync(createDto);

        // Assert
        result.Should().NotBeNull();
        result.Name.Should().Be("Nuevo Producto");
        result.Price.Should().Be(150);
        result.Stock.Should().Be(5);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateProductAsync_ExistingId_ShouldReturnUpdatedProduct()
    {
        // Arrange
        var id = Guid.NewGuid();
        var product = CreateProduct(id, "Original", 100, 10);
        var updateDto = new UpdateProductDto("Updated", 200, 20, null);
        _mockProductRepo.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(product);
        _mockProductRepo.Setup(r => r.UpdateAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product p, CancellationToken _) => p);

        // Act
        var result = await _service.UpdateProductAsync(id, updateDto);

        // Assert
        result.Should().NotBeNull();
        result!.Name.Should().Be("Updated");
        result.Price.Should().Be(200);
        result.Stock.Should().Be(20);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task UpdateProductAsync_NonExistingId_ShouldReturnNull()
    {
        // Arrange
        var id = Guid.NewGuid();
        var updateDto = new UpdateProductDto("Updated", 200, 20, null);
        _mockProductRepo.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync((Product?)null);

        // Act
        var result = await _service.UpdateProductAsync(id, updateDto);

        // Assert
        result.Should().BeNull();
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task DeleteProductAsync_ShouldReturnTrue()
    {
        // Arrange
        var id = Guid.NewGuid();
        _mockProductRepo.Setup(r => r.DeleteAsync(id, It.IsAny<CancellationToken>())).Returns(Task.CompletedTask);

        // Act
        var result = await _service.DeleteProductAsync(id);

        // Assert
        result.Should().BeTrue();
        _mockProductRepo.Verify(r => r.DeleteAsync(id, It.IsAny<CancellationToken>()), Times.Once);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    private static Product CreateProduct(Guid id, string name, decimal price, int stock)
    {
        var now = DateTime.UtcNow;
        return new Product
        {
            Id = id,
            Name = name,
            Price = price,
            Stock = stock,
            CreatedAt = now,
            UpdatedAt = now
        };
    }
}
