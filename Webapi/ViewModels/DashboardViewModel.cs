namespace Webapi.ViewModels
{
    public class DashboardViewModel
    {
        public int TotalInvoices { get; set; }
        public decimal TotalRevenue { get; set; }
        public int TotalCustomers { get; set; }
        public int TotalProducts { get; set; }
        public List<RecentInvoiceViewModel> RecentInvoices { get; set; } = new();
    }

    public class RecentInvoiceViewModel
    {
        public int Id { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
