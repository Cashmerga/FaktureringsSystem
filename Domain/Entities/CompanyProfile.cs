using System.ComponentModel.DataAnnotations;

namespace Faktureringsys.Models
{
    public class CompanyProfile
    {
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string CompanyName { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string OrganizationNumber { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [StringLength(100)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(50)]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required]
        [StringLength(200)]
        public string Street { get; set; } = string.Empty;

        [Required]
        [StringLength(20)]
        public string PostalCode { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string City { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Country { get; set; } = string.Empty;

        [StringLength(50)]
        public string? BankAccountNumber { get; set; }

        [StringLength(10)]
        public string? ClearingNumber { get; set; }

        [StringLength(50)]
        public string? IBAN { get; set; }

        [StringLength(20)]
        public string? SWIFT { get; set; }

        [StringLength(10)]
        public string? DefaultOCRPrefix { get; set; }

        [Range(0, 100)]
        public decimal DefaultVATPercentage { get; set; } = 25.0m;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? UpdatedAt { get; set; }
    }
}
