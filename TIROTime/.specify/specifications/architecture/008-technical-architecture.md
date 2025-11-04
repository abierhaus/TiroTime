# Technical Architecture
**Specification ID**: 008
**Version**: 1.0
**Status**: Draft
**Parent**: [001 - TiroTime Overview](../001-tirotime-overview.md)

## Overview

This document defines the technical architecture for TiroTime, a modern .NET web application following Domain-Driven Design (DDD) principles, Clean Architecture, and CQRS patterns. The architecture ensures scalability, maintainability, testability, and adherence to the principles established in the [Constitution](../../constitution.md).

## Architectural Principles

1. **Domain-Centric Design**: Business logic resides in the domain layer
2. **Separation of Concerns**: Clear boundaries between layers
3. **Dependency Inversion**: Dependencies point inward toward domain
4. **CQRS**: Command-Query Responsibility Segregation for scalability
5. **Event-Driven**: Domain events for decoupling and eventual consistency
6. **Testability**: All components designed for easy testing
7. **Security by Default**: Security built into every layer
8. **Performance Awareness**: Caching, async, and optimization throughout

## High-Level Architecture

```
┌─────────────────────────────────────────────────────────────────┐
│                        Presentation Layer                       │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────────────┐  │
│  │  Web API     │  │  SignalR     │  │  Background Jobs     │  │
│  │  (REST)      │  │  (Real-time) │  │  (Hangfire)          │  │
│  └──────────────┘  └──────────────┘  └──────────────────────┘  │
└─────────────────────────────────────────────────────────────────┘
                            │
                            ▼
┌─────────────────────────────────────────────────────────────────┐
│                       Application Layer                         │
│  ┌──────────────────────────────────────────────────────────┐  │
│  │  Commands & Queries (MediatR)                            │  │
│  │  - Command Handlers, Query Handlers                      │  │
│  │  - DTOs, Validators (FluentValidation)                   │  │
│  └──────────────────────────────────────────────────────────┘  │
│  ┌──────────────────────────────────────────────────────────┐  │
│  │  Application Services                                     │  │
│  │  - Orchestration, Use Cases                              │  │
│  └──────────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────────┘
                            │
                            ▼
┌─────────────────────────────────────────────────────────────────┐
│                         Domain Layer                            │
│  ┌──────────────────────────────────────────────────────────┐  │
│  │  Aggregates, Entities, Value Objects                     │  │
│  │  Domain Events, Domain Services                          │  │
│  │  Business Rules and Invariants                           │  │
│  └──────────────────────────────────────────────────────────┘  │
│  ┌──────────────────────────────────────────────────────────┐  │
│  │  Repository Interfaces, Specifications                   │  │
│  └──────────────────────────────────────────────────────────┘  │
└─────────────────────────────────────────────────────────────────┘
                            │
                            ▼
┌─────────────────────────────────────────────────────────────────┐
│                     Infrastructure Layer                        │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────────────┐  │
│  │ Persistence  │  │   Caching    │  │  External Services   │  │
│  │ (EF Core)    │  │   (Redis)    │  │  (Email, Storage)    │  │
│  └──────────────┘  └──────────────┘  └──────────────────────┘  │
│  ┌──────────────┐  ┌──────────────┐  ┌──────────────────────┐  │
│  │ Identity     │  │   Logging    │  │  File Generation     │  │
│  │ (JWT)        │  │   (Serilog)  │  │  (Excel, PDF)        │  │
│  └──────────────┘  └──────────────┘  └──────────────────────┘  │
└─────────────────────────────────────────────────────────────────┘
```

## Project Structure

