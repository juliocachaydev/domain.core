using Jcg.Domain.Core.Dispatcher;
using Jcg.Domain.Core.Repository;
using Jcg.Domain.Core.Tests.Persistence;
using Microsoft.EntityFrameworkCore;

namespace Jcg.Domain.Core.Tests.Domain;

public class ReduceInventoryWhenOrderIsShipped : IDomainEventHandler<OrderShipped>
{
    private readonly IRepository _repository;
    private readonly AppDbContext _db;

    public ReduceInventoryWhenOrderIsShipped(IRepository repository, AppDbContext db)
    {
        _repository = repository;
        _db = db;
    }

    public async Task HandleAsync(OrderShipped domainEvent)
    {
        var inventoryFromQuery = await _db.Inventories.AsNoTracking()
            .Include(e => e.Items).FirstAsync();

        // tracked
        var inventory = await _repository.LoadOrThrowAsync<Inventory>(inventoryFromQuery.Id);
        foreach (var detail in domainEvent.ShipmentDetails)
            inventory.UpdateInventory(detail.ProductId, detail.Quantity);

        await _repository.CommitChangesAsync();
    }
}