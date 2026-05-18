using Business.Dtos.Invoices;
using Business.Sevices;
using Data.Context;
using Faktureringsys.Models;
using Microsoft.EntityFrameworkCore;

namespace Tests.Services
{
    public class InvoiceServiceTests
    {
        private ApplicationDbContext CreateInMemoryContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;
            return new ApplicationDbContext(options);
        }

        private static async Task SeedBaseDataAsync(ApplicationDbContext context)
        {
            context.Customers.Add(new Customer { Id = 1, Name = "Alice", Email = "alice@example.com" });
            context.Products.AddRange(
                new Product { Id = 1, Name = "Widget", Price = 100m },
                new Product { Id = 2, Name = "Gadget", Price = 50m }
            );
            await context.SaveChangesAsync();
        }

        // ── CreateInvoiceAsync ────────────────────────────────────────────────

        [Fact]
        public async Task CreateInvoiceAsync_ValidDto_CreatesInvoiceWithCorrectTotals()
        {
            using var context = CreateInMemoryContext(nameof(CreateInvoiceAsync_ValidDto_CreatesInvoiceWithCorrectTotals));
            await SeedBaseDataAsync(context);
            var service = new InvoiceService(context);

            var dto = new CreateInvoiceDto
            {
                CustomerId = 1,
                Items = new List<CreateInvoiceItemDto>
                {
                    new CreateInvoiceItemDto { ProductId = 1, Quantity = 2 },  // 2 x 100 = 200
                    new CreateInvoiceItemDto { ProductId = 2, Quantity = 3 }   // 3 x 50  = 150
                }
            };

            var invoice = await service.CreateInvoiceAsync(dto);

            Assert.NotNull(invoice);
            Assert.Equal(1, invoice.CustomerId);
            Assert.Equal(2, invoice.Items.Count);
            Assert.Equal(350m, invoice.SubTotal);        // 200 + 150
            Assert.Equal(25m, invoice.VATPercentage);
            Assert.Equal(87.5m, invoice.VATAmount);      // 350 * 0.25
            Assert.Equal(437.5m, invoice.TotalAmount);   // 350 + 87.5
        }

        [Fact]
        public async Task CreateInvoiceAsync_SingleItem_CalculatesUnitPriceFromProduct()
        {
            using var context = CreateInMemoryContext(nameof(CreateInvoiceAsync_SingleItem_CalculatesUnitPriceFromProduct));
            await SeedBaseDataAsync(context);
            var service = new InvoiceService(context);

            var dto = new CreateInvoiceDto
            {
                CustomerId = 1,
                Items = new List<CreateInvoiceItemDto>
                {
                    new CreateInvoiceItemDto { ProductId = 1, Quantity = 1 }  // 1 x 100 = 100
                }
            };

            var invoice = await service.CreateInvoiceAsync(dto);

            var item = Assert.Single(invoice.Items);
            Assert.Equal(100m, item.UnitPrice);
            Assert.Equal(100m, item.TotalPrice);
        }

        [Fact]
        public async Task CreateInvoiceAsync_LoadsNavigationProperties()
        {
            using var context = CreateInMemoryContext(nameof(CreateInvoiceAsync_LoadsNavigationProperties));
            await SeedBaseDataAsync(context);
            var service = new InvoiceService(context);

            var dto = new CreateInvoiceDto
            {
                CustomerId = 1,
                Items = new List<CreateInvoiceItemDto>
                {
                    new CreateInvoiceItemDto { ProductId = 1, Quantity = 1 }
                }
            };

            var invoice = await service.CreateInvoiceAsync(dto);

            Assert.NotNull(invoice.Customer);
            Assert.Equal("Alice", invoice.Customer.Name);
            Assert.NotNull(invoice.Items.First().Product);
            Assert.Equal("Widget", invoice.Items.First().Product.Name);
        }

        [Fact]
        public async Task CreateInvoiceAsync_InvalidCustomer_ThrowsException()
        {
            using var context = CreateInMemoryContext(nameof(CreateInvoiceAsync_InvalidCustomer_ThrowsException));
            await SeedBaseDataAsync(context);
            var service = new InvoiceService(context);

            var dto = new CreateInvoiceDto
            {
                CustomerId = 99,
                Items = new List<CreateInvoiceItemDto>
                {
                    new CreateInvoiceItemDto { ProductId = 1, Quantity = 1 }
                }
            };

            var ex = await Assert.ThrowsAsync<Exception>(() => service.CreateInvoiceAsync(dto));
            Assert.Contains("Customer not found", ex.Message);
        }

