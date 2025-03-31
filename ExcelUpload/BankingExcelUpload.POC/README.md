
# Banking Excel Upload/Download POC (.NET 5)

This project demonstrates uploading and downloading Excel files using various popular .NET libraries for managing bank account data.

## ✅ Supported Libraries

| Library        | License         | Commercial Use | Maintained | Ease of Use | Best For         |
|----------------|------------------|----------------|------------|-------------|------------------|
| **ClosedXML**  | MIT              | ✅ Yes         | ✅ Active   | ✅ Easy      | Modern Excel     |
| **NPOI**       | Apache 2.0       | ✅ Yes         | ⚠️ Moderate| ⚠️ Verbose   | Legacy Excel `.xls` + `.xlsx` |
| **EPPlus**     | Polyform NC      | ❌ No (Paid)   | ✅ Active   | ✅ Easy      | Rich Excel features |
| **OpenXML SDK**| MIT              | ✅ Yes         | ✅ Active   | ❌ Verbose   | Low-level control |

> 💡 **Recommended for most commercial use:** `ClosedXML` – open source, easy to use, and modern.

---

## 📦 Features

- Upload Excel files containing bank account data
- Download templates for each library
- Controllers built using:
  - EPPlus
  - ClosedXML
  - NPOI
  - OpenXML SDK
- Swagger UI enabled for quick testing

---

## 🔧 Setup Instructions

1. Clone the repository or extract the zip
2. Run the project in Visual Studio or via CLI:
   ```bash
   dotnet restore
   dotnet run
   ```
3. Open Swagger UI at:
   ```
   http://localhost:<port>/swagger
   ```

---

## 📄 Model Structure

```csharp
public class BankAccount
{
    public string AccountHolderName { get; set; }
    public string AccountNumber { get; set; }
    public string BankName { get; set; }
    public string IFSCCode { get; set; }
    public string Branch { get; set; }
    public DateTime OpeningDate { get; set; }
}
```

---

## 🧪 Benchmarking

Use [BenchmarkDotNet](https://benchmarkdotnet.org/) to compare read performance across libraries with a dataset of 1000 rows.

---

## 📚 Resources

- [EPPlus on NuGet](https://www.nuget.org/packages/EPPlus)
- [ClosedXML on NuGet](https://www.nuget.org/packages/ClosedXML)
- [NPOI on NuGet](https://www.nuget.org/packages/NPOI)
- [OpenXML SDK on NuGet](https://www.nuget.org/packages/DocumentFormat.OpenXml)
