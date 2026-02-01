namespace ProductsSales.Domain.Entities;

public class Product
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public string? ImagePath { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }

    // Navegación
    public ICollection<SaleItem> SaleItems { get; set; } = new List<SaleItem>();

    public void UpdateStock(int quantity)
    {
        if (Stock - quantity < 0)
        {
            throw new InvalidOperationException($"No hay suficiente stock. Stock disponible: {Stock}");
        }
        Stock -= quantity;
    }

    public bool HasEnoughStock(int quantity) => Stock >= quantity;
}

