# Phase 0: Project Setup Tasks
**Phase**: 0 - Project Setup
**Duration**: Week 1
**Status**: Pending

## Overview
Set up the complete solution structure with Domain-Driven Design architecture using .NET Class Libraries, configure Entity Framework Core with SQL Server, and establish the foundation for the TiroTime application.

---

## Task Checklist

### 1. Solution and Project Structure
**Priority**: Critical | **Estimated Time**: 2 hours

- [ ] Create blank solution `TiroTime.sln`
- [ ] Create `TiroTime.Domain` class library (.NET 8)
- [ ] Create `TiroTime.Application` class library (.NET 8)
- [ ] Create `TiroTime.Infrastructure` class library (.NET 8)
- [ ] Create `TiroTime.Web` Razor Pages project (.NET 8)
- [ ] Create `TiroTime.Domain.Tests` xUnit project
- [ ] Create `TiroTime.Application.Tests` xUnit project
- [ ] Create `TiroTime.Infrastructure.Tests` xUnit project
- [ ] Create `TiroTime.Web.Tests` xUnit project
- [ ] Set up project references (Infrastructure → Application → Domain, Web → Application)

**Commands**:
```bash
# Create solution
dotnet new sln -n TiroTime

# Create projects
dotnet new classlib -n TiroTime.Domain -f net8.0
dotnet new classlib -n TiroTime.Application -f net8.0
dotnet new classlib -n TiroTime.Infrastructure -f net8.0
dotnet new webapp -n TiroTime.Web -f net8.0

# Create test projects
dotnet new xunit -n TiroTime.Domain.Tests -f net8.0
dotnet new xunit -n TiroTime.Application.Tests -f net8.0
dotnet new xunit -n TiroTime.Infrastructure.Tests -f net8.0
dotnet new xunit -n TiroTime.Web.Tests -f net8.0

# Add projects to solution
dotnet sln add src/TiroTime.Domain/TiroTime.Domain.csproj
dotnet sln add src/TiroTime.Application/TiroTime.Application.csproj
dotnet sln add src/TiroTime.Infrastructure/TiroTime.Infrastructure.csproj
dotnet sln add src/TiroTime.Web/TiroTime.Web.csproj
dotnet sln add tests/TiroTime.Domain.Tests/TiroTime.Domain.Tests.csproj
dotnet sln add tests/TiroTime.Application.Tests/TiroTime.Application.Tests.csproj
dotnet sln add tests/TiroTime.Infrastructure.Tests/TiroTime.Infrastructure.Tests.csproj
dotnet sln add tests/TiroTime.Web.Tests/TiroTime.Web.Tests.csproj

# Add project references
dotnet add src/TiroTime.Application reference src/TiroTime.Domain
dotnet add src/TiroTime.Infrastructure reference src/TiroTime.Application
dotnet add src/TiroTime.Web reference src/TiroTime.Application
dotnet add src/TiroTime.Web reference src/TiroTime.Infrastructure
```

**Acceptance Criteria**:
- Solution builds without errors
- All project references are correct
- Folder structure matches DDD architecture

---

### 2. Install NuGet Packages
**Priority**: Critical | **Estimated Time**: 1 hour

#### TiroTime.Domain
- [ ] No external dependencies (pure domain logic)

#### TiroTime.Application
- [ ] Install `FluentValidation` (11.9.x)
- [ ] Install `Microsoft.Extensions.DependencyInjection.Abstractions` (8.0.x)

#### TiroTime.Infrastructure
- [ ] Install `Microsoft.EntityFrameworkCore` (8.0.x)
- [ ] Install `Microsoft.EntityFrameworkCore.SqlServer` (8.0.x)
- [ ] Install `Microsoft.EntityFrameworkCore.Tools` (8.0.x)
- [ ] Install `Microsoft.AspNetCore.Identity.EntityFrameworkCore` (8.0.x)
- [ ] Install `System.IdentityModel.Tokens.Jwt` (7.0.x)
- [ ] Install `Microsoft.AspNetCore.Authentication.JwtBearer` (8.0.x)

