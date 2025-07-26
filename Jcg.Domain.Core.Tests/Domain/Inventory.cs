namespace Jcg.Domain.Core.Tests.Domain;

public class Inventory
{
    public Guid Id { get; private set; }

    public IEnumerable<InventoryItem> Items { get; private set; } = new List<InventoryItem>();

    private Inventory()
    {
    }

    public Inventory(Guid id)
    {
        Id = id;
    }

    public void AddItem(Guid productId, int quantity)
    {
        var item = new InventoryItem(productId, quantity);
        Items = Items.Append(item).ToList();
    }

    public void UpdateInventory(Guid productId, int quantityShipped)
    {
        var item = Items.FirstOrDefault(x =>
            x.ProductId == productId);

        if (item is not null) item.Quantity -= quantityShipped;
    }

    public class InventoryItem
    {
        public Guid Id { get; private set; }

        public Guid ProductId { get; private set; }

        public int Quantity { get; set; }

        private InventoryItem()
        {
        }

        public InventoryItem(Guid productId, int quantity)
        {
            Id = Guid.NewGuid();
            ProductId = productId;
            Quantity = quantity;
        }
    }
}