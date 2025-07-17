# Debug Test Script
Write-Host "=== Debug API Testing ===" -ForegroundColor Green

# Step 1: Create test user
Write-Host ""
Write-Host "1. Creating test user..." -ForegroundColor Yellow
try {
    $userResponse = Invoke-RestMethod -Uri "http://localhost:5205/api/Development/create-test-user" -Method Post
    Write-Host "User creation result: $($userResponse.message)" -ForegroundColor Green
    Write-Host "User ID: $($userResponse.id)" -ForegroundColor Gray
    Write-Host "Email: $($userResponse.email)" -ForegroundColor Gray
} catch {
    Write-Host "Failed to create user: $($_.Exception.Message)" -ForegroundColor Red
}

# Step 2: Generate token
Write-Host ""
Write-Host "2. Generating JWT token..." -ForegroundColor Yellow
try {
    $tokenResponse = Invoke-RestMethod -Uri "http://localhost:5205/api/Development/generate-token" -Method Get
    Write-Host "Token generated successfully" -ForegroundColor Green
    Write-Host "User ID in token: $($tokenResponse.userId)" -ForegroundColor Gray
    Write-Host "Email: $($tokenResponse.email)" -ForegroundColor Gray
    $token = $tokenResponse.token
    $headers = @{
        "Authorization" = "Bearer $token"
        "Content-Type" = "application/json"
    }
} catch {
    Write-Host "Failed to generate token: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

# Step 3: Validate token
Write-Host ""
Write-Host "3. Validating JWT token..." -ForegroundColor Yellow
try {
    $validationResponse = Invoke-RestMethod -Uri "http://localhost:5205/api/Development/validate-token" -Method Get -Headers $headers
    Write-Host "Token validation successful" -ForegroundColor Green
    Write-Host "User ID from token: $($validationResponse.UserId)" -ForegroundColor Gray
    Write-Host "Is Authenticated: $($validationResponse.IsAuthenticated)" -ForegroundColor Gray
} catch {
    Write-Host "Failed to validate token: $($_.Exception.Message)" -ForegroundColor Red
}

# Step 4: Try to create board
Write-Host ""
Write-Host "4. Creating a board..." -ForegroundColor Yellow
try {
    $boardData = @{
        title = "Debug Test Board"
        description = "Debug test board"
        isPublic = $true
    }
    $boardJson = $boardData | ConvertTo-Json
    $board = Invoke-RestMethod -Uri "http://localhost:5205/api/Boards" -Method Post -Headers $headers -Body $boardJson
    Write-Host "Board created successfully" -ForegroundColor Green
    Write-Host "Board ID: $($board.id)" -ForegroundColor Gray
    Write-Host "Board Title: $($board.title)" -ForegroundColor Gray
} catch {
    Write-Host "Failed to create board: $($_.Exception.Message)" -ForegroundColor Red
    if ($_.Exception.Response) {
        Write-Host "Status Code: $($_.Exception.Response.StatusCode)" -ForegroundColor Red
    }
}

Write-Host ""
Write-Host "Debug test completed!" -ForegroundColor Green 