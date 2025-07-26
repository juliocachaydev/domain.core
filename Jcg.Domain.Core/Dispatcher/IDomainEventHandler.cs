using System.Threading.Tasks;
using Jcg.Domain.Core.Domain;

namespace Jcg.Domain.Core.Dispatcher
{
    /// <summary>
    /// Handles a domain event. This is a scoped service, you can inject dependencies from the DI Container via the
    /// constructor.
    /// </summary>
    /// <typeparam name="TDomainEvent"></typeparam>
    public interface IDomainEventHandler<TDomainEvent> where TDomainEvent : IDomainEvent
    {
        /// <summary>
        /// Executes the handler logic for the given domain event. This is called by the library. You can also use it for testing.
        /// </summary>
        /// <param name="domainEvent">The domain event</param>
        /// <returns>A completed task</returns>
        Task HandleAsync(TDomainEvent domainEvent);
    }
}