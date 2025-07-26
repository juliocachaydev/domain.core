using System;
using System.Threading.Tasks;
using Jcg.Domain.Core.Domain;
using Jcg.Domain.Core.LibrarySupport;
using Jcg.Domain.Core.Repository;

namespace Jcg.Domain.Core.Dispatcher;

internal class DomainEventDispatcher : IDomainEventDispatcher
{
    private readonly IDomainEventHandlersCollection _domainEventHandlersCollection;
    private readonly IEntityFactoryAdapter _entityFactoryAdapter;

    public DomainEventDispatcher(
        IDomainEventHandlersCollection domainEventHandlersCollection,
        IEntityFactoryAdapter entityFactoryAdapter)
    {
        _domainEventHandlersCollection = domainEventHandlersCollection;
        _entityFactoryAdapter = entityFactoryAdapter;
    }

    public async Task DispatchAsync(IDomainEvent domainEvent)
    {
        _domainEventHandlersCollection.ScanForHandlers();
        var handlers = _domainEventHandlersCollection.CreateHandlers(_entityFactoryAdapter, domainEvent);

        foreach (var h in handlers)
        {
            var method = h.GetType().GetMethod("HandleAsync");
            if (method != null)
            {
                var task = (Task)method.Invoke(h, new object[] { domainEvent });
                await task;
            }
        }
    }
}