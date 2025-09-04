# TaskHub Development Environment Stop Script
# Stops all running development services

param(
    [switch]$Help,
    [switch]$Force,
    [switch]$KillProcesses
)

function Show-Help {
    Write-Host "TaskHub Development Environment Stop Script" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "USAGE:" -ForegroundColor Yellow
    Write-Host "    .\stop-dev.ps1 [OPTIONS]"
    Write-Host ""
    Write-Host "OPTIONS:" -ForegroundColor Yellow
    Write-Host "    -KillProcesses    Automatically stop development processes"
    Write-Host "    -Force            Skip confirmation prompts"
    Write-Host "    -Help             Show this help message"
    Write-Host ""
    Write-Host "This script will:" -ForegroundColor Yellow
    Write-Host "  - Stop all Docker containers"
    Write-Host "  - Show/optionally kill running development processes"
    Write-Host "  - Provide cleanup status"
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

function Stop-DockerContainers {
    Write-Host "Stopping Docker containers..." -ForegroundColor Cyan

    try {
        $result = docker-compose down 2>$null
        if ($LASTEXITCODE -eq 0) {
            Write-Status "Docker containers stopped"
            return $true
        }
        else {
            Write-Error "Failed to stop some Docker containers"
            return $false
        }
    }
    catch {
        Write-Error "Error stopping Docker containers: $($_.Exception.Message)"
        return $false
    }
}

function Stop-RunningProcesses {
    Write-Host "Checking for running development processes..." -ForegroundColor Cyan

    # Look for dotnet processes
    $dotnetProcesses = Get-Process -Name "dotnet" -ErrorAction SilentlyContinue

    # Look for node processes
    $nodeProcesses = Get-Process -Name "node" -ErrorAction SilentlyContinue

    # Look for pnpm processes
    $pnpmProcesses = Get-Process -Name "pnpm" -ErrorAction SilentlyContinue

    $allProcesses = @()
    if ($dotnetProcesses) { $allProcesses += $dotnetProcesses }
    if ($nodeProcesses) { $allProcesses += $nodeProcesses }
    if ($pnpmProcesses) { $allProcesses += $pnpmProcesses }

    if ($allProcesses.Count -eq 0) {
        Write-Host "[+] No development processes found running" -ForegroundColor Green
        return
    }

    Write-Host ""
    Write-Host "Found $($allProcesses.Count) development processes:" -ForegroundColor Yellow
    foreach ($process in $allProcesses) {
        Write-Host "  - $($process.Name) (PID: $($process.Id))" -ForegroundColor Gray
    }

    # Determine if we should kill processes
    $shouldKill = $KillProcesses
    if (-not $KillProcesses -and -not $Force) {
        Write-Host ""
        $response = Read-Host "Do you want to stop these processes? (y/n)"
        $shouldKill = ($response -eq 'y' -or $response -eq 'Y')
    }

    if ($shouldKill) {
        Write-Host ""
        Write-Host "Stopping processes..." -ForegroundColor Yellow
        $stoppedCount = 0

        foreach ($process in $allProcesses) {
            try {
                $process.Kill()
                $process.WaitForExit(5000)
                Write-Host "[+] Stopped $($process.Name) (PID: $($process.Id))" -ForegroundColor Green
                $stoppedCount++
            }
            catch {
                Write-Host "[!] Failed to stop $($process.Name) (PID: $($process.Id)): $($_.Exception.Message)" -ForegroundColor Yellow
            }
        }

        Write-Host ""
        Write-Host "Successfully stopped $stoppedCount out of $($allProcesses.Count) processes" -ForegroundColor Cyan
    }
    else {
        Write-Host ""
        Write-Host "Processes not stopped. To stop them manually:" -ForegroundColor Cyan
        Write-Host "  1. Use Ctrl+C in each terminal window" -ForegroundColor Gray
        Write-Host "  2. Or close the terminal windows manually" -ForegroundColor Gray
        Write-Host "  3. Or use Task Manager to end processes" -ForegroundColor Gray
        Write-Host "  4. Or run: .\stop-dev.ps1 -KillProcesses" -ForegroundColor Gray
    }
}

function Show-DockerStatus {
    Write-Host "Docker containers status:" -ForegroundColor Cyan

    try {
        $runningContainers = docker-compose ps --services --filter "status=running" 2>$null
        if ($runningContainers) {
            Write-Host "Still running:" -ForegroundColor Yellow
            foreach ($container in $runningContainers) {
                Write-Host "  - $container" -ForegroundColor Gray
            }
        }
        else {
            Write-Status "All containers stopped"
        }
    }
    catch {
        Write-Host "Unable to check container status: $($_.Exception.Message)" -ForegroundColor Gray
    }
}

# Main execution
if ($Help) {
    Show-Help
    exit 0
}

Write-Host "TaskHub Development Environment Stop" -ForegroundColor Red
Write-Host "====================================" -ForegroundColor Red
Write-Host ""

# Stop Docker containers
$result = Stop-DockerContainers

# Handle running processes
Stop-RunningProcesses

# Show Docker status
Write-Host ""
Show-DockerStatus

Write-Host ""
Write-Host "Stop script completed!" -ForegroundColor Green
if ($KillProcesses) {
    Write-Host "All development processes and containers have been stopped." -ForegroundColor Green
}
else {
    Write-Host "Note: Use -KillProcesses to automatically stop development processes" -ForegroundColor Gray
} 