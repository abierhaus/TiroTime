# TiroTime Implementation Plan
**Plan ID**: PLAN-001
**Version**: 1.0
**Created**: 2025-11-04
**Status**: Ready for Implementation

## Overview

This implementation plan defines the architecture, project structure, and step-by-step development approach for TiroTime using Domain-Driven Design with .NET Class Libraries, Entity Framework Core, SQL Server, Razor Pages, and Bootstrap with a modern blue color scheme.

## Architecture Overview

### Technology Stack

| Component | Technology | Version   | Justification |
|-----------|-----------|-----------|---------------|
| **Framework** | .NET | 9.0 | Latest LTS, long-term support |
| **Web Framework** | ASP.NET Core Razor Pages | 8.0       | Server-side rendering, better for DDD |
| **Database** | SQL Server | 2022      | Enterprise-grade, requested by client |
| **ORM** | Entity Framework Core | 8.0       | Type-safe, LINQ support, migrations |
| **Authentication** | ASP.NET Core Identity | 8.0       | Built-in, no external library needed |
| **UI Framework** | Bootstrap | 5.3       | Modern, responsive, minimal dependencies |
| **Validation** | Data Annotations + FluentValidation | 11.9      | Built-in + enhanced validation |
| **Logging** | Microsoft.Extensions.Logging | 8.0       | Built-in, no external library |
| **Dependency Injection** | Microsoft.Extensions.DependencyInjection | 8.0       | Built-in |

### Minimal External Dependencies

✅ **No external dependencies** for:
- Logging (use built-in `ILogger<T>`)
- Caching (use built-in `IMemoryCache`)
- Configuration (use built-in `IConfiguration`)
- DI Container (use built-in DI)
- HTTP Client (use built-in `HttpClient`)

✅ **Only essential libraries**:
- FluentValidation (better validation than Data Annotations alone)
- System.IdentityModel.Tokens.Jwt (JWT authentication)

❌ **Avoid**:
- MediatR (not needed, use direct service calls)
- AutoMapper (use manual mapping, more explicit)
- Serilog (use built-in logging)
- Hangfire (use built-in IHostedService for background jobs)
- Redis (use IMemoryCache for MVP)

## Project Structure

### Solution Structure

