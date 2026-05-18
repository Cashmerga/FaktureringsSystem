using Business.Dtos.Invoices;
using Business.Sevices;
using Data.Context;
using Faktureringsys.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.EntityFrameworkCore;
using Moq;
using Webapi.Controllers;
using Webapi.ViewModels;

namespace Tests.ViewControllers
{
    public class InvoiceViewControllerTests
    {
        private ApplicationDbContext CreateInMemoryContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;
            return new ApplicationDbContext(options);
        }

        private static void SetupTempData(Controller controller)
        {
            var tempData = new Mock<ITempDataDictionary>();
            controller.TempData = tempData.Object;
        }

        private static (Customer customer, Product product, Invoice invoice) SeedInvoice(ApplicationDbContext context)
        {
            var customer = new Customer { Id = 1, Name = "Alice", Email = "alice@example.com" };
            var product = new Product { Id = 1, Name = "Widget", Price = 100m };
            var invoice = new Invoice
            {
                Id = 1,
                CustomerId = 1,
                Customer = customer,
                CreatedAt = DateTime.UtcNow,
                SubTotal = 200m,
                VATPercentage = 25m,
                VATAmount = 50m,
                TotalAmount = 250m,
                Items = new List<InvoiceItem>
                {
                    new InvoiceItem { Id = 1, ProductId = 1, Product = product, Quantity = 2, UnitPrice = 100m }
                }
            };

            context.Customers.Add(customer);
            context.Products.Add(product);
            context.Invoices.Add(invoice);
            context.SaveChanges();

            return (customer, product, invoice);
        }

        // ── Index ─────────────────────────────────────────────────────────────

        [Fact]
        public async Task Index_ReturnsViewWithInvoiceIndexViewModel()
        {
            using var context = CreateInMemoryContext(nameof(Index_ReturnsViewWithInvoiceIndexViewModel));
            SeedInvoice(context);

            var controller = new InvoiceViewController(context, new Mock<IInvoiceService>().Object);
            var result = await controller.Index();

            var view = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<InvoiceIndexViewModel>(view.Model);
            Assert.Single(model.Invoices);
            Assert.Equal("Alice", model.Invoices[0].CustomerName);
        }

        [Fact]
        public async Task Index_EmptyDb_ReturnsViewWithEmptyList()
        {
            using var context = CreateInMemoryContext(nameof(Index_EmptyDb_ReturnsViewWithEmptyList));

            var controller = new InvoiceViewController(context, new Mock<IInvoiceService>().Object);
            var result = await controller.Index();

            var view = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<InvoiceIndexViewModel>(view.Model);
            Assert.Empty(model.Invoices);
        }

        // ── Details ───────────────────────────────────────────────────────────

        [Fact]
        public async Task Details_ExistingId_ReturnsViewWithInvoiceDetailsViewModel()
        {
            using var context = CreateInMemoryContext(nameof(Details_ExistingId_ReturnsViewWithInvoiceDetailsViewModel));
            SeedInvoice(context);

            var controller = new InvoiceViewController(context, new Mock<IInvoiceService>().Object);
            var result = await controller.Details(1);

            var view = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<InvoiceDetailsViewModel>(view.Model);
            Assert.Equal(1, model.Invoice.Id);
            Assert.Equal("Alice", model.Invoice.CustomerName);
            Assert.Equal("Alice", model.Customer.Name);
            Assert.Single(model.Invoice.Items);
        }

        [Fact]
        public async Task Details_NonExistingId_ReturnsNotFound()
        {
            using var context = CreateInMemoryContext(nameof(Details_NonExistingId_ReturnsNotFound));

            var controller = new InvoiceViewController(context, new Mock<IInvoiceService>().Object);
            var result = await controller.Details(99);

            Assert.IsType<NotFoundResult>(result);
        }

        // ── Create GET ────────────────────────────────────────────────────────

        [Fact]
        public async Task Create_Get_ReturnsViewWithFormViewModel()
        {
            using var context = CreateInMemoryContext(nameof(Create_Get_ReturnsViewWithFormViewModel));
            context.Customers.Add(new Customer { Name = "Alice" });
            context.Products.Add(new Product { Name = "Widget", Price = 10m });
            await context.SaveChangesAsync();

            var controller = new InvoiceViewController(context, new Mock<IInvoiceService>().Object);
            var result = await controller.Create();

            var view = Assert.IsType<ViewResult>(result);
            var model = Assert.IsType<InvoiceFormViewModel>(view.Model);
            Assert.Single(model.Customers);
            Assert.Single(model.Products);
        }

        // ── Create POST ───────────────────────────────────────────────────────

