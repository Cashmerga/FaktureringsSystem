using Data.Context;
using Faktureringsys.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using Moq;
using Webapi.Controllers;
using Webapi.Dtos.Products;

namespace Tests.ViewControllers
{
    public class ProductViewControllerTests
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
        public async Task Index_ReturnsViewWithAllProducts()
        {
            using var context = CreateInMemoryContext(nameof(Index_ReturnsViewWithAllProducts));
            context.Products.AddRange(
                new Product { Name = "Widget", Price = 9.99m },
                new Product { Name = "Gadget", Price = 19.99m }
            );
            await context.SaveChangesAsync();

            var controller = new ProductViewController(context);
            var result = await controller.Index();

            var view = Assert.IsType<ViewResult>(result);
            var model = Assert.IsAssignableFrom<IEnumerable<ProductResponseDto>>(view.Model);
            Assert.Equal(2, model.Count());
        }

        [Fact]
        public async Task Details_ExistingId_ReturnsViewWithProduct()
        {
            using var context = CreateInMemoryContext(nameof(Details_ExistingId_ReturnsViewWithProduct));
            context.Products.Add(new Product { Id = 1, Name = "Widget", Price = 9.99m });
            await context.SaveChangesAsync();

            var controller = new ProductViewController(context);
            var result = await controller.Details(1);

            var view = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<ProductResponseDto>(view.Model);
            Assert.Equal("Widget", model.Name);
        }

        [Fact]
        public async Task Details_NonExistingId_ReturnsNotFound()
        {
            using var context = CreateInMemoryContext(nameof(Details_NonExistingId_ReturnsNotFound));
            var controller = new ProductViewController(context);

            var result = await controller.Details(99);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public void Create_Get_ReturnsView()
        {
            using var context = CreateInMemoryContext(nameof(Create_Get_ReturnsView));
            var controller = new ProductViewController(context);

            var result = controller.Create();

            Assert.IsType<ViewResult>(result);
        }

        [Fact]
        public async Task Create_Post_ValidDto_RedirectsToIndex()
        {
            using var context = CreateInMemoryContext(nameof(Create_Post_ValidDto_RedirectsToIndex));
            var controller = new ProductViewController(context);
            SetupTempData(controller);
            var dto = new CreateProductDto { Name = "Widget", Price = 9.99m, Description = "A widget" };

            var result = await controller.Create(dto);

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirect.ActionName);
            Assert.Equal(1, await context.Products.CountAsync());
        }

        [Fact]
        public async Task Create_Post_InvalidModel_ReturnsView()
        {
            using var context = CreateInMemoryContext(nameof(Create_Post_InvalidModel_ReturnsView));
            var controller = new ProductViewController(context);
            controller.ModelState.AddModelError("Name", "Required");
            var dto = new CreateProductDto { Name = "", Price = 0 };

            var result = await controller.Create(dto);

            Assert.IsType<ViewResult>(result);
            Assert.Equal(0, await context.Products.CountAsync());
        }

        [Fact]
        public async Task Edit_Get_ExistingId_ReturnsViewWithDto()
        {
            using var context = CreateInMemoryContext(nameof(Edit_Get_ExistingId_ReturnsViewWithDto));
            context.Products.Add(new Product { Id = 1, Name = "Widget", Price = 9.99m });
            await context.SaveChangesAsync();

            var controller = new ProductViewController(context);
            var result = await controller.Edit(1);

            var view = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<UpdateProductDto>(view.Model);
            Assert.Equal("Widget", model.Name);
            Assert.Equal(9.99m, model.Price);
        }

        [Fact]
        public async Task Edit_Get_NonExistingId_ReturnsNotFound()
        {
            using var context = CreateInMemoryContext(nameof(Edit_Get_NonExistingId_ReturnsNotFound));
            var controller = new ProductViewController(context);

            var result = await controller.Edit(99);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task Edit_Post_ValidDto_RedirectsToIndex()
        {
            using var context = CreateInMemoryContext(nameof(Edit_Post_ValidDto_RedirectsToIndex));
            context.Products.Add(new Product { Id = 1, Name = "Widget", Price = 9.99m });
            await context.SaveChangesAsync();

            var controller = new ProductViewController(context);
            SetupTempData(controller);
            var dto = new UpdateProductDto { Name = "Super Widget", Price = 14.99m, Description = "Updated" };

            var result = await controller.Edit(1, dto);

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirect.ActionName);
            var updated = await context.Products.FindAsync(1);
            Assert.Equal("Super Widget", updated!.Name);
            Assert.Equal(14.99m, updated.Price);
        }

        [Fact]
        public async Task DeleteConfirmed_ExistingId_RedirectsToIndex()
        {
            using var context = CreateInMemoryContext(nameof(DeleteConfirmed_ExistingId_RedirectsToIndex));
            context.Products.Add(new Product { Id = 1, Name = "Widget", Price = 9.99m });
            await context.SaveChangesAsync();

            var controller = new ProductViewController(context);
            SetupTempData(controller);
            var result = await controller.DeleteConfirmed(1);

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Index", redirect.ActionName);
            Assert.Equal(0, await context.Products.CountAsync());
        }

        [Fact]
        public async Task DeleteConfirmed_NonExistingId_ReturnsNotFound()
        {
            using var context = CreateInMemoryContext(nameof(DeleteConfirmed_NonExistingId_ReturnsNotFound));
            var controller = new ProductViewController(context);

            var result = await controller.DeleteConfirmed(99);

            Assert.IsType<NotFoundResult>(result);
        }
    }
}
