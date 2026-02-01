using FluentAssertions;
using ProductsSales.Application.DTOs;
using ProductsSales.Application.Validators;
using Xunit;

namespace ProductsSales.Tests.Validators;

public class ProductValidatorsTests
{
    [Fact]
    public void CreateProductValidator_EmptyName_ShouldHaveError()
    {
        // Arrange
        var validator = new CreateProductValidator();
        var dto = new CreateProductDto("", 10, 5, null);

        // Act
        var result = validator.Validate(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "El nombre es requerido");
    }

    [Fact]
    public void CreateProductValidator_NameTooLong_ShouldHaveError()
    {
        // Arrange
        var validator = new CreateProductValidator();
        var dto = new CreateProductDto(new string('a', 201), 10, 5, null);

        // Act
        var result = validator.Validate(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "El nombre no puede exceder 200 caracteres");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void CreateProductValidator_PriceZeroOrNegative_ShouldHaveError(decimal price)
    {
        // Arrange
        var validator = new CreateProductValidator();
        var dto = new CreateProductDto("Product", price, 5, null);

        // Act
        var result = validator.Validate(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "El precio debe ser mayor a 0");
    }

    [Fact]
    public void CreateProductValidator_NegativeStock_ShouldHaveError()
    {
        // Arrange
        var validator = new CreateProductValidator();
        var dto = new CreateProductDto("Product", 10, -1, null);

        // Act
        var result = validator.Validate(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "El stock no puede ser negativo");
    }

    [Fact]
    public void CreateProductValidator_ValidDto_ShouldNotHaveErrors()
    {
        // Arrange
        var validator = new CreateProductValidator();
        var dto = new CreateProductDto("Valid Product", 99.99m, 10, null);

        // Act
        var result = validator.Validate(dto);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }

    [Fact]
    public void UpdateProductValidator_EmptyName_ShouldHaveError()
    {
        // Arrange
        var validator = new UpdateProductValidator();
        var dto = new UpdateProductDto("", 10, 5, null);

        // Act
        var result = validator.Validate(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "El nombre es requerido");
    }

    [Fact]
    public void UpdateProductValidator_NameTooLong_ShouldHaveError()
    {
        // Arrange
        var validator = new UpdateProductValidator();
        var dto = new UpdateProductDto(new string('a', 201), 10, 5, null);

        // Act
        var result = validator.Validate(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "El nombre no puede exceder 200 caracteres");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void UpdateProductValidator_PriceZeroOrNegative_ShouldHaveError(decimal price)
    {
        // Arrange
        var validator = new UpdateProductValidator();
        var dto = new UpdateProductDto("Product", price, 5, null);

        // Act
        var result = validator.Validate(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "El precio debe ser mayor a 0");
    }

    [Fact]
    public void UpdateProductValidator_NegativeStock_ShouldHaveError()
    {
        // Arrange
        var validator = new UpdateProductValidator();
        var dto = new UpdateProductDto("Product", 10, -1, null);

        // Act
        var result = validator.Validate(dto);

        // Assert
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "El stock no puede ser negativo");
    }

    [Fact]
    public void UpdateProductValidator_ValidDto_ShouldNotHaveErrors()
    {
        // Arrange
        var validator = new UpdateProductValidator();
        var dto = new UpdateProductDto("Valid Product", 99.99m, 10, null);

        // Act
        var result = validator.Validate(dto);

        // Assert
        result.IsValid.Should().BeTrue();
        result.Errors.Should().BeEmpty();
    }
}
