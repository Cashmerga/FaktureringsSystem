using Business.Dtos.Invoices;
using Business.Sevices;
using Data.Context;
using Faktureringsys.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;
using Webapi.Controllers;

namespace Tests.Controllers
{
    public class InvoicesControllerTests
    {
        private ApplicationDbContext CreateInMemoryContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;
            return new ApplicationDbContext(options);
        }

        private static Invoice MakeInvoice(int id, Customer customer)
        {
            var product = new Product { Id = 1, Name = "Widget", Price = 10m };
            var item = new InvoiceItem { Id = 1, Product = product, ProductId = 1, Quantity = 2, UnitPrice = 10m };
            return new Invoice
            {
                Id = id,
                CreatedAt = DateTime.UtcNow,
                Customer = customer,
                CustomerId = customer.Id,
                Items = new List<InvoiceItem> { item },
                SubTotal = 20m,
                VATPercentage = 25m,
                VATAmount = 5m,
                TotalAmount = 25m
            };
        }

        [Fact]
        public async Task GetInvoices_ReturnsOkWithAllInvoices()
        {
            using var context = CreateInMemoryContext(nameof(GetInvoices_ReturnsOkWithAllInvoices));
            var customer = new Customer { Id = 1, Name = "Alice" };
            context.Customers.Add(customer);
            context.Invoices.Add(MakeInvoice(1, customer));
            await context.SaveChangesAsync();

            var invoiceServiceMock = new Mock<IInvoiceService>();
            var pdfServiceMock = new Business.Sevices.PdfService();
            var controller = new InvoicesController(context, invoiceServiceMock.Object, pdfServiceMock);

            var result = await controller.GetInvoices();

            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var invoices = Assert.IsAssignableFrom<IEnumerable<InvoiceResponseDto>>(ok.Value);
            Assert.Single(invoices);
        }

        [Fact]
        public async Task GetInvoice_ExistingId_ReturnsOkWithInvoice()
        {
            using var context = CreateInMemoryContext(nameof(GetInvoice_ExistingId_ReturnsOkWithInvoice));
            var customer = new Customer { Id = 1, Name = "Alice" };
            context.Customers.Add(customer);
            context.Invoices.Add(MakeInvoice(1, customer));
            await context.SaveChangesAsync();

            var invoiceServiceMock = new Mock<IInvoiceService>();
            var pdfServiceMock = new Business.Sevices.PdfService();
            var controller = new InvoicesController(context, invoiceServiceMock.Object, pdfServiceMock);

            var result = await controller.GetInvoice(1);

            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var dto = Assert.IsType<InvoiceResponseDto>(ok.Value);
            Assert.Equal(1, dto.Id);
            Assert.Equal("Alice", dto.CustomerName);
        }

        [Fact]
        public async Task GetInvoice_NonExistingId_ReturnsNotFound()
        {
            using var context = CreateInMemoryContext(nameof(GetInvoice_NonExistingId_ReturnsNotFound));
            var invoiceServiceMock = new Mock<IInvoiceService>();
            var pdfServiceMock = new Business.Sevices.PdfService();
            var controller = new InvoicesController(context, invoiceServiceMock.Object, pdfServiceMock);

            var result = await controller.GetInvoice(99);

            Assert.IsType<NotFoundResult>(result.Result);
        }

