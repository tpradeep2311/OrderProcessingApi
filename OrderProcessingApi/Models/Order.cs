namespace OrderProcessingApi.Models
{
    public enum OrderStatus
    {
        Pending,
        Fulfilled,
        Canceled
    }

    public class Order
    {
        public Guid Id { get; set; }
        public List<OrderItem> Items { get; set; } = new();
        public OrderStatus Status { get; set; } = OrderStatus.Pending;
        public DateTime CreatedAt { get; set; }
    }
}
