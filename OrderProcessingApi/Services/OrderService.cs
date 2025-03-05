using OrderProcessingApi.Models;
using OrderProcessingApi.Repositories;

namespace OrderProcessingApi.Services
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepo;
        private readonly IProductRepository _productRepo;
        private readonly INotificationService _notificationService;
        private readonly object _lock = new();

        public OrderService(IOrderRepository orderRepo, IProductRepository productRepo, INotificationService notificationService)
        {
            _orderRepo = orderRepo;
            _productRepo = productRepo;
            _notificationService = notificationService;
        }

        public async Task<Order> PlaceOrderAsync(List<OrderItem> items)
        {
            // Validate inventory and reserve stock
            foreach (var item in items)
            {
                var product = await _productRepo.GetByIdAsync(item.ProductId);
                if (product == null)
                    throw new Exception("Product not found.");
                if (product.StockQuantity < item.Quantity)
                    throw new Exception($"Insufficient stock for product {product.Name}.");
            }

            // Reserve stock with thread-safety
            foreach (var item in items)
            {
                lock (_lock)
                {
                    var product = _productRepo.GetByIdAsync(item.ProductId).Result;
                    if (product.StockQuantity < item.Quantity)
                        throw new Exception($"Insufficient stock for product {product.Name} during reservation.");
                    product.StockQuantity -= item.Quantity;
                    _productRepo.UpdateAsync(product).Wait();
                }
            }

            // Create and save order
            var order = new Order 
            { 
                Items = items, 
                Status = OrderStatus.Pending,
                CreatedAt = DateTime.UtcNow
            };
            await _orderRepo.AddAsync(order);
            return order;
        }

        public async Task CancelOrderAsync(Guid orderId)
        {
            var order = await _orderRepo.GetByIdAsync(orderId);
            if (order == null)
                throw new Exception("Order not found.");
            if (order.Status != OrderStatus.Pending)
                throw new Exception("Only pending orders can be canceled.");

            // Restore stock
            foreach (var item in order.Items)
            {
                lock (_lock)
                {
                    var product = _productRepo.GetByIdAsync(item.ProductId).Result;
                    product.StockQuantity += item.Quantity;
                    _productRepo.UpdateAsync(product).Wait();
                }
            }

            order.Status = OrderStatus.Canceled;
            await _orderRepo.UpdateAsync(order);
        }

        public async Task ProcessPendingOrdersAsync()
        {
            // Process each pending order (simulate processing delay)
            var orders = await _orderRepo.GetAllAsync();
            foreach (var order in orders.Where(o => o.Status == OrderStatus.Pending))
            {
                if (DateTime.UtcNow - order.CreatedAt < TimeSpan.FromSeconds(100))
                {
                    // Not yet ready to be fulfilled
                    continue;
                }

                await Task.Delay(1000); // Simulate delay
                order.Status = OrderStatus.Fulfilled;
                await _orderRepo.UpdateAsync(order);
                _notificationService.SendNotification($"Order {order.Id} fulfilled.");
            }
        }

        public async Task<Order> GetOrderAsync(Guid orderId)
        {
            return await _orderRepo.GetByIdAsync(orderId);
        }
    }
}