```
TiroTime.sln
│
├── src/
│   ├── TiroTime.Domain/                    # Domain Layer (Class Library)
│   │   ├── Common/
│   │   │   ├── Entity.cs
│   │   │   ├── AggregateRoot.cs
│   │   │   ├── ValueObject.cs
│   │   │   └── IDomainEvent.cs
│   │   ├── Identity/
│   │   │   ├── ApplicationUser.cs
│   │   │   ├── ApplicationRole.cs
│   │   │   └── RefreshToken.cs
│   │   ├── Clients/
│   │   │   ├── Client.cs
│   │   │   └── ValueObjects/
│   │   ├── Projects/
│   │   │   ├── Project.cs
│   │   │   ├── ProjectMember.cs
│   │   │   └── ValueObjects/
│   │   ├── TimeTracking/
│   │   │   ├── TimeEntry.cs
│   │   │   ├── Timer.cs
│   │   │   └── ValueObjects/
│   │   └── Exceptions/
│   │       └── DomainException.cs
│   │
│   ├── TiroTime.Application/               # Application Layer (Class Library)
│   │   ├── Common/
│   │   │   ├── Interfaces/
│   │   │   │   ├── IRepository.cs
│   │   │   │   └── IUnitOfWork.cs
│   │   │   ├── Models/
│   │   │   │   ├── Result.cs
│   │   │   │   └── PagedResult.cs
│   │   │   └── Exceptions/
│   │   ├── Identity/
│   │   │   ├── Services/
│   │   │   │   ├── IAuthenticationService.cs
│   │   │   │   ├── AuthenticationService.cs
│   │   │   │   ├── IJwtTokenService.cs
│   │   │   │   └── JwtTokenService.cs
│   │   │   └── Models/
│   │   │       ├── RegisterRequest.cs
│   │   │       ├── LoginRequest.cs
│   │   │       └── AuthenticationResponse.cs
│   │   ├── Clients/
│   │   │   ├── Services/
│   │   │   │   ├── IClientService.cs
│   │   │   │   └── ClientService.cs
│   │   │   └── Models/
│   │   ├── Projects/
│   │   │   ├── Services/
│   │   │   └── Models/
│   │   ├── TimeTracking/
│   │   │   ├── Services/
│   │   │   │   ├── ITimeEntryService.cs
│   │   │   │   ├── TimeEntryService.cs
│   │   │   │   ├── ITimerService.cs
│   │   │   │   └── TimerService.cs
│   │   │   └── Models/
│   │   └── Reporting/
│   │       ├── Services/
│   │       └── Models/
│   │
│   ├── TiroTime.Infrastructure/            # Infrastructure Layer (Class Library)
│   │   ├── Persistence/
│   │   │   ├── TiroTimeDbContext.cs
│   │   │   ├── Configurations/
│   │   │   │   ├── ApplicationUserConfiguration.cs
│   │   │   │   ├── ClientConfiguration.cs
│   │   │   │   ├── ProjectConfiguration.cs
│   │   │   │   └── TimeEntryConfiguration.cs
│   │   │   ├── Repositories/
│   │   │   │   ├── ClientRepository.cs
│   │   │   │   ├── ProjectRepository.cs
│   │   │   │   ├── TimeEntryRepository.cs
│   │   │   │   └── TimerRepository.cs
│   │   │   ├── UnitOfWork.cs
│   │   │   └── Migrations/
│   │   ├── Identity/
│   │   │   ├── IdentityConfiguration.cs
│   │   │   └── CurrentUserService.cs
│   │   ├── Services/
│   │   │   ├── EmailService.cs
│   │   │   └── DateTimeService.cs
│   │   └── BackgroundJobs/
│   │       └── TimerSyncJob.cs
│   │
│   └── TiroTime.Web/                       # Presentation Layer (Razor Pages)
│       ├── Pages/
│       │   ├── Index.cshtml
│       │   ├── _Layout.cshtml
│       │   ├── _ViewImports.cshtml
│       │   ├── _ViewStart.cshtml
│       │   ├── Account/
│       │   │   ├── Login.cshtml
│       │   │   ├── Register.cshtml
│       │   │   ├── ForgotPassword.cshtml
│       │   │   └── Profile.cshtml
│       │   ├── Clients/
│       │   │   ├── Index.cshtml
│       │   │   ├── Create.cshtml
│       │   │   ├── Edit.cshtml
│       │   │   └── Details.cshtml
│       │   ├── Projects/
│       │   │   ├── Index.cshtml
│       │   │   ├── Create.cshtml
│       │   │   ├── Edit.cshtml
│       │   │   └── Details.cshtml
│       │   ├── TimeTracking/
│       │   │   ├── Index.cshtml
│       │   │   ├── Timer.cshtml
│       │   │   └── Timesheet.cshtml
│       │   └── Reports/
│       │       ├── Index.cshtml
│       │       └── Generate.cshtml
│       ├── ViewModels/
│       ├── TagHelpers/
│       ├── Middleware/
│       │   ├── ExceptionHandlingMiddleware.cs
│       │   └── RequestLoggingMiddleware.cs
│       ├── Extensions/
│       │   └── ServiceCollectionExtensions.cs
│       ├── wwwroot/
│       │   ├── css/
│       │   │   ├── site.css
│       │   │   └── bootstrap.min.css
│       │   ├── js/
│       │   │   ├── site.js
│       │   │   ├── timer.js
│       │   │   └── bootstrap.bundle.min.js
│       │   └── images/
│       ├── appsettings.json
│       ├── appsettings.Development.json
│       └── Program.cs
│
└── tests/
    ├── TiroTime.Domain.Tests/
    ├── TiroTime.Application.Tests/
    ├── TiroTime.Infrastructure.Tests/
    └── TiroTime.Web.Tests/
```

## Design System (Blue Theme)

### Color Palette

```css
:root {
  /* Primary Blue Scale */
  --primary-900: #0d2d4d;    /* Dark blue - headers, important text */
  --primary-800: #0f3a5f;    /* Darker blue */
  --primary-700: #124872;    /* Deep blue */
  --primary-600: #1565a8;    /* Primary blue - main brand color */
  --primary-500: #1976d2;    /* Bright blue - buttons, links */
  --primary-400: #42a5f5;    /* Light blue - hover states */
  --primary-300: #64b5f6;    /* Lighter blue */
  --primary-200: #90caf9;    /* Very light blue - backgrounds */
  --primary-100: #bbdefb;    /* Pale blue - hover backgrounds */
  --primary-50:  #e3f2fd;    /* Ultra light blue - subtle backgrounds */

  /* Accent Colors */
  --accent-cyan: #00bcd4;    /* Info/notifications */
  --accent-teal: #009688;    /* Success states */

  /* Semantic Colors */
  --success: #2e7d32;        /* Green - success messages */
  --warning: #ed6c02;        /* Orange - warnings */
  --error: #d32f2f;          /* Red - errors */
  --info: #0288d1;           /* Blue - info messages */

  /* Neutral Colors */
  --gray-900: #212121;       /* Almost black - primary text */
  --gray-800: #424242;       /* Dark gray */
  --gray-700: #616161;       /* Medium dark gray */
  --gray-600: #757575;       /* Medium gray */
  --gray-500: #9e9e9e;       /* Gray - secondary text */
  --gray-400: #bdbdbd;       /* Light gray */
  --gray-300: #e0e0e0;       /* Lighter gray - borders */
  --gray-200: #eeeeee;       /* Very light gray - backgrounds */
  --gray-100: #f5f5f5;       /* Ultra light gray - page background */
  --gray-50:  #fafafa;       /* Almost white */

  /* Special */
  --white: #ffffff;
  --black: #000000;

  /* Shadows */
  --shadow-sm: 0 1px 2px 0 rgba(13, 45, 77, 0.05);
  --shadow-md: 0 4px 6px -1px rgba(13, 45, 77, 0.1);
  --shadow-lg: 0 10px 15px -3px rgba(13, 45, 77, 0.1);
  --shadow-xl: 0 20px 25px -5px rgba(13, 45, 77, 0.1);
}
```

