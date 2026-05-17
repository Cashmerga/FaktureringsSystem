# Invoice Pricing Logic Documentation

## 🎯 Overview
When creating or updating invoices, the API automatically fetches product prices from the database to ensure data integrity and prevent price manipulation.

---

## 🔒 Security & Data Integrity

### Why Client Doesn't Send Prices

**Problem with old approach:**
```json
❌ BAD - Client could manipulate prices
{
  "customerId": 1,
  "items": [
    {
      "productId": 2,
      "quantity": 3,
      "unitPrice": 0.01  // 🚨 Client could send any price!
    }
  ]
}
```

**New secure approach:**
```json
✅ GOOD - Prices fetched from database
{
  "customerId": 1,
  "items": [
    {
      "productId": 2,
      "quantity": 3
      // unitPrice is fetched from Product table automatically
    }
  ]
}
```

---

## 🔄 How It Works

### POST /api/invoices - Create Invoice

**Step-by-step flow:**

1. **Client sends** (simplified request):
```json
{
  "customerId": 1,
  "items": [
    { "productId": 2, "quantity": 3 },
    { "productId": 5, "quantity": 1 }
  ]
}
```

2. **Backend validates**:
   - Customer exists (ID: 1)
   - All products exist (IDs: 2, 5)

3. **Backend fetches product prices**:
```csharp
var existingProducts = await _context.Products
    .Where(p => productIds.Contains(p.Id))
    .ToDictionaryAsync(p => p.Id, p => p);
```

4. **Backend creates invoice items**:
```csharp
Items = dto.Items.Select(itemDto => new InvoiceItem
{
    ProductId = itemDto.ProductId,
    Quantity = itemDto.Quantity,
    UnitPrice = existingProducts[itemDto.ProductId].Price // From DB
}).ToList()
```

5. **Backend calculates total**:
```csharp
invoice.TotalAmount = invoice.Items.Sum(item => item.Quantity * item.UnitPrice);
```

6. **Response returned**:
```json
{
  "id": 42,
  "createdAt": "2025-01-15T14:30:00Z",
  "customerId": 1,
  "customerName": "Acme Corp",
  "totalAmount": 449.97,  // Automatically calculated
  "items": [
    {
      "id": 101,
      "productId": 2,
      "productName": "Premium Widget",
      "quantity": 3,
      "unitPrice": 99.99,  // From Product.Price
      "totalPrice": 299.97
    },
    {
      "id": 102,
      "productId": 5,
      "productName": "Deluxe Service",
      "quantity": 1,
      "unitPrice": 150.00,  // From Product.Price
      "totalPrice": 150.00
    }
  ]
}
```

---

## 📊 Database Schema

### Product Table
```sql
CREATE TABLE Products (
    Id INT PRIMARY KEY IDENTITY,
    Name NVARCHAR(100) NOT NULL,
    Price DECIMAL(18,2) NOT NULL,  -- ⬅️ Source of truth
    Description NVARCHAR(500)
)
```

### InvoiceItem Table
```sql
CREATE TABLE InvoiceItems (
    Id INT PRIMARY KEY IDENTITY,
    InvoiceId INT NOT NULL,
    ProductId INT NOT NULL,
    Quantity INT NOT NULL,
    UnitPrice DECIMAL(18,2) NOT NULL,  -- ⬅️ Historical snapshot
    FOREIGN KEY (InvoiceId) REFERENCES Invoices(Id),
    FOREIGN KEY (ProductId) REFERENCES Products(Id)
)
```

---

## 💡 Why Store UnitPrice in InvoiceItem?

Even though we fetch the price from `Product.Price`, we **store it in InvoiceItem** because:

1. **Historical accuracy** - If product price changes later, old invoices should show the price that was valid at the time
2. **Audit trail** - Legal requirement in many jurisdictions
3. **Price changes** - Products can have sales, discounts, or price updates
4. **Data integrity** - Invoice should be immutable once created

### Example Scenario:
```
Day 1: Product "Widget" costs $99.99
Day 1: Customer buys 2 widgets → Invoice saves unitPrice = $99.99

Day 2: Product "Widget" price changes to $89.99
Day 2: Old invoice STILL shows $99.99 (correct!)
Day 2: New invoice saves unitPrice = $89.99
```

---

## 🔧 Code Implementation

### DTO (No UnitPrice)
```csharp
public class CreateInvoiceItemDto
{
    [Required]
    [Range(1, int.MaxValue)]
    public int ProductId { get; set; }

    [Required]
    [Range(1, int.MaxValue)]
    public int Quantity { get; set; }

    // ❌ No UnitPrice property - fetched from DB
}
```

### Controller Logic
```csharp
public async Task<ActionResult<InvoiceResponseDto>> CreateInvoice(CreateInvoiceDto dto)
{
    // 1. Verify customer
    var customerExists = await _context.Customers.AnyAsync(c => c.Id == dto.CustomerId);
    if (!customerExists)
        return BadRequest("Customer not found");

    // 2. Fetch products with prices
    var productIds = dto.Items.Select(i => i.ProductId).Distinct().ToList();
    var existingProducts = await _context.Products
        .Where(p => productIds.Contains(p.Id))
        .ToDictionaryAsync(p => p.Id, p => p);

    if (existingProducts.Count != productIds.Count)
        return BadRequest("One or more products not found");

    // 3. Create invoice with prices from DB
    var invoice = new Invoice
    {
        CustomerId = dto.CustomerId,
        CreatedAt = DateTime.UtcNow,
        Items = dto.Items.Select(itemDto => new InvoiceItem
        {
            ProductId = itemDto.ProductId,
            Quantity = itemDto.Quantity,
            UnitPrice = existingProducts[itemDto.ProductId].Price  // ⬅️ KEY LINE
        }).ToList()
    };

    // 4. Calculate total
    invoice.TotalAmount = invoice.Items.Sum(item => item.Quantity * item.UnitPrice);

    // 5. Save and return
    _context.Invoices.Add(invoice);
    await _context.SaveChangesAsync();

    return CreatedAtAction(nameof(GetInvoice), new { id = invoice.Id }, response);
}
```

---

## ✅ Benefits

| Aspect | Benefit |
|--------|---------|
| **Security** | Client cannot manipulate prices |
| **Data Integrity** | Prices always match current product data |
| **Consistency** | Single source of truth (Product table) |
| **Audit Trail** | Historical prices preserved in InvoiceItem |
| **Simplicity** | Frontend doesn't need to manage prices |
| **Validation** | Backend validates product existence |

---

## 🧪 Testing in Swagger

### Before (with UnitPrice in request):
```json
❌ Request body showed:
{
  "productId": 0,
  "quantity": 0,
  "unitPrice": 0.01  // ⬅️ Confusing and dangerous
}
```

### After (without UnitPrice):
```json
✅ Request body shows:
{
  "productId": 0,
  "quantity": 0
  // unitPrice is gone - much cleaner!
}
```

Response still includes unitPrice (from database):
```json
{
  "items": [
    {
      "productId": 2,
      "productName": "Widget",
      "quantity": 3,
      "unitPrice": 99.99,  // ✅ Fetched from DB
      "totalPrice": 299.97
    }
  ]
}
```

---

## 📝 Summary

✅ **Request DTOs** (Create/Update) - No `unitPrice` field  
✅ **Response DTOs** (Get) - Include `unitPrice` (from DB)  
✅ **Entity (InvoiceItem)** - Stores `UnitPrice` (historical snapshot)  
✅ **Database (Product)** - Source of truth for current prices  

This design follows **best practices** for e-commerce and invoicing systems! 🎉
