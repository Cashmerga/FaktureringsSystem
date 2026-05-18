using Data.Context;
using Faktureringsys.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Business.Dtos.Invoices;
using Business.Sevices;

namespace Webapi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InvoicesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly IInvoiceService _invoiceService;

        public InvoicesController(ApplicationDbContext context, IInvoiceService invoiceService)
        {
            _context = context;
            _invoiceService = invoiceService;
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
            try
            {
                // Anropa service för att skapa invoice
                var invoice = await _invoiceService.CreateInvoiceAsync(dto);

                // Skapa response DTO
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
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPut("{id}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> UpdateInvoice(int id, UpdateInvoiceDto dto)
        {
            try
            {
                // Anropa service för att uppdatera invoice
                await _invoiceService.UpdateInvoiceAsync(id, dto);
                return NoContent();
            }
            catch (Exception ex)
            {
                // Om invoice inte finns, returnera 404
                if (ex.Message.Contains("Invoice not found"))
                {
                    return NotFound();
                }
                // Andra fel (customer/products not found) returnerar 400
                return BadRequest(ex.Message);
            }
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