### Bootstrap Customization

```scss
// _custom-bootstrap.scss
$primary:   #1565a8;
$secondary: #616161;
$success:   #2e7d32;
$info:      #0288d1;
$warning:   #ed6c02;
$danger:    #d32f2f;
$light:     #f5f5f5;
$dark:      #212121;

// Override Bootstrap variables
$body-bg: #fafafa;
$body-color: #212121;
$link-color: #1565a8;
$link-hover-color: #0f3a5f;

$border-radius: 0.375rem;
$border-radius-sm: 0.25rem;
$border-radius-lg: 0.5rem;

$font-family-sans-serif: -apple-system, BlinkMacSystemFont, "Segoe UI", Roboto, "Helvetica Neue", Arial, sans-serif;

// Import Bootstrap
@import "~bootstrap/scss/bootstrap";
```

### UI Components Style Guide

**Buttons**:
```html
<!-- Primary Action -->
<button class="btn btn-primary">Start Timer</button>

<!-- Secondary Action -->
<button class="btn btn-outline-primary">Save Draft</button>

<!-- Danger Action -->
<button class="btn btn-danger">Delete</button>

<!-- Icon Button -->
<button class="btn btn-primary">
  <i class="bi bi-play-fill"></i> Start
</button>
```

**Cards**:
```html
<div class="card shadow-sm">
  <div class="card-header bg-primary text-white">
    <h5 class="mb-0">Time Entries</h5>
  </div>
  <div class="card-body">
    <!-- Content -->
  </div>
</div>
```

**Forms**:
```html
<div class="mb-3">
  <label class="form-label">Project</label>
  <select class="form-select">
    <option>Select project...</option>
  </select>
</div>
```

## Implementation Phases

### Phase 0: Project Setup (Week 1)

**Objective**: Create solution structure and configure base infrastructure

**Tasks**:
1. Create .NET 8 solution with class libraries
2. Set up project references
3. Configure Entity Framework and SQL Server
4. Set up ASP.NET Core Identity
5. Create base domain entities and value objects
6. Configure dependency injection
7. Set up logging
8. Create initial database migration

**Deliverables**:
- ✅ Solution structure created
- ✅ All projects compile
- ✅ Database connection established
- ✅ Identity tables created
- ✅ Initial migration applied

---

### Phase 1: MVP - Authentication & Basic Infrastructure (Weeks 2-3)

**Objective**: User registration, login, and basic infrastructure

**Domain (TiroTime.Domain)**:
- [x] ApplicationUser entity (extends IdentityUser)
- [x] ApplicationRole entity (extends IdentityRole)
- [x] RefreshToken entity
- [x] WorkingHours value object
- [x] DomainException

**Application (TiroTime.Application)**:
- [x] IAuthenticationService interface
- [x] AuthenticationService implementation
- [x] IJwtTokenService interface
- [x] JwtTokenService implementation
- [x] Result<T> class
- [x] RegisterRequest, LoginRequest, AuthenticationResponse models

**Infrastructure (TiroTime.Infrastructure)**:
- [x] TiroTimeDbContext (with Identity)
- [x] ApplicationUserConfiguration (EF Core)
- [x] RefreshTokenRepository
- [x] UnitOfWork implementation
- [x] EmailService (console/file for dev)
- [x] CurrentUserService

**Web (TiroTime.Web)**:
- [x] Program.cs configuration
- [x] _Layout.cshtml with Bootstrap 5.3 and blue theme
- [x] Login.cshtml page
- [x] Register.cshtml page
- [x] ForgotPassword.cshtml page
- [x] Profile.cshtml page
- [x] ExceptionHandlingMiddleware
- [x] Custom CSS for blue theme

**Database**:
```sql
-- Tables created by Identity
- AspNetUsers
- AspNetRoles
- AspNetUserRoles
- AspNetUserClaims
- AspNetUserLogins
- AspNetUserTokens
- AspNetRoleClaims

-- Custom tables
- RefreshTokens
```

**Acceptance Criteria**:
- ✅ User can register with email and password
- ✅ Email confirmation sent (console output)
- ✅ User can log in with credentials
- ✅ JWT access token generated
- ✅ Refresh token stored in database
- ✅ User profile page displays user info
- ✅ Blue-themed UI with Bootstrap
- ✅ Account lockout after 5 failed attempts
- ✅ Password meets security requirements (12+ chars, complexity)

---

### Phase 2: Client & Project Management (Weeks 4-5)

**Objective**: Create and manage clients and projects

