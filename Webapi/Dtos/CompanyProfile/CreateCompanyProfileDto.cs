using System.ComponentModel.DataAnnotations;

namespace Webapi.Dtos.CompanyProfile
{
    public class CreateCompanyProfileDto
    {
        [Required(ErrorMessage = "Company name is required")]
        [StringLength(200)]
        public string CompanyName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Organization number is required")]
        [StringLength(50)]
        public string OrganizationNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email format")]
        [StringLength(100)]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Phone number is required")]
        [StringLength(50)]
        public string PhoneNumber { get; set; } = string.Empty;

        [Required(ErrorMessage = "Street address is required")]
        [StringLength(200)]
        public string Street { get; set; } = string.Empty;

        [Required(ErrorMessage = "Postal code is required")]
        [StringLength(20)]
        public string PostalCode { get; set; } = string.Empty;

        [Required(ErrorMessage = "City is required")]
        [StringLength(100)]
        public string City { get; set; } = string.Empty;

        [Required(ErrorMessage = "Country is required")]
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

        [Range(0, 100, ErrorMessage = "VAT percentage must be between 0 and 100")]
        public decimal DefaultVATPercentage { get; set; } = 25.0m;
    }
}
