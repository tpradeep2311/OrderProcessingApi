using System.Threading.Tasks;
using OrderProcessingApi.Models;
using OrderProcessingApi.Repositories;
using Xunit;

namespace OrderProcessingApi.Tests
{
    public class InMemoryProductRepositoryTests
    {
        [Fact]
        public async Task AddAndGetProduct_Success()
        {
            // Arrange
            var repository = new InMemoryProductRepository();
            var product = new Product { Name = "New Product", Price = 15.5m, StockQuantity = 20 };

            // Act
            await repository.AddAsync(product);
            var fetchedProduct = await repository.GetByIdAsync(product.Id);

            // Assert
            Assert.NotNull(fetchedProduct);
            Assert.Equal(product.Name, fetchedProduct.Name);
            Assert.Equal(product.Price, fetchedProduct.Price);
            Assert.Equal(product.StockQuantity, fetchedProduct.StockQuantity);
        }

        [Fact]
        public async Task UpdateProduct_Success()
        {
            // Arrange
            var repository = new InMemoryProductRepository();
            var product = new Product { Name = "Update Product", Price = 10m, StockQuantity = 5 };
            await repository.AddAsync(product);

            // Act
            product.Name = "Updated Name";
            product.Price = 12m;
            product.StockQuantity = 8;
            await repository.UpdateAsync(product);
            var updatedProduct = await repository.GetByIdAsync(product.Id);

            // Assert
            Assert.NotNull(updatedProduct);
            Assert.Equal("Updated Name", updatedProduct.Name);
            Assert.Equal(12m, updatedProduct.Price);
            Assert.Equal(8, updatedProduct.StockQuantity);
        }

        [Fact]
        public async Task DeleteProduct_Success()
        {
            // Arrange
            var repository = new InMemoryProductRepository();
            var product = new Product { Name = "Delete Product", Price = 20m, StockQuantity = 10 };
            await repository.AddAsync(product);

            // Act
            await repository.DeleteAsync(product.Id);
            var deletedProduct = await repository.GetByIdAsync(product.Id);

            // Assert
            Assert.Null(deletedProduct);
        }
    }
}
