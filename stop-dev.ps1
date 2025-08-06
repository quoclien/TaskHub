# TaskHub Development Environment Stop Script
# Stops all running development services

param(
    [switch]$Help
)

function Show-Help {
    Write-Host "TaskHub Development Environment Stop Script" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "USAGE:" -ForegroundColor Yellow
    Write-Host "    .\stop-dev.ps1"
    Write-Host ""
    Write-Host "This script will:" -ForegroundColor Yellow
    Write-Host "  - Stop all Docker containers"
    Write-Host "  - Show running development processes"
    Write-Host "  - Provide guidance on stopping manual processes"
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
        docker-compose down 2>$null
        if ($LASTEXITCODE -eq 0) {
            Write-Status "Docker containers stopped"
            return $true
        } else {
            Write-Error "Failed to stop some Docker containers"
            return $false
        }
    } catch {
        Write-Error "Error stopping Docker containers"
        return $false
    }
}

function Show-RunningProcesses {
    Write-Host "Checking for running development processes..." -ForegroundColor Cyan
    
    # Look for dotnet processes
    $dotnetProcesses = Get-Process -Name "dotnet" -ErrorAction SilentlyContinue
    
    # Look for node processes  
    $nodeProcesses = Get-Process -Name "node" -ErrorAction SilentlyContinue
    
    # Look for pnpm processes
    $pnpmProcesses = Get-Process -Name "pnpm" -ErrorAction SilentlyContinue
    
    $allProcesses = @($dotnetProcesses) + @($nodeProcesses) + @($pnpmProcesses)
    
    if ($allProcesses.Count -eq 0) {
        Write-Status "No development processes found running"
        return
    }
    
    Write-Host ""
    Write-Host "Found $($allProcesses.Count) development processes:" -ForegroundColor Yellow
    foreach ($process in $allProcesses) {
        Write-Host "  - $($process.Name) (PID: $($process.Id))" -ForegroundColor Gray
    }
    
    Write-Host ""
    Write-Warning "To stop these processes:"
    Write-Host "  1. Use Ctrl+C in each terminal window" -ForegroundColor Gray
    Write-Host "  2. Or close the terminal windows manually" -ForegroundColor Gray
    Write-Host "  3. Or use Task Manager to end processes" -ForegroundColor Gray
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
        } else {
            Write-Status "All containers stopped"
        }
    } catch {
        Write-Host "Unable to check container status" -ForegroundColor Gray
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
Stop-DockerContainers | Out-Null

# Show running processes
Show-RunningProcesses

# Show Docker status
Write-Host ""
Show-DockerStatus

Write-Host ""
Write-Host "Stop script completed!" -ForegroundColor Green
Write-Host "Note: Development servers in terminal windows need to be stopped manually" -ForegroundColor Gray 