```
TiroTime/
├── src/
│   ├── TiroTime.Domain/                      # Domain Layer
│   │   ├── Common/
│   │   │   ├── AggregateRoot.cs
│   │   │   ├── Entity.cs
│   │   │   ├── ValueObject.cs
│   │   │   └── IDomainEvent.cs
│   │   ├── Identity/                         # Identity Context
│   │   │   ├── User.cs
│   │   │   ├── ValueObjects/
│   │   │   ├── Events/
│   │   │   └── IUserRepository.cs
│   │   ├── Clients/                          # Client Context
│   │   │   ├── Client.cs
│   │   │   ├── ValueObjects/
│   │   │   ├── Events/
│   │   │   └── IClientRepository.cs
│   │   ├── Projects/                         # Project Context
│   │   │   ├── Project.cs
│   │   │   ├── ProjectMember.cs
│   │   │   ├── ValueObjects/
│   │   │   ├── Events/
│   │   │   └── IProjectRepository.cs
│   │   ├── TimeTracking/                     # Time Tracking Context
│   │   │   ├── TimeEntry.cs
│   │   │   ├── Timer.cs
│   │   │   ├── ValueObjects/
│   │   │   ├── Events/
│   │   │   ├── Services/
│   │   │   │   └── TimeEntryValidationService.cs
│   │   │   ├── ITimeEntryRepository.cs
│   │   │   └── ITimerRepository.cs
│   │   └── Reporting/                        # Reporting Context
│   │       ├── ExportJob.cs
│   │       ├── Invoice.cs
│   │       └── IExportJobRepository.cs
│   │
│   ├── TiroTime.Application/                 # Application Layer
│   │   ├── Common/
│   │   │   ├── Behaviors/
│   │   │   │   ├── ValidationBehavior.cs
│   │   │   │   ├── LoggingBehavior.cs
│   │   │   │   └── PerformanceBehavior.cs
│   │   │   ├── Exceptions/
│   │   │   └── Interfaces/
│   │   ├── Identity/
│   │   │   ├── Commands/
│   │   │   │   ├── RegisterUser/
│   │   │   │   ├── LoginUser/
│   │   │   │   └── ChangePassword/
│   │   │   ├── Queries/
│   │   │   │   ├── GetUserProfile/
│   │   │   │   └── GetUsers/
│   │   │   └── DTOs/
│   │   ├── Clients/
│   │   │   ├── Commands/
│   │   │   │   ├── CreateClient/
│   │   │   │   ├── UpdateClient/
│   │   │   │   └── ArchiveClient/
│   │   │   ├── Queries/
│   │   │   │   ├── GetClient/
│   │   │   │   └── GetClients/
│   │   │   └── DTOs/
│   │   ├── Projects/
│   │   │   ├── Commands/
│   │   │   ├── Queries/
│   │   │   └── DTOs/
│   │   ├── TimeTracking/
│   │   │   ├── Commands/
│   │   │   │   ├── StartTimer/
│   │   │   │   ├── StopTimer/
│   │   │   │   ├── CreateTimeEntry/
│   │   │   │   ├── UpdateTimeEntry/
│   │   │   │   └── DeleteTimeEntry/
│   │   │   ├── Queries/
│   │   │   │   ├── GetRunningTimer/
│   │   │   │   ├── GetTimeEntries/
│   │   │   │   └── GetTimeEntrySummary/
│   │   │   └── DTOs/
│   │   └── Reporting/
│   │       ├── Commands/
│   │       │   ├── GenerateReport/
│   │       │   └── ExportReport/
│   │       ├── Queries/
│   │       │   └── GetReportData/
│   │       └── Services/
│   │           ├── IExcelExportService.cs
│   │           └── IPdfExportService.cs
│   │
│   ├── TiroTime.Infrastructure/              # Infrastructure Layer
│   │   ├── Persistence/
│   │   │   ├── TiroTimeDbContext.cs
│   │   │   ├── Configurations/              # EF Core configurations
│   │   │   │   ├── UserConfiguration.cs
│   │   │   │   ├── ClientConfiguration.cs
│   │   │   │   ├── ProjectConfiguration.cs
│   │   │   │   └── TimeEntryConfiguration.cs
│   │   │   ├── Repositories/
│   │   │   │   ├── UserRepository.cs
│   │   │   │   ├── ClientRepository.cs
│   │   │   │   ├── ProjectRepository.cs
│   │   │   │   ├── TimeEntryRepository.cs
│   │   │   │   └── TimerRepository.cs
│   │   │   └── Migrations/
│   │   ├── Identity/
│   │   │   ├── JwtTokenService.cs
│   │   │   ├── PasswordHasher.cs
│   │   │   └── CurrentUserService.cs
│   │   ├── Caching/
│   │   │   └── RedisCacheService.cs
│   │   ├── Logging/
│   │   │   └── SerilogConfiguration.cs
│   │   ├── Exports/
│   │   │   ├── ExcelExportService.cs
│   │   │   └── PdfExportService.cs
│   │   ├── Email/
│   │   │   └── EmailService.cs
│   │   ├── Storage/
│   │   │   └── BlobStorageService.cs
│   │   └── BackgroundJobs/
│   │       ├── ExportJobProcessor.cs
│   │       └── ReportScheduler.cs
│   │
│   ├── TiroTime.API/                         # Presentation Layer
│   │   ├── Controllers/
│   │   │   ├── AuthController.cs
│   │   │   ├── UsersController.cs
│   │   │   ├── ClientsController.cs
│   │   │   ├── ProjectsController.cs
│   │   │   ├── TimeEntriesController.cs
│   │   │   ├── TimerController.cs
│   │   │   └── ReportsController.cs
│   │   ├── Hubs/                            # SignalR Hubs
│   │   │   └── TimerHub.cs
│   │   ├── Middleware/
│   │   │   ├── ExceptionHandlingMiddleware.cs
│   │   │   ├── RequestLoggingMiddleware.cs
│   │   │   └── PerformanceMonitoringMiddleware.cs
│   │   ├── Filters/
│   │   │   ├── ValidationFilter.cs
│   │   │   └── AuthorizationFilter.cs
│   │   ├── Extensions/
│   │   │   ├── ServiceCollectionExtensions.cs
│   │   │   └── WebApplicationExtensions.cs
│   │   ├── Program.cs
│   │   └── appsettings.json
│   │
│   └── TiroTime.Shared/                      # Shared Kernel
│       ├── Results/
│       │   └── Result.cs
│       ├── Exceptions/
│       │   └── DomainException.cs
│       └── Extensions/
│           └── DateTimeExtensions.cs
│
├── tests/
│   ├── TiroTime.Domain.Tests/
│   │   ├── Identity/
│   │   ├── Clients/
│   │   ├── Projects/
│   │   └── TimeTracking/
│   ├── TiroTime.Application.Tests/
│   │   ├── Commands/
│   │   └── Queries/
│   ├── TiroTime.Infrastructure.Tests/
│   │   ├── Repositories/
│   │   └── Services/
│   ├── TiroTime.API.Tests/
│   │   └── Controllers/
│   └── TiroTime.IntegrationTests/
│       ├── TimeTracking/
│       └── Reporting/
│
└── docs/
    └── architecture/
        └── diagrams/
```