        [Fact]
        public async Task CreateInvoice_ValidDto_ReturnsCreated()
        {
            using var context = CreateInMemoryContext(nameof(CreateInvoice_ValidDto_ReturnsCreated));
            var customer = new Customer { Id = 1, Name = "Alice" };
            var product = new Product { Id = 1, Name = "Widget", Price = 10m };
            context.Customers.Add(customer);
            context.Products.Add(product);
            await context.SaveChangesAsync();

            var createdInvoice = MakeInvoice(1, customer);

            var invoiceServiceMock = new Mock<IInvoiceService>();
            invoiceServiceMock
                .Setup(s => s.CreateInvoiceAsync(It.IsAny<CreateInvoiceDto>()))
                .ReturnsAsync(createdInvoice);

            var pdfServiceMock = new Business.Sevices.PdfService();
            var controller = new InvoicesController(context, invoiceServiceMock.Object, pdfServiceMock);

            var dto = new CreateInvoiceDto
            {
                CustomerId = 1,
                Items = new List<CreateInvoiceItemDto> { new CreateInvoiceItemDto { ProductId = 1, Quantity = 2 } }
            };

            var result = await controller.CreateInvoice(dto);

            var created = Assert.IsType<CreatedAtActionResult>(result.Result);
            var response = Assert.IsType<InvoiceResponseDto>(created.Value);
            Assert.Equal(1, response.Id);
            Assert.Equal("Alice", response.CustomerName);
        }

        [Fact]
        public async Task CreateInvoice_ServiceThrows_ReturnsBadRequest()
        {
            using var context = CreateInMemoryContext(nameof(CreateInvoice_ServiceThrows_ReturnsBadRequest));
            var invoiceServiceMock = new Mock<IInvoiceService>();
            invoiceServiceMock
                .Setup(s => s.CreateInvoiceAsync(It.IsAny<CreateInvoiceDto>()))
                .ThrowsAsync(new Exception("Customer not found"));

            var pdfServiceMock = new Business.Sevices.PdfService();
            var controller = new InvoicesController(context, invoiceServiceMock.Object, pdfServiceMock);

            var dto = new CreateInvoiceDto
            {
                CustomerId = 99,
                Items = new List<CreateInvoiceItemDto> { new CreateInvoiceItemDto { ProductId = 1, Quantity = 1 } }
            };

            var result = await controller.CreateInvoice(dto);

            var bad = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.Equal("Customer not found", bad.Value);
        }

        [Fact]
        public async Task DeleteInvoice_ExistingId_ReturnsNoContent()
        {
            using var context = CreateInMemoryContext(nameof(DeleteInvoice_ExistingId_ReturnsNoContent));
            var customer = new Customer { Id = 1, Name = "Alice" };
            context.Customers.Add(customer);
            context.Invoices.Add(MakeInvoice(1, customer));
            await context.SaveChangesAsync();

            var invoiceServiceMock = new Mock<IInvoiceService>();
            var pdfServiceMock = new Business.Sevices.PdfService();
            var controller = new InvoicesController(context, invoiceServiceMock.Object, pdfServiceMock);

            var result = await controller.DeleteInvoice(1);

            Assert.IsType<NoContentResult>(result);
            Assert.Equal(0, await context.Invoices.CountAsync());
        }

        [Fact]
        public async Task DeleteInvoice_NonExistingId_ReturnsNotFound()
        {
            using var context = CreateInMemoryContext(nameof(DeleteInvoice_NonExistingId_ReturnsNotFound));
            var invoiceServiceMock = new Mock<IInvoiceService>();
            var pdfServiceMock = new Business.Sevices.PdfService();
            var controller = new InvoicesController(context, invoiceServiceMock.Object, pdfServiceMock);

            var result = await controller.DeleteInvoice(99);

            Assert.IsType<NotFoundResult>(result);
        }

        [Fact]
        public async Task UpdateInvoice_NotFound_ReturnsNotFound()
        {
            using var context = CreateInMemoryContext(nameof(UpdateInvoice_NotFound_ReturnsNotFound));
            var invoiceServiceMock = new Mock<IInvoiceService>();
            invoiceServiceMock
                .Setup(s => s.UpdateInvoiceAsync(It.IsAny<int>(), It.IsAny<UpdateInvoiceDto>()))
                .ThrowsAsync(new Exception("Invoice not found"));

            var pdfServiceMock = new Business.Sevices.PdfService();
            var controller = new InvoicesController(context, invoiceServiceMock.Object, pdfServiceMock);

            var result = await controller.UpdateInvoice(99, new UpdateInvoiceDto());

            Assert.IsType<NotFoundResult>(result);
        }
    }
}
