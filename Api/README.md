# minimal-api

A minimal API project built with ASP.NET Core.

## Getting Started

### Prerequisites
- .NET SDK (version specified in the project file)
- Git

### Installation

1. Clone the repository:
```bash
git clone <repository-url>
cd minimal-api
```

2. Restore dependencies:
```bash
dotnet restore
```

3. Build the project:
```bash
dotnet build
```

4. Run the application:
```bash
dotnet run --project Api/minimal-api.csproj
```

## Troubleshooting

### Git Issues

If you encounter Git-related errors such as "cannot lock ref" errors during fetch/pull operations, please refer to our [Git Troubleshooting Guide](../TROUBLESHOOTING_GIT.md).

Quick fix for Git ref locking issues:
```bash
# From the repository root
./fix-git-refs.sh
```

For more detailed information, see [TROUBLESHOOTING_GIT.md](../TROUBLESHOOTING_GIT.md).