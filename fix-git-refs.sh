#!/bin/bash

# Git Ref Locking Issue - Automated Fix Script
# This script helps resolve Git ref locking errors automatically

set -e  # Exit on any error

# Colors for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Function to print colored messages
print_info() {
    echo -e "${GREEN}[INFO]${NC} $1"
}

print_warning() {
    echo -e "${YELLOW}[WARNING]${NC} $1"
}

print_error() {
    echo -e "${RED}[ERROR]${NC} $1"
}

# Function to check if we're in a git repository
check_git_repo() {
    if ! git rev-parse --is-inside-work-tree > /dev/null 2>&1; then
        print_error "Not a git repository. Please run this script from within a git repository."
        exit 1
    fi
    print_info "Git repository detected."
}

# Function to backup current state
backup_state() {
    print_info "Creating backup of current state..."
    
    # Check if there are any uncommitted changes
    if ! git diff-index --quiet HEAD -- 2>/dev/null; then
        print_warning "Uncommitted changes detected. Stashing them for safety..."
        git stash save "fix-git-refs-backup-$(date +%Y%m%d-%H%M%S)" || true
        echo "1" > /tmp/git-refs-stash-created
    else
        print_info "No uncommitted changes to backup."
        echo "0" > /tmp/git-refs-stash-created
    fi
}

# Function to restore state if needed
restore_state() {
    if [ -f /tmp/git-refs-stash-created ]; then
        if [ "$(cat /tmp/git-refs-stash-created)" = "1" ]; then
            print_info "Restoring stashed changes..."
            git stash pop || print_warning "Could not restore stashed changes. Use 'git stash list' to see them."
        fi
        rm -f /tmp/git-refs-stash-created
    fi
}

# Function to identify problematic refs
identify_problematic_refs() {
    print_info "Attempting to fetch to identify problematic refs..."
    
    # Capture fetch output to identify problematic refs
    local fetch_output=$(git fetch origin 2>&1 || true)
    
    if echo "$fetch_output" | grep -q "cannot lock ref"; then
        print_warning "Found ref locking issues:"
        echo "$fetch_output" | grep "cannot lock ref" | while read -r line; do
            echo "  - $line"
        done
        return 0
    else
        print_info "No obvious ref locking issues detected in fetch output."
        return 1
    fi
}

# Function to fix specific ref
fix_specific_ref() {
    local ref=$1
    print_info "Attempting to fix ref: $ref"
    
    # Try to update-ref first (safer)
    if git update-ref -d "$ref" 2>/dev/null; then
        print_info "Successfully removed ref using update-ref: $ref"
        return 0
    fi
    
    # If update-ref fails, try manual deletion
    local ref_file=".git/$ref"
    if [ -f "$ref_file" ]; then
        print_warning "Manually removing ref file: $ref_file"
        rm -f "$ref_file"
        return 0
    fi
    
    print_warning "Could not find or remove ref: $ref"
    return 1
}

# Function to perform cleanup
cleanup_refs() {
    print_info "Starting ref cleanup process..."
    
    # Method 1: Try pruning first
    print_info "Method 1: Pruning stale refs..."
    if git fetch --prune origin 2>&1; then
        print_info "Prune completed successfully."
        return 0
    fi
    
    # Method 2: Try to identify and fix specific refs
    print_info "Method 2: Identifying and fixing specific problematic refs..."
    local fetch_errors=$(git fetch origin 2>&1 || true)
    
    # Extract problematic ref names
    echo "$fetch_errors" | grep "cannot lock ref" | grep -oP "refs/remotes/origin/[^':]+" | while read -r ref; do
        fix_specific_ref "$ref"
    done
    
    # Try fetching again
    print_info "Attempting fetch after fixing specific refs..."
    if git fetch origin 2>&1; then
        print_info "Fetch successful after fixing specific refs."
        return 0
    fi
    
    # Method 3: Nuclear option - remove all remote refs
    print_warning "Method 3: Removing all remote tracking refs (this is safe)..."
    if [ -d .git/refs/remotes/origin ]; then
        rm -rf .git/refs/remotes/origin/
        print_info "Removed all remote tracking refs."
    fi
    
    # Recreate refs
    print_info "Recreating refs by fetching..."
    if git fetch origin 2>&1; then
        print_info "Successfully recreated refs."
        return 0
    else
        print_error "Failed to recreate refs. Manual intervention may be required."
        return 1
    fi
}

# Function to verify fix
verify_fix() {
    print_info "Verifying the fix..."
    
    # Try to fetch
    if git fetch origin > /dev/null 2>&1; then
        print_info "✓ Fetch successful"
    else
        print_warning "✗ Fetch still has issues"
        return 1
    fi
    
    # Check repository health
    print_info "Checking repository health..."
    if git fsck --no-progress > /dev/null 2>&1; then
        print_info "✓ Repository health check passed"
    else
        print_warning "✗ Repository health check found issues"
    fi
    
    # Show current status
    print_info "Current repository status:"
    git status
    
    return 0
}

# Main execution
main() {
    echo ""
    echo "========================================="
    echo "Git Ref Locking Issue - Automated Fix"
    echo "========================================="
    echo ""
    
    # Check if we're in a git repo
    check_git_repo
    
    # Create backup
    backup_state
    
    # Attempt to identify issues
    identify_problematic_refs
    
    # Perform cleanup
    if cleanup_refs; then
        print_info "Cleanup completed successfully!"
    else
        print_error "Cleanup encountered issues."
    fi
    
    # Verify the fix
    if verify_fix; then
        print_info "✓ All checks passed!"
    else
        print_warning "Some issues remain. Please review the output above."
    fi
    
    # Restore state if needed
    restore_state
    
    echo ""
    echo "========================================="
    echo "Fix process completed!"
    echo "========================================="
    echo ""
    echo "If issues persist, please refer to GIT_REF_LOCKING_ISSUE.md"
    echo "for additional troubleshooting steps."
    echo ""
}

# Run main function
main "$@"