## Layer Responsibilities

### 1. Domain Layer

**Responsibility**: Core business logic and rules

**Contains**:
- Aggregates, Entities, Value Objects
- Domain Events
- Domain Services
- Repository Interfaces
- Domain Exceptions

**Dependencies**: None (pure domain logic)

**Rules**:
- No dependencies on other layers
- No infrastructure concerns (database, HTTP, etc.)
- Only business logic and invariants
- Immutable value objects
- Rich domain models with behavior

### 2. Application Layer

**Responsibility**: Use case orchestration and application logic

**Contains**:
- Commands and Queries (CQRS)
- Command/Query Handlers (MediatR)
- DTOs (Data Transfer Objects)
- Validators (FluentValidation)
- Application Services
- Pipeline Behaviors

**Dependencies**: Domain Layer only

**Rules**:
- No direct database access (uses repositories)
- No domain logic (delegates to domain)
- Coordinates domain objects
- Maps domain models to DTOs
- Handles transactions

### 3. Infrastructure Layer

**Responsibility**: Technical implementation details

**Contains**:
- Database access (EF Core, repositories)
- External service integrations
- Caching (Redis)
- Logging (Serilog)
- File generation (Excel, PDF)
- Email sending
- Background job processing
- Identity and authentication

**Dependencies**: Application and Domain layers

**Rules**:
- Implements interfaces defined in Domain/Application
- All I/O operations here
- No business logic
- Framework-specific code

### 4. Presentation Layer (API)

**Responsibility**: HTTP endpoints and API contracts

**Contains**:
- Controllers (Web API)
- SignalR Hubs
- Middleware
- Request/Response models
- API documentation (Swagger)

**Dependencies**: Application layer only

**Rules**:
- No direct domain access
- No business logic
- Thin controllers (delegate to MediatR)
- Input validation
- HTTP-specific concerns only

## Technology Stack

### Backend

