using System;
using System.Threading.Tasks;
using OrderProcessingApi.Models;
using OrderProcessingApi.Repositories;
using Xunit;

namespace OrderProcessingApi.Tests
{
    public class InMemoryOrderRepositoryTests
    {
        [Fact]
        public async Task AddAndGetOrder_Success()
        {
            // Arrange
            var repository = new InMemoryOrderRepository();
            var order = new Order
            {
                Items = new System.Collections.Generic.List<OrderItem>
                {
                    new OrderItem { ProductId = Guid.NewGuid(), Quantity = 1 }
                },
                Status = OrderStatus.Pending
            };

            // Act
            await repository.AddAsync(order);
            var fetchedOrder = await repository.GetByIdAsync(order.Id);

            // Assert
            Assert.NotNull(fetchedOrder);
            Assert.Equal(order.Status, fetchedOrder.Status);
            Assert.Equal(order.Items.Count, fetchedOrder.Items.Count);
        }

        [Fact]
        public async Task UpdateOrder_Success()
        {
            // Arrange
            var repository = new InMemoryOrderRepository();
            var order = new Order
            {
                Items = new System.Collections.Generic.List<OrderItem>
                {
                    new OrderItem { ProductId = Guid.NewGuid(), Quantity = 2 }
                },
                Status = OrderStatus.Pending
            };

            await repository.AddAsync(order);

            // Act
            order.Status = OrderStatus.Fulfilled;
            await repository.UpdateAsync(order);
            var updatedOrder = await repository.GetByIdAsync(order.Id);

            // Assert
            Assert.NotNull(updatedOrder);
            Assert.Equal(OrderStatus.Fulfilled, updatedOrder.Status);
        }
    }
}
