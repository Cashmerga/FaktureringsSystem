# Company Profile Implementation

## Overview
The `CompanyProfile` entity stores your company's information needed for invoice generation and PDF creation. This is a **single-tenant system** - only one company profile should exist.

---

## Entity Structure

### CompanyProfile (Domain Layer)
Located: `Domain\Entities\CompanyProfile.cs`

```csharp
public class CompanyProfile
{
    public int Id { get; set; }

    // Basic Company Info
    public string CompanyName { get; set; }
    public string OrganizationNumber { get; set; }
    public string Email { get; set; }
    public string PhoneNumber { get; set; }

    // Address
    public string Street { get; set; }
    public string PostalCode { get; set; }
    public string City { get; set; }
    public string Country { get; set; }

    // Banking Info
    public string? BankAccountNumber { get; set; }
    public string? ClearingNumber { get; set; }
    public string? IBAN { get; set; }
    public string? SWIFT { get; set; }

    // Invoice Settings
    public string? DefaultOCRPrefix { get; set; }
    public decimal DefaultVATPercentage { get; set; } = 25.0m;

    // Metadata
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}
```

---

## API Endpoints

### GET /api/companyprofile
Retrieve the company profile.

**Response (200 OK):**
```json
{
  "id": 1,
  "companyName": "Acme AB",
  "organizationNumber": "556677-8899",
  "email": "info@acme.se",
  "phoneNumber": "+46 8 123 456",
  "street": "Storgatan 1",
  "postalCode": "111 22",
  "city": "Stockholm",
  "country": "Sweden",
  "bankAccountNumber": "1234567890",
  "clearingNumber": "8000",
  "iban": "SE35 5000 0000 0554 9108 8888",
  "swift": "ESSESESS",
  "defaultOCRPrefix": "99",
  "defaultVATPercentage": 25.0,
  "createdAt": "2025-01-15T10:30:00Z",
  "updatedAt": "2025-01-20T14:45:00Z"
}
```

**Response (404 Not Found):**
```json
{
  "message": "Company profile not found. Please create one first."
}
```

---

### PUT /api/companyprofile
Create or update the company profile (upsert logic).

**Request Body:**
```json
{
  "companyName": "Acme AB",
  "organizationNumber": "556677-8899",
  "email": "info@acme.se",
  "phoneNumber": "+46 8 123 456",
  "street": "Storgatan 1",
  "postalCode": "111 22",
  "city": "Stockholm",
  "country": "Sweden",
  "bankAccountNumber": "1234567890",
  "clearingNumber": "8000",
  "iban": "SE35 5000 0000 0554 9108 8888",
  "swift": "ESSESESS",
  "defaultOCRPrefix": "99",
  "defaultVATPercentage": 25.0
}
```

**Response (201 Created):** - If profile didn't exist
**Response (200 OK):** - If profile was updated

Both return the same structure as GET.

---

## Database Schema

**Table:** `CompanyProfiles`

| Column | Type | Nullable | Description |
|--------|------|----------|-------------|
| Id | int | No | Primary key (IDENTITY) |
| CompanyName | nvarchar(200) | No | Company name |
| OrganizationNumber | nvarchar(50) | No | Org number (e.g., 556677-8899) |
| Email | nvarchar(100) | No | Contact email |
| PhoneNumber | nvarchar(50) | No | Phone number |
| Street | nvarchar(200) | No | Street address |
| PostalCode | nvarchar(20) | No | Postal code |
| City | nvarchar(100) | No | City |
| Country | nvarchar(100) | No | Country |
| BankAccountNumber | nvarchar(50) | Yes | Bank account |
| ClearingNumber | nvarchar(10) | Yes | Clearing number (Swedish) |
| IBAN | nvarchar(50) | Yes | International bank account |
| SWIFT | nvarchar(20) | Yes | SWIFT/BIC code |
| DefaultOCRPrefix | nvarchar(10) | Yes | OCR reference prefix |
| DefaultVATPercentage | decimal(18,2) | No | Default VAT (e.g., 25.0) |
| CreatedAt | datetime2 | No | Creation timestamp |
| UpdatedAt | datetime2 | Yes | Last update timestamp |

**Index:** Unique index on `Id`

---

## Usage in Invoice PDF Generation

The CompanyProfile will be used when generating invoice PDFs:

### Example Usage (Future Implementation)
```csharp
var companyProfile = await _context.CompanyProfiles.FirstAsync();
var invoice = await _context.Invoices
    .Include(i => i.Customer)
    .Include(i => i.Items)
    .FirstAsync(i => i.Id == invoiceId);

// Generate PDF with company info
var pdf = new InvoicePdfGenerator()
    .WithSender(companyProfile)  // ← Company info as sender
    .WithRecipient(invoice.Customer)
    .WithItems(invoice.Items)
    .WithBankDetails(
        account: companyProfile.BankAccountNumber,
        clearing: companyProfile.ClearingNumber,
        iban: companyProfile.IBAN,
        swift: companyProfile.SWIFT
    )
    .WithOCR(companyProfile.DefaultOCRPrefix, invoice.Id)
    .WithVAT(companyProfile.DefaultVATPercentage)
    .Generate();
```