#### TiroTime.Web
- [ ] Install `Microsoft.EntityFrameworkCore.Design` (8.0.x)
- [ ] Install `Microsoft.AspNetCore.Identity.UI` (8.0.x)

#### Test Projects
- [ ] Install `FluentAssertions` (6.12.x) in all test projects
- [ ] Install `NSubstitute` (5.1.x) in Application.Tests
- [ ] Install `Microsoft.EntityFrameworkCore.InMemory` (8.0.x) in Infrastructure.Tests

**Commands**:
```bash
# Application
dotnet add src/TiroTime.Application package FluentValidation
dotnet add src/TiroTime.Application package Microsoft.Extensions.DependencyInjection.Abstractions

# Infrastructure
dotnet add src/TiroTime.Infrastructure package Microsoft.EntityFrameworkCore
dotnet add src/TiroTime.Infrastructure package Microsoft.EntityFrameworkCore.SqlServer
dotnet add src/TiroTime.Infrastructure package Microsoft.EntityFrameworkCore.Tools
dotnet add src/TiroTime.Infrastructure package Microsoft.AspNetCore.Identity.EntityFrameworkCore
dotnet add src/TiroTime.Infrastructure package System.IdentityModel.Tokens.Jwt
dotnet add src/TiroTime.Infrastructure package Microsoft.AspNetCore.Authentication.JwtBearer

# Web
dotnet add src/TiroTime.Web package Microsoft.EntityFrameworkCore.Design
dotnet add src/TiroTime.Web package Microsoft.AspNetCore.Identity.UI

# Tests
dotnet add tests/TiroTime.Domain.Tests package FluentAssertions
dotnet add tests/TiroTime.Application.Tests package FluentAssertions
dotnet add tests/TiroTime.Application.Tests package NSubstitute
dotnet add tests/TiroTime.Infrastructure.Tests package FluentAssertions
dotnet add tests/TiroTime.Infrastructure.Tests package Microsoft.EntityFrameworkCore.InMemory
```

**Acceptance Criteria**:
- All packages restore successfully
- No package version conflicts
- Solution builds without errors

---

### 3. Domain Layer - Base Classes
**Priority**: Critical | **Estimated Time**: 2 hours

- [ ] Create `Common/Entity.cs` (base entity class)
- [ ] Create `Common/AggregateRoot.cs` (aggregate root base)
- [ ] Create `Common/ValueObject.cs` (value object base)
- [ ] Create `Common/IDomainEvent.cs` (domain event interface)
- [ ] Create `Exceptions/DomainException.cs` (domain exception)

**Files to Create**:

`TiroTime.Domain/Common/Entity.cs`:
```csharp
namespace TiroTime.Domain.Common;

public abstract class Entity
{
    public Guid Id { get; protected set; }

    protected Entity()
    {
        Id = Guid.NewGuid();
    }

    protected Entity(Guid id)
    {
        Id = id;
    }

    public override bool Equals(object? obj)
    {
        if (obj is not Entity other)
            return false;

        if (ReferenceEquals(this, other))
            return true;

        if (GetType() != other.GetType())
            return false;

        return Id == other.Id;
    }

    public override int GetHashCode()
    {
        return Id.GetHashCode();
    }
}
```

`TiroTime.Domain/Common/AggregateRoot.cs`:
```csharp
namespace TiroTime.Domain.Common;

public abstract class AggregateRoot : Entity
{
    private readonly List<IDomainEvent> _domainEvents = new();

    public IReadOnlyCollection<IDomainEvent> DomainEvents => _domainEvents.AsReadOnly();

    protected void AddDomainEvent(IDomainEvent domainEvent)
    {
        _domainEvents.Add(domainEvent);
    }

    public void ClearDomainEvents()
    {
        _domainEvents.Clear();
    }
}
```

