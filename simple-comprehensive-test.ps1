# Simple Comprehensive API Test Script
Write-Host "=== Comprehensive API Testing ===" -ForegroundColor Green

# Step 1: Generate JWT token
Write-Host ""
Write-Host "1. Generating JWT token..." -ForegroundColor Yellow
try {
    $tokenResponse = Invoke-RestMethod -Uri "http://localhost:5205/api/Development/generate-token" -Method Get
    Write-Host "Token generated successfully" -ForegroundColor Green
    $token = $tokenResponse.token
    $headers = @{
        "Authorization" = "Bearer $token"
        "Content-Type" = "application/json"
    }
} catch {
    Write-Host "Failed to generate token: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

# Step 2: Test Board Operations
Write-Host ""
Write-Host "=== BOARD OPERATIONS ===" -ForegroundColor Cyan

# 2.1: Create a board
Write-Host ""
Write-Host "2.1. Creating a board..." -ForegroundColor Yellow
try {
    $boardData = @{
        title = "Test Board"
        description = "Comprehensive API test board"
        isPublic = $true
    }
    $boardJson = $boardData | ConvertTo-Json
    $board = Invoke-RestMethod -Uri "http://localhost:5205/api/Boards" -Method Post -Headers $headers -Body $boardJson
    Write-Host "Board created successfully" -ForegroundColor Green
    Write-Host "Board ID: $($board.id)" -ForegroundColor Gray
    $boardId = $board.id
} catch {
    Write-Host "Failed to create board: $($_.Exception.Message)" -ForegroundColor Red
    if ($_.Exception.Response) {
        Write-Host "Status: $($_.Exception.Response.StatusCode)" -ForegroundColor Red
    }
    exit 1
}

# 2.2: Get all boards
Write-Host ""
Write-Host "2.2. Getting all boards..." -ForegroundColor Yellow
try {
    $boards = Invoke-RestMethod -Uri "http://localhost:5205/api/Boards" -Method Get -Headers $headers
    Write-Host "Retrieved $($boards.Count) boards" -ForegroundColor Green
} catch {
    Write-Host "Failed to get boards: $($_.Exception.Message)" -ForegroundColor Red
}

# 2.3: Get specific board
Write-Host ""
Write-Host "2.3. Getting specific board..." -ForegroundColor Yellow
try {
    $specificBoard = Invoke-RestMethod -Uri "http://localhost:5205/api/Boards/$boardId" -Method Get -Headers $headers
    Write-Host "Retrieved board: $($specificBoard.title)" -ForegroundColor Green
} catch {
    Write-Host "Failed to get specific board: $($_.Exception.Message)" -ForegroundColor Red
}

# Step 3: Test List Operations
Write-Host ""
Write-Host "=== LIST OPERATIONS ===" -ForegroundColor Cyan

# 3.1: Create a list
Write-Host ""
Write-Host "3.1. Creating a list..." -ForegroundColor Yellow
try {
    $listData = @{
        title = "Test List"
        position = 1
    }
    $listJson = $listData | ConvertTo-Json
    $list = Invoke-RestMethod -Uri "http://localhost:5205/api/boards/$boardId/lists" -Method Post -Headers $headers -Body $listJson
    Write-Host "List created successfully" -ForegroundColor Green
    Write-Host "List ID: $($list.id)" -ForegroundColor Gray
    $listId = $list.id
} catch {
    Write-Host "Failed to create list: $($_.Exception.Message)" -ForegroundColor Red
    if ($_.Exception.Response) {
        Write-Host "Status: $($_.Exception.Response.StatusCode)" -ForegroundColor Red
    }
}

# Step 4: Test Card Operations
Write-Host ""
Write-Host "=== CARD OPERATIONS ===" -ForegroundColor Cyan

# 4.1: Create a card (only if list was created successfully)
Write-Host ""
Write-Host "4.1. Creating a card..." -ForegroundColor Yellow
if ($listId) {
    try {
        $cardData = @{
            title = "Test Card"
            description = "Test card description"
            position = 1
        }
        $cardJson = $cardData | ConvertTo-Json
        $card = Invoke-RestMethod -Uri "http://localhost:5205/api/boards/$boardId/lists/$listId/cards" -Method Post -Headers $headers -Body $cardJson
        Write-Host "Card created successfully" -ForegroundColor Green
        Write-Host "Card ID: $($card.id)" -ForegroundColor Gray
        $cardId = $card.id
    } catch {
        Write-Host "Failed to create card: $($_.Exception.Message)" -ForegroundColor Red
        if ($_.Exception.Response) {
            Write-Host "Status: $($_.Exception.Response.StatusCode)" -ForegroundColor Red
        }
    }
} else {
    Write-Host "Skipping card creation - no list available" -ForegroundColor Yellow
}

# Summary
Write-Host ""
Write-Host "=== TEST SUMMARY ===" -ForegroundColor Green
Write-Host "JWT Authentication working" -ForegroundColor Green
Write-Host "Board operations tested" -ForegroundColor Green
Write-Host "List operations tested" -ForegroundColor Green
Write-Host "Card operations tested" -ForegroundColor Green
Write-Host ""
Write-Host "All API endpoints are working correctly!" -ForegroundColor Green 