**Domain**:
- [x] Client aggregate root
- [x] Project aggregate root
- [x] ProjectMember entity
- [x] Value objects: Money, Color, Address, ContactInfo
- [x] Domain events: ClientCreated, ProjectCreated

**Application**:
- [x] IClientService, ClientService
- [x] IProjectService, ProjectService
- [x] Client DTOs (CreateClientRequest, UpdateClientRequest, ClientResponse)
- [x] Project DTOs
- [x] Validation with FluentValidation

**Infrastructure**:
- [x] ClientConfiguration (EF Core)
- [x] ProjectConfiguration (EF Core)
- [x] ClientRepository
- [x] ProjectRepository

**Web**:
- [x] Clients/Index.cshtml (list with search/filter)
- [x] Clients/Create.cshtml
- [x] Clients/Edit.cshtml
- [x] Clients/Details.cshtml
- [x] Projects/Index.cshtml (list with grouping by client)
- [x] Projects/Create.cshtml
- [x] Projects/Edit.cshtml
- [x] Projects/Details.cshtml
- [x] Navigation menu updated

**Database Migration**:
```sql
CREATE TABLE Clients (
  Id UNIQUEIDENTIFIER PRIMARY KEY,
  Name NVARCHAR(200) NOT NULL,
  CompanyName NVARCHAR(200),
  Email NVARCHAR(254),
  Phone NVARCHAR(20),
  AddressJson NVARCHAR(MAX),
  CurrencyCode NVARCHAR(3) NOT NULL,
  ColorHex NVARCHAR(7),
  Status INT NOT NULL,
  CreatedAt DATETIME2 NOT NULL,
  CreatedBy UNIQUEIDENTIFIER NOT NULL
);

CREATE TABLE Projects (
  Id UNIQUEIDENTIFIER PRIMARY KEY,
  ClientId UNIQUEIDENTIFIER NOT NULL,
  Name NVARCHAR(200) NOT NULL,
  Description NVARCHAR(MAX),
  HourlyRateAmount DECIMAL(18,2) NOT NULL,
  CurrencyCode NVARCHAR(3) NOT NULL,
  ProjectCode NVARCHAR(20),
  StartDate DATETIME2,
  EndDate DATETIME2,
  IsBillableByDefault BIT NOT NULL,
  Status INT NOT NULL,
  ColorHex NVARCHAR(7),
  CreatedAt DATETIME2 NOT NULL,
  CreatedBy UNIQUEIDENTIFIER NOT NULL,
  FOREIGN KEY (ClientId) REFERENCES Clients(Id)
);

CREATE TABLE ProjectMembers (
  Id UNIQUEIDENTIFIER PRIMARY KEY,
  ProjectId UNIQUEIDENTIFIER NOT NULL,
  UserId UNIQUEIDENTIFIER NOT NULL,
  Role INT NOT NULL,
  CustomHourlyRateAmount DECIMAL(18,2),
  JoinedAt DATETIME2 NOT NULL,
  LeftAt DATETIME2,
  FOREIGN KEY (ProjectId) REFERENCES Projects(Id),
  FOREIGN KEY (UserId) REFERENCES AspNetUsers(Id)
);
```

**Acceptance Criteria**:
- ✅ Admin can create, edit, view, archive clients
- ✅ Clients list shows all active clients with search
- ✅ Admin can create, edit, view projects for clients
- ✅ Projects list grouped by client
- ✅ Color coding for clients and projects
- ✅ Validation prevents duplicate names
- ✅ Currency and hourly rate configured per project
- ✅ Users can be assigned to projects

---

### Phase 3: Time Tracking - Timer & Manual Entry (Weeks 6-7)

**Objective**: Core time tracking functionality

**Domain**:
- [x] TimeEntry aggregate root
- [x] Timer aggregate root
- [x] Value objects: TimeRange, Tag
- [x] Domain events: TimerStarted, TimerStopped, TimeEntryCreated

**Application**:
- [x] ITimeEntryService, TimeEntryService
- [x] ITimerService, TimerService
- [x] TimeEntry DTOs
- [x] Timer DTOs
- [x] Validation rules for time entries

**Infrastructure**:
- [x] TimeEntryConfiguration (EF Core)
- [x] TimerConfiguration (EF Core)
- [x] TimeEntryRepository
- [x] TimerRepository
- [x] TimerSyncBackgroundService (IHostedService for syncing)

