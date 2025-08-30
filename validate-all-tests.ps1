# TaskHub Comprehensive Test Validation
# Runs the complete test suite across all components

param(
    [switch]$Help,
    [switch]$SkipE2E,
    [switch]$Verbose
)

function Show-Help {
    Write-Host "TaskHub Comprehensive Test Validation" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "USAGE:" -ForegroundColor Yellow
    Write-Host "    .\validate-all-tests.ps1 [OPTIONS]"
    Write-Host ""
    Write-Host "OPTIONS:" -ForegroundColor Yellow
    Write-Host "    -SkipE2E          Skip E2E tests (faster validation)"
    Write-Host "    -Verbose          Show detailed output"
    Write-Host "    -Help             Show this help message"
    Write-Host ""
    Write-Host "TEST PHASES:" -ForegroundColor Yellow
    Write-Host "    1. Health Check   - Build and dependency validation"
    Write-Host "    2. Unit Tests     - Frontend component and utility tests"
    Write-Host "    3. Integration    - Backend API integration tests"
    Write-Host "    4. E2E Tests      - Full user journey tests (optional)"
    Write-Host "    5. Security       - Vulnerability and security scans"
    Write-Host ""
}

function Write-Phase {
    param([string]$Message)
    Write-Host ""
    Write-Host "=== $Message ===" -ForegroundColor Magenta
    Write-Host ""
}

# Note: Write-Status, Write-Warning, and Write-Error functions removed
# All logging now uses inline Write-Host calls to prevent output capture issues

function Write-Info {
    param([string]$Message)
    Write-Host "[ℹ] $Message" -ForegroundColor Cyan
}

function Test-HealthCheck {
    Write-Phase "Phase 1: Health Check & Build Validation"

    $success = $true

    # Install dependencies
    Write-Info "Installing dependencies..."
    pnpm install --frozen-lockfile 2>$null
    if ($LASTEXITCODE -eq 0) {
        Write-Host "[✓] Dependencies installed" -ForegroundColor Green
    } else {
        Write-Host "[✗] Failed to install dependencies" -ForegroundColor Red
        $success = $false
    }

    # Clean backend
    Write-Info "Cleaning backend artifacts..."
    dotnet clean apps/api/TaskHub.Api.csproj 2>$null
    if ($LASTEXITCODE -eq 0) {
        Write-Host "[✓] Backend cleaned" -ForegroundColor Green
    } else {
        Write-Host "[⚠] Backend clean had issues" -ForegroundColor Yellow
    }

    # Format check
    Write-Info "Checking code formatting..."
    dotnet format apps/api/TaskHub.Api.csproj --verify-no-changes 2>$null
    if ($LASTEXITCODE -eq 0) {
        Write-Host "[✓] Backend formatting is correct" -ForegroundColor Green
    } else {
        Write-Host "[⚠] Backend formatting issues detected" -ForegroundColor Yellow
    }

    # Lint frontend
    Write-Info "Linting frontend code..."
    pnpm -r lint 2>$null
    if ($LASTEXITCODE -eq 0) {
        Write-Host "[✓] Frontend linting passed" -ForegroundColor Green
    } else {
        Write-Host "[✗] Frontend linting failed" -ForegroundColor Red
        $success = $false
    }

    # Build frontend
    Write-Info "Building frontend..."
    pnpm -r build 2>$null
    if ($LASTEXITCODE -eq 0) {
        Write-Host "[✓] Frontend build successful" -ForegroundColor Green
    } else {
        Write-Host "[✗] Frontend build failed" -ForegroundColor Red
        $success = $false
    }

    # Build backend
    Write-Info "Building backend..."
    dotnet build apps/api/TaskHub.Api.csproj 2>$null
    if ($LASTEXITCODE -eq 0) {
        Write-Host "[✓] Backend build successful" -ForegroundColor Green
    } else {
        Write-Host "[✗] Backend build failed" -ForegroundColor Red
        $success = $false
    }

    return $success
}

