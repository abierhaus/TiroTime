# TiroTime ⏱️

> **Professional time tracking system built with ASP.NET Core and Domain-Driven Design**

[![CI](https://github.com/abierhaus/TiroTime/actions/workflows/ci.yml/badge.svg)](https://github.com/abierhaus/TiroTime/actions/workflows/ci.yml)
[![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?logo=dotnet)](https://dotnet.microsoft.com/)
[![ASP.NET Core](https://img.shields.io/badge/ASP.NET%20Core-10.0-512BD4)](https://dotnet.microsoft.com/apps/aspnet)
[![Entity Framework](https://img.shields.io/badge/Entity%20Framework-10.0-512BD4)](https://docs.microsoft.com/ef/)
[![Docker](https://img.shields.io/badge/Docker-Ready-2496ED?logo=docker)](https://www.docker.com/)
[![License](https://img.shields.io/badge/License-MIT-green.svg)](LICENSE)

TiroTime is a modern, self-hosted time tracking application designed for freelancers, consultants, and small teams. Built with clean architecture principles and domain-driven design, it provides a robust foundation for tracking billable hours across multiple clients and projects.
The main motivation is not to implement yet another time tracking system, but to demonstrate what is possible in 2025 with AI tools like Claude Code and GitHub specs.

## ✨ Features

### 🎯 Core Functionality
- **⏱️ Smart Timer** - Start/stop timer for real-time tracking
- **📝 Manual Entries** - Add time entries retrospectively
- **📊 Monthly Overview** - View and manage entries by month with easy navigation
- **✏️ Inline Editing** - Quick time adjustments directly in the overview table
- **🎨 Project Colors** - Visual identification with customizable project colors
- **💰 Hourly Rates** - Track billable hours with project-specific rates
- **📈 Statistics Dashboard** - Today, week, and month summaries

### 👥 Multi-Client Management
- **Client Profiles** - Comprehensive client information with contacts
- **Project Organization** - Multiple projects per client
- **Budget Tracking** - Monitor project budgets and time spent
- **Custom Rates** - Different hourly rates per project

### 🔐 Security & Authentication
- **ASP.NET Core Identity** - Secure user authentication
- **Role-Based Access** - Admin, Manager, and User roles
- **JWT Support** - API authentication ready
- **Password Policies** - Configurable security requirements

### 🐳 Deployment
- **Docker Support** - One-command deployment with docker-compose
- **Auto-Restart** - Container configured for automatic startup
- **Database Migrations** - Automatic on application start
- **User Secrets** - Secure configuration management

## 🏗️ Architecture

TiroTime follows **Clean Architecture** and **Domain-Driven Design** principles, organized into four distinct layers:

```
TiroTime/
├── src/
│   ├── TiroTime.Domain/           # 🎯 Domain Layer
│   │   ├── Entities/               # Domain entities (TimeEntry, Project, Client)
│   │   ├── ValueObjects/           # Immutable value objects (Money, Email, Address)
│   │   ├── Identity/               # User and role entities
│   │   └── Common/                 # Base classes and interfaces
│   │
│   ├── TiroTime.Application/       # 📋 Application Layer
│   │   ├── Interfaces/             # Service contracts
│   │   ├── DTOs/                   # Data transfer objects
│   │   └── Services/               # Business logic orchestration
│   │
│   ├── TiroTime.Infrastructure/    # 🔧 Infrastructure Layer
│   │   ├── Persistence/            # EF Core DbContext and repositories
│   │   ├── Services/               # Service implementations
│   │   └── Migrations/             # Database migrations
│   │
│   └── TiroTime.Web/               # 🌐 Presentation Layer
│       ├── Pages/                  # Razor Pages
│       ├── Services/               # Web-specific services
│       └── wwwroot/                # Static assets
│
├── docs/                           # 📚 Documentation
│   └── specs/                      # GitHub Specifications
│
├── Dockerfile                      # 🐳 Container definition
├── docker-compose.yml              # 🐳 Docker orchestration
└── README.md                       # 📖 This file
```

### 🎯 Domain Layer
The heart of the application containing business logic and rules:
- **Entities**: `TimeEntry`, `Project`, `Client`, `ApplicationUser`
- **Value Objects**: `Money`, `Email`, `PhoneNumber`, `Address`, `WorkingHours`
- **Domain Events**: Support for domain event publishing
- **Business Rules**: Encapsulated within entities and value objects

### 📋 Application Layer
Orchestrates domain logic and defines use cases:
- **Service Interfaces**: `ITimeEntryService`, `IProjectService`, `IClientService`
- **DTOs**: Data contracts for cross-layer communication
- **Result Pattern**: Type-safe error handling without exceptions

### 🔧 Infrastructure Layer
Implements technical concerns:
- **Entity Framework Core**: Database access with SQL Server
- **Repositories**: Generic repository pattern
- **Unit of Work**: Transaction management
- **Service Implementations**: Concrete service classes

### 🌐 Presentation Layer
ASP.NET Core Razor Pages application:
- **Server-Side Rendering**: Razor Pages for UI
- **JavaScript Enhancement**: Inline editing, real-time timers
- **Bootstrap 5**: Responsive design
- **Minimal Dependencies**: Clean, maintainable frontend

## 📐 Specifications

Project requirements and features are documented using **[GitHub Specs](https://github.com/github/spec-kit)**. Each feature is specified in detail with:

- **Clear Requirements**: What needs to be built
- **Acceptance Criteria**: Definition of done
- **Technical Details**: Implementation notes
- **User Stories**: Use cases and workflows

> 💡 All specifications are tracked as GitHub Issues with the `spec` label in the `docs/specs/` directory.

### Specification Format
Specifications follow a structured format:
```markdown
# Feature: [Feature Name]

## Overview
Brief description of the feature

## Requirements
- Functional requirements
- Non-functional requirements

## User Stories
- As a [user type], I want to [action], so that [benefit]

## Technical Details
- Architecture considerations
- Implementation notes

## Acceptance Criteria
- [ ] Criterion 1
- [ ] Criterion 2
```

## 🚀 Getting Started

### Prerequisites
- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) (Version siehe `global.json`)
- [SQL Server](https://www.microsoft.com/sql-server) (or Docker)
- [Docker Desktop](https://www.docker.com/products/docker-desktop/) (optional)

### 🐳 Quick Start with Docker

1. **Clone the repository**
   ```bash
   git clone https://github.com/yourusername/TiroTime.git
   cd TiroTime
   ```

2. **Configure environment** (optional)
   ```bash
   cp .env.example .env
   # Edit .env with your settings
   ```

3. **Start with Docker Compose**
   ```bash
   docker compose up -d --build
   ```

4. **Access the application**
   - Open http://localhost:5000
   - Default admin: `admin@tirotime.com` / `Admin123!@#$`

### 💻 Development Setup

1. **Clone the repository**
   ```bash
   git clone https://github.com/yourusername/TiroTime.git
   cd TiroTime
   ```

2. **Set up SQL Server database**
   ```sql
   CREATE DATABASE TiroTime;
   CREATE LOGIN usrTiroTime WITH PASSWORD = 'YourPassword';
   USE TiroTime;
   CREATE USER usrTiroTime FOR LOGIN usrTiroTime;
   ALTER ROLE db_owner ADD MEMBER usrTiroTime;
   ```

3. **Configure connection string**
   ```bash
   dotnet user-secrets init --project src/TiroTime.Web
   dotnet user-secrets set "ConnectionStrings:DefaultConnection" \
     "Server=localhost;Database=TiroTime;User Id=usrTiroTime;Password=YourPassword;TrustServerCertificate=True" \
     --project src/TiroTime.Web
   ```

4. **Run the application**
   ```bash
   dotnet run --project src/TiroTime.Web
   ```

5. **Access the application**
   - Navigate to https://localhost:5001
   - Default credentials: `admin@tirotime.com` / `Admin123!@#$`

### 🔑 Optional: Seed Standard User

Configure a standard user via user secrets:
```bash
dotnet user-secrets set "SeedUsers:StandardUser:Email" "user@example.com" --project src/TiroTime.Web
dotnet user-secrets set "SeedUsers:StandardUser:FirstName" "John" --project src/TiroTime.Web
dotnet user-secrets set "SeedUsers:StandardUser:LastName" "Doe" --project src/TiroTime.Web
dotnet user-secrets set "SeedUsers:StandardUser:Password" "SecurePassword123!" --project src/TiroTime.Web
```

## 🛠️ Technology Stack

### Backend
- **Framework**: ASP.NET Core 10.0 (Razor Pages + Minimal APIs)
- **ORM**: Entity Framework Core 10.0 (DbContext-Pooling, Retry-Strategie)
- **Database**: SQL Server
- **Authentication**: ASP.NET Core Identity + JWT
- **Patterns**: DDD, Clean Architecture, Repository, Unit of Work
- **Build**: Central Package Management (`Directory.Packages.props`), gemeinsame Properties in `Directory.Build.props`, SDK-Pinning per `global.json`
- **Tests**: xUnit v3

### Frontend
- **UI Framework**: Razor Pages
- **CSS Framework**: Bootstrap 5.3
- **Icons**: Bootstrap Icons
- **JavaScript**: Vanilla JS (no framework overhead)

### DevOps
- **Containerization**: Docker
- **Orchestration**: Docker Compose
- **CI/CD**: GitHub Actions
  - Automated builds on every push
  - Unit test execution
  - Docker image validation

## 📚 Documentation

- **[Docker Deployment](README.Docker.md)** - Detailed Docker setup guide
- **[GitHub Actions](.github/workflows/README.md)** - CI/CD pipeline documentation
- **[Architecture Decision Records](docs/adr/)** - Key architectural decisions
- **[API Documentation](docs/api/)** - API endpoints (coming soon)
- **[Contributing Guide](CONTRIBUTING.md)** - How to contribute

## 🔄 Continuous Integration

TiroTime uses **GitHub Actions** for automated CI/CD:

- ✅ **Build Validation** - Builds all projects on every push
- ✅ **Automated Testing** - Runs all unit tests automatically
- ✅ **Docker Verification** - Validates Docker image builds
- ✅ **Pull Request Checks** - Ensures PR quality before merge

View the workflow status in the [Actions tab](https://github.com/abierhaus/TiroTime/actions).

## 🤝 Contributing

Contributions are welcome! Please read our [Contributing Guide](CONTRIBUTING.md) for details on our code of conduct and the process for submitting pull requests.

### Development Workflow
1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add some amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## 📝 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🙏 Acknowledgments

- Built with ❤️ using ASP.NET Core
- Inspired by modern time tracking needs
- Designed with Domain-Driven Design principles
- Icons by [Bootstrap Icons](https://icons.getbootstrap.com/)

## 📬 Contact

- **Project Link**: [https://github.com/abierhaus//TiroTime](https://github.com/abierhaus/TiroTime)
- **Issues**: [https://github.com/abierhaus/TiroTime/issues](https://github.com/abierhaus/TiroTime/issues)

---

<p align="center">Made with ⏱️ and .NET</p>
