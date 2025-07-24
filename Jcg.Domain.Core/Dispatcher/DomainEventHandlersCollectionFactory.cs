using System.Reflection;
using Jcg.Domain.Core.LibrarySupport;

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