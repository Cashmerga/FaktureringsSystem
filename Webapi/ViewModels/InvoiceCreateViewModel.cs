using System.ComponentModel.DataAnnotations;

namespace Webapi.ViewModels
{
    public class InvoiceCreateViewModel
    {
        [Required(ErrorMessage = "Please select a customer")]
        [Display(Name = "Customer")]
        public int CustomerId { get; set; }

        public List<InvoiceItemViewModel> Items { get; set; } = new();
    }

    public class InvoiceItemViewModel
    {
        [Required(ErrorMessage = "Please select a product")]
        [Display(Name = "Product")]
        public int ProductId { get; set; }

        [Required(ErrorMessage = "Quantity is required")]
        [Range(1, int.MaxValue, ErrorMessage = "Quantity must be at least 1")]
        [Display(Name = "Quantity")]
        public int Quantity { get; set; }
    }
}
