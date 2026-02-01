namespace ProductsSales.Domain.Entities;

public class User
{
    public Guid Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;

    // Navegación
    public ICollection<Sale> Sales { get; set; } = new List<Sale>();
}

