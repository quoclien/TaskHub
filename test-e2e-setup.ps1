# TaskHub E2E Testing Environment Setup
# Sets up the complete environment for running E2E tests

param(
    [switch]$Help,
    [switch]$RunTests,
    [switch]$Cleanup
)

function Show-Help {
    Write-Host "TaskHub E2E Testing Environment Setup" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "USAGE:" -ForegroundColor Yellow
    Write-Host "    .\test-e2e-setup.ps1 [OPTIONS]"
    Write-Host ""
    Write-Host "OPTIONS:" -ForegroundColor Yellow
    Write-Host "    -RunTests         Setup environment and run E2E tests"
    Write-Host "    -Cleanup          Stop all services and cleanup"
    Write-Host "    -Help             Show this help message"
    Write-Host ""
    Write-Host "EXAMPLES:" -ForegroundColor Yellow
    Write-Host "    .\test-e2e-setup.ps1 -RunTests    # Setup and run E2E tests"
    Write-Host "    .\test-e2e-setup.ps1 -Cleanup     # Cleanup all services"
    Write-Host ""
}

function Write-Status {
    param([string]$Message)
    Write-Host "[E2E] $Message" -ForegroundColor Green
}

function Write-Warning {
    param([string]$Message)
    Write-Host "[E2E] $Message" -ForegroundColor Yellow
}

function Write-Error {
    param([string]$Message)
    Write-Host "[E2E] $Message" -ForegroundColor Red
}

function Test-Prerequisites {
    Write-Host "Checking E2E test prerequisites..." -ForegroundColor Cyan
    $allGood = $true
    
    # Check Docker
    try {
        $null = docker version --format "{{.Server.Version}}" 2>$null
        if ($LASTEXITCODE -eq 0) {
            Write-Status "Docker is available"
        } else {
            Write-Error "Docker is not running"
            $allGood = $false
        }
    } catch {
        Write-Error "Docker not found"
        $allGood = $false
    }
    
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
    
    # Check Playwright
    try {
        $playwrightVersion = pnpm --filter @taskhub/web exec playwright --version 2>$null
        if ($LASTEXITCODE -eq 0) {
            Write-Status "Playwright is available"
        } else {
            Write-Warning "Playwright may need to be installed"
        }
    } catch {
        Write-Warning "Could not check Playwright version"
    }
    
    return $allGood
}

function Start-TestEnvironment {
    Write-Host "Setting up E2E test environment..." -ForegroundColor Cyan
    
    # Install dependencies
    Write-Host "Installing dependencies..."
    pnpm install --frozen-lockfile 2>$null
    if ($LASTEXITCODE -eq 0) {
        Write-Status "Dependencies installed"
    } else {
        Write-Warning "Some dependencies may not be up to date"
    }
    
    # Start database
    Write-Host "Starting PostgreSQL database for E2E tests..."
    docker-compose -f docker-compose.dev.yml up -d db 2>$null
    if ($LASTEXITCODE -eq 0) {
        Write-Status "Database container started"
        
        # Wait for database to be ready
        Write-Host "Waiting for database to be ready..."
        $attempts = 0
        $maxAttempts = 15
        
        do {
            Start-Sleep -Seconds 2
            $attempts++
            $dbStatus = docker-compose -f docker-compose.dev.yml exec -T db pg_isready -U postgres 2>$null
            
            if ($attempts -eq 8) {
                Write-Host "Still waiting for database..." -ForegroundColor Yellow
            }
        } while ($dbStatus -notlike "*accepting connections*" -and $attempts -lt $maxAttempts)
        
        if ($attempts -lt $maxAttempts) {
            Write-Status "Database is ready"
        } else {
            Write-Error "Database failed to start within timeout"
            return $false
        }
    } else {
        Write-Error "Failed to start database container"
        return $false
    }
    
    # Run migrations
    Write-Host "Applying database migrations..."
    Push-Location "apps\api"
    try {
        dotnet ef database update 2>$null
        if ($LASTEXITCODE -eq 0) {
            Write-Status "Database migrations applied"
        } else {
            Write-Warning "Migration failed, continuing anyway"
        }
    } finally {
        Pop-Location
    }
    
    # Start backend API in background
    Write-Host "Starting backend API for E2E tests..."
    $backendProcess = Start-Process "dotnet" -ArgumentList @(
        "run", "--project", "apps\api\TaskHub.Api.csproj"
    ) -PassThru -WindowStyle Hidden
    
    if ($backendProcess) {
        Write-Status "Backend API started (PID: $($backendProcess.Id))"
        
        # Wait for API to be ready
        Write-Host "Waiting for API to be ready..."
        $apiReady = $false
        $attempts = 0
        $maxAttempts = 15
        
        do {
            Start-Sleep -Seconds 3
            $attempts++
            try {
                $response = Invoke-WebRequest -Uri "http://localhost:5000/health" -TimeoutSec 5 -ErrorAction SilentlyContinue
                if ($response.StatusCode -eq 200) {
                    $apiReady = $true
                    Write-Status "API is ready"
                }
            } catch {
                # API not ready yet
            }
            
            if ($attempts -eq 8) {
                Write-Host "Still waiting for API..." -ForegroundColor Yellow
            }
        } while (-not $apiReady -and $attempts -lt $maxAttempts)
        
        if (-not $apiReady) {
            Write-Warning "API may not be fully ready, but continuing with tests"
        }
    } else {
        Write-Error "Failed to start backend API"
        return $false
    }
    
    # Install Playwright browsers if needed
    Write-Host "Ensuring Playwright browsers are installed..."
    pnpm --filter @taskhub/web exec playwright install 2>$null
    if ($LASTEXITCODE -eq 0) {
        Write-Status "Playwright browsers ready"
    } else {
        Write-Warning "Playwright browser installation may have issues"
    }
    
    return $true
}