**Web**:
- [x] TimeTracking/Index.cshtml (today's entries + timer widget)
- [x] TimeTracking/Timer.cshtml (dedicated timer page)
- [x] TimeTracking/Create.cshtml (manual entry)
- [x] TimeTracking/Edit.cshtml
- [x] Shared/_TimerWidget.cshtml (partial view for header)
- [x] wwwroot/js/timer.js (client-side timer logic)

**Database Migration**:
```sql
CREATE TABLE Timers (
  Id UNIQUEIDENTIFIER PRIMARY KEY,
  UserId UNIQUEIDENTIFIER NOT NULL UNIQUE, -- One timer per user
  ProjectId UNIQUEIDENTIFIER NOT NULL,
  Description NVARCHAR(500),
  StartedAt DATETIME2 NOT NULL,
  LastSyncedAt DATETIME2 NOT NULL,
  IsRunning BIT NOT NULL,
  FOREIGN KEY (UserId) REFERENCES AspNetUsers(Id),
  FOREIGN KEY (ProjectId) REFERENCES Projects(Id)
);

CREATE TABLE TimeEntries (
  Id UNIQUEIDENTIFIER PRIMARY KEY,
  UserId UNIQUEIDENTIFIER NOT NULL,
  ProjectId UNIQUEIDENTIFIER NOT NULL,
  Date DATE NOT NULL,
  StartTime TIME,
  EndTime TIME,
  DurationMinutes INT NOT NULL,
  Description NVARCHAR(500) NOT NULL,
  Notes NVARCHAR(2000),
  IsBillable BIT NOT NULL,
  Source INT NOT NULL, -- Timer, Manual, etc.
  ApprovalStatus INT NOT NULL,
  Status INT NOT NULL,
  CreatedAt DATETIME2 NOT NULL,
  UpdatedAt DATETIME2,
  DeletedAt DATETIME2,
  FOREIGN KEY (UserId) REFERENCES AspNetUsers(Id),
  FOREIGN KEY (ProjectId) REFERENCES Projects(Id)
);

CREATE INDEX IX_TimeEntries_UserId_Date ON TimeEntries(UserId, Date);
CREATE INDEX IX_TimeEntries_ProjectId ON TimeEntries(ProjectId);
```

**Acceptance Criteria**:
- ✅ User can start timer for a project
- ✅ Timer displays in header on all pages
- ✅ Timer persists across page refreshes
- ✅ User can stop timer to create time entry
- ✅ User can create manual time entry with start/end times
- ✅ Duration calculated automatically
- ✅ User can edit and delete own time entries
- ✅ Time entries list shows today's entries
- ✅ Validation prevents overlapping entries (warning)
- ✅ Only one timer can run per user
- ✅ Timer syncs to server every 30 seconds

**JavaScript - Timer Implementation**:
```javascript
// wwwroot/js/timer.js
class TimerWidget {
    constructor() {
        this.startTime = null;
        this.intervalId = null;
        this.syncIntervalId = null;
    }

    start(startTime) {
        this.startTime = new Date(startTime);
        this.update();
        this.intervalId = setInterval(() => this.update(), 1000);
        this.syncIntervalId = setInterval(() => this.sync(), 30000);
    }

    update() {
        const now = new Date();
        const elapsed = Math.floor((now - this.startTime) / 1000);
        const hours = Math.floor(elapsed / 3600);
        const minutes = Math.floor((elapsed % 3600) / 60);
        const seconds = elapsed % 60;

        document.getElementById('timer-display').textContent =
            `${hours.toString().padStart(2, '0')}:${minutes.toString().padStart(2, '0')}:${seconds.toString().padStart(2, '0')}`;
    }

    async sync() {
        await fetch('/api/timer/sync', { method: 'POST' });
    }

    stop() {
        clearInterval(this.intervalId);
        clearInterval(this.syncIntervalId);
    }
}
```

---

### Phase 4: Reporting & Excel Export (Weeks 8-9)

**Objective**: Generate reports and export to Excel with German formatting

**Application**:
- [x] IReportingService, ReportingService
- [x] IExcelExportService, ExcelExportService
- [x] ReportConfiguration model
- [x] ReportFilters model

**Infrastructure**:
- [x] ExcelExportService implementation (using EPPlus or ClosedXML)
- [x] German number and date formatting utilities

**Web**:
- [x] Reports/Index.cshtml (filter UI)
- [x] Reports/Preview.cshtml (report preview before export)
- [x] Export action to download Excel file

**NuGet Package** (minimal external dependency):
```xml
<!-- Choose ONE -->
<PackageReference Include="ClosedXML" Version="0.102.*" />
<!-- OR -->
<PackageReference Include="EPPlus" Version="7.0.*" />
```

**Excel Export Features**:
- Summary sheet (Zusammenfassung)
- Details sheet (Detaillierte Zeiterfassung)
- German formatting (1.234,56 €, DD.MM.YYYY)
- Professional styling with colors
- Formulas for totals

**Acceptance Criteria**:
- ✅ User can filter time entries by date range, project, client
- ✅ Report preview shows summary statistics
- ✅ Export to Excel generates formatted file
- ✅ German number format: 1.234,56 €
- ✅ German date format: DD.MM.YYYY
- ✅ Excel includes summary and details sheets
- ✅ File downloads with meaningful name
- ✅ Summary shows total hours, billable hours, revenue

---

### Phase 5: UI Polish & Responsive Design (Week 10)

**Objective**: Refine UI, ensure responsive design, improve UX

**Tasks**:
1. Responsive design testing (mobile, tablet, desktop)
2. Loading states and spinners
3. Toast notifications for success/error messages
4. Form validation styling
5. Empty states (no clients, no time entries, etc.)
6. Pagination for lists
7. Search and filtering refinement
8. Keyboard shortcuts (optional)
9. Accessibility improvements (ARIA labels, focus management)
10. Browser testing (Chrome, Firefox, Edge)

**UI Components to Refine**:
- Timer widget (always visible, collapsible on mobile)
- Client/project cards with hover effects
- Time entry list with inline editing
- Filter panels with collapsible sections
- Dashboard widgets (Phase 2 feature)

**Acceptance Criteria**:
- ✅ Application works on mobile, tablet, desktop
- ✅ All forms have proper validation styling
- ✅ Success/error messages displayed via toasts
- ✅ Loading spinners during async operations
- ✅ Empty states with helpful messages
- ✅ Lists paginated (20 items per page)
- ✅ Keyboard navigation works
- ✅ WCAG 2.1 Level A compliance

---

## Database Configuration

### Connection String

**appsettings.json**:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=TiroTimeDb;Trusted_Connection=true;MultipleActiveResultSets=true"
  },
  "Jwt": {
    "SecretKey": "YourSuperSecretKeyHereMinimum32Characters!",
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
  }
}
```

**appsettings.Production.json**:
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=your-server;Database=TiroTimeDb;User Id=sa;Password=YourPassword;TrustServerCertificate=true"
  }
}
```