        [Fact]
        public async Task Create_Post_ValidModel_RedirectsToDetails()
        {
            using var context = CreateInMemoryContext(nameof(Create_Post_ValidModel_RedirectsToDetails));
            var customer = new Customer { Id = 1, Name = "Alice" };
            var product = new Product { Id = 1, Name = "Widget", Price = 100m };
            context.Customers.Add(customer);
            context.Products.Add(product);
            await context.SaveChangesAsync();

            var createdInvoice = new Invoice
            {
                Id = 42,
                CustomerId = 1,
                Customer = customer,
                Items = new List<InvoiceItem>
                {
                    new InvoiceItem { ProductId = 1, Product = product, Quantity = 1, UnitPrice = 100m }
                },
                SubTotal = 100m,
                VATPercentage = 25m,
                VATAmount = 25m,
                TotalAmount = 125m
            };

            var invoiceServiceMock = new Mock<IInvoiceService>();
            invoiceServiceMock
                .Setup(s => s.CreateInvoiceAsync(It.IsAny<CreateInvoiceDto>()))
                .ReturnsAsync(createdInvoice);

            var controller = new InvoiceViewController(context, invoiceServiceMock.Object);
            SetupTempData(controller);

            var model = new InvoiceCreateViewModel
            {
                CustomerId = 1,
                Items = new List<InvoiceItemViewModel>
                {
                    new InvoiceItemViewModel { ProductId = 1, Quantity = 1 }
                }
            };

            var result = await controller.Create(model);

            var redirect = Assert.IsType<RedirectToActionResult>(result);
            Assert.Equal("Details", redirect.ActionName);
            Assert.Equal(42, redirect.RouteValues!["id"]);
        }

        [Fact]
        public async Task Create_Post_NoItems_ReturnsViewWithModelError()
        {
            using var context = CreateInMemoryContext(nameof(Create_Post_NoItems_ReturnsViewWithModelError));
            context.Customers.Add(new Customer { Name = "Alice" });
            context.Products.Add(new Product { Name = "Widget", Price = 10m });
            await context.SaveChangesAsync();

            var controller = new InvoiceViewController(context, new Mock<IInvoiceService>().Object);
            var model = new InvoiceCreateViewModel { CustomerId = 1, Items = new List<InvoiceItemViewModel>() };

            var result = await controller.Create(model);

            var view = Assert.IsType<ViewResult>(result);
            Assert.IsType<InvoiceFormViewModel>(view.Model);
            Assert.False(controller.ModelState.IsValid);
        }

        [Fact]
        public async Task Create_Post_InvalidModelState_ReturnsViewWithFormViewModel()
        {
            using var context = CreateInMemoryContext(nameof(Create_Post_InvalidModelState_ReturnsViewWithFormViewModel));
            context.Customers.Add(new Customer { Name = "Alice" });
            context.Products.Add(new Product { Name = "Widget", Price = 10m });
            await context.SaveChangesAsync();

            var controller = new InvoiceViewController(context, new Mock<IInvoiceService>().Object);
            controller.ModelState.AddModelError("CustomerId", "Required");
            var model = new InvoiceCreateViewModel
            {
                Items = new List<InvoiceItemViewModel>
                {
                    new InvoiceItemViewModel { ProductId = 1, Quantity = 1 }
                }
            };

            var result = await controller.Create(model);

            var view = Assert.IsType<ViewResult>(result);
            var vm = Assert.IsType<InvoiceFormViewModel>(view.Model);
            Assert.Single(vm.Customers);
            Assert.Single(vm.Products);
        }

        [Fact]
        public async Task Create_Post_ServiceThrows_ReturnsViewWithModelError()
        {
            using var context = CreateInMemoryContext(nameof(Create_Post_ServiceThrows_ReturnsViewWithModelError));
            context.Customers.Add(new Customer { Name = "Alice" });
            context.Products.Add(new Product { Name = "Widget", Price = 10m });
            await context.SaveChangesAsync();

            var invoiceServiceMock = new Mock<IInvoiceService>();
            invoiceServiceMock
                .Setup(s => s.CreateInvoiceAsync(It.IsAny<CreateInvoiceDto>()))
                .ThrowsAsync(new Exception("Customer not found."));

            var controller = new InvoiceViewController(context, invoiceServiceMock.Object);
            var model = new InvoiceCreateViewModel
            {
                CustomerId = 99,
                Items = new List<InvoiceItemViewModel>
                {
                    new InvoiceItemViewModel { ProductId = 1, Quantity = 1 }
                }
            };

            var result = await controller.Create(model);

            var view = Assert.IsType<ViewResult>(result);
            Assert.IsType<InvoiceFormViewModel>(view.Model);
            Assert.False(controller.ModelState.IsValid);
        }
    }
}
