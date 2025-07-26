using Jcg.Domain.Core.Domain;

namespace Jcg.Domain.Core.Tests.Domain;

public class Order : AggregateBase, IOrderRoot
{
    public Guid Id { get; private set; }

    public IEnumerable<OrderLine> Lines { get; private set; } = new List<OrderLine>();

    private Order()
    {
    }

    public Order(Guid id)
    {
        Id = id;
    }

    public void ShipOrder()
    {
        var ev = new OrderShipped()
        {
            ShipmentDetails = Lines.Select(x => new OrderShipped.ProductQuantity(x.ProductId, x.Quantity)).ToArray()
        };

        AddDomainEvent(ev);
    }

    public void AddLine(Guid productId, int quantity)
    {
        var linesList = (Lines as List<OrderLine>)!;
        linesList.Add(new OrderLine(productId, quantity));
    }

    public override void AssertEntityStateIsValid()
    {
        if (Lines.GroupBy(x => x.ProductId).Any(g => g.Count() > 1))
            throw new InvalidOperationException("Duplicated product lines are not allowed.");
    }

    public class OrderLine
    {
        public Guid Id { get; private set; }

        public Guid ProductId { get; private set; }

        public int Quantity { get; private set; }

        private OrderLine()
        {
        }

        public OrderLine(Guid productId, int quantity)
        {
            Id = Guid.NewGuid();
            ProductId = productId;
            Quantity = quantity;
        }
    }
}