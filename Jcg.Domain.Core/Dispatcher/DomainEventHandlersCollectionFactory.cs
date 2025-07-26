using System.Reflection;
using Jcg.Domain.Core.LibrarySupport;

namespace Jcg.Domain.Core.Dispatcher;

/// <summary>
/// Creates instances of <see cref="IDomainEventHandlersCollection"/>.
/// </summary>
public static class DomainEventHandlersCollectionFactory
{
    /// <summary>
    /// Creates a DomainEventHandlersCollection configured to scan the specified assemblies for domain event handlers. The scanning process is not
    /// done on construction but when you call the <see cref="IDomainEventHandlersCollection.ScanForHandlers"/> method.
    /// </summary>
    /// <param name="assembliesToScan">The assemblies to scan for handlers</param>
    /// <returns>An instance of the collection</returns>
    public static IDomainEventHandlersCollection CreateDomainEventHandlersCollection(
        Assembly[] assembliesToScan)
    {
        return new DomainEventHandlersCollection(assembliesToScan);
    }
}