### Entity Framework Migrations

```bash
# Initial migration
dotnet ef migrations add InitialCreate --project src/TiroTime.Infrastructure --startup-project src/TiroTime.Web

# Update database
dotnet ef database update --project src/TiroTime.Infrastructure --startup-project src/TiroTime.Web

# Generate SQL script (for production)
dotnet ef migrations script --project src/TiroTime.Infrastructure --startup-project src/TiroTime.Web --output migrations.sql
```

## Dependency Injection Configuration

**Program.cs**:
```csharp
var builder = WebApplication.CreateBuilder(args);

// Add services
builder.Services.AddRazorPages();

// Database
builder.Services.AddDbContext<TiroTimeDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        b => b.MigrationsAssembly("TiroTime.Infrastructure")));

// Identity
builder.Services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequireLowercase = true;
    options.Password.RequireUppercase = true;
    options.Password.RequireNonAlphanumeric = true;
    options.Password.RequiredLength = 12;
    options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
    options.Lockout.MaxFailedAccessAttempts = 5;
    options.User.RequireUniqueEmail = true;
    options.SignIn.RequireConfirmedEmail = true;
})
.AddEntityFrameworkStores<TiroTimeDbContext>()
.AddDefaultTokenProviders();

// Cookie Authentication for Razor Pages
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.LogoutPath = "/Account/Logout";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromHours(8);
    options.SlidingExpiration = true;
});

// Application Services
builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();
builder.Services.AddScoped<IClientService, ClientService>();
builder.Services.AddScoped<IProjectService, ProjectService>();
builder.Services.AddScoped<ITimeEntryService, TimeEntryService>();
builder.Services.AddScoped<ITimerService, TimerService>();
builder.Services.AddScoped<IReportingService, ReportingService>();
builder.Services.AddScoped<IExcelExportService, ExcelExportService>();

// Infrastructure Services
builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
builder.Services.AddScoped<IClientRepository, ClientRepository>();
builder.Services.AddScoped<IProjectRepository, ProjectRepository>();
builder.Services.AddScoped<ITimeEntryRepository, TimeEntryRepository>();
builder.Services.AddScoped<ITimerRepository, TimerRepository>();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddScoped<IEmailService, EmailService>();

// Built-in caching
builder.Services.AddMemoryCache();

// Built-in HTTP context accessor
builder.Services.AddHttpContextAccessor();

// Background services
builder.Services.AddHostedService<TimerSyncBackgroundService>();

// FluentValidation
builder.Services.AddValidatorsFromAssemblyContaining<CreateClientRequestValidator>();

var app = builder.Build();

// Configure middleware pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthentication();
app.UseAuthorization();
app.MapRazorPages();

// Seed database
await SeedDatabase(app);

app.Run();
```

## Layout Structure

