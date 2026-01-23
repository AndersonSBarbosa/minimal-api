# PowerShell Script to fix Git ref locking issues
# This script resolves "cannot lock ref" errors that occur during git fetch/pull operations

# Requires -Version 5.1

function Write-ColorOutput {
    param(
        [string]$Message,
        [string]$ForegroundColor = "White"
    )
    Write-Host $Message -ForegroundColor $ForegroundColor
}

function Test-GitRepository {
    try {
        git rev-parse --git-dir 2>$null | Out-Null
        return $true
    }
    catch {
        return $false
    }
}

function Backup-GitRefs {
    Write-ColorOutput "Creating backup of .git directory..." "Yellow"
    $backupDir = ".git_backup_$(Get-Date -Format 'yyyyMMdd_HHmmss')"
    Copy-Item -Path ".git" -Destination $backupDir -Recurse -Force
    Write-ColorOutput "Backup created at: $backupDir" "Green"
    Write-Host ""
}

function Remove-PackedRefs {
    Write-ColorOutput "Fixing packed-refs file..." "Yellow"
    $packedRefsPath = ".git\packed-refs"
    
    if (Test-Path $packedRefsPath) {
        Remove-Item $packedRefsPath -Force
        Write-ColorOutput "Removed packed-refs file" "Green"
    }
    else {
        Write-Host "No packed-refs file found"
    }
    Write-Host ""
}

function Remove-CorruptedRefs {
    Write-ColorOutput "Scanning for corrupted ref files..." "Yellow"
    
    $remoteRefsPath = ".git\refs\remotes\origin"
    if (Test-Path $remoteRefsPath) {
        Get-ChildItem -Path $remoteRefsPath -File -Recurse | ForEach-Object {
            $branchName = $_.Name
            $refPath = "refs/remotes/origin/$branchName"
            
            git show-ref --verify $refPath 2>$null | Out-Null
            if ($LASTEXITCODE -ne 0) {
                Write-ColorOutput "Found potentially corrupted ref: $($_.FullName)" "Yellow"
                Remove-Item $_.FullName -Force
                Write-ColorOutput "Removed: $($_.FullName)" "Green"
            }
        }
    }
    Write-Host ""
}

function Invoke-PruneRemoteRefs {
    Write-ColorOutput "Pruning remote references..." "Yellow"
    git remote prune origin
    Write-ColorOutput "Remote references pruned" "Green"
    Write-Host ""
}

function Invoke-FetchAndUpdate {
    Write-ColorOutput "Fetching from remote repository..." "Yellow"
    
    git fetch --prune origin 2>&1 | Out-Null
    if ($LASTEXITCODE -eq 0) {
        Write-ColorOutput "Fetch completed successfully" "Green"
    }
    else {
        Write-ColorOutput "Fetch failed, trying alternative method..." "Red"
        
        # Remove all remote refs and fetch fresh
        $remoteRefsPath = ".git\refs\remotes\origin"
        if (Test-Path $remoteRefsPath) {
            Write-ColorOutput "Removing all remote refs..." "Yellow"
            Remove-Item $remoteRefsPath -Recurse -Force
            New-Item -ItemType Directory -Path $remoteRefsPath -Force | Out-Null
        }
        
        Write-ColorOutput "Fetching again..." "Yellow"
        git fetch origin
        Write-ColorOutput "Fetch completed" "Green"
    }
    Write-Host ""
}

function Update-CurrentBranch {
    Write-ColorOutput "Updating current branch..." "Yellow"
    
    $currentBranch = git branch --show-current
    
    if ($currentBranch) {
        Write-Host "Current branch: $currentBranch"
        
        git show-ref --verify "refs/remotes/origin/$currentBranch" 2>$null | Out-Null
        if ($LASTEXITCODE -eq 0) {
            Write-ColorOutput "Pulling latest changes..." "Yellow"
            git pull origin $currentBranch
            Write-ColorOutput "Branch updated successfully" "Green"
        }
        else {
            Write-ColorOutput "No remote tracking branch found for $currentBranch" "Yellow"
        }
    }
    else {
        Write-ColorOutput "Not on any branch (detached HEAD state)" "Yellow"
    }
    Write-Host ""
}

# Main execution
Write-Host "==================================="
Write-Host "Git Ref Locking Issue Fixer"
Write-Host "==================================="
Write-Host ""

# Check if we're in a git repository
if (-not (Test-GitRepository)) {
    Write-ColorOutput "Error: Not in a git repository" "Red"
    exit 1
}

Write-ColorOutput "This script will attempt to fix Git ref locking issues." "Yellow"
Write-Host "Common symptoms:"
Write-Host "  - Error: cannot lock ref 'refs/remotes/origin/BRANCH_NAME'"
Write-Host "  - unable to update local ref"
Write-Host ""

# Ask for confirmation
$confirmation = Read-Host "Do you want to proceed? (y/n)"
if ($confirmation -ne 'y' -and $confirmation -ne 'Y') {
    Write-Host "Operation cancelled."
    exit 0
}

Write-Host ""
Write-Host "Starting Git ref repair process..."
Write-Host ""

try {
    # Execute repair steps
    Backup-GitRefs
    Remove-PackedRefs
    Remove-CorruptedRefs
    Invoke-PruneRemoteRefs
    Invoke-FetchAndUpdate
    Update-CurrentBranch
    
    Write-ColorOutput "===================================" "Green"
    Write-ColorOutput "Git ref repair completed!" "Green"
    Write-ColorOutput "===================================" "Green"
    Write-Host ""
    Write-Host "If you still experience issues, you may need to:"
    Write-Host "  1. Check your network connection to the remote repository"
    Write-Host "  2. Verify you have the correct permissions"
    Write-Host "  3. Consider cloning the repository fresh in a new directory"
    Write-Host ""
}
catch {
    Write-ColorOutput "An error occurred during the repair process:" "Red"
    Write-ColorOutput $_.Exception.Message "Red"
    Write-Host ""
    Write-Host "Please check the error message and try manual fixes from TROUBLESHOOTING_GIT.md"
    exit 1
}
