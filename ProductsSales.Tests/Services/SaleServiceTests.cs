using FluentAssertions;
using Moq;
using ProductsSales.Application.DTOs;
using ProductsSales.Application.Interfaces;
using ProductsSales.Application.Services;
using ProductsSales.Domain.Entities;
using Xunit;

namespace ProductsSales.Tests.Services;

public class SaleServiceTests
{
    private readonly Mock<ISaleRepository> _mockSaleRepo;
    private readonly Mock<IProductRepository> _mockProductRepo;
    private readonly Mock<IUserRepository> _mockUserRepo;
    private readonly Mock<IUnitOfWork> _mockUnitOfWork;
    private readonly SaleService _service;

    public SaleServiceTests()
    {
        _mockSaleRepo = new Mock<ISaleRepository>();
        _mockProductRepo = new Mock<IProductRepository>();
        _mockUserRepo = new Mock<IUserRepository>();
        _mockUnitOfWork = new Mock<IUnitOfWork>();
        _service = new SaleService(
            _mockSaleRepo.Object,
            _mockProductRepo.Object,
            _mockUserRepo.Object,
            _mockUnitOfWork.Object);
    }

    [Fact]
    public async Task CreateSaleAsync_ValidSale_ShouldReturnSaleDto()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var product = CreateProduct(productId, "Product A", 100, 10);
        var user = CreateUser(userId, "testuser");

        var createDto = new CreateSaleDto(userId, new List<CreateSaleItemDto>
        {
            new(productId, 2)
        });

        _mockProductRepo.Setup(r => r.GetByIdAsync(productId, It.IsAny<CancellationToken>())).ReturnsAsync(product);
        _mockProductRepo.Setup(r => r.UpdateAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Product p, CancellationToken _) => p);
        _mockUserRepo.Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>())).ReturnsAsync(user);
        _mockSaleRepo.Setup(r => r.CreateAsync(It.IsAny<Sale>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Sale s, CancellationToken _) => s);

        // Act
        var result = await _service.CreateSaleAsync(createDto);

        // Assert
        result.Should().NotBeNull();
        result.Total.Should().Be(200);
        result.Items.Should().HaveCount(1);
        result.Items[0].Quantity.Should().Be(2);
        result.Items[0].UnitPrice.Should().Be(100);
        _mockUnitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateSaleAsync_ProductNotFound_ShouldThrow()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var productId = Guid.NewGuid();

        var createDto = new CreateSaleDto(userId, new List<CreateSaleItemDto>
        {
            new(productId, 2)
        });

        _mockProductRepo.Setup(r => r.GetByIdAsync(productId, It.IsAny<CancellationToken>())).ReturnsAsync((Product?)null);

        // Act
        var act = () => _service.CreateSaleAsync(createDto);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Producto con ID*no encontrado*");
    }

    [Fact]
    public async Task CreateSaleAsync_InsufficientStock_ShouldThrow()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var productId = Guid.NewGuid();
        var product = CreateProduct(productId, "Product A", 100, 5);

        var createDto = new CreateSaleDto(userId, new List<CreateSaleItemDto>
        {
            new(productId, 10)
        });

        _mockProductRepo.Setup(r => r.GetByIdAsync(productId, It.IsAny<CancellationToken>())).ReturnsAsync(product);

        // Act
        var act = () => _service.CreateSaleAsync(createDto);

        // Assert
        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*No hay suficiente stock*");
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

    private static User CreateUser(Guid id, string username)
    {
        return new User
        {
            Id = id,
            Username = username,
            PasswordHash = "hash"
        };
    }
}
