using Data.Context;
using Faktureringsys.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Webapi.Controllers;
using Webapi.Dtos.Products;

namespace Tests.Controllers
{
    public class ProductsControllerTests
    {
        private ApplicationDbContext CreateInMemoryContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;
            return new ApplicationDbContext(options);
        }

        [Fact]
        public async Task GetProducts_ReturnsOkWithAllProducts()
        {
            using var context = CreateInMemoryContext(nameof(GetProducts_ReturnsOkWithAllProducts));
            context.Products.AddRange(
                new Product { Name = "Widget", Price = 9.99m, Description = "A widget" },
                new Product { Name = "Gadget", Price = 19.99m, Description = "A gadget" }
            );
            await context.SaveChangesAsync();

            var controller = new ProductsController(context);
            var result = await controller.GetProducts();

            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var products = Assert.IsAssignableFrom<IEnumerable<ProductResponseDto>>(ok.Value);
            Assert.Equal(2, products.Count());
        }

        [Fact]
        public async Task GetProduct_ExistingId_ReturnsOkWithProduct()
        {
            using var context = CreateInMemoryContext(nameof(GetProduct_ExistingId_ReturnsOkWithProduct));
            context.Products.Add(new Product { Id = 1, Name = "Widget", Price = 9.99m });
            await context.SaveChangesAsync();

            var controller = new ProductsController(context);
            var result = await controller.GetProduct(1);

            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var dto = Assert.IsType<ProductResponseDto>(ok.Value);
            Assert.Equal("Widget", dto.Name);
            Assert.Equal(9.99m, dto.Price);
        }

        [Fact]
        public async Task GetProduct_NonExistingId_ReturnsNotFound()
        {
            using var context = CreateInMemoryContext(nameof(GetProduct_NonExistingId_ReturnsNotFound));
            var controller = new ProductsController(context);

            var result = await controller.GetProduct(99);

            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task CreateProduct_ValidDto_ReturnsCreated()
        {
            using var context = CreateInMemoryContext(nameof(CreateProduct_ValidDto_ReturnsCreated));
            var controller = new ProductsController(context);
            var dto = new CreateProductDto { Name = "Widget", Price = 9.99m, Description = "A fine widget" };

            var result = await controller.CreateProduct(dto);

            var created = Assert.IsType<CreatedAtActionResult>(result.Result);
            var response = Assert.IsType<ProductResponseDto>(created.Value);
            Assert.Equal("Widget", response.Name);
            Assert.Equal(9.99m, response.Price);
            Assert.Equal(1, await context.Products.CountAsync());
        }

        [Fact]
        public async Task UpdateProduct_ExistingId_ReturnsNoContent()
        {
            using var context = CreateInMemoryContext(nameof(UpdateProduct_ExistingId_ReturnsNoContent));
            context.Products.Add(new Product { Id = 1, Name = "Widget", Price = 9.99m });
            await context.SaveChangesAsync();

            var controller = new ProductsController(context);
            var dto = new UpdateProductDto { Name = "Super Widget", Price = 14.99m, Description = "Updated" };

            var result = await controller.UpdateProduct(1, dto);

            Assert.IsType<NoContentResult>(result);
            var updated = await context.Products.FindAsync(1);
            Assert.Equal("Super Widget", updated!.Name);
            Assert.Equal(14.99m, updated.Price);
        }

        [Fact]
        public async Task UpdateProduct_NonExistingId_ReturnsNotFound()
        {
            using var context = CreateInMemoryContext(nameof(UpdateProduct_NonExistingId_ReturnsNotFound));
            var controller = new ProductsController(context);
            var dto = new UpdateProductDto { Name = "Ghost", Price = 1m };

            var result = await controller.UpdateProduct(99, dto);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task DeleteProduct_ExistingId_ReturnsNoContent()
        {
            using var context = CreateInMemoryContext(nameof(DeleteProduct_ExistingId_ReturnsNoContent));
            context.Products.Add(new Product { Id = 1, Name = "Widget", Price = 9.99m });
            await context.SaveChangesAsync();

            var controller = new ProductsController(context);
            var result = await controller.DeleteProduct(1);

            Assert.IsType<NoContentResult>(result);
            Assert.Equal(0, await context.Products.CountAsync());
        }

        [Fact]
        public async Task DeleteProduct_NonExistingId_ReturnsNotFound()
        {
            using var context = CreateInMemoryContext(nameof(DeleteProduct_NonExistingId_ReturnsNotFound));
            var controller = new ProductsController(context);

            var result = await controller.DeleteProduct(99);

            Assert.IsType<NotFoundResult>(result);
        }
    }
}
