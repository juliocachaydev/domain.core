using Jcg.Domain.Core.LibrarySupport;

namespace Jcg.Domain.Core.Repository;

public static class RepositoryFactory
{
    public static IRepository Create(
        IDatabaseAdapter databaseAdapter,
        RepositoryStrategyCollection strategyCollection,
        IDomainEventDispatcher dispatcher,
        IEntityFactoryAdapter entityFactoryAdapter)
    {
        return new IRepository.Imp(databaseAdapter, strategyCollection, dispatcher, entityFactoryAdapter);
    }
}