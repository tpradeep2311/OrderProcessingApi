using OrderProcessingApi.Models;
using System.Collections.Concurrent;

namespace OrderProcessingApi.Repositories
{
    public class InMemoryOrderRepository : IOrderRepository
    {
        private readonly ConcurrentDictionary<Guid, Order> _orders = new();

        public Task AddAsync(Order order)
        {
            order.Id = Guid.NewGuid();
            _orders.TryAdd(order.Id, order);
            return Task.CompletedTask;
        }

        public Task<IEnumerable<Order>> GetAllAsync()
        {
            return Task.FromResult(_orders.Values.AsEnumerable());
        }

        public Task<Order> GetByIdAsync(Guid id)
        {
            _orders.TryGetValue(id, out var order);
            return Task.FromResult(order);
        }

        public Task UpdateAsync(Order order)
        {
            _orders[order.Id] = order;
            return Task.CompletedTask;
        }
    }
}
