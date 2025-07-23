using Jcg.Domain.Core.Exceptions;
using Jcg.Domain.Core.Repository;
using Jcg.Domain.Core.Tests.Domain;
using Jcg.Domain.Core.Tests.Persistence;
using Jcg.Domain.Core.Tests.TestCommon;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Jcg.Domain.Core.Tests;

public class DomainCoreTests
{
    private Order? LoadOrderFromDatabaseInDifferentScope(IServiceProvider sp, Guid id)
    {
        using var scope = sp.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        return db.Orders.Include(e => e.Lines)
            .FirstOrDefault(x => x.Id == id);
    }
    
    private Inventory? LoadInventoryFromDatabase(IServiceScope scope, Guid id)
    {
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        return db.Inventories.Include(e => e.Items)
            .FirstOrDefault(x => x.Id == id);
    }

    private void AddOrderToDatabaseInDifferentScope(IServiceProvider sp, Order order)
    {
        using var scope = sp.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.Orders.Add(order);
        db.SaveChanges();
    }
    
    private void AddInventoryToDatabaseInDifferentScope(IServiceProvider sp, Inventory inventory)
    {
        using var scope = sp.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        db.Inventories.Add(inventory);
        db.SaveChanges();
    }

    private IServiceProvider SetupServices()
    {
        var sp = ServiceProviderFactory
            .Create(services => services.AddApplicationServices());

        using var scope = sp.CreateScope();
        var db = sp.GetRequiredService<AppDbContext>();
        db.Database.EnsureCreated();
        db.Database.Migrate();
        return sp;
    }

    private IServiceScope GetRepository(IServiceProvider sp, out IRepository repository)
    {
        var scope = sp.CreateScope();
        
        repository = scope.ServiceProvider.GetRequiredService<IRepository>();

        return scope;
    }

    
    [Fact]
    public async Task CanAddAnEntity()
    {
        // ***** ARRANGE *****

        var sp = SetupServices();

        using var scope = GetRepository(sp, out var repository);

        var order = new Order(Guid.NewGuid());

        // ***** ACT *****

        await repository.Add(order);
        await repository.CommitChanges();

        // ***** ASSERT *****
        
        var result = LoadOrderFromDatabaseInDifferentScope(sp, order.Id);
        
        Assert.Equivalent(order, result);
    }

    [Fact]
    public async Task CanUpdateAnEntity()
    {
        // ***** ARRANGE *****

        // Same as creating a DI container for an ASP Net Core application
        var sp = SetupServices();

        var order = new Order(Guid.NewGuid());
        
        AddOrderToDatabaseInDifferentScope(sp, order);
        
        using var scope = GetRepository(sp, out var repository);
        
        // ***** ACT *****
        
        var orderFromDb = await repository.LoadOrThrow<Order>(order.Id);
        orderFromDb.AddLine(Guid.NewGuid(), 10);
        await repository.CommitChanges();

        // ***** ASSERT *****
        
        var result = LoadOrderFromDatabaseInDifferentScope(sp, order.Id);
        
        // here is the line we added in the ACT section
        Assert.NotEmpty(result.Lines);
        
    }

    [Fact]
    public async Task CanRemoveAnEntity()
    {
        var sp = SetupServices();

        var order = new Order(Guid.NewGuid());
        
        AddOrderToDatabaseInDifferentScope(sp, order);
        
        using var scope = GetRepository(sp, out var repository);
        
        // ***** ACT *****

        await repository.Remove<Order>(order.Id);
        await repository.CommitChanges();

        // ***** ASSERT *****

        var result = LoadOrderFromDatabaseInDifferentScope(sp, order.Id);
        
        // Was deleted
        Assert.Null(result);
    }
    
    [Fact]
    public async Task RepositoryEnforcesInvariantsOnCommit()
    {
        // ***** ARRANGE *****
        
        /*
         * Order has an invariant: Item Ids must be unique in the order.
         */

        var sp = SetupServices();

        var order = new Order(Guid.NewGuid());
        
        using var scope = GetRepository(sp, out var repository);
        var productId = Guid.NewGuid();
        
        order.AddLine(productId, 10);
        
        // ***** ACT *****

        // now, order is invalid because we are trying to add the same product again
        order.AddLine(productId, 20);

        // No exception yet, until we commit.
        await repository.Add(order);

        var result = await Record.ExceptionAsync(async () => await repository.CommitChanges());

        // ***** ASSERT *****

        Assert.NotNull(result);
        Assert.Matches("Duplicated product lines are not allowed.", result.Message);
    }
    
