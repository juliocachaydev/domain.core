using System.Reflection;
using Jcg.Domain.Core.Exceptions;
using Jcg.Domain.Core.LibrarySupport;
using Jcg.Domain.Core.Repository;
using Jcg.Domain.Core.Tests.TestCommon;

namespace Jcg.Domain.Core.Tests.LibrarySupport;

public class RepositoryStrategyCollectionTests
{
    private RepositoryStrategyCollection CreateSut() => new();

    private EntityFactoryAdapter CreateEntityFactoryAdapter()
    {
        return new(ServiceProviderFactory.Create());
    }
    
    [Fact]
    public void CreateStrategyOrThrow_AssertsStrategyExists()
    {
        // ***** ARRANGE *****

        var sut = CreateSut();

        var entityFactory = CreateEntityFactoryAdapter();
        
        sut.ScanForStrategies(entityFactory, [Assembly.GetExecutingAssembly()]);

        // ***** ACT *****

        // There is no IEntityStrategy defined for SomeClass, so this should throw an exception.
        var result = Record.Exception(() => sut.CreateStrategyOrThrow(entityFactory, typeof(SomeClass)));

        // ***** ASSERT *****
        
        Assert.NotNull(result);
        Assert.IsType<EntityStrategyNotFoundException>(result);
    }

    [Fact]
    public void CreateStrategyOrThrow_CanCreateInstancesOfTheEntityStrategyForTheType()
    {
        // ***** ARRANGE *****
        
        var sut = CreateSut();

        var entityFactory = CreateEntityFactoryAdapter();
        
        sut.ScanForStrategies(entityFactory, [Assembly.GetExecutingAssembly()]);

        // ***** ACT *****

        // The S1 Entity Strategy is defined for the string type (see S1 EntityTypes array), therefore, this should return an instance of S1.
        var result = sut.CreateStrategyOrThrow(entityFactory, typeof(SomeEntity));

        // ***** ASSERT *****
        
        Assert.IsType<S1>(result);
    }

    class SomeClass
    {
        
    }

    class SomeEntity
    {
        
    }

    class S1 : IEntityStrategy
    {
        // Any type will do.
        public Type[] EntityTypes { get; } = [typeof(SomeEntity)];
        
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