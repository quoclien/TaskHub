# TaskHub Backend Startup Script
# This script starts the backend services for development

Write-Host "Starting TaskHub Backend Services..." -ForegroundColor Green

# Check if Docker is running
try {
    docker version | Out-Null
    Write-Host "✓ Docker is running" -ForegroundColor Green
} catch {
    Write-Host "✗ Docker is not running. Please start Docker Desktop first." -ForegroundColor Red
    Write-Host "You can still use the frontend with mock authentication." -ForegroundColor Yellow
    exit 1
}

# Start the backend services
Write-Host "Starting database and API services..." -ForegroundColor Yellow
docker-compose up -d db api

# Wait a moment for services to start
Start-Sleep -Seconds 5

# Check if services are running
$apiStatus = docker-compose ps api
if ($apiStatus -like "*Up*") {
    Write-Host "✓ API service is running on http://localhost:5000" -ForegroundColor Green
    Write-Host "✓ Development token endpoint: http://localhost:5000/api/Development/generate-token" -ForegroundColor Green
} else {
    Write-Host "✗ API service failed to start" -ForegroundColor Red
    Write-Host "Check the logs with: docker-compose logs api" -ForegroundColor Yellow
}

$dbStatus = docker-compose ps db
if ($dbStatus -like "*Up*") {
    Write-Host "✓ Database service is running" -ForegroundColor Green
} else {
    Write-Host "✗ Database service failed to start" -ForegroundColor Red
}

Write-Host ""
Write-Host "Frontend Development:" -ForegroundColor Cyan
Write-Host "  - Frontend: http://localhost:3000" -ForegroundColor White
Write-Host "  - Sign-in will now use real development tokens" -ForegroundColor White
Write-Host ""
Write-Host "To stop services: docker-compose down" -ForegroundColor Yellow
Write-Host "To view logs: docker-compose logs -f" -ForegroundColor Yellow 