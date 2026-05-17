# DTO Structure Documentation

## Overview
This API uses Data Transfer Objects (DTOs) to ensure clean API contracts and prevent circular references in Swagger/responses.

## Folder Structure
```
Webapi/
└── Dtos/
    ├── Customers/
    │   ├── CustomerResponseDto.cs
    │   ├── CreateCustomerDto.cs
    │   └── UpdateCustomerDto.cs
    ├── Products/
    │   ├── ProductResponseDto.cs
    │   ├── CreateProductDto.cs
    │   └── UpdateProductDto.cs
    └── Invoices/
        ├── InvoiceResponseDto.cs
        ├── InvoiceItemResponseDto.cs
        ├── CreateInvoiceDto.cs
        ├── CreateInvoiceItemDto.cs
        └── UpdateInvoiceDto.cs
```

## DTO Patterns

### Response DTOs
- **Purpose**: Data returned from GET requests
- **Characteristics**: 
  - Contains only necessary fields
  - No navigation properties
  - Flattened relationships (e.g., `CustomerName` instead of `Customer` object)
  - No circular references

### Create DTOs
- **Purpose**: Data for POST requests
- **Characteristics**:
  - No `Id` property (auto-generated)
  - Required validation attributes
  - Only fields needed to create the entity

### Update DTOs
- **Purpose**: Data for PUT requests
- **Characteristics**:
  - Similar to Create DTOs
  - No `Id` in body (comes from route)
  - Full object replacement pattern

## API Endpoints

### Customers (`/api/customers`)
```
GET    /api/customers          → List<CustomerResponseDto>
GET    /api/customers/{id}     → CustomerResponseDto
POST   /api/customers          ← CreateCustomerDto
PUT    /api/customers/{id}     ← UpdateCustomerDto
DELETE /api/customers/{id}
```

### Products (`/api/products`)
```
GET    /api/products           → List<ProductResponseDto>
GET    /api/products/{id}      → ProductResponseDto
POST   /api/products           ← CreateProductDto
PUT    /api/products/{id}      ← UpdateProductDto
DELETE /api/products/{id}
```

### Invoices (`/api/invoices`)
```
GET    /api/invoices                    → List<InvoiceResponseDto>
GET    /api/invoices/{id}               → InvoiceResponseDto
GET    /api/invoices/customer/{id}      → List<InvoiceResponseDto>
POST   /api/invoices                    ← CreateInvoiceDto
PUT    /api/invoices/{id}               ← UpdateInvoiceDto
DELETE /api/invoices/{id}
```

### Company Profile (`/api/companyprofile`)
```
GET    /api/companyprofile              → CompanyProfileResponseDto
PUT    /api/companyprofile              ← UpdateCompanyProfileDto (upsert)
```

## Example Requests

### Create Customer
```json
POST /api/customers
{
  "name": "Acme Corp",
  "email": "contact@acme.com",
  "address": "123 Business St"
}
```

### Create Invoice
```json
POST /api/invoices
{
  "customerId": 1,
  "items": [
    {
      "productId": 1,
      "quantity": 2
    },
    {
      "productId": 2,
      "quantity": 1
    }
  ]
}
```

**Note**: `unitPrice` and `totalAmount` are **automatically calculated** by the backend:
- `unitPrice` is fetched from the product's current price in the database
- `totalAmount` is calculated as sum of (quantity × unitPrice) for all items

## Example Response

### Get Invoice
```json
GET /api/invoices/1
{
  "id": 1,
  "createdAt": "2025-01-15T10:30:00Z",
  "customerId": 1,
  "customerName": "Acme Corp",
  "totalAmount": 349.97,
  "items": [
    {
      "id": 1,
      "productId": 1,
      "productName": "Premium Widget",
      "quantity": 2,
      "unitPrice": 99.99,
      "totalPrice": 199.98
    },
    {
      "id": 2,
      "productId": 2,
      "productName": "Deluxe Gadget",
      "quantity": 1,
      "unitPrice": 149.99,
      "totalPrice": 149.99
    }
  ]
}
```

## Benefits

✅ **No Circular References** - DTOs prevent Entity Framework navigation properties from causing JSON serialization loops

✅ **Clean Swagger Documentation** - API documentation shows only relevant fields

✅ **Better Performance** - Only necessary data is transferred

✅ **API Versioning** - Easy to change DTOs without affecting database entities

✅ **Validation** - Data annotations on DTOs for input validation

✅ **Security** - Entities are never directly exposed via API

## Entity Mapping

Controllers manually map between DTOs and EF Core entities:

```csharp
// Entity → Response DTO (in LINQ query)
.Select(c => new CustomerResponseDto
{
    Id = c.Id,
    Name = c.Name,
    Email = c.Email,
    Address = c.Address
})

// Create DTO → Entity (in POST method)
var customer = new Customer
{
    Name = dto.Name,
    Email = dto.Email,
    Address = dto.Address
};
```

## Validation

All Create/Update DTOs include validation attributes:
- `[Required]` - Field must be provided
- `[EmailAddress]` - Valid email format
- `[StringLength(n)]` - Maximum length
- `[Range(min, max)]` - Numeric range
- `[MinLength(n)]` - Minimum collection size

ModelState is automatically validated by ASP.NET Core's `[ApiController]` attribute.
