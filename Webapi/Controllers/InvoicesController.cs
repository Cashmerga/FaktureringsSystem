using Data.Context;
using Faktureringsys.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Webapi.Dtos.Invoices;

namespace Webapi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InvoicesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public InvoicesController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<InvoiceResponseDto>>> GetInvoices()
        {
            var invoices = await _context.Invoices
                .Include(i => i.Customer)
                .Include(i => i.Items)
                    .ThenInclude(item => item.Product)
                .Select(i => new InvoiceResponseDto
                {
                    Id = i.Id,
                    CreatedAt = i.CreatedAt,
                    CustomerId = i.CustomerId,
                    CustomerName = i.Customer.Name,
                    TotalAmount = i.TotalAmount,
                    Items = i.Items.Select(item => new InvoiceItemResponseDto
                    {
                        Id = item.Id,
                        ProductId = item.ProductId,
                        ProductName = item.Product.Name,
                        Quantity = item.Quantity,
                        UnitPrice = item.UnitPrice,
                        TotalPrice = item.TotalPrice
                    }).ToList()
                })
                .ToListAsync();

            return Ok(invoices);
        }

        [HttpGet("{id}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<InvoiceResponseDto>> GetInvoice(int id)
        {
            var invoice = await _context.Invoices
                .Include(i => i.Customer)
                .Include(i => i.Items)
                    .ThenInclude(item => item.Product)
                .Where(i => i.Id == id)
                .Select(i => new InvoiceResponseDto
                {
                    Id = i.Id,
                    CreatedAt = i.CreatedAt,
                    CustomerId = i.CustomerId,
                    CustomerName = i.Customer.Name,
                    TotalAmount = i.TotalAmount,
                    Items = i.Items.Select(item => new InvoiceItemResponseDto
                    {
                        Id = item.Id,
                        ProductId = item.ProductId,
                        ProductName = item.Product.Name,
                        Quantity = item.Quantity,
                        UnitPrice = item.UnitPrice,
                        TotalPrice = item.TotalPrice
                    }).ToList()
                })
                .FirstOrDefaultAsync();

            if (invoice == null)
            {
                return NotFound();
            }

            return Ok(invoice);
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<InvoiceResponseDto>> CreateInvoice(CreateInvoiceDto dto)
        {
            var customerExists = await _context.Customers.AnyAsync(c => c.Id == dto.CustomerId);
            if (!customerExists)
            {
                return BadRequest("Customer not found");
            }

            var productIds = dto.Items.Select(i => i.ProductId).Distinct().ToList();
            var existingProducts = await _context.Products
                .Where(p => productIds.Contains(p.Id))
                .ToDictionaryAsync(p => p.Id, p => p);

            if (existingProducts.Count != productIds.Count)
            {
                var missingIds = productIds.Except(existingProducts.Keys).ToList();
                return BadRequest($"Products not found: {string.Join(", ", missingIds)}");
            }

            var invoice = new Invoice
            {
                CustomerId = dto.CustomerId,
                CreatedAt = DateTime.UtcNow,
                Items = dto.Items.Select(itemDto => new InvoiceItem
                {
                    ProductId = itemDto.ProductId,
                    Quantity = itemDto.Quantity,
                    UnitPrice = existingProducts[itemDto.ProductId].Price
                }).ToList()
            };

            invoice.TotalAmount = invoice.Items.Sum(item => item.Quantity * item.UnitPrice);

            _context.Invoices.Add(invoice);
            await _context.SaveChangesAsync();

            await _context.Entry(invoice).Reference(i => i.Customer).LoadAsync();
            foreach (var item in invoice.Items)
            {
                await _context.Entry(item).Reference(i => i.Product).LoadAsync();
            }

            var response = new InvoiceResponseDto
            {
                Id = invoice.Id,
                CreatedAt = invoice.CreatedAt,
                CustomerId = invoice.CustomerId,
                CustomerName = invoice.Customer.Name,
                TotalAmount = invoice.TotalAmount,
                Items = invoice.Items.Select(item => new InvoiceItemResponseDto
                {
                    Id = item.Id,
                    ProductId = item.ProductId,
                    ProductName = item.Product.Name,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice,
                    TotalPrice = item.TotalPrice
                }).ToList()
            };

            return CreatedAtAction(nameof(GetInvoice), new { id = invoice.Id }, response);
        }

        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateInvoice(int id, UpdateInvoiceDto dto)
        {
            var invoice = await _context.Invoices
                .Include(i => i.Items)
                .FirstOrDefaultAsync(i => i.Id == id);

            if (invoice == null)
            {
                return NotFound();
            }

            var customerExists = await _context.Customers.AnyAsync(c => c.Id == dto.CustomerId);
            if (!customerExists)
            {
                return BadRequest("Customer not found");
            }

            var productIds = dto.Items.Select(i => i.ProductId).Distinct().ToList();
            var existingProducts = await _context.Products
                .Where(p => productIds.Contains(p.Id))
                .ToDictionaryAsync(p => p.Id, p => p);

            if (existingProducts.Count != productIds.Count)
            {
                var missingIds = productIds.Except(existingProducts.Keys).ToList();
                return BadRequest($"Products not found: {string.Join(", ", missingIds)}");
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

            invoice.TotalAmount = invoice.Items.Sum(item => item.Quantity * item.UnitPrice);

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!InvoiceExists(id))
                {
                    return NotFound();
                }
                throw;
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteInvoice(int id)
        {
            var invoice = await _context.Invoices.FindAsync(id);
            if (invoice == null)
            {
                return NotFound();
            }

            _context.Invoices.Remove(invoice);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        [HttpGet("customer/{customerId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<IEnumerable<InvoiceResponseDto>>> GetInvoicesByCustomer(int customerId)
        {
            var invoices = await _context.Invoices
                .Include(i => i.Customer)
                .Include(i => i.Items)
                    .ThenInclude(item => item.Product)
                .Where(i => i.CustomerId == customerId)
                .Select(i => new InvoiceResponseDto
                {
                    Id = i.Id,
                    CreatedAt = i.CreatedAt,
                    CustomerId = i.CustomerId,
                    CustomerName = i.Customer.Name,
                    TotalAmount = i.TotalAmount,
                    Items = i.Items.Select(item => new InvoiceItemResponseDto
                    {
                        Id = item.Id,
                        ProductId = item.ProductId,
                        ProductName = item.Product.Name,
                        Quantity = item.Quantity,
                        UnitPrice = item.UnitPrice,
                        TotalPrice = item.TotalPrice
                    }).ToList()
                })
                .ToListAsync();

            return Ok(invoices);
        }

        private bool InvoiceExists(int id)
        {
            return _context.Invoices.Any(e => e.Id == id);
        }
    }
}
