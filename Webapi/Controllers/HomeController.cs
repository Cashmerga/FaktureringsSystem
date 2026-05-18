using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Webapi.Models;
using Webapi.ViewModels;
using Data.Context;

namespace Webapi.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly ApplicationDbContext _context;

        public HomeController(ILogger<HomeController> logger, ApplicationDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var viewModel = new DashboardViewModel
            {
                TotalInvoices = await _context.Invoices.CountAsync(),
                TotalRevenue = await _context.Invoices.SumAsync(i => (decimal?)i.TotalAmount) ?? 0,
                TotalCustomers = await _context.Customers.CountAsync(),
                TotalProducts = await _context.Products.CountAsync(),
                RecentInvoices = await _context.Invoices
                    .Include(i => i.Customer)
                    .OrderByDescending(i => i.CreatedAt)
                    .Take(5)
                    .Select(i => new RecentInvoiceViewModel
                    {
                        Id = i.Id,
                        CustomerName = i.Customer.Name,
                        TotalAmount = i.TotalAmount,
                        CreatedAt = i.CreatedAt
                    })
                    .ToListAsync()
            };

            return View(viewModel);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
