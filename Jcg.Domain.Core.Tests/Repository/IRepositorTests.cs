using System.Reflection;
using Jcg.Domain.Core.Domain;
using Jcg.Domain.Core.Exceptions;
using Jcg.Domain.Core.LibrarySupport;
using Jcg.Domain.Core.Repository;
using Jcg.Domain.Core.Tests.TestCommon;
using Microsoft.Extensions.DependencyInjection;
using Moq;

namespace Jcg.Domain.Core.Tests.Repository;

public class IRepositorTests
{
    private Database CreateDatabase() => new();

    private Mock<IDomainEventDispatcher> CreateDispatcherMock() => new();
    
    private IDatabaseAdapter CreateDatabaseAdapter(Database database) =>  new DatabaseAdapter(database);

    
    
    private RepositoryStrategyCollection CreateRepositoryStrategyCollection() => new();
    
    private IEntityFactoryAdapter CreateEntityFactoryAdapter(Action<IServiceCollection> configureServices)
    {
        return new EntityFactoryAdapter(ServiceProviderFactory.Create(configureServices));
    }

    private IRepository CreateSut(IDatabaseAdapter databaseAdapter, RepositoryStrategyCollection strategyCollection,
        IDomainEventDispatcher dispatcher, IEntityFactoryAdapter entityFactoryAdapter)
    {
        return new IRepository.Imp(
            databaseAdapter,
            strategyCollection,
            dispatcher,
            entityFactoryAdapter);
    }

    private IRepository CreateSut(out Database db, out Mock<IDomainEventDispatcher> dispatcherMock)
    {
        var dbImp = CreateDatabase();
        var dbAdapter = CreateDatabaseAdapter(dbImp);
        dispatcherMock = CreateDispatcherMock();
        var entityFactoryImp = CreateEntityFactoryAdapter(services => services.AddSingleton<Database>(_ => dbImp));
        var strategyCollection = CreateRepositoryStrategyCollection();
        strategyCollection.ScanForStrategies(entityFactoryImp,[Assembly.GetExecutingAssembly()]);

        db = dbImp;
        
        return CreateSut(
            dbAdapter,
            strategyCollection,
            dispatcherMock.Object,
            entityFactoryImp);
    }

    [Fact]
    public async Task CanAddAnEntity()
    {
        // ***** ARRANGE *****

        // The library does not access the database directly, it uses an adapter to keep it agnostic of the database technology.
        var db = CreateDatabase();
        var dbAdapter = CreateDatabaseAdapter(db);

        // a dispatcher can be implemented with a Mediator, message bus or any other mechanism.
        var dispatcher = CreateDispatcherMock();

        // An abstraction over a mechanism to create instances of entities using a DI container or any other factory. For
        // instance, this can be implemented as a wrapper around the IServiceProvider and ActivatorUtilities.CreateInstance.
        var entityFactory = CreateEntityFactoryAdapter(services => services.AddSingleton<Database>(_ => db));
        
        // This is the class that scans the assembly for entity strategies. The Repository uses it internally to create 
        // strategy instances.
        var strategyCollection = CreateRepositoryStrategyCollection();
        
        // Has to be initialized. Note here we are passing the entity factory. This is necessary because internally this method has to
        // create instances of the entity strategies it finds so it can read the Types property and decide for which types a given 
        // strategy can be applied.
        strategyCollection.ScanForStrategies(entityFactory,[Assembly.GetExecutingAssembly()]);

        var sut = CreateSut(
            dbAdapter,
            strategyCollection,
            dispatcher.Object,
            entityFactory);

        var entity1AsClass = new Entity1();
        var entity1AsInterface = (IEntity1Root) new Entity1();


        // ***** ACT *****

        /*
         * Observe that the repository will work with any type defined in an IEntityStrategy.Types array.
         * The intention here is that we can use polymorphism to ask the repository for an entity as a given interface, therefore
         * allowing us to implement the Interface Segregation Principle.
         */
        await sut.Add(entity1AsClass);
        await sut.Add(entity1AsInterface);

        // ***** ASSERT *****
        
        Assert.True(db.Contains(entity1AsClass.Id, entity1AsClass));
        Assert.True(db.Contains(((Entity1)entity1AsInterface)!.Id, entity1AsInterface));
    }