        [Fact]
        public async Task CreateInvoiceAsync_InvalidProduct_ThrowsException()
        {
            using var context = CreateInMemoryContext(nameof(CreateInvoiceAsync_InvalidProduct_ThrowsException));
            await SeedBaseDataAsync(context);
            var service = new InvoiceService(context);

            var dto = new CreateInvoiceDto
            {
                CustomerId = 1,
                Items = new List<CreateInvoiceItemDto>
                {
                    new CreateInvoiceItemDto { ProductId = 99, Quantity = 1 }
                }
            };

            var ex = await Assert.ThrowsAsync<Exception>(() => service.CreateInvoiceAsync(dto));
            Assert.Contains("Products not found", ex.Message);
            Assert.Contains("99", ex.Message);
        }

        [Fact]
        public async Task CreateInvoiceAsync_PersistsInvoiceToDatabase()
        {
            using var context = CreateInMemoryContext(nameof(CreateInvoiceAsync_PersistsInvoiceToDatabase));
            await SeedBaseDataAsync(context);
            var service = new InvoiceService(context);

            var dto = new CreateInvoiceDto
            {
                CustomerId = 1,
                Items = new List<CreateInvoiceItemDto>
                {
                    new CreateInvoiceItemDto { ProductId = 1, Quantity = 1 }
                }
            };

            var invoice = await service.CreateInvoiceAsync(dto);

            Assert.Equal(1, await context.Invoices.CountAsync());
            Assert.Equal(1, await context.InvoiceItems.CountAsync());
            Assert.True(invoice.Id > 0);
        }

        // ── UpdateInvoiceAsync ────────────────────────────────────────────────

        [Fact]
        public async Task UpdateInvoiceAsync_ValidDto_UpdatesInvoiceTotals()
        {
            using var context = CreateInMemoryContext(nameof(UpdateInvoiceAsync_ValidDto_UpdatesInvoiceTotals));
            await SeedBaseDataAsync(context);

            // create an invoice first
            var existing = new Invoice
            {
                Id = 1, CustomerId = 1,
                Items = new List<InvoiceItem>
                {
                    new InvoiceItem { ProductId = 1, Quantity = 1, UnitPrice = 100m }
                },
                SubTotal = 100m, VATPercentage = 25m, VATAmount = 25m, TotalAmount = 125m
            };
            context.Invoices.Add(existing);
            await context.SaveChangesAsync();

            var service = new InvoiceService(context);

            var dto = new UpdateInvoiceDto
            {
                CustomerId = 1,
                Items = new List<CreateInvoiceItemDto>
                {
                    new CreateInvoiceItemDto { ProductId = 2, Quantity = 4 }  // 4 x 50 = 200
                }
            };

            var updated = await service.UpdateInvoiceAsync(1, dto);

            Assert.Equal(200m, updated.SubTotal);
            Assert.Equal(50m, updated.VATAmount);
            Assert.Equal(250m, updated.TotalAmount);
            Assert.Single(updated.Items);
            Assert.Equal(2, updated.Items.First().ProductId);
        }

        [Fact]
        public async Task UpdateInvoiceAsync_ReplacesAllItems()
        {
            using var context = CreateInMemoryContext(nameof(UpdateInvoiceAsync_ReplacesAllItems));
            await SeedBaseDataAsync(context);

            var existing = new Invoice
            {
                Id = 1, CustomerId = 1,
                Items = new List<InvoiceItem>
                {
                    new InvoiceItem { ProductId = 1, Quantity = 2, UnitPrice = 100m },
                    new InvoiceItem { ProductId = 2, Quantity = 1, UnitPrice = 50m }
                }
            };
            context.Invoices.Add(existing);
            await context.SaveChangesAsync();

            var service = new InvoiceService(context);

            var dto = new UpdateInvoiceDto
            {
                CustomerId = 1,
                Items = new List<CreateInvoiceItemDto>
                {
                    new CreateInvoiceItemDto { ProductId = 1, Quantity = 1 }
                }
            };

            var updated = await service.UpdateInvoiceAsync(1, dto);

            Assert.Single(updated.Items);
            Assert.Equal(1, await context.InvoiceItems.CountAsync());
        }

