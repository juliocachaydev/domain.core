using Jcg.Domain.Core.Domain;
using Jcg.Domain.Core.LibrarySupport;

namespace Jcg.Domain.Core.Dispatcher
{
    public interface IDomainEventHandlersCollection
    {
        /// <summary>
        /// Scans the provided assemblies for implementations of IEntityStrategy, and stores them internall so they are
        /// available for later use. Call this method once, when your application starts. This class should be available as a singleton
        /// </summary>
        /// <param name="sp">The service provider is necessary because the types for which an IEntityStrategy applies are defined as a property
        /// (see Types array), to read that property this library must create an instance of that implementation</param>
        /// <param name="assembliesToScan"></param>
        /// <exception cref="NotImplementedException"></exception>
        void ScanForHandlers();

        /// <summary>
        /// Given a scope, this method creates an instance of the IEntityStrategy or throws an exception if the entityType does not
        /// correspond to any of the IEntityStrategy found by the ScanForStrategies method.
        /// </summary>
        /// <param name="sp"></param>
        /// <param name="entityType"></param>
        /// <returns></returns>
        /// <exception cref="NotImplementedException"></exception>
        object[] CreateHandlers(IEntityFactoryAdapter factory, IDomainEvent domainEvent);
    }
}