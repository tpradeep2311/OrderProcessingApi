using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using Xunit;
using OrderProcessingApi.Models;
using OrderProcessingApi.Repositories;
using OrderProcessingApi.Services;

namespace OrderProcessingApi.Tests
{
    public class OrderServiceTests
    {
        [Fact]
        public async Task PlaceOrder_Success()
        {
            // Arrange
            var productId = Guid.NewGuid();
            var product = new Product { Id = productId, Name = "Test Product", Price = 10m, StockQuantity = 10 };
            var orderItem = new OrderItem { ProductId = productId, Quantity = 2 };

            var mockProductRepo = new Mock<IProductRepository>();
            var mockOrderRepo = new Mock<IOrderRepository>();
            var mockNotificationService = new Mock<INotificationService>();

            mockProductRepo.Setup(r => r.GetByIdAsync(productId)).ReturnsAsync(product);
            mockProductRepo.Setup(r => r.UpdateAsync(It.IsAny<Product>())).Returns(Task.CompletedTask);
            mockOrderRepo.Setup(r => r.AddAsync(It.IsAny<Order>())).Returns(Task.CompletedTask);

            var orderService = new OrderService(mockOrderRepo.Object, mockProductRepo.Object, mockNotificationService.Object);

            // Act
            var order = await orderService.PlaceOrderAsync(new List<OrderItem> { orderItem });

            // Assert
            Assert.NotNull(order);
            Assert.Single(order.Items);
            Assert.Equal(OrderStatus.Pending, order.Status);
            Assert.Equal(8, product.StockQuantity);
            mockProductRepo.Verify(r => r.UpdateAsync(It.Is<Product>(p => p.Id == productId && p.StockQuantity == 8)), Times.Once);
        }

        [Fact]
        public async Task PlaceOrder_InsufficientStock_ThrowsException()
        {
            // Arrange
            var productId = Guid.NewGuid();
            var product = new Product { Id = productId, Name = "Test Product", Price = 10m, StockQuantity = 1 };
            var orderItem = new OrderItem { ProductId = productId, Quantity = 2 };

            var mockProductRepo = new Mock<IProductRepository>();
            var mockOrderRepo = new Mock<IOrderRepository>();
            var mockNotificationService = new Mock<INotificationService>();

            mockProductRepo.Setup(r => r.GetByIdAsync(productId)).ReturnsAsync(product);

            var orderService = new OrderService(mockOrderRepo.Object, mockProductRepo.Object, mockNotificationService.Object);

            // Act & Assert
            var ex = await Assert.ThrowsAsync<Exception>(() => orderService.PlaceOrderAsync(new List<OrderItem> { orderItem }));
            Assert.Contains("Insufficient stock", ex.Message);
        }

        [Fact]
        public async Task CancelOrder_Success()
        {
            // Arrange
            var productId = Guid.NewGuid();
            // Assume initial stock of 5 units
            var product = new Product { Id = productId, Name = "Test Product", Price = 10m, StockQuantity = 5 };
            var orderItem = new OrderItem { ProductId = productId, Quantity = 2 };

            var order = new Order
            {
                Id = Guid.NewGuid(),
                Items = new List<OrderItem> { orderItem },
                Status = OrderStatus.Pending
            };

            var mockProductRepo = new Mock<IProductRepository>();
            var mockOrderRepo = new Mock<IOrderRepository>();
            var mockNotificationService = new Mock<INotificationService>();

            mockProductRepo.Setup(r => r.GetByIdAsync(productId)).ReturnsAsync(product);
            mockProductRepo.Setup(r => r.UpdateAsync(It.IsAny<Product>())).Returns(Task.CompletedTask);

            mockOrderRepo.Setup(r => r.GetByIdAsync(order.Id)).ReturnsAsync(order);
            mockOrderRepo.Setup(r => r.UpdateAsync(It.IsAny<Order>())).Returns(Task.CompletedTask);

            var orderService = new OrderService(mockOrderRepo.Object, mockProductRepo.Object, mockNotificationService.Object);

            // Act
            await orderService.CancelOrderAsync(order.Id);

            // Assert
            Assert.Equal(OrderStatus.Canceled, order.Status);
            // Stock should be restored: initial 5 + 2 from canceled order = 7
            Assert.Equal(7, product.StockQuantity);
            mockProductRepo.Verify(r => r.UpdateAsync(It.Is<Product>(p => p.Id == productId && p.StockQuantity == 7)), Times.Once);
        }

        [Fact]
        public async Task ProcessPendingOrders_FulfillsOrderAndSendsNotification()
        {
            // Arrange
            var orderId = Guid.NewGuid();
            var order = new Order
            {
                Id = orderId,
                Items = new List<OrderItem>(),
                Status = OrderStatus.Pending
            };

            var mockOrderRepo = new Mock<IOrderRepository>();
            var mockProductRepo = new Mock<IProductRepository>();
            var mockNotificationService = new Mock<INotificationService>();

            // Setup the order repository to return our pending order
            mockOrderRepo.Setup(r => r.GetAllAsync()).ReturnsAsync(new List<Order> { order });
            mockOrderRepo.Setup(r => r.UpdateAsync(It.IsAny<Order>())).Returns(Task.CompletedTask);

            var orderService = new OrderService(mockOrderRepo.Object, mockProductRepo.Object, mockNotificationService.Object);

            // Act
            await orderService.ProcessPendingOrdersAsync();

            // Assert
            Assert.Equal(OrderStatus.Fulfilled, order.Status);
            // Verify that a notification is sent with the order ID in the message
            mockNotificationService.Verify(n => n.SendNotification(It.Is<string>(s => s.Contains(orderId.ToString()))), Times.Once);
        }

        [Fact]
        public async Task ConcurrentOrderPlacement_UpdatesInventoryCorrectly()
        {
            // Arrange
            var productId = Guid.NewGuid();
            // Assume product starts with 100 units in stock
            var product = new Product { Id = productId, Name = "Concurrent Product", Price = 20m, StockQuantity = 100 };

            var mockProductRepo = new Mock<IProductRepository>();
            var mockOrderRepo = new Mock<IOrderRepository>();
            var mockNotificationService = new Mock<INotificationService>();

            // Use a shared product instance for all repository calls
            mockProductRepo.Setup(r => r.GetByIdAsync(productId)).ReturnsAsync(() => product);
            mockProductRepo.Setup(r => r.UpdateAsync(It.IsAny<Product>())).Returns(Task.CompletedTask);
            mockOrderRepo.Setup(r => r.AddAsync(It.IsAny<Order>())).Returns(Task.CompletedTask);

            var orderService = new OrderService(mockOrderRepo.Object, mockProductRepo.Object, mockNotificationService.Object);

            // Create 50 concurrent orders, each ordering 1 unit of the product
            int concurrentOrders = 50;
            var tasks = new List<Task<Order>>();
            for (int i = 0; i < concurrentOrders; i++)
            {
                var orderItem = new OrderItem { ProductId = productId, Quantity = 1 };
                tasks.Add(orderService.PlaceOrderAsync(new List<OrderItem> { orderItem }));
            }

            // Act
            var orders = await Task.WhenAll(tasks);

            // Assert
            Assert.Equal(concurrentOrders, orders.Length);
            // Final stock should be 100 - 50 = 50
            Assert.Equal(50, product.StockQuantity);
        }
    }
}
