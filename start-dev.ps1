# TaskHub Development Environment Startup Script
# Starts all required services for local development

param(
    [switch]$Frontend,
    [switch]$Backend,
    [switch]$All,
    [switch]$Help
)

function Show-Help {
    Write-Host "TaskHub Development Environment Startup Script" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "USAGE:" -ForegroundColor Yellow
    Write-Host "    .\start-dev.ps1 [OPTIONS]"
    Write-Host ""
    Write-Host "OPTIONS:" -ForegroundColor Yellow
    Write-Host "    -All              Start all services (default)"
    Write-Host "    -Frontend         Start only frontend development server"
    Write-Host "    -Backend          Start only backend development server"
    Write-Host "    -Help             Show this help message"
    Write-Host ""
    Write-Host "EXAMPLES:" -ForegroundColor Yellow
    Write-Host "    .\start-dev.ps1                    # Start all services"
    Write-Host "    .\start-dev.ps1 -Frontend          # Start only frontend"
    Write-Host "    .\start-dev.ps1 -Backend           # Start only backend"
    Write-Host ""
}

function Write-Status {
    param([string]$Message)
    Write-Host "[OK] $Message" -ForegroundColor Green
}

function Write-Warning {
    param([string]$Message)
    Write-Host "[WARN] $Message" -ForegroundColor Yellow
}

function Write-Error {
    param([string]$Message)
    Write-Host "[ERROR] $Message" -ForegroundColor Red
}

function Test-DockerDesktop {
    Write-Host "Checking Docker Desktop..." -ForegroundColor Cyan
    
    # Check if Docker Desktop process is running
    $dockerProcess = Get-Process -Name "Docker Desktop" -ErrorAction SilentlyContinue
    if (-not $dockerProcess) {
        Write-Error "Docker Desktop is not running"
        Write-Host "Please start Docker Desktop and wait for it to fully initialize" -ForegroundColor Yellow
        Write-Host "Then run this script again" -ForegroundColor Yellow
        return $false
    }
    
    # Test Docker daemon connectivity
    try {
        $null = docker version --format "{{.Server.Version}}" 2>$null
        if ($LASTEXITCODE -eq 0) {
            Write-Status "Docker Desktop is running and accessible"
            return $true
        } else {
            Write-Warning "Docker Desktop is starting up, please wait a moment"
            return $false
        }
    } catch {
        Write-Warning "Docker Desktop is still initializing"
        return $false
    }
}

function Test-Prerequisites {
    Write-Host "Checking prerequisites..." -ForegroundColor Cyan
    $allGood = $true
    
    # Check .NET
    try {
        $dotnetVersion = dotnet --version 2>$null
        if ($LASTEXITCODE -eq 0) {
            Write-Status ".NET SDK version: $dotnetVersion"
        } else {
            Write-Error ".NET SDK not found"
            $allGood = $false
        }
    } catch {
        Write-Error ".NET SDK not found"
        $allGood = $false
    }
    
    # Check PNPM
    try {
        $pnpmVersion = pnpm --version 2>$null
        if ($LASTEXITCODE -eq 0) {
            Write-Status "PNPM version: $pnpmVersion"
        } else {
            Write-Error "PNPM not found"
            $allGood = $false
        }
    } catch {
        Write-Error "PNPM not found"
        $allGood = $false
    }
    
    return $allGood
}

function Start-Database {
    Write-Host "Starting PostgreSQL database..." -ForegroundColor Cyan
    
    # Start database container
    docker-compose up -d db 2>$null
    if ($LASTEXITCODE -eq 0) {
        Write-Status "Database container started"
        
        # Wait for database to be ready
        Write-Host "Waiting for database to be ready..."
        $attempts = 0
        $maxAttempts = 15
        
        do {
            Start-Sleep -Seconds 2
            $attempts++
            $dbStatus = docker-compose exec -T db pg_isready -U postgres 2>$null
            
            if ($attempts -eq 8) {
                Write-Host "Still waiting for database..." -ForegroundColor Yellow
            }
        } while ($dbStatus -notlike "*accepting connections*" -and $attempts -lt $maxAttempts)
        
        if ($attempts -lt $maxAttempts) {
            Write-Status "Database is ready"
            return $true
        } else {
            Write-Error "Database failed to start within timeout"
            return $false
        }
    } else {
        Write-Error "Failed to start database container"
        return $false
    }
}

function Start-Migrations {
    Write-Host "Applying database migrations..." -ForegroundColor Cyan
    
    Push-Location "apps\api"
    try {
        dotnet ef database update 2>$null
        if ($LASTEXITCODE -eq 0) {
            Write-Status "Database migrations applied"
            return $true
        } else {
            Write-Warning "Migration failed, continuing anyway"
            return $false
        }
    } finally {
        Pop-Location
    }
}

