using Jcg.Domain.Core.Repository;
using Jcg.Domain.Core.Tests.Domain;
using Microsoft.EntityFrameworkCore;

namespace Jcg.Domain.Core.Tests.Persistence;

public class InventoryEntityStrategy : IEntityStrategy
{
    private readonly AppDbContext _db;
    public Type[] EntityTypes { get; } = [typeof(Inventory)];

    public InventoryEntityStrategy(AppDbContext db)
    {
        _db = db;
    }
    
    private Inventory Cast<T>(T entity) where T : class
    {
        // This works because we know that the library will only call this method with an Order or IOrderRoot type, both
        // can be cast to Order.
        return (entity as Inventory)!;
    }
    public async Task AddAsync<TEntity>(TEntity entity) where TEntity : class
    {
        await _db.Inventories.AddAsync(Cast(entity));
    }

    public async Task<TEntity?> LoadAsync<TEntity>(Guid id) where TEntity : class
    {
        var result = await _db.Inventories
            .Include(e=> e.Items)
            .FirstOrDefaultAsync(x => x.Id == id);
        if (result is null)
        {
            return null;
        }

        // this works because the IRepository will only call this method with an Inventory type.
        return result as TEntity;
    }

    public void RemoveAsync<TEntity>(TEntity entity) where TEntity : class
    {
        var cast = Cast(entity);
        _db.Inventories.Remove(cast);
    }
}