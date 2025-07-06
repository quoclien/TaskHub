# PowerShell script to remove old DTOs from Models/Dtos folder
# Run this script from the root of the project

$projectRoot = "D:\Code\TaskHub\apps\api\src"
$dtoFiles = @(
    "$projectRoot\Models\Dtos\BoardDto.cs",
    "$projectRoot\Models\Dtos\BoardMemberDto.cs",
    "$projectRoot\Models\Dtos\CardDto.cs",
    "$projectRoot\Models\Dtos\CommentDto.cs",
    "$projectRoot\Models\Dtos\CreateBoardDto.cs",
    "$projectRoot\Models\Dtos\CreateCardDto.cs",
    "$projectRoot\Models\Dtos\CreateListDto.cs",
    "$projectRoot\Models\Dtos\ListDto.cs",
    "$projectRoot\Models\Dtos\MoveCardDto.cs",
    "$projectRoot\Models\Dtos\UpdateBoardDto.cs",
    "$projectRoot\Models\Dtos\UpdateCardDto.cs",
    "$projectRoot\Models\Dtos\UpdateListDto.cs"
)

Write-Host "The following files will be removed:"
$dtoFiles | ForEach-Object { Write-Host "- $_" }

$confirmation = Read-Host "Do you want to proceed with the deletion? (y/n)"

if ($confirmation -eq 'y') {
    $dtoFiles | ForEach-Object {
        if (Test-Path $_) {
            Remove-Item $_ -Force
            Write-Host "Removed: $_"
        } else {
            Write-Host "File not found, skipping: $_"
        }
    }
    
    # Check if the Models/Dtos directory is empty and remove it if it is
    $dtoDir = "$projectRoot\Models\Dtos"
    if ((Get-ChildItem -Path $dtoDir -Recurse -Force | Measure-Object).Count -eq 0) {
        Remove-Item $dtoDir -Force -Recurse
        Write-Host "Removed empty directory: $dtoDir"
    }
    
    Write-Host "Cleanup completed successfully." -ForegroundColor Green
} else {
    Write-Host "Operation cancelled by user." -ForegroundColor Yellow
}
