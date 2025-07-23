using System.Reflection;

namespace Jcg.Domain.Core.Dispatcher
{
    public static class DomainEventHandlersCollectionFactory
    {
        public static IDomainEventHandlersCollection CreateDomainEventHandlersCollection(
            Assembly[] assembliesToScan)
        {
            return new DomainEventHandlersCollection(assembliesToScan);
        }
    }
}