# .NET 6 to .NET 8 Migration Automation

This repository contains scripts and instructions to **automate the migration** of .NET 6 projects (Web API, Console, WPF, WinForms) to **.NET 8**.

---

## ✅ Prerequisites

- .NET 8 SDK installed: [Download](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)
- PowerShell (Windows) or Bash (Linux/macOS)
- Git (optional but recommended)

---

## 🔧 Scripts Overview

### `migrate-to-dotnet8.ps1` (Windows PowerShell)
- Recursively updates all `.csproj` files:
  - `net6.0` → `net8.0`
  - `net6.0-windows` → `net8.0-windows`

```powershell
# migrate-to-dotnet8.ps1
Get-ChildItem -Recurse -Filter *.csproj | ForEach-Object {
    $content = Get-Content $_.FullName
    $newContent = $content -replace '<TargetFramework>net6.0-windows', '<TargetFramework>net8.0-windows'
    $newContent = $newContent -replace '<TargetFramework>net6.0', '<TargetFramework>net8.0'
    Set-Content $_.FullName $newContent
    Write-Host "Updated:" $_.FullName
}
```

### `migrate-to-dotnet8.sh` (Linux/macOS/Git Bash)
- Same behavior as PowerShell version using `sed`

### `full-dotnet8-migrate.ps1`
- Performs full migration steps:
  - Runs `upgrade-assistant`
  - Updates target frameworks
  - Installs `dotnet-outdated-tool` & upgrades NuGet packages
  - Cleans, rebuilds, and runs tests

```powershell
# full-dotnet8-migrate.ps1
upgrade-assistant upgrade YourSolution.sln

dotnet tool install --global dotnet-outdated-tool
dotnet outdated --upgrade

Get-ChildItem -Recurse -Filter *.csproj | ForEach-Object {
    $file = $_.FullName
    (Get-Content $file) -replace '<TargetFramework>net6.0', '<TargetFramework>net8.0' |
    Set-Content $file
    Write-Host "Updated framework in $file"
}

dotnet clean
dotnet build
dotnet test
```

---

## 🚀 How to Use

### 1. Clone your repo and backup first
```bash
git clone <your-repo>
cd <your-repo>
```

### 2. Install tools
```bash
dotnet tool install -g upgrade-assistant
dotnet tool install --global dotnet-outdated-tool
```

### 3. Run the Upgrade Assistant
```bash
upgrade-assistant upgrade YourSolution.sln
```

### 4. Run the migration script

#### On Windows (PowerShell)
```powershell
.\migrate-to-dotnet8.ps1
```

#### On Linux/macOS
```bash
bash ./migrate-to-dotnet8.sh
```

### 5. Upgrade NuGet packages
```bash
dotnet outdated --upgrade
```

### 6. Clean, Build, and Test
```bash
dotnet clean
dotnet build
dotnet test
```

---

## 🧪 Notes
- Ensure all external NuGet packages support .NET 8.
- Review changes made by `upgrade-assistant`.
- Test all application functionality after migration.

---

## 📁 Structure
```
/your-repo
├── migrate-to-dotnet8.ps1
├── migrate-to-dotnet8.sh
├── full-dotnet8-migrate.ps1
├── README.md
```

---

## 🧠 Resources
- [.NET Compatibility Docs](https://learn.microsoft.com/en-us/dotnet/core/compatibility/)
- [Upgrade Assistant](https://learn.microsoft.com/en-us/dotnet/upgrade-assistant/overview)
- [.NET 8 SDK](https://dotnet.microsoft.com/en-us/download/dotnet/8.0)

---

## 📬 Contributing / Issues
Found a bug or improvement? [Open an issue](https://github.com/your-repo/issues) or submit a PR.

---

## 🏁 License
MIT
