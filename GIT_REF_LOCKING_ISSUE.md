# Git Ref Locking Issue - Troubleshooting Guide

## Problem Description

This document addresses the Git error:
```
Error: cannot lock ref 'refs/remotes/origin/FEATURE/branch-name': 
is at <current-sha> but expected <expected-sha>
```

This error occurs when Git's local repository state becomes corrupted or out of sync with remote tracking branches.

## Root Cause

The issue typically happens when:
1. A remote branch was force-pushed or rebased
2. Git's internal refs database became corrupted
3. Concurrent Git operations caused state inconsistencies
4. Network interruptions during fetch/pull operations

## Solution Steps

### Method 1: Remove and Re-fetch the Problematic Ref (Recommended)

This is the safest and most common solution:

```bash
# 1. Identify the problematic branch from the error message
# Example: refs/remotes/origin/FEATURE/portabilidade-nova

# 2. Remove the local tracking ref
git update-ref -d refs/remotes/origin/FEATURE/portabilidade-nova

# 3. Fetch again to recreate the ref
git fetch origin

# 4. If still having issues, prune and fetch
git fetch --prune origin
```

### Method 2: Manual Ref File Cleanup

If Method 1 doesn't work:

```bash
# 1. Navigate to the git directory
cd .git/refs/remotes/origin/

# 2. Remove the problematic ref file
rm -f "FEATURE/portabilidade-nova"

# 3. Return to repository root
cd -

# 4. Fetch to recreate the ref
git fetch origin
```

### Method 3: Complete Ref Database Rebuild

For severe corruption affecting multiple refs:

```bash
# 1. Backup your local changes first
git stash save "backup before ref cleanup"

# 2. Remove all remote tracking refs
rm -rf .git/refs/remotes/origin/

# 3. Rebuild the refs
git fetch origin

# 4. Restore your changes if needed
git stash pop
```

### Method 4: Using the Utility Script

We provide a utility script for automated resolution:

```bash
# Make the script executable
chmod +x fix-git-refs.sh

# Run the script
./fix-git-refs.sh
```

## Prevention

To avoid this issue in the future:

1. **Always complete Git operations fully** - Don't interrupt fetch/pull operations
2. **Use proper Git workflow** - Avoid force-pushing to shared branches
3. **Keep Git updated** - Use the latest stable version of Git
4. **Regular maintenance** - Run `git gc` and `git fsck` periodically
5. **Avoid concurrent operations** - Don't run multiple Git commands simultaneously

## Verification

After applying the fix, verify the resolution:

```bash
# Check repository status
git status

# Verify refs are clean
git show-ref

# Try pulling again
git pull

# Check for any corruption
git fsck --full
```

## Additional Resources

- [Git Documentation - git-update-ref](https://git-scm.com/docs/git-update-ref)
- [Git Documentation - git-fetch](https://git-scm.com/docs/git-fetch)
- [Stack Overflow - Git ref lock issues](https://stackoverflow.com/questions/tagged/git+refs)

## When to Seek Additional Help

If none of these solutions work:
1. Check if you have filesystem permissions issues
2. Verify disk space availability
3. Check for filesystem corruption
4. Consider cloning a fresh copy of the repository
5. Contact your Git administrator or DevOps team

## Example Error Messages

The following error patterns indicate this issue:

```
cannot lock ref 'refs/remotes/origin/FEATURE/branch-name': is at <sha> but expected <sha>
unable to update local ref
Failed to fetch from the remote repository
```

## Quick Reference Commands

```bash
# Quick fix for single branch
git update-ref -d refs/remotes/origin/<BRANCH_NAME>
git fetch origin

# Quick fix for all branches
git fetch --prune origin

# Nuclear option - fresh start
rm -rf .git/refs/remotes/origin/
git fetch origin

# Verify repository health
git fsck --full
git gc --aggressive
```
