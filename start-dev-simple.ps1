# TaskHub Simple Development Startup Script
# This script starts TaskHub with a working configuration

Write-Host "🚀 Starting TaskHub Development Environment..." -ForegroundColor Green
Write-Host ""

# Stop any existing containers
Write-Host "🛑 Cleaning up existing containers..." -ForegroundColor Yellow
docker-compose down --remove-orphans 2>$null

# Start the database
Write-Host "🗃️  Starting database..." -ForegroundColor Cyan
docker-compose -f docker-compose.dev.yml up -d

# Wait for database to be ready
Write-Host "⏳ Waiting for database to be ready..." -ForegroundColor Yellow
Start-Sleep -Seconds 8

# Check database status
$dbStatus = docker inspect taskhub-db-dev --format='{{.State.Health.Status}}' 2>$null
if ($dbStatus -eq "healthy") {
    Write-Host "✅ Database is ready!" -ForegroundColor Green
} else {
    Write-Host "⚠️  Database may still be starting..." -ForegroundColor Yellow
}

Write-Host ""
Write-Host "🎯 Next Steps:" -ForegroundColor Cyan
Write-Host "1. Start API: " -NoNewline
Write-Host "cd apps/api && dotnet run --urls `"http://localhost:5000`"" -ForegroundColor White
Write-Host "2. Start Frontend: " -NoNewline  
Write-Host "pnpm dev:web" -ForegroundColor White
Write-Host ""
Write-Host "🔐 Test Accounts:" -ForegroundColor Green
Write-Host "   Admin: admin@example.com / admin" -ForegroundColor White
Write-Host "   Demo:  demo@taskhub.com / Demo123!" -ForegroundColor White
Write-Host ""
Write-Host "🌐 URLs:" -ForegroundColor Cyan
Write-Host "   Frontend: http://localhost:3000" -ForegroundColor White
Write-Host "   API:      http://localhost:5000" -ForegroundColor White
Write-Host "   Health:   http://localhost:5000/health" -ForegroundColor White
Write-Host ""
Write-Host "💡 To stop: docker-compose -f docker-compose.dev.yml down" -ForegroundColor Yellow