    [Fact]
    public async Task ShipAnOrder_ReducesInventory()
    {
        // ***** ARRANGE *****
        
        /*
         * Here we are showing the functionality of the domain event dispatcher. When we ship an order, the
         * inventory (a different aggregate) is reduced in the same transaction. This is done by the domain event:
         * OrderShipped
         */

        // Same as creating a DI container for an ASP Net Core application
        var sp = SetupServices();

        using var scope = GetRepository(sp, out var repository);

        var order = new Order(Guid.NewGuid());
        var productId = Guid.NewGuid();
        
        // We will order 10 items of a product
        order.AddLine(productId, 10);

        // We have 20 on inventory
        var inventory = new Inventory(Guid.NewGuid());
        inventory.AddItem(productId, 20);
        AddInventoryToDatabaseInDifferentScope(sp, inventory);
        
        // ***** ACT *****

        // We ship the order so the inventory should automatically be updated via the OrderShipped domain event.
        order.ShipOrder();

        
        await repository.Add(order);
        await repository.CommitChanges();

        // ***** ASSERT *****

        var inventoryFromDb = LoadInventoryFromDatabase(scope, inventory.Id);
        
        Assert.Equivalent(10, inventoryFromDb!.Items.First(x=> x.ProductId == productId).Quantity);
    }

    [Fact]
    public async Task CanRemoveAnOrder()
    {
        // ***** ARRANGE *****

        var sp = SetupServices();

        var order = new Order(Guid.NewGuid());

        AddOrderToDatabaseInDifferentScope(sp, order);

        using var scope = GetRepository(sp, out var repository);
        
        // ***** ACT *****

        await repository.Remove<Order>(order.Id);
        await repository.CommitChanges();

        // ***** ASSERT *****

        var result = LoadOrderFromDatabaseInDifferentScope(sp, order.Id);
        
        // was deleted
        Assert.Null(result);

    }

    [Fact]
    public async Task LoadOrThrow_WhenEntityNotFound_Throws()
    {
        // ***** ARRANGE *****

        var sp = SetupServices();

        using var scope = GetRepository(sp, out var repository);
        
        // ***** ACT *****

        var result = await Record.ExceptionAsync(async () => await repository.LoadOrThrow<Order>(Guid.NewGuid()));

        // ***** ASSERT *****

        Assert.NotNull(result);
        Assert.IsType<EntityNotFoundException>(result);
    }

    [Fact]
    public async Task LoadOrThrow_WhenEntityFound_ReturnsIt()
    {
        // ***** ARRANGE *****

        var sp = SetupServices();

        var order = new Order(Guid.NewGuid());

        AddOrderToDatabaseInDifferentScope(sp, order);

        using var scope = GetRepository(sp, out var repository);
        
        // ***** ACT *****

        var result = await repository.LoadOrThrow<Order>(order.Id);

        // ***** ASSERT *****

        Assert.Equivalent(order, result);
    }

    [Fact]
    public async Task Load_WhenEntityNotFound_ReturnsNull()
    {
        // ***** ARRANGE *****

        var sp = SetupServices();

        using var scope = GetRepository(sp, out var repository);
        
        // ***** ACT *****

        var result = await repository.Load<Order>(Guid.NewGuid());

        // ***** ASSERT *****

        Assert.Null(result);
    }
    
    [Fact]
    public async Task Load_WhenEntityFound_ReturnsIt()
    {
        // ***** ARRANGE *****

        var sp = SetupServices();

        var order = new Order(Guid.NewGuid());

        AddOrderToDatabaseInDifferentScope(sp, order);

        using var scope = GetRepository(sp, out var repository);
        
        // ***** ACT *****

        var result = await repository.Load<Order>(order.Id);

        // ***** ASSERT *****

        Assert.Equivalent(order, result);
    }
}