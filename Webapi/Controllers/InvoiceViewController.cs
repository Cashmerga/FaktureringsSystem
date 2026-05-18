using Data.Context;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Webapi.ViewModels;
using Webapi.Dtos.Customers;
using Webapi.Dtos.Products;
using Business.Dtos.Invoices;
using Business.Sevices;

namespace Webapi.Controllers
{
    [Route("Invoices")]
    public class InvoiceViewController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IInvoiceService _invoiceService;

        public InvoiceViewController(ApplicationDbContext context, IInvoiceService invoiceService)
        {
            _context = context;
            _invoiceService = invoiceService;
        }

        [HttpGet("")]
        [HttpGet("Index")]
        public async Task<IActionResult> Index()
        {
            var invoices = await _context.Invoices
                .Include(i => i.Customer)
                .OrderByDescending(i => i.CreatedAt)
                .Select(i => new InvoiceResponseDto
                {
                    Id = i.Id,
                    CreatedAt = i.CreatedAt,
                    CustomerId = i.CustomerId,
                    CustomerName = i.Customer.Name,
                    SubTotal = i.SubTotal,
                    VATPercentage = i.VATPercentage,
                    VATAmount = i.VATAmount,
                    TotalAmount = i.TotalAmount
                })
                .ToListAsync();

            var viewModel = new InvoiceIndexViewModel
            {
                Invoices = invoices
            };

            return View(viewModel);
        }

        [HttpGet("Details/{id}")]
        public async Task<IActionResult> Details(int id)
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
                    SubTotal = i.SubTotal,
                    VATPercentage = i.VATPercentage,
                    VATAmount = i.VATAmount,
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

            var customer = await _context.Customers
                .Where(c => c.Id == invoice.CustomerId)
                .Select(c => new CustomerResponseDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Email = c.Email,
                    Address = c.Address
                })
                .FirstOrDefaultAsync();

            var viewModel = new InvoiceDetailsViewModel
            {
                Invoice = invoice,
                Customer = customer ?? new CustomerResponseDto()
            };

            return View(viewModel);
        }

        [HttpGet("Create")]
        public async Task<IActionResult> Create()
        {
            var customers = await _context.Customers
                .Select(c => new CustomerResponseDto
                {
                    Id = c.Id,
                    Name = c.Name,
                    Email = c.Email,
                    Address = c.Address
                })
                .ToListAsync();

            var products = await _context.Products
                .Select(p => new ProductResponseDto
                {
                    Id = p.Id,
                    Name = p.Name,
                    Price = p.Price,
                    Description = p.Description
                })
                .ToListAsync();

            var viewModel = new InvoiceFormViewModel
            {
                Invoice = new InvoiceCreateViewModel(),
                Customers = customers,
                Products = products
            };

            return View(viewModel);
        }

        [HttpPost("Create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(InvoiceCreateViewModel model)
        {
            if (!ModelState.IsValid || model.Items == null || !model.Items.Any())
            {
                var customers = await _context.Customers
                    .Select(c => new CustomerResponseDto
                    {
                        Id = c.Id,
                        Name = c.Name,
                        Email = c.Email,
                        Address = c.Address
                    })
                    .ToListAsync();

                var products = await _context.Products
                    .Select(p => new ProductResponseDto
                    {
                        Id = p.Id,
                        Name = p.Name,
                        Price = p.Price,
                        Description = p.Description
                    })
                    .ToListAsync();

                var viewModel = new InvoiceFormViewModel
                {
                    Invoice = model,
                    Customers = customers,
                    Products = products
                };

                if (model.Items == null || !model.Items.Any())
                {
                    ModelState.AddModelError("", "Please add at least one item to the invoice.");
                }

                return View(viewModel);
            }

            try
            {
                var dto = new CreateInvoiceDto
                {
                    CustomerId = model.CustomerId,
                    Items = model.Items.Select(i => new CreateInvoiceItemDto
                    {
                        ProductId = i.ProductId,
                        Quantity = i.Quantity
                    }).ToList()
                };

                var invoice = await _invoiceService.CreateInvoiceAsync(dto);

                TempData["SuccessMessage"] = "Invoice created successfully!";
                return RedirectToAction(nameof(Details), new { id = invoice.Id });
            }
            catch (Exception ex)
            {
                var customers = await _context.Customers
                    .Select(c => new CustomerResponseDto
                    {
                        Id = c.Id,
                        Name = c.Name,
                        Email = c.Email,
                        Address = c.Address
                    })
                    .ToListAsync();

                var products = await _context.Products
                    .Select(p => new ProductResponseDto
                    {
                        Id = p.Id,
                        Name = p.Name,
                        Price = p.Price,
                        Description = p.Description
                    })
                    .ToListAsync();

                var viewModel = new InvoiceFormViewModel
                {
                    Invoice = model,
                    Customers = customers,
                    Products = products
                };

                ModelState.AddModelError("", $"Error creating invoice: {ex.Message}");
                return View(viewModel);
            }
        }
    }
}
