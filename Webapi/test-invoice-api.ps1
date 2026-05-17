# Test Invoice API with PowerShell

# Base URL - update with your actual port
$baseUrl = "https://localhost:5001/api"

Write-Host "=== Invoice API Test Script ===" -ForegroundColor Cyan
Write-Host ""

# 1. Create a Customer
Write-Host "1. Creating customer..." -ForegroundColor Yellow
$customer = @{
    name = "Test Customer AB"
    email = "test@customer.se"
    address = "Testgatan 1, Stockholm"
} | ConvertTo-Json

try {
    $customerResponse = Invoke-RestMethod -Uri "$baseUrl/customers" -Method Post -Body $customer -ContentType "application/json"
    $customerId = $customerResponse.id
    Write-Host "✓ Customer created with ID: $customerId" -ForegroundColor Green
}
catch {
    Write-Host "✗ Failed to create customer: $_" -ForegroundColor Red
    exit
}

# 2. Create Products
Write-Host ""
Write-Host "2. Creating products..." -ForegroundColor Yellow

$product1 = @{
    name = "Premium Widget"
    price = 99.99
    description = "High quality widget"
} | ConvertTo-Json

$product2 = @{
    name = "Deluxe Service"
    price = 150.00
    description = "Professional service package"
} | ConvertTo-Json

try {
    $productResponse1 = Invoke-RestMethod -Uri "$baseUrl/products" -Method Post -Body $product1 -ContentType "application/json"
    $productId1 = $productResponse1.id
    Write-Host "✓ Product 1 created with ID: $productId1 (Price: $($productResponse1.price))" -ForegroundColor Green

    $productResponse2 = Invoke-RestMethod -Uri "$baseUrl/products" -Method Post -Body $product2 -ContentType "application/json"
    $productId2 = $productResponse2.id
    Write-Host "✓ Product 2 created with ID: $productId2 (Price: $($productResponse2.price))" -ForegroundColor Green
}
catch {
    Write-Host "✗ Failed to create products: $_" -ForegroundColor Red
    exit
}

# 3. Create Invoice (WITHOUT UnitPrice in request)
Write-Host ""
Write-Host "3. Creating invoice..." -ForegroundColor Yellow
Write-Host "   Note: UnitPrice is NOT sent - backend fetches from Product table" -ForegroundColor Cyan

$invoice = @{
    customerId = $customerId
    items = @(
        @{
            productId = $productId1
            quantity = 2
        },
        @{
            productId = $productId2
            quantity = 1
        }
    )
} | ConvertTo-Json -Depth 3

Write-Host ""
Write-Host "Request body:" -ForegroundColor Cyan
Write-Host $invoice -ForegroundColor Gray

try {
    $invoiceResponse = Invoke-RestMethod -Uri "$baseUrl/invoices" -Method Post -Body $invoice -ContentType "application/json"
    $invoiceId = $invoiceResponse.id

    Write-Host ""
    Write-Host "✓ Invoice created with ID: $invoiceId" -ForegroundColor Green
    Write-Host "  Customer: $($invoiceResponse.customerName)" -ForegroundColor White
    Write-Host "  Total Amount: $($invoiceResponse.totalAmount) SEK" -ForegroundColor White
    Write-Host ""
    Write-Host "  Items:" -ForegroundColor White
    foreach ($item in $invoiceResponse.items) {
        Write-Host "    - $($item.productName)" -ForegroundColor White
        Write-Host "      Quantity: $($item.quantity), Unit Price: $($item.unitPrice) SEK, Total: $($item.totalPrice) SEK" -ForegroundColor Gray
    }
}
catch {
    Write-Host "✗ Failed to create invoice: $_" -ForegroundColor Red
    exit
}

# 4. Get Invoice
Write-Host ""
Write-Host "4. Fetching invoice..." -ForegroundColor Yellow

try {
    $fetchedInvoice = Invoke-RestMethod -Uri "$baseUrl/invoices/$invoiceId" -Method Get
    Write-Host "✓ Invoice fetched successfully" -ForegroundColor Green
    Write-Host "  Response includes UnitPrice (fetched from DB):" -ForegroundColor Cyan
    $fetchedInvoice | ConvertTo-Json -Depth 3 | Write-Host -ForegroundColor Gray
}
catch {
    Write-Host "✗ Failed to fetch invoice: $_" -ForegroundColor Red
}

Write-Host ""
Write-Host "=== Test Complete ===" -ForegroundColor Cyan