    [Theory]
    [InlineData(true, false)]
    [InlineData(false, true)]
    public async Task Load_ReturnsEntityOrNull(
    bool entityExists, bool shouldReturnNull)
    {
        // ***** ARRANGE *****

        var sut = CreateSut(out var db, out _);

        var entity = new Entity1();
        if (entityExists)
        {
            db.Entities.Add(entity.Id, entity);
        }

        // ***** ACT *****

        var result = await sut.Load<Entity1>(entity.Id);

        // ***** ASSERT *****

        if (shouldReturnNull)
        {
            Assert.Null(result);
        }
        else
        {
            Assert.NotNull(result);
            Assert.Same(entity, result);
        }
    }
    

    [Theory]
    [InlineData(true, false)]
    [InlineData(false, true)]
    public async Task LoadOrThrow_ReturnsEntityOrThrows(
        bool entityExists, bool shouldThrow)
    {
        // ***** ARRANGE *****

        var sut = CreateSut(out var db, out _);
        
        var entity = new Entity1();
        if (entityExists)
        {
            db.Entities.Add(entity.Id, entity);
        }

        Entity1? returnedEntity = null;

        // ***** ACT *****

        var result = await Record.ExceptionAsync(async () => returnedEntity = await sut.LoadOrThrow<Entity1>(entity.Id));

        // ***** ASSERT *****
        
        if (shouldThrow)
        {
            Assert.NotNull(result);
            Assert.IsType<EntityNotFoundException>(result);
        }
        else
        {
            Assert.Null(result);
            Assert.NotNull(returnedEntity);
            Assert.Same(entity, returnedEntity);
        }
    
    }

    [Fact]
    public async Task CanRemoveEntity()
    {
        // ***** ARRANGE *****
        
        var sut = CreateSut(out var db, out _);
        
        var entity = new Entity1();
        db.Entities.Add(entity.Id, entity);

        // ***** ACT *****

        await sut.Remove<Entity1>(entity.Id);

        // ***** ASSERT *****
        
        Assert.Empty(db.Entities.Values);
    }

    [Fact]
    public async Task CommitChanges_WhenTrackedEntityIsAggregateBase_AssertsEntityState_DispatchesDomainEvents_CommitsChanges()
    {
        // ***** ARRANGE *****
        
        var sut = CreateSut(out var db, out var dispatcherMock);

        // See, we are using the interface, we still expect the library to dispatch domain events because the entity implmentation
        // is an AggregateBase.
        var entity = (IEntity1Root)new Entity1();
        // Here I am casting just so I can get the Id which is not exposed in the interface.
        db.Entities.Add(((Entity1)entity)!.Id, entity);
        
        // domain event
        var domainEvent = Mock.Of<IDomainEvent>();
        // This method is defined in the AggregateBase class
        ((Entity1)entity)!.AddDomainEvent(domainEvent);
        
        // In a real implementatiion, the DatabaseAdapter would use the underlying database or a custom change tracker to track those entiteis that
        // were changed and therefore should dispatch domain events. In this example, we just track everything in the databse class.
        // This assertion is just for documentation purposes.
        Assert.Contains(db.GetTrackedEntities(), e => e.Equals(entity));
        
        // This one is also just for documentation purposes. Just showing that the database tracked entities contains the entity with the domain event that  
        // was added
        Assert.Contains(db.GetTrackedEntities().Cast<Entity1>().SelectMany(x => x.DomainEvents), de => de.Equals(domainEvent));
        
        // This assertion is for documentation purposes, just showing that, before we commit changes, the entity.AssertEntityStateIsValid method
        // was not called.
        Assert.False(((Entity1)entity)!.AssertEntityStateIsValidWasCalled);

        // ***** ACT *****

        await sut.CommitChanges();

        // ***** ASSERT *****
        
        // When the repository commits, it first dispatches domain events,
        dispatcherMock.Verify(x=>
            x.DispatchAsync(domainEvent));
        
        // Then, it asserts invariants for all tracked entities that are AggregateBase. Because the databse is intended to be transactional, an exception
        // here would prevent all the changes from being committed (that is why we rely on a change tracker)
        Assert.True(((Entity1)entity)!.AssertEntityStateIsValidWasCalled);
        
        // Finally, it commits the transaction.
        Assert.True(db.CommitChangesWasCalled);
    }

