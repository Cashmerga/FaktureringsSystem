namespace Faktureringsys.Models
{
    public class Invoice
    {
        public int Id { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public int CustomerId { get; set; }
        public Customer Customer { get; set; }

        public decimal TotalAmount { get; set; }

        public ICollection<InvoiceItem> Items { get; set; } = new List<InvoiceItem>();

        public decimal VATPercentage { get; set; }

        public decimal VATAmount { get; set; }

        public decimal SubTotal { get; set; }


    }
}
