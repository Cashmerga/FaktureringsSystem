using Data.Context;
using Faktureringsys.Models;
using Microsoft.EntityFrameworkCore;
using Business.Dtos.Invoices;

namespace Business.Sevices
{
    public class InvoiceService : IInvoiceService
    {
        private readonly ApplicationDbContext _context;


        public InvoiceService(ApplicationDbContext context)
        {
            _context = context;
        }


        public async Task<Invoice> CreateInvoiceAsync(CreateInvoiceDto dto)
        {
            var customer = await _context.Customers
                .FirstOrDefaultAsync(c => c.Id == dto.CustomerId);

            if (customer == null)
            {
                throw new Exception("Customer not found.");
            }

            var productIds = dto.Items.Select(i => i.ProductId).Distinct().ToList();
            var existingProducts = await _context.Products
                .Where(p => productIds.Contains(p.Id))
                .ToDictionaryAsync(p => p.Id, p => p);

            if (existingProducts.Count != productIds.Count)
            {
                var missingIds = productIds.Except(existingProducts.Keys).ToList();
                throw new Exception($"Products not found: {string.Join(", ", missingIds)}");
            }

            var invoice = new Invoice
            {
                CustomerId = dto.CustomerId,
                CreatedAt = DateTime.UtcNow,
                Items = new List<InvoiceItem>()
            };

            foreach (var itemDto in dto.Items)
            {
                decimal unitPrice = existingProducts[itemDto.ProductId].Price;

                var invoiceItem = new InvoiceItem
                {
                    ProductId = itemDto.ProductId,
                    Quantity = itemDto.Quantity,
                    UnitPrice = unitPrice
                };

                invoice.Items.Add(invoiceItem);
            }

            CalculateInvoiceTotals(invoice);

            _context.Invoices.Add(invoice);
            await _context.SaveChangesAsync();

            await _context.Entry(invoice).Reference(i => i.Customer).LoadAsync();
            foreach (var item in invoice.Items)
            {
                await _context.Entry(item).Reference(i => i.Product).LoadAsync();
            }

            return invoice;
        }

        public async Task<Invoice> UpdateInvoiceAsync(int id, UpdateInvoiceDto dto)
        {
            var invoice = await _context.Invoices
                .Include(i => i.Items)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (invoice == null)
            {
                throw new Exception("Invoice not found.");
            }

            var customerExists = await _context.Customers.AnyAsync(c => c.Id == dto.CustomerId);
            if (!customerExists)
            {
                throw new Exception("Customer not found.");
            }

            var productIds = dto.Items.Select(i => i.ProductId).Distinct().ToList();
            var existingProducts = await _context.Products
                .Where(p => productIds.Contains(p.Id))
                .ToDictionaryAsync(p => p.Id, p => p);

            if (existingProducts.Count != productIds.Count)
            {
                var missingIds = productIds.Except(existingProducts.Keys).ToList();
                throw new Exception($"Products not found: {string.Join(", ", missingIds)}");
            }

            invoice.CustomerId = dto.CustomerId;

            _context.InvoiceItems.RemoveRange(invoice.Items);

            invoice.Items = dto.Items.Select(itemDto => new InvoiceItem
            {
                InvoiceId = id,
                ProductId = itemDto.ProductId,
                Quantity = itemDto.Quantity,
                UnitPrice = existingProducts[itemDto.ProductId].Price
            }).ToList();

            CalculateInvoiceTotals(invoice);

            await _context.SaveChangesAsync();

            await _context.Entry(invoice).Reference(i => i.Customer).LoadAsync();
            foreach (var item in invoice.Items)
            {
                await _context.Entry(item).Reference(i => i.Product).LoadAsync();
            }

            return invoice;
        }

        public async Task<Invoice?> GetInvoiceByIdAsync(int id)
        {
            var invoice = await _context.Invoices
                .Include(i => i.Customer)
                .Include(i => i.Items)
                    .ThenInclude(item => item.Product)
                .FirstOrDefaultAsync(i => i.Id == id);

            return invoice;
        }
        private void CalculateInvoiceTotals(Invoice invoice)
        {
            decimal subtotal = invoice.Items.Sum(item =>
               item.Quantity * item.UnitPrice);

              decimal vatPercentage = 25m;

              decimal vatAmount = subtotal * (vatPercentage / 100m);

              decimal total = subtotal + vatAmount;

            invoice.SubTotal = subtotal;
            invoice.VATPercentage = vatPercentage;
            invoice.VATAmount = vatAmount;
            invoice.TotalAmount = total;
        }
    }
}
