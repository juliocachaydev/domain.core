using Jcg.Domain.Core.Tests.Domain;
using Microsoft.EntityFrameworkCore;

namespace Jcg.Domain.Core.Tests.Persistence;

public class AppDbContext : DbContext
{
    public DbSet<Inventory> Inventories { get; set; } = null!;
    
    public DbSet<Order> Orders { get; set; } = null!;
    
    public AppDbContext(DbContextOptions options) : base(options)
    {
        
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        var inventory = modelBuilder.Entity<Inventory>();
        inventory.HasKey(e => e.Id);
        inventory.Property(e => e.Id).ValueGeneratedNever();
        inventory.HasMany(e => e.Items).WithOne();
        
        var inventoryItem = modelBuilder.Entity<Inventory.InventoryItem>();
        inventoryItem.HasKey(e => e.Id);
        inventoryItem.Property(e => e.Id).ValueGeneratedNever();
        
        var order = modelBuilder.Entity<Order>();
        order.HasKey(e => e.Id);
        order.Property(e => e.Id).ValueGeneratedNever();
        order.HasMany(e => e.Lines).WithOne();

        var line = modelBuilder.Entity<Order.OrderLine>();
        line.HasKey(e => e.Id);
        line.Property(e => e.Id).ValueGeneratedNever();
        
        base.OnModelCreating(modelBuilder);
    }
}