    [Fact]
    public async Task CommitChanges_WhenTrackedEntityIsNotAggregateBase_CommitsChanges()
    {
        // ***** ARRANGE *****
        
        /*
         * The library does not require the  entity to be an AggregateBase, but in that case, the invariants are not asserted and no domain events
         * are dispatched.
         */
        
        var sut = CreateSut(out var db, out var dispatcherMock);

        // See, we are using the interface, we still expect the library to dispatch domain events because the entity implmentation
        // is an AggregateBase.
        var entity = new Entity2();

        // ***** ACT *****
        
        await sut.CommitChanges();

        // ***** ASSERT *****
        
        /*
         * Here, verifying that the method above does not crash proves that the Dispatcher and the AssertEntityStateIsValid methods
         * were not called. both require the entity to be an AggregateBase.
         */
        
        Assert.True(db.CommitChangesWasCalled);
    }

    interface IEntity1Root
    {
        
    }

    // This is just a mock database, for a real use case, this could be EF Core, Dapper or even a NoSQL database.
    class Database
    {
        public Dictionary<Guid, object> Entities = new();

        // for testing purposes, we can check if CommitChanges was called.
        public bool CommitChangesWasCalled = false;

        public object? LoadWithTracking(Guid id)
        {
            if (Entities.TryGetValue(id, out var result))
            {
                return result;
            }
            return null;
        }

        public void CommitChanges()
        {
            CommitChangesWasCalled = true;
        }

        public IEnumerable<object> GetTrackedEntities() => Entities.Values;

        public bool Contains<T>(Guid id, T instance)
        {
            if (Entities.TryGetValue(id, out var result))
            {
                return result.Equals(instance);
            }

            return false;
        }

    }

    class DatabaseAdapter : IDatabaseAdapter
    {
        private readonly Database _db;

        public DatabaseAdapter(Database db)
        {
            _db = db;
        }
        
        public ICollection<object> GetTrackEntities()
        {
            return _db.GetTrackedEntities().ToArray();
        }

        public Task SaveChangesAsync()
        {
            _db.CommitChanges();
            return Task.CompletedTask;
        }
    }

    class Entity1 : AggregateBase, IEntity1Root
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        
        public bool AssertEntityStateIsValidWasCalled = false;
        public override void AssertEntityStateIsValid()
        {
            AssertEntityStateIsValidWasCalled = true;
        }
    }

    // I designed this library so it will work with any class. The typical use case for a repository is to handle aggregates, but,
    // this is a decision that a developer can make.
    class Entity2
    {
        // There is no requirement for the naming of a key, the library is agnostic of the implementation, that is abstracted throught
        // the entity strategy
        public Guid ThisEntityId { get; set; } = Guid.NewGuid();
    }

    class Entity1Strategy : IEntityStrategy
    {
        private readonly Database _db;

        public Entity1Strategy(Database db)
        {
            _db = db;
        }
        // this configuration of the EntityTypes array allows the repository to handle both the concrete type and the interface type.
        public Type[] EntityTypes { get; } = [typeof(Entity1), typeof(IEntity1Root)];
        
        public Task Add<TEntity>(TEntity entity) where TEntity : class
        {
            if (entity is Entity1 cast)
            {
                _db.Entities.Add(cast.Id, cast);
            }

            return Task.CompletedTask;
        }

        public Task<TEntity?> Load<TEntity>(Guid id) where TEntity : class
        {
            var result = _db.LoadWithTracking(id);
            if (result is not null)
            {
                return Task.FromResult((TEntity) result);
            }

            return Task.FromResult((TEntity?) null);
        }

        public void Remove<TEntity>(TEntity entity) where TEntity : class
        {
            if (entity is Entity1 cast)
            {
                _db.Entities.Remove(cast.Id);
            }
        }
    }
    
    class Entity2Strategy : IEntityStrategy
    {
        public Entity2Strategy(Database db)
        {
            
        }
        // this configuration of the EntityTypes array allows the repository to handle both the concrete type and the interface type.
        public Type[] EntityTypes { get; } = [typeof(Entity2)];
        
        /*
         * I am not gonna implement the methods below because I am not planning on using those for testing for this particular entity type.
         */
        
        public Task Add<TEntity>(TEntity entity) where TEntity : class
        {
            throw new NotImplementedException();
        }

        public Task<TEntity?> Load<TEntity>(Guid id) where TEntity : class
        {
            throw new NotImplementedException();
        }

        public void Remove<TEntity>(TEntity entity) where TEntity : class
        {
            throw new NotImplementedException();
        }
    }
}