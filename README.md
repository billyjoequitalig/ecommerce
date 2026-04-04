# Jrq.Ecommerce (n-tier) - ASP.NET Core MVC (.NET 9)

## Overview

This repository contains an n-tier e-commerce application implemented with ASP.NET Core MVC targeting .NET 9. The solution separates concerns across layers: Presentation (MVC web app), Application/Services, Domain, and Infrastructure (data access).

## Projects

- `Jrq.Ecommerce` - MVC web project (controllers, views, static assets).
- `Jrq.DataAccess` - EF Core context, migrations, repository implementations.
- `Jrq.Models` - Domain and DTO model classes.
- `Jrq.Utility` - Shared helpers and utilities.

## Prerequisites

- .NET 9 SDK
- SQL Server / LocalDB or SQLite for local development
- Git
- Visual Studio 2022 or VS Code (optional)

## Build and run (CLI)

1. From repository root:

```powershell
dotnet restore
```

2. Apply EF Core migrations (adjust the paths if your projects are in different folders):

```powershell
dotnet ef database update --project Jrq.DataAccess --startup-project Jrq.Ecommerce
```

3. Run the web app:

```powershell
cd Jrq.Ecommerce
dotnet run
```

Open the URL shown in the console (e.g., `https://localhost:5001`).

## Git: add README and push to `Development`

From the repository root (PowerShell):

```powershell
git checkout Development
git pull origin Development
git add README.md
git commit -m "Add project README"
git push origin Development
```

If the `Development` branch does not exist locally, create it and push:

```powershell
git checkout -b Development
git push -u origin Development
```

## Notes

- Adjust EF Core commands and database provider settings in `Jrq.DataAccess` as needed.
- Set `Jrq.Ecommerce` as startup project when running from Visual Studio.

If you want, I can also add a `CONTRIBUTING.md` and `LICENSE` file or update this README with deployment and environment variable details.