**Acceptance Criteria**:
- Base classes compile without errors
- Proper encapsulation and OOP principles applied
- Entity equality based on Id

---

### 4. Domain Layer - Identity Entities
**Priority**: Critical | **Estimated Time**: 3 hours

- [ ] Create `Identity/ApplicationUser.cs` (extends IdentityUser<Guid>)
- [ ] Create `Identity/ApplicationRole.cs` (extends IdentityRole<Guid>)
- [ ] Create `Identity/RefreshToken.cs`
- [ ] Create `Identity/ValueObjects/WorkingHours.cs`
- [ ] Create `Identity/Enums/UserStatus.cs`

**Acceptance Criteria**:
- ApplicationUser has custom properties (FirstName, LastName, etc.)
- WorkingHours value object has validation
- RefreshToken entity tracks expiration and revocation

---

### 5. Application Layer - Base Interfaces
**Priority**: Critical | **Estimated Time**: 2 hours

- [ ] Create `Common/Interfaces/IRepository.cs` (generic repository)
- [ ] Create `Common/Interfaces/IUnitOfWork.cs`
- [ ] Create `Common/Models/Result.cs` (result pattern)
- [ ] Create `Common/Models/PagedResult.cs`
- [ ] Create `Common/Exceptions/ApplicationException.cs`

**Files to Create**:

`TiroTime.Application/Common/Interfaces/IRepository.cs`:
```csharp
namespace TiroTime.Application.Common.Interfaces;

public interface IRepository<T> where T : class
{
    Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
    Task<List<T>> GetAllAsync(CancellationToken cancellationToken = default);
    Task AddAsync(T entity, CancellationToken cancellationToken = default);
    Task UpdateAsync(T entity, CancellationToken cancellationToken = default);
    Task DeleteAsync(T entity, CancellationToken cancellationToken = default);
}
```

`TiroTime.Application/Common/Models/Result.cs`:
```csharp
namespace TiroTime.Application.Common.Models;

public class Result
{
    public bool IsSuccess { get; }
    public string Error { get; }

    protected Result(bool isSuccess, string error)
    {
        IsSuccess = isSuccess;
        Error = error;
    }

    public static Result Success() => new(true, string.Empty);
    public static Result Failure(string error) => new(false, error);
}

public class Result<T> : Result
{
    public T? Value { get; }

    private Result(bool isSuccess, T? value, string error) : base(isSuccess, error)
    {
        Value = value;
    }

    public static Result<T> Success(T value) => new(true, value, string.Empty);
    public static new Result<T> Failure(string error) => new(false, default, error);
}
```

**Acceptance Criteria**:
- Interfaces follow repository pattern
- Result pattern implemented correctly
- No external dependencies

---

### 6. Infrastructure Layer - DbContext Setup
**Priority**: Critical | **Estimated Time**: 3 hours

- [ ] Create `Persistence/TiroTimeDbContext.cs` (extends IdentityDbContext)
- [ ] Create `Persistence/Configurations/ApplicationUserConfiguration.cs`
- [ ] Create `Persistence/Configurations/RefreshTokenConfiguration.cs`
- [ ] Create `Persistence/UnitOfWork.cs`
- [ ] Configure connection string in `appsettings.json`
- [ ] Create initial migration

**Files to Create**:

`TiroTime.Infrastructure/Persistence/TiroTimeDbContext.cs`:
```csharp
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TiroTime.Domain.Identity;

namespace TiroTime.Infrastructure.Persistence;

public class TiroTimeDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>
{
    public TiroTimeDbContext(DbContextOptions<TiroTimeDbContext> options)
        : base(options)
    {
    }

    public DbSet<RefreshToken> RefreshTokens { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Apply configurations
        builder.ApplyConfigurationsFromAssembly(typeof(TiroTimeDbContext).Assembly);
    }
}
```

