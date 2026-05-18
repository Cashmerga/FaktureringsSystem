using Business.Dtos.Invoices;
using Webapi.Dtos.Customers;
using Webapi.Dtos.Products;

namespace Webapi.ViewModels
{
    public class InvoiceDetailsViewModel
    {
        public InvoiceResponseDto Invoice { get; set; } = new();
        public CustomerResponseDto Customer { get; set; } = new();
    }

    public class InvoiceIndexViewModel
    {
        public List<InvoiceResponseDto> Invoices { get; set; } = new();
    }

    public class InvoiceFormViewModel
    {
        public InvoiceCreateViewModel Invoice { get; set; } = new();
        public List<CustomerResponseDto> Customers { get; set; } = new();
        public List<ProductResponseDto> Products { get; set; } = new();
    }
}
