# PESYONG

PESYONG is a desktop catering and ordering application built with **WinUI 3** and **.NET**. It supports kiosk-based customer ordering and administrative management for menu items, catering packages, promos, and orders. The solution follows a layered architecture to separate presentation, application logic, domain models, and infrastructure concerns.

> Status: Active development (branch `main`).

## Table of Contents
- [Overview](#overview)
- [Current Scope](#current-scope)
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

## Overview
PESYONG is a desktop-based catering and food ordering system. It includes a **customer kiosk interface** for browsing and placing orders, and an **admin interface** for managing system data and monitoring incoming orders.

## Current Scope
At the current stage of development:

- The **customer side** works as a **kiosk ordering interface**
- The **admin side** supports management of meals, catering packages, promos, and customer orders
- The project uses **Microsoft SQL Server Express (SQLEXPRESS)** through **Entity Framework Core**

## Features

### Customer / Kiosk
- Browse **meals**, **catering packages**, and **kakanin**
- Add items to cart
- Apply promo codes
- Proceed through checkout flow
- Track order status

### Admin
- Admin login
- Add, edit, and manage meals
- Add, edit, and manage catering packages
- Add, edit, and manage promos
- Receive and manage customer orders

### System / Architecture
- Layered structure for presentation, application logic, domain, and infrastructure
- Entity Framework Core for database access
- Microsoft SQL Server Express (SQLEXPRESS) as the database provider

## Project Structure
- `PESYONG.Presentation` — WinUI 3 desktop UI, pages, XAML views, and presentation view models
- `PESYONG.Application` / `PESYONG.ApplicationLogic` — services, repositories, converters, and application utilities
- `PESYONG.Domain` — domain entities, enums, and core business types
- `PESYONG.Infrastructure` — `AppDbContext`, database configuration, and persistence logic

> Note: Project and folder names may be case-sensitive depending on the environment. Follow the names defined in the solution and project files.

## Technology Stack
- .NET 8
- WinUI 3 / Microsoft.UI.Xaml
- C#
- Entity Framework Core
- Microsoft SQL Server Express (SQLEXPRESS)

## Prerequisites
Before running the project, make sure you have the following installed:

- Windows 10 (build 19041+) or later
- .NET 8 SDK
- Visual Studio 2022 or newer with the WinUI 3 / desktop development workload
- Microsoft SQL Server Express (SQLEXPRESS)
- Optional: EF Core CLI

Install EF Core CLI if needed:

```bash
dotnet tool install --global dotnet-ef
```

## Getting Started

# Clone
Clone the repository and switch to the active branch:
```bash
git clone https://github.com/isteapotaspy/pesyong-app-.git
cd pesyong-app-
git checkout main
```

# Configuration
- Open the solution file in Visual Studio.
- Set PESYONG.Presentation as the startup project.
- Configure the database connection string depending on your project setup.
- The connection string may be stored in appsettings.json, user-secrets, environment variables, or directly in the infrastructure configuration.

A typical SQL Server Express connection string looks like this:
```JSON
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=.\\SQLEXPRESS;Database=PESYONGDb;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