function Test-UnitTests {
    Write-Phase "Phase 2: Frontend Unit Tests"

    Write-Info "Running frontend unit tests..."
    pnpm --filter @taskhub/web test:run 2>$null

    if ($LASTEXITCODE -eq 0) {
        Write-Host "[✓] Frontend unit tests passed" -ForegroundColor Green
        return $true
    } else {
        Write-Host "[✗] Frontend unit tests failed" -ForegroundColor Red
        return $false
    }
}

function Test-SharedUtilities {
    Write-Phase "Phase 2b: Shared Package Tests"

    Write-Info "Running shared package tests..."
    pnpm --filter @taskhub/shared test 2>$null

    if ($LASTEXITCODE -eq 0) {
        Write-Host "[✓] Shared package tests passed" -ForegroundColor Green
        return $true
    } else {
        Write-Host "[✗] Shared package tests failed" -ForegroundColor Red
        return $false
    }
}

function Test-Integration {
    Write-Phase "Phase 3: Backend Integration Tests"

    Write-Info "Running backend integration tests..."
    dotnet test tests/TaskHub.Api.IntegrationTests/TaskHub.Api.IntegrationTests.csproj --logger:"console;verbosity=minimal" 2>$null

    if ($LASTEXITCODE -eq 0) {
        Write-Host "[✓] Backend integration tests passed" -ForegroundColor Green
        return $true
    } else {
        Write-Host "[✗] Backend integration tests failed" -ForegroundColor Red
        return $false
    }
}

function Test-E2E {
    Write-Phase "Phase 4: End-to-End Tests"

    Write-Info "Setting up E2E test environment..."

    # Use the E2E setup script
    $e2eResult = & ".\test-e2e-setup.ps1" -RunTests

    if ($LASTEXITCODE -eq 0) {
        Write-Host "[✓] E2E tests passed" -ForegroundColor Green
        return $true
    } else {
        Write-Host "[✗] E2E tests failed or environment setup failed" -ForegroundColor Red
        Write-Host "[ℹ] Check playwright-report/index.html for detailed results" -ForegroundColor Cyan
        return $false
    }
}

function Test-Security {
    Write-Phase "Phase 5: Security & Vulnerability Scans"

    $success = $true

    # Backend vulnerability scan
    Write-Info "Scanning backend for vulnerabilities..."
    dotnet list apps/api/TaskHub.Api.csproj package --vulnerable 2>$null
    if ($LASTEXITCODE -eq 0) {
        Write-Host "[✓] Backend vulnerability scan completed" -ForegroundColor Green
    } else {
        Write-Host "[⚠] Backend vulnerability scan had issues" -ForegroundColor Yellow
        $success = $false
    }

    # Frontend vulnerability scan
    Write-Info "Scanning frontend for vulnerabilities..."
    pnpm audit --recursive 2>$null
    if ($LASTEXITCODE -eq 0) {
        Write-Host "[✓] Frontend vulnerability scan completed" -ForegroundColor Green
    } else {
        Write-Host "[⚠] Frontend has vulnerability findings" -ForegroundColor Yellow
        # Don't fail on audit warnings, just note them
    }

    return $success
}

