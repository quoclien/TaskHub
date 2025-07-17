# Simple JWT Authentication Test Script
Write-Host "Testing JWT Authentication..." -ForegroundColor Green

# Wait for server to start
Start-Sleep -Seconds 5

# Test 1: Generate a JWT token
Write-Host ""
Write-Host "1. Testing JWT token generation..." -ForegroundColor Yellow
try {
    $tokenResponse = Invoke-RestMethod -Uri "http://localhost:5205/api/Development/generate-token" -Method Get
    Write-Host "Token generated successfully" -ForegroundColor Green
    $token = $tokenResponse.token
    Write-Host "Token: $($token.Substring(0, 50))..." -ForegroundColor Gray
} catch {
    Write-Host "Failed to generate token: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

# Test 2: Validate the token
Write-Host ""
Write-Host "2. Testing JWT token validation..." -ForegroundColor Yellow
try {
    $headers = @{
        "Authorization" = "Bearer $token"
    }
    $validationResponse = Invoke-RestMethod -Uri "http://localhost:5205/api/Development/validate-token" -Method Get -Headers $headers
    Write-Host "Token validation successful" -ForegroundColor Green
    Write-Host "User ID: $($validationResponse.UserId)" -ForegroundColor Gray
    Write-Host "Is Authenticated: $($validationResponse.IsAuthenticated)" -ForegroundColor Gray
} catch {
    Write-Host "Failed to validate token: $($_.Exception.Message)" -ForegroundColor Red
}

# Test 3: Test protected endpoint
Write-Host ""
Write-Host "3. Testing protected endpoint..." -ForegroundColor Yellow
try {
    $boardsResponse = Invoke-RestMethod -Uri "http://localhost:5205/api/Boards" -Method Get -Headers $headers
    Write-Host "Boards endpoint accessible with token" -ForegroundColor Green
    Write-Host "Boards returned: $($boardsResponse.Count)" -ForegroundColor Gray
} catch {
    Write-Host "Failed to access boards endpoint: $($_.Exception.Message)" -ForegroundColor Red
    if ($_.Exception.Response) {
        Write-Host "Status Code: $($_.Exception.Response.StatusCode)" -ForegroundColor Red
    }
}

Write-Host ""
Write-Host "Authentication tests completed!" -ForegroundColor Green 