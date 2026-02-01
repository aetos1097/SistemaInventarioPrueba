using System.Security.Claims;
using FluentAssertions;
using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using ProductsSales.Api.Controllers;
using ProductsSales.Application.DTOs;
using ProductsSales.Application.Interfaces;
using ProductsSales.Application.Services;
using Xunit;

namespace ProductsSales.Tests.Controllers;

/// <summary>
/// Tests unitarios del ProductsController. Usa mocks de IProductService e IBlobStorageService.
/// Verifica integración con Blob Storage al eliminar productos y subir imágenes.
/// </summary>
public class ProductsControllerTests
{
    private readonly Mock<IProductService> _mockProductService;
    private readonly Mock<IBlobStorageService> _mockBlobService;
    private readonly Mock<IValidator<CreateProductDto>> _mockCreateValidator;
    private readonly Mock<IValidator<UpdateProductDto>> _mockUpdateValidator;
    private readonly ProductsController _controller;

    public ProductsControllerTests()
    {
        _mockProductService = new Mock<IProductService>();
        _mockBlobService = new Mock<IBlobStorageService>();
        _mockCreateValidator = new Mock<IValidator<CreateProductDto>>();
        _mockUpdateValidator = new Mock<IValidator<UpdateProductDto>>();
        _controller = new ProductsController(
            _mockProductService.Object,
            _mockBlobService.Object,
            _mockCreateValidator.Object,
            _mockUpdateValidator.Object);

        SetupAuthenticatedUser();
    }

    /// <summary>Simula usuario autenticado para pasar [Authorize].</summary>
    private void SetupAuthenticatedUser()
    {
        var identity = new ClaimsIdentity(new[] { new Claim(ClaimTypes.Name, "testuser") }, "Test");
        var principal = new ClaimsPrincipal(identity);
        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = principal }
        };
    }

    [Fact]
    public async Task DeleteProduct_WithImagePath_DeletesFromBlob()
    {
        // Arrange
        var id = Guid.NewGuid();
        var product = new ProductDto(id, "Test", 100, 10, "https://storage.blob.core.windows.net/container/productssales/guid_image.jpg", DateTime.UtcNow, DateTime.UtcNow);
        _mockProductService.Setup(s => s.GetProductByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(product);
        _mockProductService.Setup(s => s.DeleteProductAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        // Act
        var result = await _controller.DeleteProduct(id);

        // Assert
        result.Should().BeOfType<NoContentResult>();
        _mockBlobService.Verify(b => b.DeleteImageAsync(product.ImagePath!), Times.Once);
    }

    [Fact]
    public async Task DeleteProduct_WithoutImagePath_DoesNotCallBlobService()
    {
        // Arrange
        var id = Guid.NewGuid();
        var product = new ProductDto(id, "Test", 100, 10, null, DateTime.UtcNow, DateTime.UtcNow);
        _mockProductService.Setup(s => s.GetProductByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(product);
        _mockProductService.Setup(s => s.DeleteProductAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        // Act
        var result = await _controller.DeleteProduct(id);

        // Assert
        result.Should().BeOfType<NoContentResult>();
        _mockBlobService.Verify(b => b.DeleteImageAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task DeleteProduct_ProductNotFound_Returns404()
    {
        // Arrange
        var id = Guid.NewGuid();
        _mockProductService.Setup(s => s.GetProductByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync((ProductDto?)null);

        // Act
        var result = await _controller.DeleteProduct(id);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
        _mockBlobService.Verify(b => b.DeleteImageAsync(It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task UploadImage_ValidFile_ReturnsImageUrl()
    {
        // Arrange
        var id = Guid.NewGuid();
        var product = new ProductDto(id, "Test", 100, 10, null, DateTime.UtcNow, DateTime.UtcNow);
        var imageUrl = "https://storage.blob.core.windows.net/container/productssales/guid_image.jpg";
        var bytes = new byte[] { 0xFF, 0xD8, 0xFF };
        var stream = new MemoryStream(bytes);
        var file = new FormFile(stream, 0, bytes.Length, "file", "image.jpg")
        {
            Headers = new HeaderDictionary(),
            ContentType = "image/jpeg"
        };

        _mockProductService.Setup(s => s.GetProductByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync(product);
        _mockBlobService.Setup(b => b.UploadImageAsync(It.IsAny<Stream>(), file.FileName)).ReturnsAsync(imageUrl);
        _mockProductService.Setup(s => s.UpdateImageUrlAsync(id, imageUrl, It.IsAny<CancellationToken>())).ReturnsAsync(true);

        // Act
        var result = await _controller.UploadImage(id, file);

        // Assert
        var okResult = result.Should().BeOfType<OkObjectResult>().Subject;
        okResult.Value.Should().BeEquivalentTo(new { imageUrl });
        _mockBlobService.Verify(b => b.UploadImageAsync(It.IsAny<Stream>(), file.FileName), Times.Once);
    }

    [Fact]
    public async Task UploadImage_ProductNotFound_Returns404()
    {
        // Arrange
        var id = Guid.NewGuid();
        var bytes = new byte[10];
        var stream = new MemoryStream(bytes);
        var file = new FormFile(stream, 0, bytes.Length, "file", "image.jpg")
        {
            Headers = new HeaderDictionary(),
            ContentType = "image/jpeg"
        };
        _mockProductService.Setup(s => s.GetProductByIdAsync(id, It.IsAny<CancellationToken>())).ReturnsAsync((ProductDto?)null);

        // Act
        var result = await _controller.UploadImage(id, file);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
        _mockBlobService.Verify(b => b.UploadImageAsync(It.IsAny<Stream>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public async Task UploadImage_NoFile_ReturnsBadRequest()
    {
        // Arrange
        var id = Guid.NewGuid();

        // Act
        var result = await _controller.UploadImage(id, null!);

        // Assert
        result.Should().BeOfType<BadRequestObjectResult>();
    }
}
