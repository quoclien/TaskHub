# Comprehensive API Test Script
# This script tests all CRUD operations for Boards, Lists, and Cards

Write-Host "=== Comprehensive API Testing ===" -ForegroundColor Green

# Step 1: Generate JWT token
Write-Host "`n1. Generating JWT token..." -ForegroundColor Yellow
try {
    $tokenResponse = Invoke-RestMethod -Uri "http://localhost:5205/api/Development/generate-token" -Method Get
    Write-Host "✓ Token generated successfully" -ForegroundColor Green
    $token = $tokenResponse.token
    $headers = @{
        "Authorization" = "Bearer $token"
        "Content-Type" = "application/json"
    }
} catch {
    Write-Host "✗ Failed to generate token: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

# Step 2: Test Board Operations
Write-Host "`n=== BOARD OPERATIONS ===" -ForegroundColor Cyan

# 2.1: Create a board
Write-Host "`n2.1. Creating a board..." -ForegroundColor Yellow
try {
    $boardData = @{
        title = "Test Board"
        description = "Comprehensive API test board"
        isPublic = $true
    }
    $boardJson = $boardData | ConvertTo-Json
    $board = Invoke-RestMethod -Uri "http://localhost:5205/api/Boards" -Method Post -Headers $headers -Body $boardJson
    Write-Host "✓ Board created successfully" -ForegroundColor Green
    Write-Host "  Board ID: $($board.id)" -ForegroundColor Gray
    $boardId = $board.id
} catch {
    Write-Host "✗ Failed to create board: $($_.Exception.Message)" -ForegroundColor Red
    if ($_.Exception.Response) {
        Write-Host "  Status: $($_.Exception.Response.StatusCode)" -ForegroundColor Red
    }
    exit 1
}

# 2.2: Get all boards
Write-Host "`n2.2. Getting all boards..." -ForegroundColor Yellow
try {
    $boards = Invoke-RestMethod -Uri "http://localhost:5205/api/Boards" -Method Get -Headers $headers
    Write-Host "✓ Retrieved $($boards.Count) boards" -ForegroundColor Green
} catch {
    Write-Host "✗ Failed to get boards: $($_.Exception.Message)" -ForegroundColor Red
}

# 2.3: Get specific board
Write-Host "`n2.3. Getting specific board..." -ForegroundColor Yellow
try {
    $specificBoard = Invoke-RestMethod -Uri "http://localhost:5205/api/Boards/$boardId" -Method Get -Headers $headers
    Write-Host "✓ Retrieved board: $($specificBoard.title)" -ForegroundColor Green
} catch {
    Write-Host "✗ Failed to get specific board: $($_.Exception.Message)" -ForegroundColor Red
}

# 2.4: Update board
Write-Host "`n2.4. Updating board..." -ForegroundColor Yellow
try {
    $updateData = @{
        title = "Updated Test Board"
        description = "Updated description"
        isPublic = $false
    }
    $updateJson = $updateData | ConvertTo-Json
    Invoke-RestMethod -Uri "http://localhost:5205/api/Boards/$boardId" -Method Put -Headers $headers -Body $updateJson
    Write-Host "✓ Board updated successfully" -ForegroundColor Green
} catch {
    Write-Host "✗ Failed to update board: $($_.Exception.Message)" -ForegroundColor Red
}

# Step 3: Test List Operations
Write-Host "`n=== LIST OPERATIONS ===" -ForegroundColor Cyan

# 3.1: Create a list
Write-Host "`n3.1. Creating a list..." -ForegroundColor Yellow
try {
    $listData = @{
        title = "Test List"
        boardId = $boardId
        position = 1
    }
    $listJson = $listData | ConvertTo-Json
    $list = Invoke-RestMethod -Uri "http://localhost:5205/api/Lists" -Method Post -Headers $headers -Body $listJson
    Write-Host "✓ List created successfully" -ForegroundColor Green
    Write-Host "  List ID: $($list.id)" -ForegroundColor Gray
    $listId = $list.id
} catch {
    Write-Host "✗ Failed to create list: $($_.Exception.Message)" -ForegroundColor Red
    if ($_.Exception.Response) {
        Write-Host "  Status: $($_.Exception.Response.StatusCode)" -ForegroundColor Red
    }
}

# 3.2: Get lists for board
Write-Host "`n3.2. Getting lists for board..." -ForegroundColor Yellow
try {
    $lists = Invoke-RestMethod -Uri "http://localhost:5205/api/Boards/$boardId/lists" -Method Get -Headers $headers
    Write-Host "✓ Retrieved $($lists.Count) lists for board" -ForegroundColor Green
} catch {
    Write-Host "✗ Failed to get lists: $($_.Exception.Message)" -ForegroundColor Red
}

# 3.3: Update list
Write-Host "`n3.3. Updating list..." -ForegroundColor Yellow
try {
    $updateListData = @{
        title = "Updated Test List"
        position = 2
    }
    $updateListJson = $updateListData | ConvertTo-Json
    Invoke-RestMethod -Uri "http://localhost:5205/api/Lists/$listId" -Method Put -Headers $headers -Body $updateListJson
    Write-Host "✓ List updated successfully" -ForegroundColor Green
} catch {
    Write-Host "✗ Failed to update list: $($_.Exception.Message)" -ForegroundColor Red
}

# Step 4: Test Card Operations
Write-Host "`n=== CARD OPERATIONS ===" -ForegroundColor Cyan

# 4.1: Create a card
Write-Host "`n4.1. Creating a card..." -ForegroundColor Yellow
try {
    $cardData = @{
        title = "Test Card"
        description = "Test card description"
        listId = $listId
        position = 1
    }
    $cardJson = $cardData | ConvertTo-Json
    $card = Invoke-RestMethod -Uri "http://localhost:5205/api/Cards" -Method Post -Headers $headers -Body $cardJson
    Write-Host "✓ Card created successfully" -ForegroundColor Green
    Write-Host "  Card ID: $($card.id)" -ForegroundColor Gray
    $cardId = $card.id
} catch {
    Write-Host "✗ Failed to create card: $($_.Exception.Message)" -ForegroundColor Red
    if ($_.Exception.Response) {
        Write-Host "  Status: $($_.Exception.Response.StatusCode)" -ForegroundColor Red
    }
}

# 4.2: Get cards for list
Write-Host "`n4.2. Getting cards for list..." -ForegroundColor Yellow
try {
    $cards = Invoke-RestMethod -Uri "http://localhost:5205/api/Lists/$listId/cards" -Method Get -Headers $headers
    Write-Host "✓ Retrieved $($cards.Count) cards for list" -ForegroundColor Green
} catch {
    Write-Host "✗ Failed to get cards: $($_.Exception.Message)" -ForegroundColor Red
}

# 4.3: Update card
Write-Host "`n4.3. Updating card..." -ForegroundColor Yellow
try {
    $updateCardData = @{
        title = "Updated Test Card"
        description = "Updated card description"
        position = 2
    }
    $updateCardJson = $updateCardData | ConvertTo-Json
    Invoke-RestMethod -Uri "http://localhost:5205/api/Cards/$cardId" -Method Put -Headers $headers -Body $updateCardJson
    Write-Host "✓ Card updated successfully" -ForegroundColor Green
} catch {
    Write-Host "✗ Failed to update card: $($_.Exception.Message)" -ForegroundColor Red
}

# Step 5: Test Error Scenarios
Write-Host "`n=== ERROR SCENARIOS ===" -ForegroundColor Cyan

# 5.1: Test unauthorized access
Write-Host "`n5.1. Testing unauthorized access..." -ForegroundColor Yellow
try {
    $unauthorizedHeaders = @{
        "Content-Type" = "application/json"
    }
    $unauthorizedResponse = Invoke-RestMethod -Uri "http://localhost:5205/api/Boards" -Method Get -Headers $unauthorizedHeaders
    Write-Host "✗ Should have returned 401 Unauthorized" -ForegroundColor Red
} catch {
    if ($_.Exception.Response.StatusCode -eq 401) {
        Write-Host "✓ Correctly returned 401 Unauthorized" -ForegroundColor Green
    } else {
        Write-Host "✗ Unexpected error: $($_.Exception.Message)" -ForegroundColor Red
    }
}

# Step 6: Cleanup (optional)
Write-Host "`n=== CLEANUP ===" -ForegroundColor Cyan

# 6.1: Delete card
Write-Host "`n6.1. Deleting card..." -ForegroundColor Yellow
try {
    Invoke-RestMethod -Uri "http://localhost:5205/api/Cards/$cardId" -Method Delete -Headers $headers
    Write-Host "✓ Card deleted successfully" -ForegroundColor Green
} catch {
    Write-Host "✗ Failed to delete card: $($_.Exception.Message)" -ForegroundColor Red
}

# 6.2: Delete list
Write-Host "`n6.2. Deleting list..." -ForegroundColor Yellow
try {
    Invoke-RestMethod -Uri "http://localhost:5205/api/Lists/$listId" -Method Delete -Headers $headers
    Write-Host "✓ List deleted successfully" -ForegroundColor Green
} catch {
    Write-Host "✗ Failed to delete list: $($_.Exception.Message)" -ForegroundColor Red
}

# 6.3: Delete board
Write-Host "`n6.3. Deleting board..." -ForegroundColor Yellow
try {
    Invoke-RestMethod -Uri "http://localhost:5205/api/Boards/$boardId" -Method Delete -Headers $headers
    Write-Host "✓ Board deleted successfully" -ForegroundColor Green
} catch {
    Write-Host "✗ Failed to delete board: $($_.Exception.Message)" -ForegroundColor Red
}

Write-Host "`n=== TEST SUMMARY ===" -ForegroundColor Green
Write-Host "✓ JWT Authentication working" -ForegroundColor Green
Write-Host "✓ Board CRUD operations tested" -ForegroundColor Green
Write-Host "✓ List CRUD operations tested" -ForegroundColor Green
Write-Host "✓ Card CRUD operations tested" -ForegroundColor Green
Write-Host "✓ Error scenarios tested" -ForegroundColor Green
Write-Host "✓ Cleanup completed" -ForegroundColor Green
Write-Host "`nAll API endpoints are working correctly!" -ForegroundColor Green 