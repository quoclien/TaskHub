# JWT Authentication Test Script
# This script tests the JWT authentication functionality

Write-Host "Testing JWT Authentication..." -ForegroundColor Green

# Wait for server to start
Start-Sleep -Seconds 10

# Test 1: Generate a JWT token
Write-Host "`n1. Testing JWT token generation..." -ForegroundColor Yellow
try {
    $tokenResponse = Invoke-RestMethod -Uri "http://localhost:5205/api/Development/generate-token" -Method Get
    Write-Host "✓ Token generated successfully" -ForegroundColor Green
    Write-Host "Token: $($tokenResponse.token.Substring(0, 50))..." -ForegroundColor Gray
    $token = $tokenResponse.token
} catch {
    Write-Host "✗ Failed to generate token: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

# Test 2: Validate the token
Write-Host "`n2. Testing JWT token validation..." -ForegroundColor Yellow
try {
    $headers = @{
        "Authorization" = "Bearer $token"
    }
    $validationResponse = Invoke-RestMethod -Uri "http://localhost:5205/api/Development/validate-token" -Method Get -Headers $headers
    Write-Host "✓ Token validation successful" -ForegroundColor Green
    Write-Host "User ID: $($validationResponse.UserId)" -ForegroundColor Gray
    Write-Host "Is Authenticated: $($validationResponse.IsAuthenticated)" -ForegroundColor Gray
} catch {
    Write-Host "✗ Failed to validate token: $($_.Exception.Message)" -ForegroundColor Red
    Write-Host "Response: $($_.Exception.Response)" -ForegroundColor Red
}

# Test 3: Test protected endpoint (Boards)
Write-Host "`n3. Testing protected endpoint (GET /api/Boards)..." -ForegroundColor Yellow
try {
    $boardsResponse = Invoke-RestMethod -Uri "http://localhost:5205/api/Boards" -Method Get -Headers $headers
    Write-Host "✓ Boards endpoint accessible with token" -ForegroundColor Green
    Write-Host "Boards count: $($boardsResponse.Count)" -ForegroundColor Gray
} catch {
    Write-Host "✗ Failed to access boards endpoint: $($_.Exception.Message)" -ForegroundColor Red
    if ($_.Exception.Response) {
        Write-Host "Status Code: $($_.Exception.Response.StatusCode)" -ForegroundColor Red
    }
}

# Test 4: Test protected endpoint without token
Write-Host "`n4. Testing protected endpoint without token..." -ForegroundColor Yellow
try {
    $unauthorizedResponse = Invoke-RestMethod -Uri "http://localhost:5205/api/Boards" -Method Get
    Write-Host "✗ Endpoint should have returned 401 Unauthorized" -ForegroundColor Red
} catch {
    if ($_.Exception.Response.StatusCode -eq 401) {
        Write-Host "✓ Correctly returned 401 Unauthorized" -ForegroundColor Green
    } else {
        Write-Host "✗ Unexpected error: $($_.Exception.Message)" -ForegroundColor Red
    }
}

# Test 5: Test board creation
Write-Host "`n5. Testing board creation..." -ForegroundColor Yellow
try {
    $boardData = @{
        title = "Test Board"
        description = "Test board from authentication script"
        isPublic = $true
    }
    $boardJson = $boardData | ConvertTo-Json
    $createResponse = Invoke-RestMethod -Uri "http://localhost:5205/api/Boards" -Method Post -Headers $headers -Body $boardJson -ContentType "application/json"
    Write-Host "✓ Board created successfully" -ForegroundColor Green
    Write-Host "Board ID: $($createResponse.id)" -ForegroundColor Gray
} catch {
    Write-Host "✗ Failed to create board: $($_.Exception.Message)" -ForegroundColor Red
    if ($_.Exception.Response) {
        Write-Host "Status Code: $($_.Exception.Response.StatusCode)" -ForegroundColor Red
    }
}

Write-Host "`nAuthentication tests completed!" -ForegroundColor Green 