function Start-BackendServer {
    Write-Host "Starting backend development server..." -ForegroundColor Cyan
    
    $currentPath = Get-Location
    $backendCmd = "Set-Location '$currentPath'; dotnet watch --project apps\api\TaskHub.Api.csproj"
    
    # Try Windows Terminal first, fallback to regular PowerShell
    try {
        Start-Process "wt" -ArgumentList @(
            "-p", "Windows PowerShell",
            "--title", "TaskHub Backend",
            "powershell.exe", "-NoExit", "-Command", $backendCmd
        ) -ErrorAction Stop
        Write-Status "Backend server started in Windows Terminal"
    } catch {
        Start-Process "powershell" -ArgumentList @(
            "-NoExit", "-Command", $backendCmd
        )
        Write-Status "Backend server started in PowerShell window"
    }
}

function Start-FrontendServer {
    Write-Host "Starting frontend development server..." -ForegroundColor Cyan
    
    $currentPath = Get-Location
    $frontendCmd = "Set-Location '$currentPath'; pnpm --filter @taskhub/web dev"
    
    # Try Windows Terminal first, fallback to regular PowerShell
    try {
        Start-Process "wt" -ArgumentList @(
            "-p", "Windows PowerShell",
            "--title", "TaskHub Frontend",
            "powershell.exe", "-NoExit", "-Command", $frontendCmd
        ) -ErrorAction Stop
        Write-Status "Frontend server started in Windows Terminal"
    } catch {
        Start-Process "powershell" -ArgumentList @(
            "-NoExit", "-Command", $frontendCmd
        )
        Write-Status "Frontend server started in PowerShell window"
    }
}

function Install-Dependencies {
    Write-Host "Installing dependencies..." -ForegroundColor Cyan
    
    pnpm install --frozen-lockfile 2>$null
    if ($LASTEXITCODE -eq 0) {
        Write-Status "Dependencies installed"
        return $true
    } else {
        Write-Warning "Some dependencies may not be up to date"
        return $false
    }
}

function Show-Summary {
    Write-Host ""
    Write-Host "Development Environment Ready!" -ForegroundColor Green
    Write-Host "==============================" -ForegroundColor Green
    Write-Host ""
    Write-Host "Services:" -ForegroundColor White
    Write-Host "  Frontend:  http://localhost:3000" -ForegroundColor White
    Write-Host "  Backend:   http://localhost:5000" -ForegroundColor White
    Write-Host "  Swagger:   http://localhost:5000/swagger" -ForegroundColor White
    Write-Host "  Database:  localhost:5432 (postgres/postgres)" -ForegroundColor White
    Write-Host ""
    Write-Host "To stop services:" -ForegroundColor Gray
    Write-Host "  Containers: .\stop-dev.ps1" -ForegroundColor Gray
    Write-Host "  Servers:    Use Ctrl+C in each terminal window" -ForegroundColor Gray
    Write-Host ""
}

# Main execution
if ($Help) {
    Show-Help
    exit 0
}

Write-Host "TaskHub Development Environment Startup" -ForegroundColor Magenta
Write-Host "=======================================" -ForegroundColor Magenta
Write-Host ""

# Determine what to start
$startAll = $All -or (-not $Frontend -and -not $Backend)
$startFrontend = $Frontend -or $startAll
$startBackend = $Backend -or $startAll

# Check prerequisites
if (-not (Test-Prerequisites)) {
    Write-Error "Prerequisites check failed. Please install missing dependencies."
    exit 1
}

# Check Docker Desktop if backend needed
$dockerReady = $true
if ($startBackend) {
    $dockerReady = Test-DockerDesktop
    if (-not $dockerReady) {
        Write-Host ""
        Write-Host "Options:" -ForegroundColor Yellow
        Write-Host "  1. Start Docker Desktop and run this script again"
        Write-Host "  2. Run frontend only: .\start-dev.ps1 -Frontend"
        Write-Host ""
        exit 1
    }
}

# Install dependencies
Install-Dependencies | Out-Null

# Start backend services if Docker is ready
if ($startBackend -and $dockerReady) {
    if (Start-Database) {
        Start-Migrations | Out-Null
    } else {
        Write-Error "Database startup failed"
        exit 1
    }
}

# Start development servers
if ($startBackend) {
    Start-BackendServer
    Start-Sleep -Seconds 2
}

if ($startFrontend) {
    Start-FrontendServer
    Start-Sleep -Seconds 2
}

# Show summary
if ($startFrontend -or $startBackend) {
    Start-Sleep -Seconds 3
    Show-Summary
}

Write-Host "Startup complete!" -ForegroundColor Green 