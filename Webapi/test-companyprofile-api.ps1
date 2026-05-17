# Test Company Profile API

$baseUrl = "https://localhost:5001/api"

Write-Host "=== Company Profile API Test ===" -ForegroundColor Cyan
Write-Host ""

Write-Host "1. Creating/Updating Company Profile..." -ForegroundColor Yellow

$companyProfile = @{
    companyName = "Acme Fakturering AB"
    organizationNumber = "556677-8899"
    email = "info@acmefakturering.se"
    phoneNumber = "+46 8 123 4567"
    street = "Storgatan 10"
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

Write-Host "Request Body:" -ForegroundColor Cyan
Write-Host $companyProfile -ForegroundColor Gray
Write-Host ""

try {
    $createResponse = Invoke-RestMethod -Uri "$baseUrl/companyprofile" -Method Put -Body $companyProfile -ContentType "application/json"

    Write-Host "✓ Company Profile created/updated successfully!" -ForegroundColor Green
    Write-Host "  ID: $($createResponse.id)" -ForegroundColor White
    Write-Host "  Company: $($createResponse.companyName)" -ForegroundColor White
    Write-Host "  Org Number: $($createResponse.organizationNumber)" -ForegroundColor White
    Write-Host "  Email: $($createResponse.email)" -ForegroundColor White
    Write-Host "  Address: $($createResponse.street), $($createResponse.postalCode) $($createResponse.city)" -ForegroundColor White
    Write-Host "  Bank Account: $($createResponse.bankAccountNumber)" -ForegroundColor White
    Write-Host "  IBAN: $($createResponse.iban)" -ForegroundColor White
    Write-Host "  VAT: $($createResponse.defaultVATPercentage)%" -ForegroundColor White
}
catch {
    Write-Host "✗ Failed to create/update company profile: $_" -ForegroundColor Red
    exit
}

Write-Host ""
Write-Host "2. Retrieving Company Profile..." -ForegroundColor Yellow

try {
    $getResponse = Invoke-RestMethod -Uri "$baseUrl/companyprofile" -Method Get

    Write-Host "✓ Company Profile retrieved successfully!" -ForegroundColor Green
    Write-Host ""
    Write-Host "Full Profile:" -ForegroundColor Cyan
    $getResponse | ConvertTo-Json -Depth 3 | Write-Host -ForegroundColor Gray
}
catch {
    Write-Host "✗ Failed to retrieve company profile: $_" -ForegroundColor Red
}

Write-Host ""
Write-Host "3. Updating Company Profile..." -ForegroundColor Yellow

$updateProfile = @{
    companyName = "Acme Fakturering AB (Updated)"
    organizationNumber = "556677-8899"
    email = "updated@acmefakturering.se"
    phoneNumber = "+46 8 999 8888"
    street = "Nygatan 5"
    postalCode = "222 33"
    city = "Göteborg"
    country = "Sweden"
    bankAccountNumber = "9876543210"
    clearingNumber = "9000"
    iban = "SE35 5000 0000 0554 9108 9999"
    swift = "ESSESESS"
    defaultOCRPrefix = "88"
    defaultVATPercentage = 12.0
} | ConvertTo-Json

try {
    $updateResponse = Invoke-RestMethod -Uri "$baseUrl/companyprofile" -Method Put -Body $updateProfile -ContentType "application/json"

    Write-Host "✓ Company Profile updated!" -ForegroundColor Green
    Write-Host "  Company: $($updateResponse.companyName)" -ForegroundColor White
    Write-Host "  New Email: $($updateResponse.email)" -ForegroundColor White
    Write-Host "  New City: $($updateResponse.city)" -ForegroundColor White
    Write-Host "  New VAT: $($updateResponse.defaultVATPercentage)%" -ForegroundColor White
    Write-Host "  Updated At: $($updateResponse.updatedAt)" -ForegroundColor White
}
catch {
    Write-Host "✗ Failed to update company profile: $_" -ForegroundColor Red
}

Write-Host ""
Write-Host "=== Test Complete ===" -ForegroundColor Cyan
Write-Host ""
Write-Host "Company Profile is ready for invoice PDF generation!" -ForegroundColor Green
