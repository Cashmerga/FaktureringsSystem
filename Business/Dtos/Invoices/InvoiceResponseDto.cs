namespace Business.Dtos.Invoices
{
    public class InvoiceResponseDto
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public int CustomerId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public decimal SubTotal { get; set; }
        public decimal VATPercentage { get; set; }
        public decimal VATAmount { get; set; }
        public decimal TotalAmount { get; set; }
        public List<InvoiceItemResponseDto> Items { get; set; } = new();
    }
}
