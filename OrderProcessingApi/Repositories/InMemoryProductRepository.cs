using OrderProcessingApi.Models;
using System.Collections.Concurrent;

namespace OrderProcessingApi.Repositories
{
    public class InMemoryProductRepository : IProductRepository
    {
        private readonly ConcurrentDictionary<Guid, Product> _products = new();

        public Task AddAsync(Product product)
        {
            product.Id = Guid.NewGuid();
            _products.TryAdd(product.Id, product);
            return Task.CompletedTask;
        }

        public Task DeleteAsync(Guid id)
        {
            _products.TryRemove(id, out _);
            return Task.CompletedTask;
        }

        public Task<IEnumerable<Product>> GetAllAsync()
        {
            return Task.FromResult(_products.Values.AsEnumerable());
        }

        public Task<Product> GetByIdAsync(Guid id)
        {
            _products.TryGetValue(id, out var product);
            return Task.FromResult(product);
        }

        public Task UpdateAsync(Product product)
        {
            _products[product.Id] = product;
            return Task.CompletedTask;
        }
    }
}