function Run-E2ETests {
    Write-Host "Running E2E tests..." -ForegroundColor Cyan

    # Check if web directory exists
    if (-not (Test-Path "apps\web")) {
        Write-Error "Web application directory not found at apps/web"
        return $false
    }

    # Set environment for E2E tests
    $env:NEXT_PUBLIC_DISABLE_MOCKS = "true"
    $env:NEXT_PUBLIC_API_URL = "http://localhost:5000"

    # Run Playwright tests
    Push-Location "apps\web"
    try {
        # Check if playwright config exists
        if (-not (Test-Path "playwright.config.ts")) {
            Write-Error "Playwright configuration not found at apps/web/playwright.config.ts"
            return $false
        }

        # Check if pnpm is available
        try {
            $null = pnpm --version 2>$null
            if ($LASTEXITCODE -ne 0) {
                Write-Error "PNPM is not available or not properly configured"
                return $false
            }
        } catch {
            Write-Error "Failed to check PNPM availability: $($_.Exception.Message)"
            return $false
        }

        Write-Host "Executing Playwright tests..." -ForegroundColor Gray
        pnpm playwright test --config=playwright.config.ts --reporter=html
        $testResult = $LASTEXITCODE

        if ($testResult -eq 0) {
            Write-Status "E2E tests completed successfully"
            $reportPath = "playwright-report/index.html"
            if (Test-Path $reportPath) {
                Write-Host "Test report available at: $(Resolve-Path $reportPath)" -ForegroundColor Cyan
            }
        } else {
            Write-Error "E2E tests failed with exit code: $testResult"
            $reportPath = "playwright-report/index.html"
            if (Test-Path $reportPath) {
                Write-Host "Check the test report for details: $(Resolve-Path $reportPath)" -ForegroundColor Yellow
            } else {
                Write-Warning "Test report not found - tests may have failed to generate output"
            }
        }

        return $testResult -eq 0
    } catch {
        Write-Error "Failed to execute E2E tests: $($_.Exception.Message)"
        return $false
    } finally {
        Pop-Location
    }
}

function Stop-TestEnvironment {
    Write-Host "Cleaning up E2E test environment..." -ForegroundColor Cyan
    
    # Stop any dotnet processes (backend API)
    $dotnetProcesses = Get-Process -Name "dotnet" -ErrorAction SilentlyContinue
    foreach ($process in $dotnetProcesses) {
        if ($process.ProcessName -eq "dotnet") {
            try {
                $process.Kill()
                Write-Status "Stopped backend process (PID: $($process.Id))"
            } catch {
                Write-Warning "Could not stop process $($process.Id)"
            }
        }
    }
    
    # Stop Docker containers
    docker-compose -f docker-compose.dev.yml down 2>$null
    if ($LASTEXITCODE -eq 0) {
        Write-Status "Docker containers stopped"
    } else {
        Write-Warning "Could not stop all Docker containers"
    }
    
    Write-Status "Cleanup completed"
}

# Main execution
if ($Help) {
    Show-Help
    exit 0
}

Write-Host "TaskHub E2E Testing Environment" -ForegroundColor Magenta
Write-Host "===============================" -ForegroundColor Magenta
Write-Host ""

if ($Cleanup) {
    Stop-TestEnvironment
    exit 0
}

# Check prerequisites
if (-not (Test-Prerequisites)) {
    Write-Error "Prerequisites check failed. Please install missing dependencies."
    exit 1
}

if ($RunTests) {
    try {
        # Setup environment
        if (Start-TestEnvironment) {
            Write-Host ""
            Write-Host "Environment ready! Running E2E tests..." -ForegroundColor Green
            Write-Host ""
            
            # Run tests
            $testSuccess = Run-E2ETests
            
            Write-Host ""
            if ($testSuccess) {
                Write-Host "E2E testing completed successfully!" -ForegroundColor Green
            } else {
                Write-Host "E2E testing completed with failures." -ForegroundColor Yellow
            }
        } else {
            Write-Error "Failed to setup test environment"
            exit 1
        }
    } finally {
        Write-Host ""
        Write-Host "Cleaning up..." -ForegroundColor Cyan
        Stop-TestEnvironment
    }
} else {
    Write-Host "Use -RunTests to setup environment and run E2E tests" -ForegroundColor Yellow
    Write-Host "Use -Help for more options" -ForegroundColor Yellow
}