| Component | Technology | Version |
|-----------|-----------|---------|
| Framework | .NET | 8.0 (LTS) |
| Language | C# | 12.0 |
| Web Framework | ASP.NET Core | 8.0 |
| ORM | Entity Framework Core | 8.0 |
| Database | PostgreSQL or SQL Server | 15+ / 2022 |
| Caching | Redis | 7.0+ |
| Message Broker | MassTransit (optional) | 8.0+ |

### Libraries & Packages

| Purpose | Library | Version |
|---------|---------|---------|
| CQRS/Mediator | MediatR | 12.0+ |
| Validation | FluentValidation | 11.0+ |
| Mapping | AutoMapper (optional) | 12.0+ |
| Authentication | ASP.NET Core Identity | 8.0 |
| JWT | System.IdentityModel.Tokens.Jwt | 7.0+ |
| Logging | Serilog | 3.0+ |
| Testing | xUnit | 2.5+ |
| Mocking | NSubstitute or Moq | Latest |
| Assertions | FluentAssertions | 6.0+ |
| Test Data | Bogus | 35.0+ |
| Excel Generation | EPPlus or ClosedXML | Latest |
| PDF Generation | QuestPDF or iText | Latest |
| Background Jobs | Hangfire | 1.8+ |
| Real-time | SignalR | 8.0 |
| API Documentation | Swashbuckle (Swagger) | 6.5+ |

### Frontend (Future - Phase 2+)

| Component | Technology |
|-----------|-----------|
| Framework | React, Vue, or Blazor WebAssembly |
| State Management | Redux, Vuex, or Fluxor |
| HTTP Client | Axios or Fetch API |
| UI Framework | Material UI, Tailwind, or MudBlazor |

### DevOps & Infrastructure

| Component | Technology |
|-----------|-----------|
| Version Control | Git |
| CI/CD | GitHub Actions or Azure DevOps |
| Containerization | Docker |
| Orchestration | Kubernetes (optional) |
| Cloud Provider | Azure or AWS |
| Monitoring | Application Insights or Datadog |
| Secret Management | Azure Key Vault or AWS Secrets Manager |
| File Storage | Azure Blob Storage or AWS S3 |

## Design Patterns

### CQRS (Command Query Responsibility Segregation)

**Commands**: Modify state, return void or Result

```csharp
public class CreateTimeEntryCommand : IRequest<Result<TimeEntryId>>
{
    public Guid UserId { get; set; }
    public Guid ProjectId { get; set; }
    public DateTime Date { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
    public string Description { get; set; }
}

public class CreateTimeEntryCommandHandler : IRequestHandler<CreateTimeEntryCommand, Result<TimeEntryId>>
{
    private readonly ITimeEntryRepository _repository;
    private readonly IUnitOfWork _unitOfWork;

    public async Task<Result<TimeEntryId>> Handle(CreateTimeEntryCommand request, CancellationToken cancellationToken)
    {
        // 1. Load domain objects
        // 2. Execute domain logic
        // 3. Persist changes
        // 4. Return result
    }
}
```

**Queries**: Read data, return DTOs

```csharp
public class GetTimeEntriesQuery : IRequest<Result<List<TimeEntryDto>>>
{
    public Guid UserId { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
}

public class GetTimeEntriesQueryHandler : IRequestHandler<GetTimeEntriesQuery, Result<List<TimeEntryDto>>>
{
    private readonly ITimeEntryRepository _repository;

    public async Task<Result<List<TimeEntryDto>>> Handle(GetTimeEntriesQuery request, CancellationToken cancellationToken)
    {
        // Direct database query, bypass domain logic
        // Return DTOs for read performance
    }
}
```

### Repository Pattern

```csharp
public interface ITimeEntryRepository
{
    Task<TimeEntry?> GetByIdAsync(TimeEntryId id);
    Task<List<TimeEntry>> GetByUserAndDateRangeAsync(UserId userId, DateTime startDate, DateTime endDate);
    Task AddAsync(TimeEntry timeEntry);
    Task UpdateAsync(TimeEntry timeEntry);
    Task DeleteAsync(TimeEntry timeEntry);
}

public class TimeEntryRepository : ITimeEntryRepository
{
    private readonly TiroTimeDbContext _context;

    public async Task<TimeEntry?> GetByIdAsync(TimeEntryId id)
    {
        return await _context.TimeEntries
            .Include(te => te.Project)
            .FirstOrDefaultAsync(te => te.Id == id);
    }
    // ... other methods
}
```

