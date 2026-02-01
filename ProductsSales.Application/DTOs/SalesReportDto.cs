namespace ProductsSales.Application.DTOs;

public record SalesReportDto(
    List<SaleDto> Sales,
    decimal TotalSold,
    int CurrentPage,
    int PageSize,
    int TotalRecords,
    int TotalPages
);

