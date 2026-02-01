using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProductsSales.Application.DTOs;
using ProductsSales.Application.Interfaces;
using ProductsSales.Application.Services;

namespace ProductsSales.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ProductsController : ControllerBase
{
    private readonly IProductService _productService;
    private readonly IBlobStorageService _blobService;
    private readonly IValidator<CreateProductDto> _createValidator;
    private readonly IValidator<UpdateProductDto> _updateValidator;

    public ProductsController(IProductService productService, IBlobStorageService blobService, IValidator<CreateProductDto> createValidator, IValidator<UpdateProductDto> updateValidator)
    {
        _productService = productService;
        _blobService = blobService;
        _createValidator = createValidator;
        _updateValidator = updateValidator;
    }

    [HttpGet]
    public async Task<ActionResult<List<ProductDto>>> GetProducts()
    {
        var products = await _productService.GetAllProductsAsync();
        return Ok(products);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ProductDto>> GetProduct(Guid id)
    {
        var product = await _productService.GetProductByIdAsync(id);
        if (product == null)
        {
            return NotFound(new { error = "Product not found" });
        }

        return Ok(product);
    }

    [HttpPost]
    public async Task<ActionResult<ProductDto>> CreateProduct([FromBody] CreateProductDto dto)
    {
        var validationResult = await _createValidator.ValidateAsync(dto);
        if (!validationResult.IsValid)
        {
            return BadRequest(new { errors = validationResult.Errors.Select(e => e.ErrorMessage) });
        }

        var product = await _productService.CreateProductAsync(dto);
        return CreatedAtAction(nameof(GetProduct), new { id = product.Id }, product);
    }

    [HttpPut("{id}")]
    public async Task<ActionResult<ProductDto>> UpdateProduct(Guid id, [FromBody] UpdateProductDto dto)
    {
        var validationResult = await _updateValidator.ValidateAsync(dto);
        if (!validationResult.IsValid)
        {
            return BadRequest(new { errors = validationResult.Errors.Select(e => e.ErrorMessage) });
        }

        // Si se limpia la imagen, eliminar del Blob Storage
        var existing = await _productService.GetProductByIdAsync(id);
        if (existing != null && !string.IsNullOrEmpty(existing.ImagePath) && string.IsNullOrEmpty(dto.ImagePath))
        {
            await _blobService.DeleteImageAsync(existing.ImagePath);
        }

        var product = await _productService.UpdateProductAsync(id, dto);
        if (product == null)
        {
            return NotFound(new { error = "Product not found" });
        }

        return Ok(product);
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteProduct(Guid id)
    {
        var product = await _productService.GetProductByIdAsync(id);
        if (product == null)
            return NotFound(new { error = "Product not found" });

        var result = await _productService.DeleteProductAsync(id);
        if (!result)
            return NotFound(new { error = "Product not found" });

        if (!string.IsNullOrEmpty(product.ImagePath))
            await _blobService.DeleteImageAsync(product.ImagePath);

        return NoContent();
    }

    [HttpPost("{id}/upload-image")]
    public async Task<IActionResult> UploadImage(Guid id, IFormFile file)
    {
        if (file == null || file.Length == 0)
            return BadRequest(new { error = "No se ha proporcionado ningún archivo" });

        var product = await _productService.GetProductByIdAsync(id);
        if (product == null)
            return NotFound(new { error = "Product not found" });

        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".bmp", ".webp" };
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!allowedExtensions.Contains(extension))
            return BadRequest(new { error = "Tipo de archivo no permitido" });

        if (!string.IsNullOrEmpty(product.ImagePath))
            await _blobService.DeleteImageAsync(product.ImagePath);

        using var stream = file.OpenReadStream();
        var imageUrl = await _blobService.UploadImageAsync(stream, file.FileName);

        await _productService.UpdateImageUrlAsync(id, imageUrl);

        return Ok(new { imageUrl });
    }
}

