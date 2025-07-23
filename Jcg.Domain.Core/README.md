All the examples presented here can be found in [this test class](https://github.com/juliocachaydev/domain.core/blob/main/Jcg.Domain.Core.Tests/DomainCoreTests.cs). This is an example with EF Core

# Install the NuGet package
dotnet add package Jcg.Domain.Core

# Setup your project
My project uses EF Core, and here is my [DbContext](https://github.com/juliocachaydev/domain.core/blob/main/Jcg.Domain.Core.Tests/Persistence/AppDbContext.cs), which uses an in-memory database.

## Create a Database adapter

[DatabaseAdapter](https://github.com/juliocachaydev/domain.core/blob/main/Jcg.Domain.Core.Tests/TestCommon/DatabaseAdapter.cs)
```
public class DatabaseAdapter : IDatabaseAdapter
{
    private readonly AppDbContext _db;

    public DatabaseAdapter(
        AppDbContext db)
    {
        _db = db;
    }
    public ICollection<object> GetTrackEntities()
    {
        return _db.ChangeTracker.Entries().Select(e => e.Entity)
            .Where(e => e != null)
            .Select(e => e!)
            .ToArray();
    }

    public async Task SaveChangesAsync()
    {
        await _db.SaveChangesAsync();
    }
}
```
## Add the Library to the DI Container

[Following this recommendation by Microsoft](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/dependency-injection?view=aspnetcore-9.0#register-groups-of-services-with-extension-methods), I add all the services to the DI Container in  an extension method

[DependencyInjection Extension method](https://github.com/juliocachaydev/domain.core/blob/main/Jcg.Domain.Core.Tests/DependencyInjection.cs)
```
public static class DependencyInjection
{
    public static void AddApplicationServices(this IServiceCollection services)
    {
        // Ef Core
        var connection = new SqliteConnection("DataSource=:memory:");
        connection.Open();

        services.AddDbContext<AppDbContext>(options =>
            options.UseSqlite(connection));
        
        services.AddRepository(
            sp => new DatabaseAdapter(sp.GetRequiredService<AppDbContext>()),
            Assembly.GetExecutingAssembly());
    }
}
```
This is the code that adds the Library. You need to pass two things:
1. A Function to create the DatabaseAdapter
2. The assemblies where the Library will look for IEntityStrategy and Domain Event Handlers so that they can be wired via reflection.

```
services.AddRepository(
            sp => new DatabaseAdapter(sp.GetRequiredService<AppDbContext>()),
            Assembly.GetExecutingAssembly());
```

You can then add the application services to an ASP.NET App like this:

**Program.cs**
```
app.Services.AddApplicationServices();
```

## Setup your Entity Strategies
You tell the Library how to store entities in your database by implementing the [IEntityStrategy interface](https://github.com/juliocachaydev/domain.core/blob/main/Jcg.Domain.Common/Repository/IEntityStrategy.cs)

In my case, I have a template to write these classes, which is a mechanical process. You can see an example here: [OrderEntityStrategy](https://github.com/juliocachaydev/domain.core/blob/main/Jcg.Domain.Core.Tests/Persistence/OrderEntityStrategy.cs)
```
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
        // This works because we know that the Library will only call this method with an Order or IOrderRoot type, both
        // can be cast to Order.
        return (entity as Order)!;
    }
    public async Task Add<TEntity>(TEntity entity) where TEntity : class
    {
        await _db.Orders.AddAsync(Cast(entity));
    }

    public async Task<TEntity?> Load<TEntity>(Guid id) where TEntity : class
    {
        var result = await _db.Orders
            .Include(e=> e.Lines)
            .FirstOrDefaultAsync(x => x.Id == id);
        if (result is null)
        {
            return null;
        }

        //This is safe because the IRepository will only call this method with an Order or IOrderRoot type.
        return result as TEntity;
    }

    public void Remove<TEntity>(TEntity entity) where TEntity : class
    {
        var cast = Cast(entity);
        _db.Orders.Remove(cast);
    }
}
```

## Wire domain events
To implement domain events and handlers, first, you define a domain event like [OrderShipped](https://github.com/juliocachaydev/domain.core/blob/main/Jcg.Domain.Core.Tests/Domain/OrderShipped.cs)

A domain event contains data about an event that occurred within a given aggregate, in this case, when an Order aggregate is shipped.
```
public record OrderShipped : IDomainEvent
{
    public required ProductQuantity[] ShipmentDetails { get; init; }
    public record ProductQuantity(Guid ProductId, int Quantity);
}
```

Then, you can add Domain Event Handlers, which execute code when a domain event is dispatched by the repository on commit
```
public class ReduceInventoryWhenOrderIsShipped : IDomainEventHandler<OrderShipped>
{
    private readonly IRepository _repository;
    private readonly AppDbContext _db;

    public ReduceInventoryWhenOrderIsShipped(IRepository repository, AppDbContext db)
    {
        _repository = repository;
        _db = db;
    }
    
    public async Task HandlerAsync(OrderShipped domainEvent)
    {
        var inventoryFromQuery = await _db.Inventories.AsNoTracking()
            .Include(e=> e.Items).FirstAsync();

        // tracked
        var inventory = await _repository.LoadOrThrow<Inventory>(inventoryFromQuery.Id);
        foreach (var detail in domainEvent.ShipmentDetails)
        {
            inventory.UpdateInventory(detail.ProductId, detail.Quantity);
        }

        await _repository.CommitChanges();
    }
}
```

## Wiring Domain Event handlers and Entity Strategies
When you add the Library to the DI Container, you pass one or more assemblies
```
services.AddRepository(
            sp => new DatabaseAdapter(sp.GetRequiredService<AppDbContext>()),
            Assembly.GetExecutingAssembly()); <-- here
```

The first time someone uses the repository or dispatches a domain event, the Library scans the assembly to create an in-memory cache with all the types that implement the IEntityStrategy and IDomainEventHandler<T> interfaces.

These instances can be created by the Library as needed.

> All you need to do is tell the Library in which assembly (or assemblies) you put the Entity Strategies and the Domain Event Handlers.

# About the example
There are two entities: [Order](https://github.com/juliocachaydev/domain.core/blob/main/Jcg.Domain.Core.Tests/Domain/Order.cs) and [Inventory](https://github.com/juliocachaydev/domain.core/blob/main/Jcg.Domain.Core.Tests/Domain/Inventory.cs). The Order tells what to ship, and the Inventory counts what is left.

When you ship an order, the app updates the inventory to reflect the reduced quantity of the shipped products. It does this via a domain event: [OrderShipped](https://github.com/juliocachaydev/domain.core/blob/main/Jcg.Domain.Core.Tests/Domain/OrderShipped.cs)

In the [Test class: DomainCoreTests.cs](https://github.com/juliocachaydev/domain.core/tree/main/Jcg.Domain.Core.Tests), there are common methods to perform operations on the underlying database and to get the IRepository from the DI Container.

What is important to note here is that most of these methods create their own scope, so they act as if they were operations started in different requests in an ASP application. For instance, if you add an Order to the database, that order is stored in the database, and the scope is disposed. If you load the entity later in the same test, you will retrieve one from the database, not a cached one.

# Use Cases

The Tests you will see, get the services from a Scope. This is what an application like ASP does when you inject services in the constructor.
```
var sp = SetupServices();

        using var scope = GetRepository(sp, out var repository);
```

## CRUD Operations

```
class SomeService 
{
   private readonly IRepository _repository;

   SomeService(IRepository repository)
   {
      _repository = repository;
   }

   public async Task AddOrder()
   {
       var order = new Order(Guid.NewGuid());
       await repository.Add(order);
       await repository.CommitChanges();
   }

    public async Task UpdateOrder(Guid orderId)
   {
       // LoadOrThrow will throw an exception when the entity is not found. Load will return null. Both track the entity.
       var order = await repository.LoadOrThrow<Order>(orderId);
       // Update the order 
       orderFromDb.AddLine(Guid.NewGuid(), 10);
       await repository.CommitChanges();
   }

   public async Task DeleteOrder(Guid orderId)
   {
       await repository.Remove<Orer>(orderId);
       await repository.CommitChanges();
   }
}
```

About the CommitChanges() method.
When you call CommitChanges(), several actions are taken by the repository.

1. Dispatch the domain events for entities that were tracked.
2. Call the AssertEntityStateIsValid() method for each entity
3. If there were no exceptions in steps 1 and 2, the transaction is committed to the database.

## Domain Events
When I call the ShipOrder() method in the [Order](https://github.com/juliocachaydev/domain.core/blob/main/Jcg.Domain.Core.Tests/Domain/Order.cs) aggregate, an Order Shipped domain event is dispatched, which contains information on the products that were shipped.
```
public void ShipOrder()
    {
        var ev = new OrderShipped()
        {
            ShipmentDetails = Lines.Select(x => new OrderShipped.ProductQuantity(x.ProductId, x.Quantity)).ToArray()
        };
        
        AddDomainEvent(ev);
    }
```

The [ReduceInventoryWhenOrderIsShipped Domain Event Handler](https://github.com/juliocachaydev/domain.core/blob/main/Jcg.Domain.Core.Tests/Domain/ReduceInventoryWhenOrderIsShipped.cs) updates the inventory to reflect the shipment.

```
public class ReduceInventoryWhenOrderIsShipped : IDomainEventHandler<OrderShipped>
{
    private readonly IRepository _repository;
    private readonly AppDbContext _db;

    public ReduceInventoryWhenOrderIsShipped(IRepository repository, AppDbContext db)
    {
        _repository = repository;
        _db = db;
    }
    
    public async Task HandlerAsync(OrderShipped domainEvent)
    {
        var inventoryFromQuery = await _db.Inventories.AsNoTracking()
            .Include(e=> e.Items).FirstAsync();

        // tracked
        var inventory = await _repository.LoadOrThrow<Inventory>(inventoryFromQuery.Id);
        foreach (var detail in domainEvent.ShipmentDetails)
        {
            inventory.UpdateInventory(detail.ProductId, detail.Quantity);
        }

        await _repository.CommitChanges();
    }
}
```

This is a mechanism by which Aggregates trigger side effects elsewhere in the system. [Read more here](https://learn.microsoft.com/en-us/dotnet/architecture/microservices/microservice-ddd-cqrs-patterns/domain-events-design-implementation)