using System.ComponentModel.DataAnnotations;

namespace Business.Dtos.Invoices
{
    public class UpdateInvoiceDto
    {
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "CustomerId must be greater than 0")]
        public int CustomerId { get; set; }

        [Required]
        [MinLength(1, ErrorMessage = "Invoice must have at least one item")]
        public List<CreateInvoiceItemDto> Items { get; set; } = new();
    }
}
