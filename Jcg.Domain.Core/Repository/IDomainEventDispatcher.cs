using System.Threading.Tasks;
using Jcg.Domain.Core.Domain;

namespace Jcg.Domain.Core.Repository
{
    /// <summary>
    /// Abstracts a mechanism to dispatch domain events.
    /// </summary>
    public interface IDomainEventDispatcher
    {
        /// <summary>
        /// Dispatches a domain event to all registered handlers.
        /// </summary>
        Task DispatchAsync(IDomainEvent ev);
    }
}