**Commands**:
```bash
# Create initial migration
dotnet ef migrations add InitialCreate --project src/TiroTime.Infrastructure --startup-project src/TiroTime.Web

# Update database
dotnet ef database update --project src/TiroTime.Infrastructure --startup-project src/TiroTime.Web
```

**Acceptance Criteria**:
- DbContext compiles without errors
- Initial migration created successfully
- Database created in SQL Server
- Identity tables present (AspNetUsers, AspNetRoles, etc.)

---

### 7. Infrastructure Layer - Identity Configuration
**Priority**: Critical | **Estimated Time**: 2 hours

- [ ] Create `Identity/IdentityConfiguration.cs` (extension method)
- [ ] Configure password requirements
- [ ] Configure lockout settings
- [ ] Configure token providers

**Acceptance Criteria**:
- Identity configured with proper security settings
- Password requires 12+ chars with complexity
- Account lockout after 5 failed attempts (15 min)
- Email confirmation required

---

### 8. Web Layer - Configuration
**Priority**: Critical | **Estimated Time**: 3 hours

- [ ] Configure `Program.cs` with all services
- [ ] Create `appsettings.json` with connection string and JWT settings
- [ ] Create `appsettings.Development.json`
- [ ] Add necessary `using` statements
- [ ] Configure authentication and authorization
- [ ] Set up dependency injection for all services

**Files to Create**:

