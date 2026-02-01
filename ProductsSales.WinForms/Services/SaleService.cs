using ProductsSales.Application.DTOs;

namespace ProductsSales.WinForms.Services;

public class SaleService
{
    private readonly ApiClient _apiClient;

    public SaleService(ApiClient apiClient)
    {
        _apiClient = apiClient;
    }

    public async Task<SaleDto?> GetByIdAsync(Guid id)
    {
        return await _apiClient.GetAsync<SaleDto>($"api/sales/{id}");
    }

    public async Task<SaleDto?> CreateAsync(CreateSaleDto createDto)
    {
        return await _apiClient.PostAsync<SaleDto>("api/sales", createDto);
    }

    public async Task<SalesReportDto?> GetReportAsync(DateTime from, DateTime to, int page = 1, int pageSize = 10)
    {
        var fromStr = from.ToString("yyyy-MM-ddTHH:mm:ss");
        var toStr = to.ToString("yyyy-MM-ddTHH:mm:ss");
        return await _apiClient.GetAsync<SalesReportDto>($"api/sales/report?from={Uri.EscapeDataString(fromStr)}&to={Uri.EscapeDataString(toStr)}&page={page}&pageSize={pageSize}");
    }
}
