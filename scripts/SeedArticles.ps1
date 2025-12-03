# Script to seed articles data
# Make sure the API is running before executing this script

$apiUrl = "http://localhost:5147/api/articles/seed"

try {
    Write-Host "Seeding articles..." -ForegroundColor Yellow
    
    $response = Invoke-RestMethod -Uri $apiUrl -Method POST -ContentType "application/json"
    
    Write-Host "Success!" -ForegroundColor Green
    Write-Host "Message: $($response.message)" -ForegroundColor Cyan
    Write-Host "Articles created: $($response.count)" -ForegroundColor Cyan
}
catch {
    Write-Host "Error seeding articles:" -ForegroundColor Red
    Write-Host $_.Exception.Message -ForegroundColor Red
    Write-Host "`nMake sure:" -ForegroundColor Yellow
    Write-Host "1. The API project is running" -ForegroundColor Yellow
    Write-Host "2. The API is accessible at $apiUrl" -ForegroundColor Yellow
    Write-Host "3. The database is properly configured" -ForegroundColor Yellow
}
