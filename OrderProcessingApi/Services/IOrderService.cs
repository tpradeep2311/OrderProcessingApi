using OrderProcessingApi.Models;

namespace OrderProcessingApi.Services
{
    public interface IOrderService
    {
        Task<Order> PlaceOrderAsync(List<OrderItem> items);
        Task CancelOrderAsync(Guid orderId);
        Task ProcessPendingOrdersAsync();
        Task<Order> GetOrderAsync(Guid orderId);
    }
}
