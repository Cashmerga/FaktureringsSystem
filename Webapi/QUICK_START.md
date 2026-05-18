# Quick Start Guide - Invoice Management System

## Prerequisites
- .NET 10 SDK installed
- SQL Server (or connection string configured)
- Database migrations applied

## Step 1: Apply Database Migrations (if not done)
```powershell
cd F:\Users\oskar\source\repos\FaktureringsSystem
dotnet ef database update --project Data\Data.csproj --startup-project Webapi\Webapi.csproj
```

## Step 2: Run the Application
```powershell
cd F:\Users\oskar\source\repos\FaktureringsSystem\Webapi
dotnet run
```

Or press F5 in Visual Studio.

## Step 3: Access the Application
Once running, open your browser to:
- **MVC Frontend**: https://localhost:[port]/
- **API/Swagger**: https://localhost:[port]/swagger

## Step 4: First Time Setup Flow

### 1. Create Company Profile (Optional)
```
POST https://localhost:[port]/api/companyprofile
```
(Or skip if not needed for basic functionality)

### 2. Create Your First Customer
- Navigate to: https://localhost:[port]/Customers/Create
- Fill in:
  - Name: "Acme Corporation"
  - Email: "contact@acme.com"
  - Address: "123 Main St, City"
- Click "Create"

### 3. Create Your First Product
- Navigate to: https://localhost:[port]/Products/Create
- Fill in:
  - Name: "Web Development Service"
  - Price: 1500.00
  - Description: "Professional web development"
- Click "Create"

### 4. Create Your First Invoice
- Navigate to: https://localhost:[port]/Invoices/Create
- Select customer from dropdown
- Click "Add Item"
- Select product and enter quantity
- Add more items if needed
- Review the summary (Subtotal, VAT, Total)
- Click "Create Invoice"

### 5. View Invoice Details
- You'll be redirected to the invoice details page
- Click "Download PDF" to get the PDF version

### 6. Explore the Dashboard
- Navigate to: https://localhost:[port]/
- View statistics and recent invoices

## Routes Quick Reference

### MVC Frontend (HTML Pages)
- `/` - Dashboard
- `/Customers` - Customer list
- `/Customers/Create` - Create customer
- `/Customers/Edit/{id}` - Edit customer
- `/Customers/Delete/{id}` - Delete customer
- `/Customers/Details/{id}` - View customer
- `/Products` - Product list
- `/Products/Create` - Create product
- `/Products/Edit/{id}` - Edit product
- `/Products/Delete/{id}` - Delete product
- `/Invoices` - Invoice list
- `/Invoices/Create` - Create invoice
- `/Invoices/Details/{id}` - View invoice

### API Endpoints (JSON)
- `GET /api/customers` - List customers
- `POST /api/customers` - Create customer
- `GET /api/products` - List products
- `POST /api/products` - Create product
- `GET /api/invoices` - List invoices
- `POST /api/invoices` - Create invoice
- `GET /api/invoices/{id}/pdf` - Download invoice PDF

## Troubleshooting

### Build Errors
```powershell
dotnet clean
dotnet build
```

### Database Issues
Make sure connection string in `appsettings.json` is correct:
```json
{
  "ConnectionStrings": {
	"SqlConnection": "your-connection-string-here"
  }
}
```

### Port Already in Use
Check `Properties\launchSettings.json` and change the port if needed.

### Views Not Found
Make sure you're navigating to the correct routes:
- Use `/Customers` not `/api/customers` for the web UI
- Use `/api/customers` for API calls

## Features Checklist
✅ Dashboard with statistics
✅ Customer CRUD (Create, Read, Update, Delete)
✅ Product CRUD
✅ Invoice creation with multiple items
✅ Invoice listing
✅ Invoice details view
✅ PDF generation and download
✅ Dynamic form with live calculations
✅ VAT calculation (25%)
✅ Form validation
✅ Success messages
✅ Clean Bootstrap UI

## Next Steps
1. Add more customers and products
2. Create invoices for different customers
3. Download PDFs
4. Explore the API via Swagger
5. Customize VAT percentage (currently 25%)
6. Add company profile information

Enjoy your Invoice Management System! 🎉
