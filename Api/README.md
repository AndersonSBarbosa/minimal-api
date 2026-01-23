# minimal-api

A minimal API project built with .NET.

## Troubleshooting

### Git Pull/Fetch Issues

If you encounter Git ref locking errors such as:
```
Error: cannot lock ref 'refs/remotes/origin/FEATURE/branch-name'
```

Please refer to the [Git Ref Locking Issue Troubleshooting Guide](../GIT_REF_LOCKING_ISSUE.md) for detailed resolution steps.

**Quick Fix:**
```bash
# Run the automated fix script from the repository root
cd ..
./fix-git-refs.sh
```

For manual fixes and detailed explanations, see [GIT_REF_LOCKING_ISSUE.md](../GIT_REF_LOCKING_ISSUE.md).