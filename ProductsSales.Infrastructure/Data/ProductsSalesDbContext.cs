using Microsoft.EntityFrameworkCore;
using ProductsSales.Domain.Entities;

namespace ProductsSales.Infrastructure.Data;

public class ProductsSalesDbContext : DbContext
{
    public ProductsSalesDbContext(DbContextOptions<ProductsSalesDbContext> options) : base(options)
    {
    }

    public DbSet<Product> Products { get; set; }
    public DbSet<Sale> Sales { get; set; }
    public DbSet<SaleItem> SaleItems { get; set; }
    public DbSet<User> Users { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Producto
        modelBuilder.Entity<Product>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Price).HasColumnType("decimal(18,2)");
            entity.Property(e => e.Stock).IsRequired();
            entity.Property(e => e.ImagePath).HasMaxLength(500);
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.UpdatedAt).IsRequired();

            entity.HasMany(e => e.SaleItems)
                .WithOne(e => e.Product)
                .HasForeignKey(e => e.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Venta
        modelBuilder.Entity<Sale>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Date).IsRequired();
            entity.Property(e => e.Total).HasColumnType("decimal(18,2)").IsRequired();
            entity.Property(e => e.UserId).IsRequired();

            entity.HasOne(e => e.User)
                .WithMany(e => e.Sales)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasMany(e => e.Items)
                .WithOne(e => e.Sale)
                .HasForeignKey(e => e.SaleId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Ítem de venta
        modelBuilder.Entity<SaleItem>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.SaleId).IsRequired();
            entity.Property(e => e.ProductId).IsRequired();
            entity.Property(e => e.Quantity).IsRequired();
            entity.Property(e => e.UnitPrice).HasColumnType("decimal(18,2)").IsRequired();
            entity.Property(e => e.LineTotal).HasColumnType("decimal(18,2)").IsRequired();
        });

        // Usuario
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Username).IsRequired().HasMaxLength(100);
            entity.Property(e => e.PasswordHash).IsRequired().HasMaxLength(500);
            entity.HasIndex(e => e.Username).IsUnique();
        });
    }
}