### Unit of Work Pattern

```csharp
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
    Task BeginTransactionAsync();
    Task CommitTransactionAsync();
    Task RollbackTransactionAsync();
}

public class UnitOfWork : IUnitOfWork
{
    private readonly TiroTimeDbContext _context;

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        // Dispatch domain events before saving
        await DispatchDomainEventsAsync();
        return await _context.SaveChangesAsync(cancellationToken);
    }
}
```

### Result Pattern

```csharp
public class Result<T>
{
    public bool IsSuccess { get; }
    public T Value { get; }
    public string Error { get; }

    public static Result<T> Success(T value) => new(true, value, null);
    public static Result<T> Failure(string error) => new(false, default, error);
}
```

### Specification Pattern (Optional)

```csharp
public class ActiveProjectsSpecification : Specification<Project>
{
    public override Expression<Func<Project, bool>> ToExpression()
    {
        return project => project.Status == ProjectStatus.Active;
    }
}
```

## Database Design

### Entity Framework Core Configuration

```csharp
public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");

        builder.HasKey(u => u.Id);

        builder.Property(u => u.Id)
            .HasConversion(
                id => id.Value,
                value => new UserId(value));

        builder.OwnsOne(u => u.Email, email =>
        {
            email.Property(e => e.Value)
                .HasColumnName("Email")
                .HasMaxLength(254)
                .IsRequired();

            email.HasIndex(e => e.Value).IsUnique();
        });

        builder.OwnsOne(u => u.Name, name =>
        {
            name.Property(n => n.FirstName)
                .HasColumnName("FirstName")
                .HasMaxLength(50);

            name.Property(n => n.LastName)
                .HasColumnName("LastName")
                .HasMaxLength(50);
        });

        builder.Property(u => u.Role)
            .HasConversion<string>();

        // Additional configurations...
    }
}
```

### Database Schema Overview

**Users Table**
- Id (GUID, PK)
- Email (string, unique)
- FirstName, LastName
- PasswordHash
- Role, Status
- Timestamps

**Clients Table**
- Id (GUID, PK)
- Name (string, unique)
- CompanyName
- ContactInfo (JSON or separate table)
- Address (JSON or separate table)
- Currency
- Status

**Projects Table**
- Id (GUID, PK)
- ClientId (FK)
- Name
- Description
- HourlyRate, Currency
- StartDate, EndDate
- Budget info
- Status
- Timestamps

**ProjectMembers Table**
- Id (GUID, PK)
- ProjectId (FK)
- UserId (FK)
- Role
- CustomHourlyRate
- JoinedAt, LeftAt

**TimeEntries Table**
- Id (GUID, PK)
- UserId (FK)
- ProjectId (FK)
- Date
- StartTime, EndTime
- Duration
- Description, Notes
- IsBillable
- ApprovalStatus
- Status (Active/Deleted)
- Timestamps

**Timers Table**
- Id (GUID, PK)
- UserId (FK, unique - one timer per user)
- ProjectId (FK)
- Description
- StartedAt
- IsRunning

**ExportJobs Table**
- Id (GUID, PK)
- UserId (FK)
- Format
- Configuration (JSON)
- Status
- FileUrl
- CreatedAt, CompletedAt, ExpiresAt

### Indexes

```sql
-- Performance indexes
CREATE INDEX IX_TimeEntries_UserId_Date ON TimeEntries(UserId, Date);
CREATE INDEX IX_TimeEntries_ProjectId ON TimeEntries(ProjectId);
CREATE INDEX IX_TimeEntries_Date ON TimeEntries(Date);
CREATE INDEX IX_Projects_ClientId ON Projects(ClientId);
CREATE INDEX IX_Projects_Status ON Projects(Status);
CREATE INDEX IX_ProjectMembers_UserId ON ProjectMembers(UserId);
```

## API Design

### RESTful Endpoints

**Authentication**
```
POST   /api/v1/auth/register
POST   /api/v1/auth/login
POST   /api/v1/auth/refresh-token
POST   /api/v1/auth/logout
```

**Users**
```
GET    /api/v1/users
GET    /api/v1/users/{id}
GET    /api/v1/users/me
PUT    /api/v1/users/me
PUT    /api/v1/users/me/password
```

