namespace Business.Dtos.Invoices
{
    public class InvoiceResponseDto
    {
        public int Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public int CustomerId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public List<InvoiceItemResponseDto> Items { get; set; } = new();
    }
}
