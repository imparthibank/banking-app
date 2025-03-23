
# 🏦 BankingApp - Clean Hexagonal Architecture with .NET 5 Web API

A clean and modular banking web API built using **.NET 5**, **Hexagonal Architecture**, and key open-source tools like **Dapper**, **PostgreSQL**, **AutoMapper**, and **NUnit**.

---

## 📦 Project Structure

```
BankingApp.sln
│
├── BankingApp.WebApi/                  → Driving Adapter (API)
│   ├── Adapters/Input/Controllers/     → REST Controllers
│   ├── Startup.cs / Program.cs
│   └── appsettings.json
│
├── BankingApp.Application/            → Application Layer
│   ├── DTOs/                           → Input/Output Models
│   ├── UseCases/                       → Service Logic
│   ├── Validation/                     → Validation Logic
│   ├── Ports/Input/                    → Input Ports (Interfaces)
│   └── Common/                         → Result wrappers
│
├── BankingApp.Core/                   → Domain Layer
│   ├── Entities/                       → Core domain entities
│   └── Ports/Output/                   → Output Ports (Repository Interfaces)
│
├── BankingApp.Infrastructure/         → Driven Adapter (DB)
│   ├── Adapters/Output/Repositories/  → Dapper-based Repos
│   └── Config/ConnectionFactory.cs    → PostgreSQL Connection Factory
│
├── BankingApp.Tests/                  → Unit Tests
│   ├── UseCases/
│   ├── Validation/
│   ├── Adapters/Input/Controllers/
│   └── Adapters/Output/Repositories/
```

---

## 🧱 Architecture

This project follows **Hexagonal (Ports and Adapters)** architecture:

- **Core (Domain)** — contains `Entities` and `Repository interfaces`
- **Application** — contains use cases, validation, DTOs, and input port interfaces
- **Infrastructure** — database interaction using Dapper and PostgreSQL
- **Web API** — the input adapter, exposes REST endpoints
- **Tests** — unit test projects for all layers using NUnit and Moq

---

## 🔧 Tech Stack

| Layer         | Stack / Tool                          |
|---------------|----------------------------------------|
| Framework     | .NET 5 Web API                         |
| ORM           | Dapper                                 |
| Database      | PostgreSQL                             |
| Mapping       | AutoMapper                             |
| Validation    | Manual + Fluent-style (custom logic)   |
| Testing       | NUnit + Moq                            |
| Logging       | ILogger<T> with file-based logging     |
| Architecture  | Hexagonal (Clean Ports and Adapters)   |
| DI/Startup    | Microsoft.Extensions.DependencyInjection |

---

## ✅ Features

- Create, Read, Update, Delete (CRUD) for Bank Accounts
- Validation for:
  - Name, Email, Mobile, PAN, AccountNumber uniqueness
  - Date of Birth presence
- Uses `Result` / `Result<T>` pattern for uniform API responses
- AutoMapper-powered mapping between DTOs and Entities
- Unit-tested business logic and validation
- Logging integrated using `ILogger<T>`

---

## 🧪 Run Tests

```bash
dotnet test
```

---

## 🖥️ Run the Application

1. Ensure PostgreSQL is running (e.g. via Docker or local)
2. Update `appsettings.json` with correct DB credentials
3. Run the app:

```bash
dotnet run --project BankingApp.WebApi
```

Visit Swagger at: [http://localhost:5000/swagger](http://localhost:5000/swagger)

---

## 🗃️ SQL Table

```sql
CREATE TABLE bank_accounts (
    id UUID PRIMARY KEY,
    name TEXT NOT NULL,
    account_number TEXT NOT NULL UNIQUE,
    email TEXT NOT NULL UNIQUE,
    date_of_birth DATE NOT NULL,
    nominee TEXT,
    mobile_number TEXT NOT NULL UNIQUE,
    pan TEXT NOT NULL UNIQUE
);
```

---

## 🐳 Docker Support (Optional)

Want to add Docker + docker-compose for PostgreSQL and the API? Open an issue or PR!

---

## 🤝 Contributing

Feel free to fork and contribute. PRs are welcome!

---

## 📄 License

This project is licensed under the MIT License.
