using Data.Context;
using Faktureringsys.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Webapi.Controllers;
using Webapi.Dtos.Customers;

namespace Tests.Controllers
{
    public class CustomersControllerTests
    {
        private ApplicationDbContext CreateInMemoryContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;
            return new ApplicationDbContext(options);
        }

        [Fact]
        public async Task GetCustomers_ReturnsOkWithAllCustomers()
        {
            using var context = CreateInMemoryContext(nameof(GetCustomers_ReturnsOkWithAllCustomers));
            context.Customers.AddRange(
                new Customer { Name = "Alice", Email = "alice@example.com", Address = "1 Main St" },
                new Customer { Name = "Bob", Email = "bob@example.com", Address = "2 Side St" }
            );
            await context.SaveChangesAsync();

            var controller = new CustomersController(context);
            var result = await controller.GetCustomers();

            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var customers = Assert.IsAssignableFrom<IEnumerable<CustomerResponseDto>>(ok.Value);
            Assert.Equal(2, customers.Count());
        }

        [Fact]
        public async Task GetCustomer_ExistingId_ReturnsOkWithCustomer()
        {
            using var context = CreateInMemoryContext(nameof(GetCustomer_ExistingId_ReturnsOkWithCustomer));
            context.Customers.Add(new Customer { Id = 1, Name = "Alice", Email = "alice@example.com" });
            await context.SaveChangesAsync();

            var controller = new CustomersController(context);
            var result = await controller.GetCustomer(1);

            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var dto = Assert.IsType<CustomerResponseDto>(ok.Value);
            Assert.Equal("Alice", dto.Name);
        }

        [Fact]
        public async Task GetCustomer_NonExistingId_ReturnsNotFound()
        {
            using var context = CreateInMemoryContext(nameof(GetCustomer_NonExistingId_ReturnsNotFound));
            var controller = new CustomersController(context);

            var result = await controller.GetCustomer(99);

            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task CreateCustomer_ValidDto_ReturnsCreated()
        {
            using var context = CreateInMemoryContext(nameof(CreateCustomer_ValidDto_ReturnsCreated));
            var controller = new CustomersController(context);
            var dto = new CreateCustomerDto { Name = "Charlie", Email = "charlie@example.com", Address = "3 New Ave" };

            var result = await controller.CreateCustomer(dto);

            var created = Assert.IsType<CreatedAtActionResult>(result.Result);
            var response = Assert.IsType<CustomerResponseDto>(created.Value);
            Assert.Equal("Charlie", response.Name);
            Assert.Equal(1, await context.Customers.CountAsync());
        }

        [Fact]
        public async Task UpdateCustomer_ExistingId_ReturnsNoContent()
        {
            using var context = CreateInMemoryContext(nameof(UpdateCustomer_ExistingId_ReturnsNoContent));
            context.Customers.Add(new Customer { Id = 1, Name = "Alice", Email = "alice@example.com" });
            await context.SaveChangesAsync();

            var controller = new CustomersController(context);
            var dto = new UpdateCustomerDto { Name = "Alice Updated", Email = "alice2@example.com", Address = "New Address" };

            var result = await controller.UpdateCustomer(1, dto);

            Assert.IsType<NoContentResult>(result);
            var updated = await context.Customers.FindAsync(1);
            Assert.Equal("Alice Updated", updated!.Name);
        }

        [Fact]
        public async Task UpdateCustomer_NonExistingId_ReturnsNotFound()
        {
            using var context = CreateInMemoryContext(nameof(UpdateCustomer_NonExistingId_ReturnsNotFound));
            var controller = new CustomersController(context);
            var dto = new UpdateCustomerDto { Name = "Ghost" };

            var result = await controller.UpdateCustomer(99, dto);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task DeleteCustomer_ExistingId_ReturnsNoContent()
        {
            using var context = CreateInMemoryContext(nameof(DeleteCustomer_ExistingId_ReturnsNoContent));
            context.Customers.Add(new Customer { Id = 1, Name = "Alice" });
            await context.SaveChangesAsync();

            var controller = new CustomersController(context);
            var result = await controller.DeleteCustomer(1);

            Assert.IsType<NoContentResult>(result);
            Assert.Equal(0, await context.Customers.CountAsync());
        }

        [Fact]
        public async Task DeleteCustomer_NonExistingId_ReturnsNotFound()
        {
            using var context = CreateInMemoryContext(nameof(DeleteCustomer_NonExistingId_ReturnsNotFound));
            var controller = new CustomersController(context);

            var result = await controller.DeleteCustomer(99);

            Assert.IsType<NotFoundResult>(result);
        }
    }
}
