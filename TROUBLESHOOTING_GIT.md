# Git Troubleshooting Guide

## Common Git Issues and Solutions

### Error: Cannot Lock Ref

#### Symptoms
```
Error: cannot lock ref 'refs/remotes/origin/BRANCH_NAME': is at COMMIT_HASH but expected ANOTHER_COMMIT_HASH
From https://...
 ! commit1..commit2  BRANCH_NAME -> origin/BRANCH_NAME  (unable to update local ref)
```

This error occurs when Git cannot update a remote reference because the local ref file contains a different commit hash than what Git expects during a fetch or pull operation.

#### Common Causes

1. **Interrupted Fetch/Pull Operation**: A previous git operation was interrupted (network issue, user cancellation, system crash)
2. **Corrupted Ref Files**: The ref files in `.git/refs/remotes/origin/` have become corrupted or out of sync
3. **Packed Refs Issues**: The `.git/packed-refs` file contains conflicting information
4. **Concurrent Git Operations**: Multiple Git operations were attempted simultaneously
5. **File System Issues**: Disk errors or file system corruption

#### Solution 1: Automated Fix Script (Recommended)

Use the provided fix script to automatically resolve the issue:

**For Linux/Mac (Bash):**
```bash
./fix-git-refs.sh
```

**For Windows (PowerShell):**
```powershell
.\fix-git-refs.ps1
```

The script will:
- Create a backup of your `.git` directory
- Remove corrupted ref files
- Prune remote references
- Re-fetch from the remote repository
- Update your current branch

#### Solution 2: Manual Fix

If you prefer to fix the issue manually, follow these steps:

##### Step 1: Backup Your Repository
```bash
# Create a backup of the .git directory
cp -r .git .git_backup_$(date +%Y%m%d_%H%M%S)
```

##### Step 2: Remove the Problematic Ref File
```bash
# Remove the specific branch ref that's causing the issue
# Replace BRANCH_NAME with the actual branch name from the error message
rm -f .git/refs/remotes/origin/BRANCH_NAME
```

##### Step 3: Clean Packed Refs (if needed)
```bash
# Remove the packed-refs file (Git will recreate it)
rm -f .git/packed-refs
```

##### Step 4: Prune Remote References
```bash
# Clean up stale remote-tracking references
git remote prune origin
```

##### Step 5: Fetch Fresh References
```bash
# Fetch all references from the remote
git fetch --all --prune
```

##### Step 6: Update Your Branch
```bash
# If you're working on a branch, pull the latest changes
git pull origin YOUR_BRANCH_NAME
```

#### Solution 3: Nuclear Option (Last Resort)

If the above solutions don't work, you can remove all remote refs and fetch fresh:

```bash
# Backup first!
cp -r .git .git_backup_$(date +%Y%m%d_%H%M%S)

# Remove all remote references
rm -rf .git/refs/remotes/origin

# Recreate the directory
mkdir -p .git/refs/remotes/origin

# Fetch everything fresh
git fetch origin

# Reset your branch to track the remote
git branch --set-upstream-to=origin/YOUR_BRANCH_NAME YOUR_BRANCH_NAME
```

#### Prevention Tips

1. **Avoid Interrupting Git Operations**: Let fetch/pull operations complete
2. **Check Network Stability**: Ensure stable connection before large operations
3. **Use Git Hooks**: Set up pre-fetch hooks to validate repository state
4. **Regular Maintenance**: Periodically run `git gc` and `git prune`
5. **One Operation at a Time**: Don't run multiple Git commands simultaneously

#### Understanding the Error

The error message shows:
- **Expected Hash**: What Git thought the reference should point to
- **Actual Hash**: What the local ref file actually contains
- **Branch Name**: The branch experiencing the issue

Git uses this information to detect inconsistencies and prevent data corruption.

### Other Common Git Issues

#### Issue: Detached HEAD State

**Symptoms**: You're not on any branch

**Solution**:
```bash
# Create a new branch from current position
git checkout -b new-branch-name

# Or, return to a specific branch
git checkout main
```

#### Issue: Merge Conflicts

**Symptoms**: Files have conflict markers after merge/pull

**Solution**:
```bash
# View conflicted files
git status

# Edit files to resolve conflicts, then:
git add CONFLICTED_FILE
git commit
```

#### Issue: Accidentally Committed to Wrong Branch

**Solution**:
```bash
# Save your changes
git log  # Note the commit hash

# Switch to correct branch
git checkout correct-branch

# Cherry-pick the commit
git cherry-pick COMMIT_HASH

# Go back and remove from wrong branch
git checkout wrong-branch
git reset --hard HEAD~1
```

## Additional Resources

- [Git Official Documentation](https://git-scm.com/doc)
- [Pro Git Book](https://git-scm.com/book/en/v2)
- [Git Troubleshooting Guide](https://git-scm.com/book/en/v2/Git-Internals-Maintenance-and-Data-Recovery)

## Getting Help

If you continue to experience issues:

1. Check the Git logs: `git reflog`
2. Verify repository integrity: `git fsck`
3. Check for disk space: `df -h`
4. Consult with your team or repository administrator
5. Consider cloning the repository fresh if data is not at risk