        [Fact]
        public async Task UpdateInvoiceAsync_NonExistingInvoice_ThrowsException()
        {
            using var context = CreateInMemoryContext(nameof(UpdateInvoiceAsync_NonExistingInvoice_ThrowsException));
            await SeedBaseDataAsync(context);
            var service = new InvoiceService(context);

            var dto = new UpdateInvoiceDto
            {
                CustomerId = 1,
                Items = new List<CreateInvoiceItemDto>
                {
                    new CreateInvoiceItemDto { ProductId = 1, Quantity = 1 }
                }
            };

            var ex = await Assert.ThrowsAsync<Exception>(() => service.UpdateInvoiceAsync(99, dto));
            Assert.Contains("Invoice not found", ex.Message);
        }

        [Fact]
        public async Task UpdateInvoiceAsync_InvalidCustomer_ThrowsException()
        {
            using var context = CreateInMemoryContext(nameof(UpdateInvoiceAsync_InvalidCustomer_ThrowsException));
            await SeedBaseDataAsync(context);

            context.Invoices.Add(new Invoice
            {
                Id = 1, CustomerId = 1,
                Items = new List<InvoiceItem>
                {
                    new InvoiceItem { ProductId = 1, Quantity = 1, UnitPrice = 100m }
                }
            });
            await context.SaveChangesAsync();

            var service = new InvoiceService(context);

            var dto = new UpdateInvoiceDto
            {
                CustomerId = 99,
                Items = new List<CreateInvoiceItemDto>
                {
                    new CreateInvoiceItemDto { ProductId = 1, Quantity = 1 }
                }
            };

            var ex = await Assert.ThrowsAsync<Exception>(() => service.UpdateInvoiceAsync(1, dto));
            Assert.Contains("Customer not found", ex.Message);
        }

        [Fact]
        public async Task UpdateInvoiceAsync_InvalidProduct_ThrowsException()
        {
            using var context = CreateInMemoryContext(nameof(UpdateInvoiceAsync_InvalidProduct_ThrowsException));
            await SeedBaseDataAsync(context);

            context.Invoices.Add(new Invoice
            {
                Id = 1, CustomerId = 1,
                Items = new List<InvoiceItem>
                {
                    new InvoiceItem { ProductId = 1, Quantity = 1, UnitPrice = 100m }
                }
            });
            await context.SaveChangesAsync();

            var service = new InvoiceService(context);

            var dto = new UpdateInvoiceDto
            {
                CustomerId = 1,
                Items = new List<CreateInvoiceItemDto>
                {
                    new CreateInvoiceItemDto { ProductId = 99, Quantity = 1 }
                }
            };

            var ex = await Assert.ThrowsAsync<Exception>(() => service.UpdateInvoiceAsync(1, dto));
            Assert.Contains("Products not found", ex.Message);
        }

        // ── GetInvoiceByIdAsync ───────────────────────────────────────────────

        [Fact]
        public async Task GetInvoiceByIdAsync_ExistingId_ReturnsInvoiceWithNavigationProperties()
        {
            using var context = CreateInMemoryContext(nameof(GetInvoiceByIdAsync_ExistingId_ReturnsInvoiceWithNavigationProperties));
            await SeedBaseDataAsync(context);

            context.Invoices.Add(new Invoice
            {
                Id = 1, CustomerId = 1,
                Items = new List<InvoiceItem>
                {
                    new InvoiceItem { ProductId = 1, Quantity = 1, UnitPrice = 100m }
                }
            });
            await context.SaveChangesAsync();

            var service = new InvoiceService(context);
            var invoice = await service.GetInvoiceByIdAsync(1);

            Assert.NotNull(invoice);
            Assert.Equal(1, invoice!.Id);
            Assert.NotNull(invoice.Customer);
            Assert.Equal("Alice", invoice.Customer.Name);
            Assert.Single(invoice.Items);
            Assert.NotNull(invoice.Items.First().Product);
        }

        [Fact]
        public async Task GetInvoiceByIdAsync_NonExistingId_ReturnsNull()
        {
            using var context = CreateInMemoryContext(nameof(GetInvoiceByIdAsync_NonExistingId_ReturnsNull));
            var service = new InvoiceService(context);

            var invoice = await service.GetInvoiceByIdAsync(99);

            Assert.Null(invoice);
        }
    }
}
