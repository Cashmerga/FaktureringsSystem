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
            // Validera customer
            var customer = await _context.Customers
                .FirstOrDefaultAsync(c => c.Id == dto.CustomerId);

            if (customer == null)
            {
                throw new Exception("Customer not found.");
            }

            // Hämta alla products i en batch query (optimering)
            var productIds = dto.Items.Select(i => i.ProductId).Distinct().ToList();
            var existingProducts = await _context.Products
                .Where(p => productIds.Contains(p.Id))
                .ToDictionaryAsync(p => p.Id, p => p);

            // Validera att alla produkter finns
            if (existingProducts.Count != productIds.Count)
            {
                var missingIds = productIds.Except(existingProducts.Keys).ToList();
                throw new Exception($"Products not found: {string.Join(", ", missingIds)}");
            }

            // Skapa invoice
            var invoice = new Invoice
            {
                CustomerId = dto.CustomerId,
                CreatedAt = DateTime.UtcNow,
                Items = new List<InvoiceItem>()
            };

            decimal totalAmount = 0;

            // Loopa igenom alla invoice items
            foreach (var itemDto in dto.Items)
            {
                // Hämta aktuellt pris från dictionary (snabbt)
                decimal unitPrice = existingProducts[itemDto.ProductId].Price;

                // Skapa invoice item
                var invoiceItem = new InvoiceItem
                {
                    ProductId = itemDto.ProductId,
                    Quantity = itemDto.Quantity,
                    UnitPrice = unitPrice
                };

                // Lägg till radtotal
                totalAmount += unitPrice * itemDto.Quantity;

                // Lägg till item i invoice
                invoice.Items.Add(invoiceItem);
            }

            // Sätt totalsumma
            invoice.TotalAmount = totalAmount;

            // Spara invoice
            _context.Invoices.Add(invoice);
            await _context.SaveChangesAsync();

            // Ladda navigation properties
            await _context.Entry(invoice).Reference(i => i.Customer).LoadAsync();
            foreach (var item in invoice.Items)
            {
                await _context.Entry(item).Reference(i => i.Product).LoadAsync();
            }

            return invoice;
        }

        public async Task<Invoice> UpdateInvoiceAsync(int id, UpdateInvoiceDto dto)
        {
            // Hämta befintlig invoice
            var invoice = await _context.Invoices
                .Include(i => i.Items)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (invoice == null)
            {
                throw new Exception("Invoice not found.");
            }

            // Validera customer
            var customerExists = await _context.Customers.AnyAsync(c => c.Id == dto.CustomerId);
            if (!customerExists)
            {
                throw new Exception("Customer not found.");
            }

            // Hämta alla products i en batch query
            var productIds = dto.Items.Select(i => i.ProductId).Distinct().ToList();
            var existingProducts = await _context.Products
                .Where(p => productIds.Contains(p.Id))
                .ToDictionaryAsync(p => p.Id, p => p);

            // Validera att alla produkter finns
            if (existingProducts.Count != productIds.Count)
            {
                var missingIds = productIds.Except(existingProducts.Keys).ToList();
                throw new Exception($"Products not found: {string.Join(", ", missingIds)}");
            }

            // Uppdatera customer
            invoice.CustomerId = dto.CustomerId;

            // Ta bort gamla items
            _context.InvoiceItems.RemoveRange(invoice.Items);

            // Skapa nya items
            invoice.Items = dto.Items.Select(itemDto => new InvoiceItem
            {
                InvoiceId = id,
                ProductId = itemDto.ProductId,
                Quantity = itemDto.Quantity,
                UnitPrice = existingProducts[itemDto.ProductId].Price
            }).ToList();

            // Beräkna ny totalsumma
            invoice.TotalAmount = invoice.Items.Sum(item => item.Quantity * item.UnitPrice);

            // Spara ändringar
            await _context.SaveChangesAsync();

            // Ladda navigation properties
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
    }
}
