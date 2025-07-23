using Jcg.Domain.Core.Domain;

namespace Jcg.Domain.Core.Tests.Domain;

public record OrderShipped : IDomainEvent
{
    public required ProductQuantity[] ShipmentDetails { get; init; }
    public record ProductQuantity(Guid ProductId, int Quantity);
}