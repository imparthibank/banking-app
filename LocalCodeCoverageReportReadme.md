## 🧪 Automate Local Code Coverage Report Generation

A single-click `.bat` file can automate the entire coverage pipeline:

### ✅ How It Works

`generate_coverage.bat` will:
1. Run tests with Coverlet and collect coverage
2. Use ReportGenerator to generate HTML reports
3. Display coverage % in terminal
4. Enforce **minimum 70% threshold**
5. Auto-open HTML dashboard in browser

### 🛠️ Prerequisites

```bash
dotnet tool install --global dotnet-reportgenerator-globaltool
dotnet add BankingApp.Tests package coverlet.collector
```

---

### ▶️ Run Locally

Simply run:

```bash
generate_coverage.bat
```

---

### 🔍 Example Output

```bash
📊 Code Coverage Summary:
Line coverage: 78.5%
✅ Coverage report opened. Code quality check passed.
```

---

### 🚦 Failure Scenario (if < 70%)

```bash
❌ Coverage is below 70%. Please improve your tests!
```

---

### 📁 Output

- `CoverageReport/index.html` → Full drill-down
- `CoverageReport/Summary.txt` → CLI view

---

## ✅ Good Practices

- Enforce this check before Git push
- Integrate into GitHub Actions later
- Target 80%+ for critical services

---

Happy Coding! 🔐📈