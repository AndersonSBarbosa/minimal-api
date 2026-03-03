#!/bin/bash

# Script to fix Git ref locking issues
# This script resolves "cannot lock ref" errors that occur during git fetch/pull operations

set -e

echo "==================================="
echo "Git Ref Locking Issue Fixer"
echo "==================================="
echo ""

# Color codes for output
RED='\033[0;31m'
GREEN='\033[0;32m'
YELLOW='\033[1;33m'
NC='\033[0m' # No Color

# Check if we're in a git repository
if ! git rev-parse --git-dir > /dev/null 2>&1; then
    echo -e "${RED}Error: Not in a git repository${NC}"
    exit 1
fi

echo -e "${YELLOW}This script will attempt to fix Git ref locking issues.${NC}"
echo "Common symptoms:"
echo "  - Error: cannot lock ref 'refs/remotes/origin/BRANCH_NAME'"
echo "  - unable to update local ref"
echo ""

# Function to backup refs
backup_refs() {
    echo -e "${YELLOW}Creating backup of .git directory...${NC}"
    BACKUP_DIR=".git_backup_$(date +%Y%m%d_%H%M%S)"
    cp -r .git "$BACKUP_DIR"
    echo -e "${GREEN}Backup created at: $BACKUP_DIR${NC}"
    echo ""
}

# Function to fix packed refs
fix_packed_refs() {
    echo -e "${YELLOW}Fixing packed-refs file...${NC}"
    if [ -f .git/packed-refs ]; then
        # Remove the packed-refs file - Git will recreate it if needed
        rm -f .git/packed-refs
        echo -e "${GREEN}Removed packed-refs file${NC}"
    else
        echo "No packed-refs file found"
    fi
    echo ""
}

# Function to prune and clean remote refs
prune_remote_refs() {
    echo -e "${YELLOW}Pruning remote references...${NC}"
    git remote prune origin
    echo -e "${GREEN}Remote references pruned${NC}"
    echo ""
}

# Function to remove corrupted ref files
remove_corrupted_refs() {
    echo -e "${YELLOW}Scanning for corrupted ref files...${NC}"
    
    # Get list of all remote branches
    if [ -d .git/refs/remotes/origin ]; then
        find .git/refs/remotes/origin -type f | while read -r ref_file; do
            if ! git show-ref --verify "refs/remotes/origin/$(basename "$ref_file")" > /dev/null 2>&1; then
                echo -e "${YELLOW}Found potentially corrupted ref: $ref_file${NC}"
                rm -f "$ref_file"
                echo -e "${GREEN}Removed: $ref_file${NC}"
            fi
        done
    fi
    echo ""
}

# Function to fetch and update refs
fetch_and_update() {
    echo -e "${YELLOW}Fetching from remote repository...${NC}"
    
    # Try to fetch with prune
    if git fetch --prune origin; then
        echo -e "${GREEN}Fetch completed successfully${NC}"
    else
        echo -e "${RED}Fetch failed, trying alternative method...${NC}"
        
        # Remove all remote refs and fetch fresh
        if [ -d .git/refs/remotes/origin ]; then
            echo -e "${YELLOW}Removing all remote refs...${NC}"
            rm -rf .git/refs/remotes/origin
            mkdir -p .git/refs/remotes/origin
        fi
        
        echo -e "${YELLOW}Fetching again...${NC}"
        git fetch origin
        echo -e "${GREEN}Fetch completed${NC}"
    fi
    echo ""
}

# Function to update current branch
update_current_branch() {
    echo -e "${YELLOW}Updating current branch...${NC}"
    
    CURRENT_BRANCH=$(git branch --show-current)
    if [ -n "$CURRENT_BRANCH" ]; then
        echo "Current branch: $CURRENT_BRANCH"
        
        # Check if remote tracking branch exists
        if git show-ref --verify "refs/remotes/origin/$CURRENT_BRANCH" > /dev/null 2>&1; then
            echo -e "${YELLOW}Pulling latest changes...${NC}"
            git pull origin "$CURRENT_BRANCH"
            echo -e "${GREEN}Branch updated successfully${NC}"
        else
            echo -e "${YELLOW}No remote tracking branch found for $CURRENT_BRANCH${NC}"
        fi
    else
        echo -e "${YELLOW}Not on any branch (detached HEAD state)${NC}"
    fi
    echo ""
}

# Main execution
main() {
    echo "Starting Git ref repair process..."
    echo ""
    
    # Ask for confirmation
    read -p "Do you want to proceed? (y/n): " -n 1 -r
    echo ""
    
    if [[ ! $REPLY =~ ^[Yy]$ ]]; then
        echo "Operation cancelled."
        exit 0
    fi
    
    # Create backup
    backup_refs
    
    # Fix steps
    fix_packed_refs
    remove_corrupted_refs
    prune_remote_refs
    fetch_and_update
    update_current_branch
    
    echo -e "${GREEN}==================================="
    echo "Git ref repair completed!"
    echo "===================================${NC}"
    echo ""
    echo "If you still experience issues, you may need to:"
    echo "  1. Check your network connection to the remote repository"
    echo "  2. Verify you have the correct permissions"
    echo "  3. Consider cloning the repository fresh in a new directory"
    echo ""
}

# Run main function
main
