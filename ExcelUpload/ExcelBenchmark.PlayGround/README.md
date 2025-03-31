
# ExcelBenchmark.PlayGround (.NET 5 Console Application)

This project benchmarks Excel file read/write performance across popular .NET libraries using [BenchmarkDotNet](https://benchmarkdotnet.org/).

## ✅ Libraries Compared

| Library        | License         | Commercial Use | Maintained | Ease of Use | Best For         |
|----------------|------------------|----------------|------------|-------------|------------------|
| **ClosedXML**  | MIT              | ✅ Yes         | ✅ Active   | ✅ Easy      | Modern Excel     |
| **NPOI**       | Apache 2.0       | ✅ Yes         | ⚠️ Moderate| ⚠️ Verbose   | Legacy Excel `.xls` + `.xlsx` |
| **EPPlus**     | Polyform NC      | ❌ No (Paid)   | ✅ Active   | ✅ Easy      | Rich Excel features |
| **OpenXML SDK**| MIT              | ✅ Yes         | ✅ Active   | ❌ Verbose   | Low-level control |

> 💡 **Recommended for most commercial use:** `ClosedXML` – open source, easy to use, and modern.

---

## 📊 Benchmark Summary

- Write 1000 rows using each Excel library
- Compare performance using BenchmarkDotNet
- Benchmark result output saved to `BenchmarkDotNet.Artifacts/` folder

---

## 🧪 How to Run Benchmarks

### 🔹 Recommended (No Debugging)
```bash
dotnet run -c Release
```
> Let BenchmarkDotNet run normally. Do **not** attach a debugger.

---

## ⚠️ Debugging Warning

You might see this popup:

> **"You are debugging a Release build of ExcelBenchmark.PlayGround.dll"**

This means:
- Breakpoints **won’t work** due to optimizations
- You **should not debug** BenchmarkDotNet benchmarks

### ✅ What to Do:

| Goal                    | Recommended Action                    |
|-------------------------|----------------------------------------|
| Accurate benchmark      | Run without debugger: `dotnet run -c Release` |
| Debug method manually   | Comment `BenchmarkRunner.Run<>()`, call method directly |
| Disable warning popup   | Choose `Continue Debugging (Don't Ask Again)` |

---

## 🛠 Manual Debug Mode (Optional)

Edit `Program.cs`:
```csharp
// For benchmark:
BenchmarkRunner.Run<ExcelBenchmark>();

// For manual debug:
var bench = new ExcelBenchmark();
bench.Setup();
bench.ClosedXml_Write(); // or another method to test
```

---

## 🔧 Setup Instructions

1. Ensure .NET 5 SDK is installed
2. Restore dependencies:
   ```bash
   dotnet restore
   ```
3. Run benchmarks:
   ```bash
   dotnet run -c Release
   ```

---

## 📦 Required Packages

```bash
dotnet add package BenchmarkDotNet --version 0.13.5
dotnet add package ClosedXML --version 0.97.0
dotnet add package EPPlus --version 5.8.0
dotnet add package NPOI --version 2.6.1
dotnet add package DocumentFormat.OpenXml --version 2.18.0
```

---

## 📚 Resources

- [BenchmarkDotNet Docs](https://benchmarkdotnet.org/articles/guides/getting-started.html)
- [ClosedXML on NuGet](https://www.nuget.org/packages/ClosedXML)
- [EPPlus on NuGet](https://www.nuget.org/packages/EPPlus)
- [NPOI on NuGet](https://www.nuget.org/packages/NPOI)
- [OpenXML SDK on NuGet](https://www.nuget.org/packages/DocumentFormat.OpenXml)
