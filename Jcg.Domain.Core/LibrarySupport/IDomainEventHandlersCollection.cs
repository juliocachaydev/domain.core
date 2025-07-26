using Jcg.Domain.Core.Domain;

namespace Jcg.Domain.Core.LibrarySupport
{
    /// <summary>
    /// A storage for domain event handlers defined in the application. Internally, this is kept as a singleton service. The library scans the assemblies once,
    /// then remembers the types of handlers it found so they can used later.
    /// </summary>
    public interface IDomainEventHandlersCollection
    {
        /// <summary>
        /// Scans the provided assemblies for implementations of IEntityStrategy, and stores them internall so they are
        /// available for later use. Call this method once, when your application starts. This class should be available as a singleton.
        /// The assemblies are passed in the constructor of the implementing class.
        /// </summary>
        void ScanForHandlers();

        /// <summary>
        /// Creates instances of the handlers for the given domain event. 
        /// </summary>
        /// <param name="factory">An abstraction to keep the implementation abnostic of the DI Container</param>
        /// <param name="domainEvent">The domain event whose handlers will be instantiated</param>
        /// <returns>The handler instances</returns>
        object[] CreateHandlers(IEntityFactoryAdapter factory, IDomainEvent domainEvent);
    }
}