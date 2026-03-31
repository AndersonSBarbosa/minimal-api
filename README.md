# Minimal API

A minimal API project built with .NET, providing a lightweight and efficient web API framework.

## Project Structure

```
minimal-api/
├── Api/                    # Main API project
│   ├── Dominio/           # Domain layer (DTOs, Entities, Services)
│   ├── Migrations/        # Database migrations
│   └── Program.cs         # Application entry point
├── Test/                  # Integration tests
├── UnitTestProject/       # Unit tests
└── GIT_REF_LOCKING_ISSUE.md  # Git troubleshooting guide
```

## Getting Started

### Prerequisites

- .NET 6.0 SDK or later
- A compatible IDE (Visual Studio, VS Code, Rider)

### Building the Project

```bash
cd Api
dotnet restore
dotnet build
```

### Running the Application

```bash
cd Api
dotnet run
```

### Running Tests

```bash
# Run unit tests
cd UnitTestProject
dotnet test

# Run integration tests
cd Test
dotnet test
```

## Troubleshooting

### Git Pull/Fetch Issues

If you encounter Git ref locking errors such as:
```
Error: cannot lock ref 'refs/remotes/origin/FEATURE/branch-name': 
is at <sha> but expected <sha>
```

We provide two solutions:

#### Automated Fix (Recommended)

Run the automated fix script:
```bash
./fix-git-refs.sh
```

The script will:
1. Detect problematic refs
2. Back up your current work
3. Clean up corrupted refs
4. Verify the fix
5. Restore your work

#### Manual Fix

For manual resolution or if the script doesn't work, follow the detailed guide:
- [Git Ref Locking Issue - Troubleshooting Guide](GIT_REF_LOCKING_ISSUE.md)

This guide includes:
- Root cause analysis
- Multiple resolution methods
- Prevention tips
- Verification steps

## Contributing

Contributions are welcome! Please ensure your changes:
- Follow the existing code style
- Include appropriate tests
- Update documentation as needed

## Common Issues and Solutions

### Issue: Git Ref Locking Error

**Symptoms:**
- `cannot lock ref 'refs/remotes/origin/...'` error
- Unable to pull or fetch from remote
- Git operations fail with ref conflicts

**Solution:**
See [GIT_REF_LOCKING_ISSUE.md](GIT_REF_LOCKING_ISSUE.md) or run `./fix-git-refs.sh`

### Issue: Build Failures

**Solution:**
```bash
# Clean and rebuild
dotnet clean
dotnet restore
dotnet build
```

### Issue: Test Failures

**Solution:**
```bash
# Rebuild and run tests
dotnet build --no-incremental
dotnet test --no-build
```

## Documentation

- [API Documentation](Api/README.md) - API-specific documentation
- [Git Troubleshooting](GIT_REF_LOCKING_ISSUE.md) - Git ref locking issue resolution

## License

Please check with the repository owner for license information.

## Support

If you encounter issues not covered in this documentation:
1. Check the troubleshooting guides
2. Review closed issues in the repository
3. Open a new issue with detailed information about your problem

---

**Note:** This repository includes utilities to help resolve common Git operational issues. If you experience Git ref locking problems, use the provided `fix-git-refs.sh` script or follow the manual steps in `GIT_REF_LOCKING_ISSUE.md`.
