using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ProductsSales.Application.DTOs;
using ProductsSales.Application.Services;

namespace ProductsSales.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SalesController : ControllerBase
{
    private readonly SaleService _saleService;
    private readonly ReportService _reportService;
    private readonly IValidator<CreateSaleDto> _validator;

    public SalesController(SaleService saleService, ReportService reportService, IValidator<CreateSaleDto> validator)
    {
        _saleService = saleService;
        _reportService = reportService;
        _validator = validator;
    }

    [HttpPost]
    public async Task<ActionResult<SaleDto>> CreateSale([FromBody] CreateSaleDto dto)
    {
        var validationResult = await _validator.ValidateAsync(dto);
        if (!validationResult.IsValid)
        {
            return BadRequest(new { errors = validationResult.Errors.Select(e => e.ErrorMessage) });
        }

        try
        {
            var sale = await _saleService.CreateSaleAsync(dto);
            return CreatedAtAction(nameof(GetSale), new { id = sale.Id }, sale);
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { error = ex.Message });
        }
    }

    [HttpGet("report")]
    public async Task<ActionResult<SalesReportDto>> GetSalesReport(
        [FromQuery] DateTime from,
        [FromQuery] DateTime to,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 10;
        if (pageSize > 100) pageSize = 100; // Límite máximo
        
        var report = await _reportService.GetSalesReportAsync(from, to, page, pageSize);
        return Ok(report);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<SaleDto>> GetSale(Guid id)
    {
        return NotFound();
    }
}

