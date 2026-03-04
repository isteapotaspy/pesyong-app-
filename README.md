
# PESYONG

PESYONG is a desktop catering and ordering application built with WinUI 3 and .NET. It provides order management, menu/package browsing, cart and checkout flows, and basic administrative utilities. The solution is organized with a clean separation between UI, application logic, domain entities, and infrastructure.

> Status: Active development (branch `main`). Target frameworks include .NET 8 and .NET 10 for parts of the solution.

## Table of Contents
- [Features](#features)
- [Project Structure](#project-structure)
- [Technology Stack](#technology-stack)
- [Prerequisites](#prerequisites)
- [Getting Started](#getting-started)
  - [Clone](#clone)
  - [Configuration](#configuration)
  - [Database](#database)
  - [Build and Run](#build-and-run)
- [Development Notes](#development-notes)
- [Troubleshooting](#troubleshooting)
- [Contributing](#contributing)
- [License](#license)

## Features
- Browse meals and catering packages
- Add items to cart and checkout
- Order history with tracking and status indicators
- Leave reviews for delivered orders
- Administrative and repository layers for data access

## Project Structure
- `PESYONG.Presentation` — WinUI 3 desktop UI projects (Pages, XAML, presentation view models).
- `PESYONG.Application` / `PESYONG.ApplicationLogic` — Application-level services, converters, repositories, and utilities.
- `PESYONG.Domain` — Domain entities, enums and types (orders, meals, users, receipts).
- `PESYONG.Infrastructure` — Database context (`AppDbContext`) and EF Core configuration.
- `PESYONG.Service` — Supporting service project (background services or hosting helpers if present).

Note: Project and folder names are case-sensitive on some systems; follow the repository layout in the solution file.

## Technology Stack
- .NET 8 (primary) — some projects may target other TFMs (e.g., .NET 10)
- WinUI 3 / Microsoft.UI.Xaml for desktop UI
- Entity Framework Core for data access
- C# 14 language features

## Prerequisites
- Windows 10 (build 19041+) or later
- .NET 8 SDK installed
- Visual Studio 2022/2023 or newer with WinUI 3 / desktop development workload OR Visual Studio Code with the C# extension
- (Optional) EF Core CLI (`dotnet tool install --global dotnet-ef`) for migrations

## Getting Started
### Clone
Clone the repository and switch to the active branch:

```bash
git clone https://github.com/isteapotaspy/pesyong-app-.git
cd pesyong-app-
git checkout main
```

### Configuration
- Open the solution in Visual Studio (`.sln`).
- Inspect `PESYONG.Infrastructure` or `appsettings.json` (if exists) for the database provider and connection string.
- Set the `PESYONG.Presentation` project as the startup project.
- If your DB provider requires secrets (connection string), configure them in user-secrets or environment variables.

### Database
If the project uses EF Core migrations:

```bash
# from solution root or Infrastructure project folder
dotnet ef database update --project PESYONG.Infrastructure --startup-project PESYONG.Presentation
```

If no migrations exist, create them or ensure the configured provider is available (SQLite/localdb/SQL Server).

### Build and Run
- From Visual Studio: build the solution and run the `PESYONG.Presentation` startup project.
- From the command line:

```bash
dotnet build
dotnet run --project PESYONG.Presentation
```


## Development Notes
- Converters (XAML value converters) are registered in `PESYONG.ApplicationLogic.Converters`. When adding converters used by XAML, ensure the XAML xmlns maps to the correct CLR namespace.
- Pages use element names and StaticResources — missing or misnamed resources can cause XAML parse exceptions at runtime. If the page crashes on navigation, check the application output for XAML parse errors and unresolved StaticResource keys.
- Keep UI logic in view models where possible; code-behind should be limited to view-specific wiring (dialogs, navigation).

## Troubleshooting
- App crashes when navigating to a page: check Visual Studio Output -> Debug for XAML parse exceptions. Common causes:
  - Missing StaticResource keys referenced by XAML
  - Duplicate converter types or mismatched namespaces
  - ElementName bindings that reference elements not present in the control template
- If the build fails due to TFM mismatch, align the project TargetFramework in the `.csproj` files.

If you encounter a navigation crash, capture the exception message and stack trace from the debugger output and open an issue or ask for help with the exact error text.

## Contributing
- Fork the repository, create a feature branch, and submit pull requests against `main` or the repository's other branches.
- Follow the existing code style and naming conventions. Keep changes focused and add unit tests when applicable.

## License
This repository does not include a license file by default. Add a `LICENSE` file if you intend to make the project open source.

## Contact
For questions about the codebase, open an issue in the repository or contact the maintainers listed in the project.

---

Generated README for developer onboarding and local development. Update any database/provider-specific instructions to match the actual infra used in `PESYONG.Infrastructure`.