---

## Single-Tenant Design

### Why Only One Profile?
This system is designed for **single-tenant** use:
- One company uses the system
- No multi-company support needed
- Simplifies PDF generation and invoice logic

### Implementation
The controller uses **upsert logic**:
```csharp
var existingProfile = await _context.CompanyProfiles.FirstOrDefaultAsync();

if (existingProfile == null)
{
    // CREATE - First time setup
    _context.CompanyProfiles.Add(newProfile);
}
else
{
    // UPDATE - Modify existing profile
    existingProfile.CompanyName = dto.CompanyName;
    // ... update other fields
}
```

Only one row exists in the database, always updated via PUT.

---

## Testing

### 1. Create Company Profile
```powershell
$body = @{
    companyName = "Acme AB"
    organizationNumber = "556677-8899"
    email = "info@acme.se"
    phoneNumber = "+46 8 123 456"
    street = "Storgatan 1"
    postalCode = "111 22"
    city = "Stockholm"
    country = "Sweden"
    bankAccountNumber = "1234567890"
    clearingNumber = "8000"
    iban = "SE35 5000 0000 0554 9108 8888"
    swift = "ESSESESS"
    defaultOCRPrefix = "99"
    defaultVATPercentage = 25.0
} | ConvertTo-Json

Invoke-RestMethod -Uri "https://localhost:5001/api/companyprofile" -Method Put -Body $body -ContentType "application/json"
```

### 2. Get Company Profile
```powershell
Invoke-RestMethod -Uri "https://localhost:5001/api/companyprofile" -Method Get
```

### 3. Update Company Profile
```powershell
$updateBody = @{
    companyName = "Acme AB (Updated)"
    organizationNumber = "556677-8899"
    email = "new-email@acme.se"
    phoneNumber = "+46 8 999 888"
    street = "Nygatan 5"
    postalCode = "222 33"
    city = "Göteborg"
    country = "Sweden"
    bankAccountNumber = "9876543210"
    clearingNumber = "8000"
    iban = "SE35 5000 0000 0554 9108 9999"
    swift = "ESSESESS"
    defaultOCRPrefix = "99"
    defaultVATPercentage = 25.0
} | ConvertTo-Json

Invoke-RestMethod -Uri "https://localhost:5001/api/companyprofile" -Method Put -Body $updateBody -ContentType "application/json"
```

---

## Validation

### Required Fields
- ✅ CompanyName
- ✅ OrganizationNumber
- ✅ Email (valid email format)
- ✅ PhoneNumber
- ✅ Street
- ✅ PostalCode
- ✅ City
- ✅ Country

### Optional Fields
- BankAccountNumber
- ClearingNumber
- IBAN
- SWIFT
- DefaultOCRPrefix

### Constraints
- Email must be valid email format
- DefaultVATPercentage must be between 0 and 100
- All string fields have max length constraints

---

## Architecture

```
Domain/
  └── Entities/
      └── CompanyProfile.cs        ← Entity definition

Data/
  └── Context/
      └── ApplicationDbContext.cs  ← DbSet + configuration
  └── Migrations/
      └── *_AddCompanyProfile.cs   ← EF Core migration

Webapi/
  └── Dtos/
      └── CompanyProfile/
          ├── CompanyProfileResponseDto.cs  ← GET response
          └── UpdateCompanyProfileDto.cs    ← PUT request
  └── Controllers/
      └── CompanyProfileController.cs       ← API endpoints
```

---

## Key Features

✅ **Single-tenant design** - Only one company profile  
✅ **Upsert logic** - PUT creates or updates  
✅ **Full validation** - Data annotations on DTOs  
✅ **Banking info** - Swedish + international formats  
✅ **OCR support** - Prefix for Swedish invoice references  
✅ **VAT settings** - Configurable default percentage  
✅ **Timestamp tracking** - CreatedAt + UpdatedAt  
✅ **Clean architecture** - Separated layers  
✅ **Ready for PDF** - All invoice sender info available  

---

## Future Enhancements

### Invoice PDF Integration
When implementing PDF generation, retrieve company profile:
```csharp
var company = await _context.CompanyProfiles.FirstAsync();
// Use company.CompanyName, company.IBAN, etc. in PDF
```

### Logo Support
Add `LogoUrl` or `LogoBase64` field for company logo in PDF.

### Multiple Bank Accounts
Extend to support multiple bank accounts if needed.

### Invoice Numbering
Use `DefaultOCRPrefix` for generating OCR references.

---

## Summary

Your company profile system is now complete and ready for invoice PDF generation! 🎉

**Next Steps:**
1. ✅ Test in Swagger (`/swagger`)
2. ✅ Create your company profile via PUT
3. ✅ Retrieve it via GET
4. 🔜 Use it in PDF generation logic
