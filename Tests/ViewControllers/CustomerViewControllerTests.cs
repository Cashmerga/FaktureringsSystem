using Data.Context;
using Faktureringsys.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using Moq;
using Webapi.Controllers;
using Webapi.Dtos.Customers;

namespace Tests.ViewControllers
{
    public class CustomerViewControllerTests
    {
        private static void SetupTempData(Controller controller)
        {
            var tempData = new Mock<ITempDataDictionary>();
            controller.TempData = tempData.Object;
        }

        private ApplicationDbContext CreateInMemoryContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;
            return new ApplicationDbContext(options);
        }

        [Fact]
        public async Task Index_ReturnsViewWithAllCustomers()
        {
            using var context = CreateInMemoryContext(nameof(Index_ReturnsViewWithAllCustomers));
            context.Customers.AddRange(
                new Customer { Name = "Alice", Email = "alice@example.com" },
                new Customer { Name = "Bob", Email = "bob@example.com" }
            );
            await context.SaveChangesAsync();

            var controller = new CustomerViewController(context);
            var result = await controller.Index();

            var view = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<IEnumerable<CustomerResponseDto>>(view.Model);
            Assert.Equal(2, model.Count());
        }

        [Fact]
        public async Task Details_ExistingId_ReturnsViewWithCustomer()
        {
            using var context = CreateInMemoryContext(nameof(Details_ExistingId_ReturnsViewWithCustomer));
            context.Customers.Add(new Customer { Id = 1, Name = "Alice", Email = "alice@example.com" });
            await context.SaveChangesAsync();

            var controller = new CustomerViewController(context);
            var result = await controller.Details(1);

            var view = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<CustomerResponseDto>(view.Model);
            Assert.Equal("Alice", model.Name);
        }

        [Fact]
        public async Task Details_NonExistingId_ReturnsNotFound()
        {
            using var context = CreateInMemoryContext(nameof(Details_NonExistingId_ReturnsNotFound));
            var controller = new CustomerViewController(context);

            var result = await controller.Details(99);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public void Create_Get_ReturnsView()
        {
            using var context = CreateInMemoryContext(nameof(Create_Get_ReturnsView));
            var controller = new CustomerViewController(context);

            var result = controller.Create();

            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public async Task Create_Post_ValidDto_RedirectsToIndex()
        {
            using var context = CreateInMemoryContext(nameof(Create_Post_ValidDto_RedirectsToIndex));
            var controller = new CustomerViewController(context);
            SetupTempData(controller);
            var dto = new CreateCustomerDto { Name = "Charlie", Email = "charlie@example.com", Address = "3 New Ave" };

            var result = await controller.Create(dto);

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirect.ActionName);
            Assert.Equal(1, await context.Customers.CountAsync());
        }

        [Fact]
        public async Task Create_Post_InvalidModel_ReturnsView()
        {
            using var context = CreateInMemoryContext(nameof(Create_Post_InvalidModel_ReturnsView));
            var controller = new CustomerViewController(context);
            controller.ModelState.AddModelError("Name", "Required");
            var dto = new CreateCustomerDto { Name = "" };

            var result = await controller.Create(dto);

            Assert.IsType<ViewResult>(result);
            Assert.Equal(0, await context.Customers.CountAsync());
        }

        [Fact]
        public async Task Edit_Get_ExistingId_ReturnsViewWithDto()
        {
            using var context = CreateInMemoryContext(nameof(Edit_Get_ExistingId_ReturnsViewWithDto));
            context.Customers.Add(new Customer { Id = 1, Name = "Alice", Email = "alice@example.com" });
            await context.SaveChangesAsync();

            var controller = new CustomerViewController(context);
            var result = await controller.Edit(1);

            var view = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<UpdateCustomerDto>(view.Model);
            Assert.Equal("Alice", model.Name);
        }

        [Fact]
        public async Task Edit_Get_NonExistingId_ReturnsNotFound()
        {
            using var context = CreateInMemoryContext(nameof(Edit_Get_NonExistingId_ReturnsNotFound));
            var controller = new CustomerViewController(context);

            var result = await controller.Edit(99);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Edit_Post_ValidDto_RedirectsToIndex()
        {
            using var context = CreateInMemoryContext(nameof(Edit_Post_ValidDto_RedirectsToIndex));
            context.Customers.Add(new Customer { Id = 1, Name = "Alice", Email = "alice@example.com" });
            await context.SaveChangesAsync();

            var controller = new CustomerViewController(context);
            SetupTempData(controller);
            var dto = new UpdateCustomerDto { Name = "Alice Updated", Email = "alice2@example.com" };

            var result = await controller.Edit(1, dto);

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirect.ActionName);
            var updated = await context.Customers.FindAsync(1);
            Assert.Equal("Alice Updated", updated!.Name);
        }

        [Fact]
        public async Task DeleteConfirmed_ExistingId_RedirectsToIndex()
        {
            using var context = CreateInMemoryContext(nameof(DeleteConfirmed_ExistingId_RedirectsToIndex));
            context.Customers.Add(new Customer { Id = 1, Name = "Alice" });
            await context.SaveChangesAsync();

            var controller = new CustomerViewController(context);
            SetupTempData(controller);
            var result = await controller.DeleteConfirmed(1);

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirect.ActionName);
            Assert.Equal(0, await context.Customers.CountAsync());
        }

        [Fact]
        public async Task DeleteConfirmed_NonExistingId_ReturnsNotFound()
        {
            using var context = CreateInMemoryContext(nameof(DeleteConfirmed_NonExistingId_ReturnsNotFound));
            var controller = new CustomerViewController(context);

            var result = await controller.DeleteConfirmed(99);

            Assert.IsType<NotFoundResult>(result);
        }
    }
}
