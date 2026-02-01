namespace ProductsSales.Domain.Entities;

public class SaleItem
{
    public Guid Id { get; set; }
    public Guid SaleId { get; set; }
    public Guid ProductId { get; set; }
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal LineTotal { get; set; }

    // Navegación
    public Sale Sale { get; set; } = null!;
    public Product Product { get; set; } = null!;

    public void CalculateLineTotal()
    {
        LineTotal = Quantity * UnitPrice;
    }
}