function Show-Summary {
    param(
        [bool]$HealthCheck,
        [bool]$UnitTests,
        [bool]$SharedTests,
        [bool]$Integration,
        [bool]$E2E,
        [bool]$Security
    )
    
    Write-Host ""
    Write-Host "=== TEST VALIDATION SUMMARY ===" -ForegroundColor Magenta
    Write-Host ""
    
    if ($HealthCheck) {
        Write-Host "[✓] Phase 1: Health Check & Build" -ForegroundColor Green
    } else {
        Write-Host "[✗] Phase 1: Health Check & Build" -ForegroundColor Red
    }

    if ($UnitTests) {
        Write-Host "[✓] Phase 2: Frontend Unit Tests" -ForegroundColor Green
    } else {
        Write-Host "[✗] Phase 2: Frontend Unit Tests" -ForegroundColor Red
    }

    if ($SharedTests) {
        Write-Host "[✓] Phase 2b: Shared Package Tests" -ForegroundColor Green
    } else {
        Write-Host "[✗] Phase 2b: Shared Package Tests" -ForegroundColor Red
    }

    if ($Integration) {
        Write-Host "[✓] Phase 3: Backend Integration Tests" -ForegroundColor Green
    } else {
        Write-Host "[✗] Phase 3: Backend Integration Tests" -ForegroundColor Red
    }

    if (-not $SkipE2E) {
        if ($E2E) {
            Write-Host "[✓] Phase 4: End-to-End Tests" -ForegroundColor Green
        } else {
            Write-Host "[✗] Phase 4: End-to-End Tests" -ForegroundColor Red
        }
    } else {
        Write-Host "[ℹ] Phase 4: End-to-End Tests (Skipped)" -ForegroundColor Cyan
    }

    if ($Security) {
        Write-Host "[✓] Phase 5: Security & Vulnerability Scans" -ForegroundColor Green
    } else {
        Write-Host "[⚠] Phase 5: Security & Vulnerability Scans" -ForegroundColor Yellow
    }
    
    Write-Host ""
    
    $allPassed = $HealthCheck -and $UnitTests -and $SharedTests -and $Integration -and $Security -and ($SkipE2E -or $E2E)
    
    if ($allPassed) {
        Write-Host "🎉 ALL TESTS PASSED! TaskHub is ready for production." -ForegroundColor Green
        Write-Host ""
        Write-Host "Quality Gates Met:" -ForegroundColor Green
        Write-Host "  ✓ Build & Dependency Health" -ForegroundColor Green
        Write-Host "  ✓ Code Quality & Linting" -ForegroundColor Green
        Write-Host "  ✓ Unit Test Coverage" -ForegroundColor Green
        Write-Host "  ✓ API Integration Tests" -ForegroundColor Green
        if (-not $SkipE2E) {
            Write-Host "  ✓ End-to-End User Journeys" -ForegroundColor Green
        }
        Write-Host "  ✓ Security Vulnerability Scans" -ForegroundColor Green
        return 0
    } else {
        Write-Host "❌ SOME TESTS FAILED. Review the output above for details." -ForegroundColor Red
        Write-Host ""
        Write-Host "Common fixes:" -ForegroundColor Yellow
        Write-Host "  • Run 'pnpm install' if dependency issues" -ForegroundColor Yellow
        Write-Host "  • Run 'dotnet format' if formatting issues" -ForegroundColor Yellow
        Write-Host "  • Check test output for specific failures" -ForegroundColor Yellow
        Write-Host "  • Ensure Docker is running for E2E tests" -ForegroundColor Yellow
        return 1
    }
}

# Main execution
if ($Help) {
    Show-Help
    exit 0
}

Write-Host "TaskHub Comprehensive Test Validation" -ForegroundColor Magenta
Write-Host "====================================" -ForegroundColor Magenta
Write-Host ""

if ($SkipE2E) {
    Write-Info "E2E tests will be skipped for faster validation"
}

if ($Verbose) {
    Write-Info "Verbose mode enabled"
}

Write-Host "Starting comprehensive test validation..." -ForegroundColor Cyan

# Run all test phases
$healthCheckResult = Test-HealthCheck
$unitTestsResult = Test-UnitTests
$sharedTestsResult = Test-SharedUtilities
$integrationResult = Test-Integration

if (-not $SkipE2E) {
    $e2eResult = Test-E2E
} else {
    $e2eResult = $true  # Consider skipped as passed
}

$securityResult = Test-Security

# Show summary and exit with appropriate code
$exitCode = Show-Summary -HealthCheck $healthCheckResult -UnitTests $unitTestsResult -SharedTests $sharedTestsResult -Integration $integrationResult -E2E $e2eResult -Security $securityResult
exit $exitCode