**Clients**
```
GET    /api/v1/clients
GET    /api/v1/clients/{id}
POST   /api/v1/clients
PUT    /api/v1/clients/{id}
DELETE /api/v1/clients/{id}
PATCH  /api/v1/clients/{id}/archive
```

**Projects**
```
GET    /api/v1/projects
GET    /api/v1/projects/{id}
POST   /api/v1/projects
PUT    /api/v1/projects/{id}
DELETE /api/v1/projects/{id}
PATCH  /api/v1/projects/{id}/status
GET    /api/v1/projects/{id}/team
POST   /api/v1/projects/{id}/team
```

**Time Tracking**
```
GET    /api/v1/timer
POST   /api/v1/timer/start
POST   /api/v1/timer/stop
DELETE /api/v1/timer

GET    /api/v1/time-entries
GET    /api/v1/time-entries/{id}
POST   /api/v1/time-entries
PUT    /api/v1/time-entries/{id}
DELETE /api/v1/time-entries/{id}
```

**Reporting**
```
POST   /api/v1/reports/generate
POST   /api/v1/reports/export/excel
POST   /api/v1/reports/export/pdf
GET    /api/v1/reports/history
GET    /api/v1/reports/history/{id}/download
```

### Response Format

**Success Response (200 OK)**
```json
{
  "data": {
    "id": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "name": "John Doe"
  },
  "metadata": {
    "timestamp": "2024-11-04T12:00:00Z",
    "version": "1.0"
  }
}
```

**Error Response (400 Bad Request)**
```json
{
  "type": "https://api.tirotime.com/errors/validation",
  "title": "One or more validation errors occurred",
  "status": 400,
  "errors": {
    "email": ["Email is required", "Invalid email format"],
    "password": ["Password must be at least 12 characters"]
  },
  "traceId": "00-abc123-def456-01"
}
```

**Pagination**
```json
{
  "data": [...],
  "pagination": {
    "page": 1,
    "pageSize": 20,
    "totalCount": 150,
    "totalPages": 8
  }
}
```

## Security Architecture

### Authentication Flow

1. User submits credentials
2. API validates credentials
3. Generate JWT access token (15 min expiry)
4. Generate JWT refresh token (7 days expiry)
5. Return tokens to client
6. Client includes access token in Authorization header
7. On expiry, use refresh token to get new access token

### JWT Token Structure

**Access Token Claims**:
- `sub`: User ID
- `email`: User email
- `role`: User role
- `exp`: Expiration timestamp
- `iat`: Issued at timestamp

**Refresh Token**:
- Stored in database with user reference
- One-time use (rotated on refresh)
- Revocable

### Authorization

**Policy-Based Authorization**:
```csharp
services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAdminRole", policy =>
        policy.RequireRole("Administrator"));

    options.AddPolicy("RequireManagerOrAdmin", policy =>
        policy.RequireRole("Manager", "Administrator"));

    options.AddPolicy("CanManageProjects", policy =>
        policy.RequireClaim("Permission", "ManageProjects"));
});
```

**Controller Authorization**:
```csharp
[Authorize(Policy = "RequireAdminRole")]
[ApiController]
[Route("api/v1/users")]
public class UsersController : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetUsers()
    {
        // Only administrators can access
    }
}
```

### Data Protection

- Passwords hashed with bcrypt (work factor 12)
- Sensitive data encrypted at rest
- TLS 1.2+ for data in transit
- SQL injection prevention (parameterized queries)
- XSS prevention (output encoding)
- CSRF protection (anti-forgery tokens)

## Caching Strategy

### Cache Layers

1. **In-Memory Cache**: Frequently accessed data (user profile, settings)
2. **Distributed Cache (Redis)**: Shared across instances
3. **Database Query Cache**: EF Core query caching

### Cache Keys

```
user:{userId}:profile
project:{projectId}
client:{clientId}
timer:{userId}
timeentries:{userId}:{date}
```

### Cache Invalidation

- Time-based expiration (TTL)
- Event-based invalidation (domain events)
- Manual invalidation on updates

**Example**:
```csharp
public class TimeEntryCreatedEventHandler : INotificationHandler<TimeEntryCreatedEvent>
{
    private readonly ICacheService _cache;

    public async Task Handle(TimeEntryCreatedEvent notification, CancellationToken cancellationToken)
    {
        // Invalidate relevant caches
        await _cache.RemoveAsync($"timeentries:{notification.UserId}:{notification.Date}");
        await _cache.RemoveAsync($"project:{notification.ProjectId}:stats");
    }
}
```

