using System.ComponentModel.DataAnnotations;

namespace Faktureringsys.Models
{
    public class Customer
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; }
        
        [EmailAddress]
        public string? Email { get; set; }

        public string? Address { get; set; }

        public ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();
    }
}
