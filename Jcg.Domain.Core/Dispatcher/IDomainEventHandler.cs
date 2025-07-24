using System.Threading.Tasks;
using Jcg.Domain.Core.Domain;

namespace Jcg.Domain.Core.Dispatcher
{
    public interface IDomainEventHandler<TDomainEvent> where TDomainEvent : IDomainEvent
    {
        Task HandleAsync(TDomainEvent domainEvent);
    }
}