## Performance Optimization

### Database Optimization

1. **Indexing**: Strategic indexes on foreign keys and query columns
2. **Pagination**: All list endpoints paginated (default 20, max 100)
3. **Projections**: Select only required columns
4. **Eager Loading**: Use Include() to prevent N+1 queries
5. **Compiled Queries**: Pre-compile frequent queries
6. **Connection Pooling**: Configure appropriate pool size

### API Optimization

1. **Async/Await**: All I/O operations asynchronous
2. **Response Compression**: Gzip compression for responses
3. **Rate Limiting**: Prevent abuse (100 req/min per user)
4. **Output Caching**: Cache GET responses
5. **Minimal APIs**: Use minimal APIs for simple endpoints (optional)

### Background Processing

Heavy operations processed asynchronously:
- Report generation (> 1,000 entries)
- Excel/PDF export (> 100 entries)
- Email sending
- Data aggregation

## Monitoring & Observability

### Logging

**Structured Logging with Serilog**:
```csharp
Log.Information("User {UserId} created time entry {TimeEntryId} for project {ProjectId}",
    userId, timeEntryId, projectId);
```

**Log Enrichment**:
- Correlation ID (for request tracing)
- User ID
- Tenant ID (if multi-tenant)
- Environment (dev, staging, prod)

### Metrics

- Request count and duration
- Error rate and types
- Database query performance
- Cache hit ratio
- Background job success/failure rate

### Health Checks

```csharp
app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = async (context, report) =>
    {
        // Custom health check response
    }
});

services.AddHealthChecks()
    .AddDbContextCheck<TiroTimeDbContext>()
    .AddRedis(configuration["Redis:ConnectionString"])
    .AddCheck<EmailServiceHealthCheck>("email");
```

### Distributed Tracing

Use Application Insights or OpenTelemetry to trace requests across services and dependencies.

## Deployment Architecture

### Development Environment
- Local SQL Server or PostgreSQL
- Local Redis instance
- File system for file storage

### Staging Environment
- Azure SQL Database or Amazon RDS
- Azure Cache for Redis or ElastiCache
- Azure Blob Storage or S3
- Application Insights

### Production Environment
- High-availability database (replicas)
- Redis cluster
- CDN for static assets
- Load balancer
- Auto-scaling (2-10 instances)
- Backup and disaster recovery

### Docker Containerization

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["TiroTime.API/TiroTime.API.csproj", "TiroTime.API/"]
RUN dotnet restore "TiroTime.API/TiroTime.API.csproj"
COPY . .
WORKDIR "/src/TiroTime.API"
RUN dotnet build "TiroTime.API.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "TiroTime.API.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TiroTime.API.dll"]
```

## Testing Strategy

### Unit Tests (70%)
- Domain logic (aggregates, value objects, domain services)
- Application logic (command/query handlers)
- Validation logic
- No dependencies (mocked)

### Integration Tests (20%)
- Repository implementations
- Database operations
- API endpoints
- External service integrations
- Use Testcontainers for real dependencies

### E2E Tests (10%)
- Critical user journeys
- Complete workflows
- Use WebApplicationFactory

### Test Data Management

```csharp
public class TestDataBuilder
{
    public static User CreateUser(string email = "test@test.com")
    {
        return User.Create(
            new Email(email),
            new PersonName("Test", "User"),
            PasswordHash.FromPlainText("TestPassword123!"));
    }
}
```

## Migration Strategy

### Phase 1: MVP (Weeks 1-10)
- Core domain model
- Basic CRUD operations
- Authentication and authorization
- Timer and manual time entry
- Simple Excel export

### Phase 2: Enhanced Features (Weeks 11-18)
- Advanced time entry methods
- PDF export
- Dashboard and analytics
- Approval workflow
- Background job processing

### Phase 3: Advanced Features (Weeks 19-28)
- Idle detection
- Invoice generation
- Report scheduling
- Advanced caching
- Performance optimization

### Phase 4: Enterprise (Weeks 29+)
- Multi-tenancy
- Advanced integrations
- Mobile apps
- AI-powered features

---

**Document Owner**: Architecture Team
**Reviewers**: Development Team, DevOps Team, Security Team
**Approval Status**: Pending
