namespace ProductsSales.Domain.Entities;

public class Sale
{
    public Guid Id { get; set; }
    public DateTime Date { get; set; }
    public decimal Total { get; set; }
    public Guid UserId { get; set; }

    // Navegación
    public User User { get; set; } = null!;
    public ICollection<SaleItem> Items { get; set; } = new List<SaleItem>();

    public void CalculateTotal()
    {
        Total = Items.Sum(item => item.LineTotal);
    }
}