`TiroTime.Web/appsettings.json`:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=TiroTimeDb;Trusted_Connection=true;MultipleActiveResultSets=true"
  },
  "Jwt": {
    "SecretKey": "YourSuperSecretKeyHereMinimum32CharactersLong!",
    "Issuer": "TiroTime",
    "Audience": "TiroTimeUsers",
    "AccessTokenExpirationMinutes": 15,
    "RefreshTokenExpirationDays": 7
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "Microsoft.EntityFrameworkCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

**Acceptance Criteria**:
- Application starts without errors
- Database connection works
- Identity services registered
- Logging configured

---

### 9. Web Layer - Layout and Base Pages
**Priority**: High | **Estimated Time**: 3 hours

- [ ] Create `Pages/Shared/_Layout.cshtml` with Bootstrap 5.3
- [ ] Create `Pages/Shared/_ValidationScriptsPartial.cshtml`
- [ ] Create `Pages/_ViewImports.cshtml`
- [ ] Create `Pages/_ViewStart.cshtml`
- [ ] Create `Pages/Index.cshtml` (home page)
- [ ] Create `Pages/Error.cshtml`
- [ ] Add Bootstrap 5.3 CDN or local files
- [ ] Add Bootstrap Icons

**Acceptance Criteria**:
- Layout uses blue color scheme
- Navigation menu present
- Bootstrap 5.3 loaded
- Responsive design works

---

### 10. Web Layer - Custom CSS (Blue Theme)
**Priority**: High | **Estimated Time**: 2 hours

- [ ] Create `wwwroot/css/site.css` with blue theme variables
- [ ] Define color palette (primary blues, grays, semantic colors)
- [ ] Style navbar with primary blue
- [ ] Style buttons and forms
- [ ] Add custom utility classes

**Files to Create**:

`TiroTime.Web/wwwroot/css/site.css`:
```css
:root {
  /* Primary Blue Scale */
  --primary-900: #0d2d4d;
  --primary-800: #0f3a5f;
  --primary-700: #124872;
  --primary-600: #1565a8;
  --primary-500: #1976d2;
  --primary-400: #42a5f5;
  --primary-300: #64b5f6;
  --primary-200: #90caf9;
  --primary-100: #bbdefb;
  --primary-50:  #e3f2fd;

  /* ... other colors ... */
}

/* Custom Bootstrap overrides */
.navbar-primary {
  background-color: var(--primary-600) !important;
}

.btn-primary {
  background-color: var(--primary-500);
  border-color: var(--primary-500);
}

.btn-primary:hover {
  background-color: var(--primary-600);
  border-color: var(--primary-600);
}

/* ... other styles ... */
```

**Acceptance Criteria**:
- Blue theme applied consistently
- Buttons use blue colors
- Navbar is blue
- Good contrast for accessibility

---

### 11. Database Seeding
**Priority**: High | **Estimated Time**: 2 hours

- [ ] Create `Infrastructure/Persistence/DataSeeder.cs`
- [ ] Seed default roles (Administrator, Manager, Employee)
- [ ] Seed default admin user
- [ ] Call seeder in `Program.cs` on startup

**Acceptance Criteria**:
- Three roles created automatically
- Admin user created with credentials
- Seeding runs on application start
- Idempotent (can run multiple times)

---

### 12. Testing Setup
**Priority**: Medium | **Estimated Time**: 2 hours

- [ ] Create sample unit test for Entity base class
- [ ] Create sample unit test for Result pattern
- [ ] Create sample integration test for DbContext
- [ ] Configure test database (in-memory)
- [ ] Verify all tests pass

**Acceptance Criteria**:
- Test projects build and run
- Sample tests pass
- Test coverage reports generated

---

### 13. Documentation
**Priority**: Low | **Estimated Time**: 1 hour

- [ ] Create `README.md` in solution root
- [ ] Document how to run the application
- [ ] Document database setup
- [ ] Document default admin credentials
- [ ] Create `.gitignore` file

**Acceptance Criteria**:
- README has clear setup instructions
- New developers can follow documentation
- Important files not tracked by Git

---

### 14. Verification and Testing
**Priority**: Critical | **Estimated Time**: 2 hours

- [ ] Build entire solution without errors
- [ ] Run all migrations successfully
- [ ] Start application and access home page
- [ ] Verify database tables created
- [ ] Verify Identity tables populated
- [ ] Run all tests and verify they pass
- [ ] Check for any warnings or errors in logs

**Acceptance Criteria**:
- ✅ Solution compiles without errors or warnings
- ✅ Database created with all tables
- ✅ Application starts on https://localhost:5001
- ✅ Home page loads successfully
- ✅ All unit tests pass
- ✅ No critical errors in logs

---

## Deliverables

At the end of Phase 0, the following should be complete:

✅ **Solution Structure**:
- 4 class libraries (Domain, Application, Infrastructure, Web)
- 4 test projects
- Proper project references

✅ **Domain Layer**:
- Base classes (Entity, AggregateRoot, ValueObject)
- ApplicationUser and ApplicationRole entities
- RefreshToken entity
- Domain exceptions

✅ **Application Layer**:
- Repository interfaces
- UnitOfWork interface
- Result pattern
- Base service interfaces

✅ **Infrastructure Layer**:
- DbContext with Identity
- UnitOfWork implementation
- EF Core configurations
- Initial migration

✅ **Web Layer**:
- Program.cs configured
- Layout with blue theme
- Home page
- Bootstrap 5.3 integrated

✅ **Database**:
- SQL Server database created
- Identity tables present
- Default roles seeded
- Admin user created

✅ **Testing**:
- Test projects set up
- Sample tests passing

---

## Success Criteria

- [ ] All tasks completed
- [ ] Solution builds without errors
- [ ] Application runs successfully
- [ ] Database connection works
- [ ] Identity configured correctly
- [ ] Tests pass
- [ ] Blue theme applied
- [ ] Ready for Phase 1 development

---

## Time Estimate
**Total**: ~30 hours (1 week with 1 developer)

## Dependencies
- Visual Studio 2022 or VS Code with C# Dev Kit
- .NET 8 SDK installed
- SQL Server (LocalDB for development)
- Git for version control

## Notes
- Use LocalDB for development (easier setup)
- Production will use full SQL Server
- Keep solution clean and organized from start
- Follow DDD principles strictly
- Test early and often

---

**Phase Owner**: Development Team
**Status**: Ready to Start
**Next Phase**: Phase 1 - Authentication & Basic Infrastructure
