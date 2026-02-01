using ProductsSales.Application.DTOs;

namespace ProductsSales.WinForms.Services;

public class ProductService
{
    private readonly ApiClient _apiClient;

    public ProductService(ApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task<List<ProductDto>> GetAllAsync()
    {
        var products = await _apiClient.GetAsync<List<ProductDto>>("api/products");
        return products ?? new List<ProductDto>();
    }

    public async Task<ProductDto?> GetByIdAsync(Guid id)
    {
        return await _apiClient.GetAsync<ProductDto>($"api/products/{id}");
    }

    public async Task<ProductDto?> CreateAsync(CreateProductDto createDto)
    {
        return await _apiClient.PostAsync<ProductDto>("api/products", createDto);
    }

    public async Task<ProductDto?> UpdateAsync(Guid id, UpdateProductDto updateDto)
    {
        return await _apiClient.PutAsync<ProductDto>($"api/products/{id}", updateDto);
    }

    public async Task DeleteAsync(Guid id)
    {
        await _apiClient.DeleteAsync($"api/products/{id}");
    }
}
