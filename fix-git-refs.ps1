# Git Ref Locking Issue - Automated Fix Script (PowerShell)
# This script helps resolve Git ref locking errors automatically on Windows

# Set error action preference
$ErrorActionPreference = "Continue"

# Function to print colored messages
function Write-Info {
    param([string]$Message)
    Write-Host "[INFO] $Message" -ForegroundColor Green
}

function Write-Warning-Custom {
    param([string]$Message)
    Write-Host "[WARNING] $Message" -ForegroundColor Yellow
}

function Write-Error-Custom {
    param([string]$Message)
    Write-Host "[ERROR] $Message" -ForegroundColor Red
}

# Function to check if we're in a git repository
function Test-GitRepository {
    try {
        git rev-parse --is-inside-work-tree 2>&1 | Out-Null
        if ($LASTEXITCODE -ne 0) {
            Write-Error-Custom "Not a git repository. Please run this script from within a git repository."
            exit 1
        }
        Write-Info "Git repository detected."
        return $true
    }
    catch {
        Write-Error-Custom "Not a git repository. Please run this script from within a git repository."
        exit 1
    }
}

# Function to backup current state
function Backup-State {
    Write-Info "Creating backup of current state..."
    
    # Create a unique temporary file
    $script:StashMarker = [System.IO.Path]::GetTempFileName()
    
    # Check if there are any uncommitted changes
    git diff-index --quiet HEAD -- 2>&1 | Out-Null
    if ($LASTEXITCODE -ne 0) {
        Write-Warning-Custom "Uncommitted changes detected. Stashing them for safety..."
        $timestamp = Get-Date -Format "yyyyMMdd-HHmmss"
        git stash save "fix-git-refs-backup-$timestamp" 2>&1 | Out-Null
        Set-Content -Path $script:StashMarker -Value "1"
    }
    else {
        Write-Info "No uncommitted changes to backup."
        Set-Content -Path $script:StashMarker -Value "0"
    }
}

# Function to restore state if needed
function Restore-State {
    if ($script:StashMarker -and (Test-Path $script:StashMarker)) {
        $stashCreated = Get-Content $script:StashMarker
        if ($stashCreated -eq "1") {
            Write-Info "Restoring stashed changes..."
            git stash pop 2>&1 | Out-Null
            if ($LASTEXITCODE -ne 0) {
                Write-Warning-Custom "Could not restore stashed changes. Use 'git stash list' to see them."
            }
        }
        Remove-Item $script:StashMarker -Force -ErrorAction SilentlyContinue
    }
}

# Function to identify problematic refs
function Find-ProblematicRefs {
    Write-Info "Attempting to fetch to identify problematic refs..."
    
    # Capture fetch output to identify problematic refs
    $fetchOutput = git fetch origin 2>&1
    
    if ($fetchOutput -match "cannot lock ref") {
        Write-Warning-Custom "Found ref locking issues:"
        $fetchOutput | Select-String "cannot lock ref" | ForEach-Object {
            Write-Host "  - $_"
        }
        return $true
    }
    else {
        Write-Info "No obvious ref locking issues detected in fetch output."
        return $false
    }
}

# Function to fix specific ref
function Fix-SpecificRef {
    param([string]$Ref)
    Write-Info "Attempting to fix ref: $Ref"
    
    # Try to update-ref first (safer)
    git update-ref -d $Ref 2>&1 | Out-Null
    if ($LASTEXITCODE -eq 0) {
        Write-Info "Successfully removed ref using update-ref: $Ref"
        return $true
    }
    
    # If update-ref fails, try manual deletion
    $refFile = ".git\$($Ref -replace '/', '\')"
    if (Test-Path $refFile) {
        Write-Warning-Custom "Manually removing ref file: $refFile"
        Remove-Item $refFile -Force
        return $true
    }
    
    Write-Warning-Custom "Could not find or remove ref: $Ref"
    return $false
}

