using Jcg.Domain.Core.Repository;
using Jcg.Domain.Core.Tests.Domain;
using Microsoft.EntityFrameworkCore;

namespace Jcg.Domain.Core.Tests.Persistence;

public class OrderEntityStrategy : IEntityStrategy
{
    private readonly AppDbContext _db;
    public Type[] EntityTypes { get; } = [typeof(Order), typeof(IOrderRoot)];

    public OrderEntityStrategy(AppDbContext db)
    {
        _db = db;
    }
    
    private Order Cast<T>(T entity) where T : class
    {
        // This works because we know that the library will only call this method with an Order or IOrderRoot type, both
        // can be cast to Order.
        return (entity as Order)!;
    }
    public async Task AddAsync<TEntity>(TEntity entity) where TEntity : class
    {
        await _db.Orders.AddAsync(Cast(entity));
    }

    public async Task<TEntity?> LoadAsync<TEntity>(Guid id) where TEntity : class
    {
        var result = await _db.Orders
            .Include(e=> e.Lines)
            .FirstOrDefaultAsync(x => x.Id == id);
        if (result is null)
        {
            return null;
        }

        // this is safe because the IRepository will only call this method with an Order or IOrderRoot type.
        return result as TEntity;
    }

    public void RemoveAsync<TEntity>(TEntity entity) where TEntity : class
    {
        var cast = Cast(entity);
        _db.Orders.Remove(cast);
    }
}