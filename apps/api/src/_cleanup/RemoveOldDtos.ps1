<#
.SYNOPSIS
    Removes old DTO files from the Models/Dtos folder.

.DESCRIPTION
    This script removes outdated Data Transfer Object (DTO) files from the
    Models/Dtos directory. It provides a confirmation prompt before deletion
    and removes the directory if it becomes empty.

.PARAMETER ProjectRoot
    The root path of the API project. Defaults to the script's location
    relative to the project structure.

.PARAMETER Force
    Skip confirmation prompt and proceed with deletion.

.PARAMETER WhatIf
    Show what would be deleted without actually deleting files.

.EXAMPLE
    .\Remove-OldDtos.ps1

    Shows confirmation prompt before removing DTO files.

.EXAMPLE
    .\Remove-OldDtos.ps1 -Force

    Removes DTO files without confirmation prompt.

.EXAMPLE
    .\Remove-OldDtos.ps1 -WhatIf

    Shows what files would be removed without deleting them.

.NOTES
    This script is intended for one-time cleanup operations.
    Make sure to backup important files before running.
#>

param(
    [Parameter(Mandatory = $false)]
    [string]$ProjectRoot,

    [Parameter(Mandatory = $false)]
    [switch]$Force,

    [Parameter(Mandatory = $false)]
    [switch]$WhatIf
)

# Set default project root if not provided
if (-not $ProjectRoot) {
    # Assume script is in apps/api/src/_cleanup/, so go up three levels
    $scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
    $ProjectRoot = Split-Path -Parent (Split-Path -Parent (Split-Path -Parent $scriptDir))
}

# Define the DTO files to remove
$dtoFiles = @(
    "$ProjectRoot\Models\Dtos\BoardDto.cs",
    "$ProjectRoot\Models\Dtos\BoardMemberDto.cs",
    "$ProjectRoot\Models\Dtos\CardDto.cs",
    "$ProjectRoot\Models\Dtos\CommentDto.cs",
    "$ProjectRoot\Models\Dtos\CreateBoardDto.cs",
    "$ProjectRoot\Models\Dtos\CreateCardDto.cs",
    "$ProjectRoot\Models\Dtos\CreateListDto.cs",
    "$ProjectRoot\Models\Dtos\ListDto.cs",
    "$ProjectRoot\Models\Dtos\MoveCardDto.cs",
    "$ProjectRoot\Models\Dtos\UpdateBoardDto.cs",
    "$ProjectRoot\Models\Dtos\UpdateCardDto.cs",
    "$ProjectRoot\Models\Dtos\UpdateListDto.cs"
)

$dtoDir = "$ProjectRoot\Models\Dtos"

function Write-Status {
    param([string]$Message)
    Write-Host "[INFO] $Message" -ForegroundColor Green
}

function Write-Warning {
    param([string]$Message)
    Write-Host "[WARN] $Message" -ForegroundColor Yellow
}

function Write-Error {
    param([string]$Message)
    Write-Host "[ERROR] $Message" -ForegroundColor Red
}

# Check if project root exists
if (-not (Test-Path $ProjectRoot)) {
    Write-Error "Project root path does not exist: $ProjectRoot"
    exit 1
}

Write-Host "TaskHub DTO Cleanup Script" -ForegroundColor Cyan
Write-Host "========================" -ForegroundColor Cyan
Write-Host ""

# Filter to only existing files
$existingFiles = $dtoFiles | Where-Object { Test-Path $_ }

if ($existingFiles.Count -eq 0) {
    Write-Status "No DTO files found to remove."
    exit 0
}

Write-Host "The following files will be removed:" -ForegroundColor Yellow
$existingFiles | ForEach-Object { Write-Host "  - $_" -ForegroundColor Gray }

$filesToSkip = $dtoFiles.Count - $existingFiles.Count
if ($filesToSkip -gt 0) {
    Write-Host ""
    Write-Warning "$filesToSkip files not found (already removed or moved)"
}

# Confirmation prompt
$shouldProceed = $Force
if (-not $Force -and -not $WhatIf) {
    Write-Host ""
    $confirmation = Read-Host "Do you want to proceed with the deletion? (y/n)"
    $shouldProceed = $confirmation -eq 'y' -or $confirmation -eq 'Y'
}

if ($WhatIf) {
    Write-Host ""
    Write-Host "WhatIf mode - No files will be deleted" -ForegroundColor Cyan
    $shouldProceed = $true
}

if ($shouldProceed) {
    if (-not $WhatIf) {
        Write-Host ""
        Write-Host "Removing files..." -ForegroundColor Yellow
    }

    $removedCount = 0
    foreach ($file in $existingFiles) {
        if ($WhatIf) {
            Write-Host "Would remove: $file" -ForegroundColor Gray
        } else {
            try {
                Remove-Item $file -Force -ErrorAction Stop
                Write-Status "Removed: $file"
                $removedCount++
            } catch {
                Write-Error "Failed to remove $file : $($_.Exception.Message)"
            }
        }
    }

    # Check if the Models/Dtos directory is empty and remove it if it is
    if ((Test-Path $dtoDir) -and -not $WhatIf) {
        $remainingItems = Get-ChildItem -Path $dtoDir -Recurse -Force
        if ($remainingItems.Count -eq 0) {
            try {
                Remove-Item $dtoDir -Force -Recurse -ErrorAction Stop
                Write-Status "Removed empty directory: $dtoDir"
            } catch {
                Write-Warning "Could not remove empty directory: $($_.Exception.Message)"
            }
        } else {
            Write-Status "Directory not empty, keeping: $dtoDir ($($remainingItems.Count) items remain)"
        }
    } elseif ($WhatIf -and (Test-Path $dtoDir)) {
        Write-Host "Would check and potentially remove directory: $dtoDir" -ForegroundColor Gray
    }

    Write-Host ""
    if ($WhatIf) {
        Write-Status "WhatIf completed - $existingFiles.Count files would be processed"
    } else {
        Write-Status "Cleanup completed successfully. Removed $removedCount files."
    }
} else {
    Write-Host ""
    Write-Host "Operation cancelled by user." -ForegroundColor Yellow
}