# Function to perform cleanup
function Repair-GitRefs {
    Write-Info "Starting ref cleanup process..."
    
    # Method 1: Try pruning first
    Write-Info "Method 1: Pruning stale refs..."
    git fetch --prune origin 2>&1 | Out-Null
    if ($LASTEXITCODE -eq 0) {
        Write-Info "Prune completed successfully."
        return $true
    }
    
    # Method 2: Try to identify and fix specific refs
    Write-Info "Method 2: Identifying and fixing specific problematic refs..."
    $fetchErrors = git fetch origin 2>&1
    
    # Extract problematic ref names more robustly
    # Match patterns like: cannot lock ref 'refs/remotes/origin/...' or "refs/remotes/origin/..."
    $problematicRefs = $fetchErrors | Select-String "cannot lock ref" | 
                       ForEach-Object { 
                           if ($_ -match "['\`"]refs/remotes/origin/([^'\`"]+)['\`"]") {
                               "refs/remotes/origin/$($matches[1])"
                           }
                       } | Where-Object { $_ -ne $null }
    
    foreach ($ref in $problematicRefs) {
        if ($ref -match "^refs/remotes/origin/") {
            Fix-SpecificRef $ref
        }
    }
    
    # Try fetching again
    Write-Info "Attempting fetch after fixing specific refs..."
    git fetch origin 2>&1 | Out-Null
    if ($LASTEXITCODE -eq 0) {
        Write-Info "Fetch successful after fixing specific refs."
        return $true
    }
    
    # Method 3: Nuclear option - remove all remote refs
    Write-Warning-Custom "Method 3: Removing all remote tracking refs (this is safe)..."
    $originRefsPath = ".git\refs\remotes\origin"
    if (Test-Path $originRefsPath) {
        Remove-Item $originRefsPath -Recurse -Force
        Write-Info "Removed all remote tracking refs."
    }
    
    # Recreate refs
    Write-Info "Recreating refs by fetching..."
    git fetch origin 2>&1 | Out-Null
    if ($LASTEXITCODE -eq 0) {
        Write-Info "Successfully recreated refs."
        return $true
    }
    else {
        Write-Error-Custom "Failed to recreate refs. Manual intervention may be required."
        return $false
    }
}

# Function to verify fix
function Test-Fix {
    Write-Info "Verifying the fix..."
    
    # Try to fetch
    git fetch origin 2>&1 | Out-Null
    if ($LASTEXITCODE -eq 0) {
        Write-Info "✓ Fetch successful"
    }
    else {
        Write-Warning-Custom "✗ Fetch still has issues"
        return $false
    }
    
    # Check repository health
    Write-Info "Checking repository health..."
    git fsck --no-progress 2>&1 | Out-Null
    if ($LASTEXITCODE -eq 0) {
        Write-Info "✓ Repository health check passed"
    }
    else {
        Write-Warning-Custom "✗ Repository health check found issues"
    }
    
    # Show current status
    Write-Info "Current repository status:"
    git status
    
    return $true
}

# Main execution
function Main {
    Write-Host ""
    Write-Host "=========================================" -ForegroundColor Cyan
    Write-Host "Git Ref Locking Issue - Automated Fix" -ForegroundColor Cyan
    Write-Host "=========================================" -ForegroundColor Cyan
    Write-Host ""
    
    # Check if we're in a git repo
    Test-GitRepository
    
    # Create backup
    Backup-State
    
    # Attempt to identify issues
    Find-ProblematicRefs
    
    # Perform cleanup
    $cleanupSuccess = Repair-GitRefs
    if ($cleanupSuccess) {
        Write-Info "Cleanup completed successfully!"
    }
    else {
        Write-Error-Custom "Cleanup encountered issues."
    }
    
    # Verify the fix
    $verifySuccess = Test-Fix
    if ($verifySuccess) {
        Write-Info "✓ All checks passed!"
    }
    else {
        Write-Warning-Custom "Some issues remain. Please review the output above."
    }
    
    # Restore state if needed
    Restore-State
    
    Write-Host ""
    Write-Host "=========================================" -ForegroundColor Cyan
    Write-Host "Fix process completed!" -ForegroundColor Cyan
    Write-Host "=========================================" -ForegroundColor Cyan
    Write-Host ""
    Write-Host "If issues persist, please refer to GIT_REF_LOCKING_ISSUE.md"
    Write-Host "for additional troubleshooting steps."
    Write-Host ""
}

# Run main function
Main
