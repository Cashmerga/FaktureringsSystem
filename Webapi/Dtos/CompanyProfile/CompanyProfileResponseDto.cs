namespace Webapi.Dtos.CompanyProfile
{
    public class CompanyProfileResponseDto
    {
        public int Id { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public string OrganizationNumber { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PhoneNumber { get; set; } = string.Empty;
        public string Street { get; set; } = string.Empty;
        public string PostalCode { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public string? BankAccountNumber { get; set; }
        public string? ClearingNumber { get; set; }
        public string? IBAN { get; set; }
        public string? SWIFT { get; set; }
        public string? DefaultOCRPrefix { get; set; }
        public decimal DefaultVATPercentage { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}
