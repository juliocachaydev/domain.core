using Jcg.Domain.Core.LibrarySupport;
using Jcg.Domain.Core.Repository;

namespace Jcg.Domain.Core.Dispatcher;

/// <summary>
/// Creates instances of <see cref="IDomainEventDispatcher"/>.
/// </summary>
internal static class DomainEventDispatcherFactory
{
    public static IDomainEventDispatcher Create(
        IDomainEventHandlersCollection domainEventHandlersCollection,
        IEntityFactoryAdapter entityFactoryAdapter)
    {
        return new DomainEventDispatcher(domainEventHandlersCollection, entityFactoryAdapter);
    }
}