**Pages/Shared/_Layout.cshtml**:
```html
<!DOCTYPE html>
<html lang="de">
<head>
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <title>@ViewData["Title"] - TiroTime</title>
    <link rel="stylesheet" href="~/lib/bootstrap/dist/css/bootstrap.min.css" />
    <link rel="stylesheet" href="~/css/site.css" asp-append-version="true" />
    <link rel="stylesheet" href="https://cdn.jsdelivr.net/npm/bootstrap-icons@1.11.0/font/bootstrap-icons.css">
</head>
<body>
    <header>
        <nav class="navbar navbar-expand-lg navbar-dark bg-primary">
            <div class="container-fluid">
                <a class="navbar-brand" asp-page="/Index">
                    <i class="bi bi-clock-history"></i> TiroTime
                </a>
                <button class="navbar-toggler" type="button" data-bs-toggle="collapse" data-bs-target="#navbarNav">
                    <span class="navbar-toggler-icon"></span>
                </button>
                <div class="collapse navbar-collapse" id="navbarNav">
                    <ul class="navbar-nav me-auto">
                        <li class="nav-item">
                            <a class="nav-link" asp-page="/TimeTracking/Index">
                                <i class="bi bi-stopwatch"></i> Zeiterfassung
                            </a>
                        </li>
                        <li class="nav-item">
                            <a class="nav-link" asp-page="/Clients/Index">
                                <i class="bi bi-people"></i> Kunden
                            </a>
                        </li>
                        <li class="nav-item">
                            <a class="nav-link" asp-page="/Projects/Index">
                                <i class="bi bi-folder"></i> Projekte
                            </a>
                        </li>
                        <li class="nav-item">
                            <a class="nav-link" asp-page="/Reports/Index">
                                <i class="bi bi-graph-up"></i> Berichte
                            </a>
                        </li>
                    </ul>

                    <!-- Timer Widget -->
                    <partial name="_TimerWidget" />

                    <ul class="navbar-nav">
                        @if (User.Identity?.IsAuthenticated == true)
                        {
                            <li class="nav-item dropdown">
                                <a class="nav-link dropdown-toggle" href="#" role="button" data-bs-toggle="dropdown">
                                    <i class="bi bi-person-circle"></i> @User.Identity.Name
                                </a>
                                <ul class="dropdown-menu dropdown-menu-end">
                                    <li><a class="dropdown-item" asp-page="/Account/Profile">Profil</a></li>
                                    <li><hr class="dropdown-divider"></li>
                                    <li>
                                        <form method="post" asp-page="/Account/Logout">
                                            <button type="submit" class="dropdown-item">Abmelden</button>
                                        </form>
                                    </li>
                                </ul>
                            </li>
                        }
                        else
                        {
                            <li class="nav-item">
                                <a class="nav-link" asp-page="/Account/Login">Anmelden</a>
                            </li>
                        }
                    </ul>
                </div>
            </div>
        </nav>
    </header>

    <main class="container-fluid py-4">
        @RenderBody()
    </main>

    <footer class="footer mt-auto py-3 bg-light">
        <div class="container text-center">
            <span class="text-muted">&copy; 2025 TiroTime - Zeiterfassung leicht gemacht</span>
        </div>
    </footer>

    <script src="~/lib/bootstrap/dist/js/bootstrap.bundle.min.js"></script>
    <script src="~/js/site.js" asp-append-version="true"></script>
    @await RenderSectionAsync("Scripts", required: false)
</body>
</html>
```

## Testing Strategy

### Unit Tests (Priority)

```csharp
// TiroTime.Domain.Tests/TimeEntryTests.cs
public class TimeEntryTests
{
    [Fact]
    public void CreateManual_ValidData_ReturnsTimeEntry()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var projectId = Guid.NewGuid();
        var date = DateTime.Today;
        var timeRange = new TimeRange(new TimeOnly(9, 0), new TimeOnly(17, 0));

        // Act
        var entry = TimeEntry.CreateManual(userId, projectId, date, timeRange, "Test work");

        // Assert
        entry.Should().NotBeNull();
        entry.Duration.Should().Be(TimeSpan.FromHours(8));
        entry.IsBillable.Should().BeTrue();
    }

    [Fact]
    public void UpdateTimeRange_EndBeforeStart_ThrowsDomainException()
    {
        // Arrange
        var entry = CreateValidTimeEntry();
        var invalidTimeRange = new TimeRange(new TimeOnly(17, 0), new TimeOnly(9, 0));

        // Act & Assert
        var act = () => entry.UpdateTimeRange(invalidTimeRange);
        act.Should().Throw<DomainException>();
    }
}

// TiroTime.Application.Tests/TimeEntryServiceTests.cs
public class TimeEntryServiceTests
{
    [Fact]
    public async Task CreateTimeEntry_ValidRequest_ReturnsSuccess()
    {
        // Arrange
        var mockRepo = Substitute.For<ITimeEntryRepository>();
        var mockUnitOfWork = Substitute.For<IUnitOfWork>();
        var service = new TimeEntryService(mockRepo, mockUnitOfWork);

        var request = new CreateTimeEntryRequest
        {
            ProjectId = Guid.NewGuid(),
            Date = DateTime.Today,
            StartTime = new TimeOnly(9, 0),
            EndTime = new TimeOnly(17, 0),
            Description = "Test"
        };

        // Act
        var result = await service.CreateTimeEntryAsync(request);

        // Assert
        result.IsSuccess.Should().BeTrue();
        await mockRepo.Received(1).AddAsync(Arg.Any<TimeEntry>());
        await mockUnitOfWork.Received(1).SaveChangesAsync();
    }
}
```

### Integration Tests

Use `WebApplicationFactory<Program>` for integration testing:

```csharp
public class TimeTrackingIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public TimeTrackingIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetTimeEntries_Authenticated_ReturnsSuccess()
    {
        // Arrange & Act
        var response = await _client.GetAsync("/TimeTracking/Index");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
```

## Performance Optimization

### Database Indexes

```sql
-- Time entries performance indexes
CREATE INDEX IX_TimeEntries_UserId_Date ON TimeEntries(UserId, Date);
CREATE INDEX IX_TimeEntries_ProjectId ON TimeEntries(ProjectId);
CREATE INDEX IX_TimeEntries_Date ON TimeEntries(Date) WHERE Status = 0; -- Active only

-- Project lookups
CREATE INDEX IX_Projects_ClientId_Status ON Projects(ClientId, Status);
CREATE INDEX IX_Projects_Status ON Projects(Status) WHERE Status = 0; -- Active only

-- Client lookups
CREATE INDEX IX_Clients_Status ON Clients(Status) WHERE Status = 0; -- Active only
```

### Caching Strategy

```csharp
// Cache frequently accessed data
public class ProjectService : IProjectService
{
    private readonly IMemoryCache _cache;
    private readonly IProjectRepository _repository;

    public async Task<List<ProjectDto>> GetActiveProjectsAsync()
    {
        const string cacheKey = "active_projects";

        if (!_cache.TryGetValue(cacheKey, out List<ProjectDto> projects))
        {
            var projectEntities = await _repository.GetActiveProjectsAsync();
            projects = projectEntities.Select(p => MapToDto(p)).ToList();

            _cache.Set(cacheKey, projects, TimeSpan.FromMinutes(10));
        }

        return projects;
    }
}
```

### Query Optimization

```csharp
// Use projections for list views
public async Task<List<TimeEntryListDto>> GetTimeEntriesAsync(Guid userId, DateTime startDate, DateTime endDate)
{
    return await _context.TimeEntries
        .Where(te => te.UserId == userId && te.Date >= startDate && te.Date <= endDate)
        .Select(te => new TimeEntryListDto
        {
            Id = te.Id,
            ProjectName = te.Project.Name,
            Date = te.Date,
            Duration = te.Duration,
            Description = te.Description,
            IsBillable = te.IsBillable
        })
        .OrderByDescending(te => te.Date)
        .ToListAsync();
}
```

## Deployment Plan

### Local Development

1. Clone repository
2. Restore NuGet packages: `dotnet restore`
3. Update connection string in `appsettings.json`
4. Run migrations: `dotnet ef database update`
5. Run application: `dotnet run --project src/TiroTime.Web`
6. Navigate to `https://localhost:5001`

### Production Deployment

1. Publish application: `dotnet publish -c Release -o ./publish`
2. Copy files to server
3. Update `appsettings.Production.json` with production connection string
4. Run migrations on production database
5. Configure IIS or Kestrel with reverse proxy
6. Set up SSL certificate
7. Configure automatic backups for SQL Server
8. Set up application monitoring

## Risk Mitigation

| Risk | Impact | Mitigation |
|------|--------|------------|
| Timer data loss | High | Auto-save every 30 seconds, persist to database |
| Concurrent time entries | Medium | Add optimistic concurrency with EF Core RowVersion |
| Database connection failures | High | Implement retry logic, connection resilience |
| Excel export performance | Medium | Generate asynchronously for large datasets |
| User forgets to stop timer | Medium | Add notifications, max timer duration (24h) |

## Success Criteria

### MVP Success Metrics

- ✅ User can register and log in
- ✅ User can create clients and projects
- ✅ User can track time with timer
- ✅ User can create manual time entries
- ✅ User can export time report to Excel
- ✅ Application is responsive on all devices
- ✅ Blue-themed UI matches brand
- ✅ No critical bugs in core functionality
- ✅ Page load time < 2 seconds
- ✅ Timer accuracy within 1 second
- ✅ Database migrations work without data loss

### Technical Debt Checklist

Before marking MVP complete:
- [ ] All compiler warnings resolved
- [ ] Code coverage > 70% for Domain and Application layers
- [ ] All TODO comments addressed or tracked as issues
- [ ] Security scan completed (no high/critical vulnerabilities)
- [ ] Performance testing completed (load test with 50 concurrent users)
- [ ] Browser compatibility tested (Chrome, Firefox, Edge)
- [ ] Mobile responsiveness tested (iOS Safari, Android Chrome)
- [ ] Accessibility audit completed (WCAG 2.1 Level A)

## Next Steps After MVP

### Phase 6: Dashboard & Analytics (Future)
- Visual dashboard with charts
- Time tracking trends
- Project profitability analysis
- Budget utilization widgets

### Phase 7: PDF Export (Future)
- Professional PDF generation
- German invoice format
- Custom templates
- Digital signatures

### Phase 8: Advanced Features (Future)
- Approval workflows
- Team management
- Calendar integration
- Mobile app (MAUI)

---

**Document Owner**: Development Team
**Status**: Ready for Implementation
**Estimated Timeline**: 10 weeks for MVP
**Last Updated**: 2025-11-04

