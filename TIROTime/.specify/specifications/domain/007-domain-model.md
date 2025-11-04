# Domain Model & Bounded Contexts
**Specification ID**: 007
**Version**: 2.0
**Status**: Draft
**Parent**: [001 - TiroTime Overview](../001-tirotime-overview.md)

## Overview

This document defines the Domain-Driven Design (DDD) domain model for TiroTime, including bounded contexts, aggregates, entities, value objects, domain events, and domain services. The model follows tactical DDD patterns and maintains the ubiquitous language established in requirements.

**Note**: The Identity & Access Context uses **ASP.NET Core Identity** for user management and authentication. This bounded context integrates Identity framework entities (`ApplicationUser`, `ApplicationRole`) with custom domain logic and value objects.

## Bounded Contexts

TiroTime is organized into four primary bounded contexts:

### 1. Identity & Access Context
**Responsibility**: User authentication, authorization, and profile management

**Technology**: Uses **ASP.NET Core Identity** for authentication infrastructure

**Core Concepts**:
- User accounts and credentials (via `ApplicationUser : IdentityUser<Guid>`)
- Authentication sessions (JWT tokens + Refresh tokens)
- Role-based permissions (via `ApplicationRole : IdentityRole<Guid>`)
- Password management (handled by Identity's `UserManager` and `SignInManager`)
- Email verification (via Identity's token providers)

**Aggregate Roots**:
- ApplicationUser (extends IdentityUser)
- RefreshToken (custom entity for JWT refresh token management)

**Key Operations**:
- Register user (via `UserManager.CreateAsync`)
- Authenticate user (via `SignInManager.PasswordSignInAsync`)
- Reset password (via `UserManager.ResetPasswordAsync`)
- Manage user roles (via `UserManager.AddToRoleAsync`, `UserManager.RemoveFromRoleAsync`)
- Update user profile (custom logic on ApplicationUser)
- Generate and validate JWT tokens (custom service)

---

### 2. Client & Project Context
**Responsibility**: Managing clients, projects, and organizational structure

**Core Concepts**:
- Clients (customers/organizations)
- Projects (work engagements)
- Project teams and assignments
- Hourly rates
- Project budgets

**Aggregate Roots**:
- Client
- Project

**Key Operations**:
- Create/update clients
- Create/update projects
- Assign users to projects
- Track project budgets
- Archive projects

---

### 3. Time Tracking Context
**Responsibility**: Recording and managing time entries

**Core Concepts**:
- Time entries (manual and timer-based)
- Running timers
- Time entry approval
- Billable vs. non-billable time
- Tags and categorization

**Aggregate Roots**:
- TimeEntry
- Timer

**Key Operations**:
- Start/stop timer
- Create manual time entry
- Edit time entry
- Submit for approval
- Approve/reject entries
- Delete entries

---

### 4. Reporting & Invoicing Context
**Responsibility**: Generating reports, exports, and invoices

**Core Concepts**:
- Time reports and summaries
- Export templates
- Excel and PDF generation
- Invoices (Phase 3)
- Report scheduling

**Aggregate Roots**:
- Report
- ExportJob
- Invoice (Phase 3)

**Key Operations**:
- Generate reports
- Export to Excel/PDF
- Create invoice from time entries
- Schedule reports
- Track invoice status

---

## Context Map

```
┌─────────────────────────────────────────────────────────────┐
│                                                               │
│  ┌──────────────────────┐        ┌──────────────────────┐   │
│  │  Identity & Access   │        │  Client & Project    │   │
│  │      Context         │───────▶│      Context         │   │
│  │                      │  Uses  │                      │   │
│  │  - User              │        │  - Client            │   │
│  │  - Role              │        │  - Project           │   │
│  │  - Session           │        │  - ProjectMember     │   │
│  └──────────────────────┘        └──────────────────────┘   │
│            │                                │                │
│            │                                │                │
│            │                                │                │
│            ▼                                ▼                │
│  ┌──────────────────────┐        ┌──────────────────────┐   │
│  │   Time Tracking      │───────▶│   Reporting &        │   │
│  │      Context         │  Feeds │   Invoicing Context  │   │
│  │                      │        │                      │   │
│  │  - TimeEntry         │        │  - Report            │   │
│  │  - Timer             │        │  - ExportJob         │   │
│  │  - ApprovalRequest   │        │  - Invoice           │   │
│  └──────────────────────┘        └──────────────────────┘   │
│                                                               │
└─────────────────────────────────────────────────────────────┘

Relationships:
- Time Tracking depends on Identity & Access (for user information)
- Time Tracking depends on Client & Project (for project details)
- Reporting & Invoicing depends on all contexts (for data aggregation)
```

---

## Detailed Domain Model

## 1. Identity & Access Context

### User Aggregate

**User** (Aggregate Root)

```csharp
public class User : AggregateRoot
{
    // Identity
    public UserId Id { get; private set; }
    public Email Email { get; private set; }
    public PersonName Name { get; private set; }

    // Authentication
    private PasswordHash _passwordHash;
    private List<PasswordHash> _previousPasswordHashes;
    public bool EmailVerified { get; private set; }
    public EmailVerificationToken? VerificationToken { get; private set; }

    // Authorization
    public UserRole Role { get; private set; }
    public UserStatus Status { get; private set; }

    // Security
    public LoginAttemptTracker LoginAttempts { get; private set; }
    public AccountLockStatus LockStatus { get; private set; }

    // Profile
    public ContactInfo? ContactInfo { get; private set; }
    public string? ProfilePictureUrl { get; private set; }
    public UserPreferences Preferences { get; private set; }
    public WorkingHours? DefaultWorkingHours { get; private set; }

    // Audit
    public DateTime CreatedAt { get; private set; }
    public DateTime? LastLoginAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    // Factory Methods
    public static User Create(Email email, PersonName name, PasswordHash passwordHash);
    public static User CreateWithVerification(Email email, PersonName name, PasswordHash passwordHash);

    // Domain Methods - Authentication
    public void VerifyEmail();
    public void UpdatePassword(PasswordHash newPasswordHash);
    public void GeneratePasswordResetToken(out PasswordResetToken token);
    public void ResetPassword(PasswordResetToken token, PasswordHash newPasswordHash);
    public void RecordLoginAttempt(bool successful, IPAddress ipAddress);
    public void UnlockAccount();

    // Domain Methods - Authorization
    public void ChangeRole(UserRole newRole, UserId changedBy);
    public void Activate();
    public void Deactivate();
    public bool HasPermission(Permission permission);

    // Domain Methods - Profile
    public void UpdateProfile(PersonName name, ContactInfo? contactInfo);
    public void UpdatePreferences(UserPreferences preferences);
    public void UpdateWorkingHours(WorkingHours workingHours);
    public void UpdateProfilePicture(string pictureUrl);

    // Domain Invariants
    private void EnsureNotLocked();
    private void EnsureEmailVerified();
    private void EnsurePasswordNotReused(PasswordHash newPasswordHash);
}
```

### Value Objects

```csharp
public record UserId(Guid Value);

public record Email
{
    public string Value { get; init; }

    public Email(string value)
    {
        if (!IsValidEmail(value))
            throw new DomainException("Invalid email format");
        Value = value.ToLowerInvariant();
    }

    private static bool IsValidEmail(string email) => /* validation */;
}

public record PersonName
{
    public string FirstName { get; init; }
    public string LastName { get; init; }
    public string FullName => $"{FirstName} {LastName}";

    public PersonName(string firstName, string lastName)
    {
        if (string.IsNullOrWhiteSpace(firstName) || string.IsNullOrWhiteSpace(lastName))
            throw new DomainException("Name cannot be empty");
        FirstName = firstName.Trim();
        LastName = lastName.Trim();
    }
}

public record PasswordHash
{
    public string Hash { get; init; }
    public string Algorithm { get; init; } // "bcrypt", "argon2"

    public static PasswordHash FromPlainText(string plainText);
    public bool Verify(string plainText);
}

public record UserPreferences
{
    public string LanguageCode { get; init; } // "de", "en"
    public string TimeZone { get; init; } // "Europe/Berlin"
    public string DateFormat { get; init; } // "DD.MM.YYYY"
    public string TimeFormat { get; init; } // "24h", "12h"
    public string CurrencyCode { get; init; } // "EUR"
}

public record WorkingHours
{
    public TimeOnly StartTime { get; init; }
    public TimeOnly EndTime { get; init; }
    public DayOfWeek[] WorkingDays { get; init; }
    public decimal HoursPerDay { get; init; }
    public decimal HoursPerWeek { get; init; }

    public bool IsWorkingDay(DateTime date);
    public bool IsWithinWorkingHours(TimeOnly time);
}

public record ContactInfo
{
    public string? PhoneNumber { get; init; }
    public Address? Address { get; init; }
}

public record LoginAttemptTracker
{
    public int FailedAttempts { get; private set; }
    public DateTime? LastAttemptAt { get; private set; }

    public void RecordSuccess();
    public void RecordFailure();
    public bool ShouldLockAccount() => FailedAttempts >= 5;
}

public record AccountLockStatus
{
    public bool IsLocked { get; init; }
    public DateTime? LockedUntil { get; init; }
    public string? LockReason { get; init; }

    public bool IsCurrentlyLocked() => IsLocked && LockedUntil > DateTime.UtcNow;
}
```

### Enumerations

```csharp
public enum UserRole
{
    Employee = 0,
    Manager = 1,
    Administrator = 2
}

public enum UserStatus
{
    Active = 0,
    Inactive = 1,
    Locked = 2
}

public enum Permission
{
    ManageUsers,
    ManageClients,
    ManageProjects,
    ViewAllTimeEntries,
    ApproveTimeEntries,
    ViewAllReports,
    ManageSystemSettings
}
```

### Domain Events

```csharp
public record UserRegisteredEvent(UserId UserId, Email Email, DateTime OccurredAt) : IDomainEvent;
public record UserEmailVerifiedEvent(UserId UserId, DateTime OccurredAt) : IDomainEvent;
public record UserPasswordChangedEvent(UserId UserId, DateTime OccurredAt) : IDomainEvent;
public record UserRoleChangedEvent(UserId UserId, UserRole OldRole, UserRole NewRole, DateTime OccurredAt) : IDomainEvent;
public record UserAccountLockedEvent(UserId UserId, string Reason, DateTime OccurredAt) : IDomainEvent;
public record UserLoginAttemptedEvent(UserId UserId, bool Successful, IPAddress IpAddress, DateTime OccurredAt) : IDomainEvent;
```

---

## 2. Client & Project Context

### Client Aggregate

**Client** (Aggregate Root)

```csharp
public class Client : AggregateRoot
{
    // Identity
    public ClientId Id { get; private set; }
    public string Name { get; private set; }

    // Details
    public string? CompanyName { get; private set; }
    public ContactInfo? PrimaryContact { get; private set; }
    public Address? Address { get; private set; }
    public Address? BillingAddress { get; private set; }
    public TaxId? TaxId { get; private set; }

    // Settings
    public CurrencyCode Currency { get; private set; }
    public Color Color { get; private set; }
    public string? Notes { get; private set; }
    public ClientStatus Status { get; private set; }

    // Audit
    public UserId CreatedBy { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    // Collections (navigation only, not part of aggregate)
    public IReadOnlyList<ProjectId> ProjectIds { get; private set; }

    // Factory Methods
    public static Client Create(
        string name,
        string? companyName,
        CurrencyCode currency,
        UserId createdBy);

    // Domain Methods
    public void UpdateDetails(string name, string? companyName, ContactInfo? primaryContact);
    public void UpdateAddress(Address address);
    public void UpdateBillingAddress(Address billingAddress);
    public void UpdateTaxId(TaxId taxId);
    public void ChangeColor(Color color);
    public void Archive();
    public void Restore();
    public void AddNotes(string notes);

    // Domain Invariants
    private void EnsureNameNotEmpty();
    private void EnsureStatusAllowsModification();

    // Business Rules
    public bool CanBeDeleted();
}
```

### Project Aggregate

**Project** (Aggregate Root)

```csharp
public class Project : AggregateRoot
{
    // Identity
    public ProjectId Id { get; private set; }
    public ClientId ClientId { get; private set; }
    public string Name { get; private set; }
    public ProjectCode? Code { get; private set; }

    // Details
    public string? Description { get; private set; }
    public Money HourlyRate { get; private set; }
    public ProjectPeriod? Period { get; private set; }
    public ProjectBudget? Budget { get; private set; }

    // Settings
    public bool IsBillableByDefault { get; private set; }
    public ProjectStatus Status { get; private set; }
    public Color Color { get; private set; }

    // Team (part of aggregate)
    private List<ProjectMember> _teamMembers;
    public IReadOnlyList<ProjectMember> TeamMembers => _teamMembers.AsReadOnly();

    // Audit
    public UserId CreatedBy { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }

    // Factory Methods
    public static Project Create(
        ClientId clientId,
        string name,
        Money hourlyRate,
        UserId createdBy);

    // Domain Methods - Project Management
    public void UpdateDetails(string name, string? description, Money hourlyRate);
    public void UpdatePeriod(ProjectPeriod period);
    public void UpdateBudget(ProjectBudget budget);
    public void UpdateCode(ProjectCode code);
    public void ChangeStatus(ProjectStatus newStatus, string? reason);
    public void ChangeColor(Color color);
    public void ToggleBillableByDefault();

    // Domain Methods - Team Management
    public void AddTeamMember(UserId userId, ProjectRole role, Money? customRate = null);
    public void RemoveTeamMember(UserId userId);
    public void UpdateMemberRole(UserId userId, ProjectRole newRole);
    public void UpdateMemberRate(UserId userId, Money customRate);
    public bool IsUserAssigned(UserId userId);
    public ProjectRole GetUserRole(UserId userId);
    public Money GetEffectiveHourlyRate(UserId userId);

    // Business Rules
    public bool CanTrackTime() => Status == ProjectStatus.Active;
    public bool CanBeDeleted();
    public bool IsBudgetExceeded(TimeSpan totalTrackedTime);
    public BudgetUtilization GetBudgetUtilization(TimeSpan totalTrackedTime, Money totalRevenue);

    // Domain Invariants
    private void EnsureAtLeastOneOwner();
    private void EnsureProjectNameUnique();
    private void EnsureHourlyRatePositive();
}
```

**ProjectMember** (Entity within Project aggregate)

```csharp
public class ProjectMember : Entity
{
    public ProjectMemberId Id { get; private set; }
    public UserId UserId { get; private set; }
    public ProjectRole Role { get; private set; }
    public Money? CustomHourlyRate { get; private set; }
    public DateTime JoinedAt { get; private set; }
    public DateTime? LeftAt { get; private set; }

    internal ProjectMember(UserId userId, ProjectRole role, Money? customRate = null);

    internal void ChangeRole(ProjectRole newRole);
    internal void UpdateCustomRate(Money rate);
    internal void MarkAsLeft();

    public bool IsActive() => LeftAt == null;
}
```

### Value Objects

```csharp
public record ClientId(Guid Value);
public record ProjectId(Guid Value);
public record ProjectMemberId(Guid Value);

public record ProjectCode
{
    public string Value { get; init; }

    public ProjectCode(string value)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Length > 20)
            throw new DomainException("Invalid project code");
        Value = value.ToUpperInvariant();
    }
}

public record Money
{
    public decimal Amount { get; init; }
    public CurrencyCode Currency { get; init; }

    public Money(decimal amount, CurrencyCode currency)
    {
        if (amount < 0)
            throw new DomainException("Amount cannot be negative");
        Amount = amount;
        Currency = currency;
    }

    public Money Add(Money other);
    public Money Subtract(Money other);
    public Money Multiply(decimal factor);
    public Money Divide(decimal divisor);
}

public record CurrencyCode
{
    public string Code { get; init; } // ISO 4217: EUR, USD, GBP

    public CurrencyCode(string code)
    {
        if (!IsValidCurrencyCode(code))
            throw new DomainException("Invalid currency code");
        Code = code.ToUpperInvariant();
    }
}

public record Color
{
    public string HexValue { get; init; }

    public Color(string hexValue)
    {
        if (!IsValidHexColor(hexValue))
            throw new DomainException("Invalid hex color");
        HexValue = hexValue.ToUpperInvariant();
    }
}

public record TaxId
{
    public string Value { get; init; }
    public TaxIdType Type { get; init; } // VAT, EIN, etc.
}

public record Address
{
    public string Street { get; init; }
    public string City { get; init; }
    public string PostalCode { get; init; }
    public string? StateProvince { get; init; }
    public string Country { get; init; } // ISO 3166-1 alpha-2
}

public record ProjectPeriod
{
    public DateTime? StartDate { get; init; }
    public DateTime? EndDate { get; init; }

    public ProjectPeriod(DateTime? startDate, DateTime? endDate)
    {
        if (startDate.HasValue && endDate.HasValue && endDate < startDate)
            throw new DomainException("End date must be after start date");
        StartDate = startDate;
        EndDate = endDate;
    }

    public bool IsActive(DateTime date);
    public bool HasEnded(DateTime date);
}

public record ProjectBudget
{
    public BudgetType Type { get; init; }
    public decimal Limit { get; init; }

    public ProjectBudget(BudgetType type, decimal limit)
    {
        if (limit <= 0)
            throw new DomainException("Budget limit must be positive");
        Type = type;
        Limit = limit;
    }
}

public record BudgetUtilization
{
    public decimal CurrentValue { get; init; }
    public decimal Limit { get; init; }
    public decimal PercentageUsed => (CurrentValue / Limit) * 100;
    public bool IsOverBudget => CurrentValue > Limit;
    public bool IsNearLimit => PercentageUsed >= 80;
}
```

### Enumerations

```csharp
public enum ClientStatus
{
    Active = 0,
    Inactive = 1
}

public enum ProjectStatus
{
    Active = 0,
    Paused = 1,
    Completed = 2,
    Archived = 3
}

public enum ProjectRole
{
    Viewer = 0,
    Member = 1,
    Owner = 2
}

public enum BudgetType
{
    Hours = 0,
    Amount = 1
}
```

### Domain Events

```csharp
public record ClientCreatedEvent(ClientId ClientId, string Name, UserId CreatedBy, DateTime OccurredAt) : IDomainEvent;
public record ClientArchivedEvent(ClientId ClientId, DateTime OccurredAt) : IDomainEvent;

public record ProjectCreatedEvent(ProjectId ProjectId, ClientId ClientId, string Name, UserId CreatedBy, DateTime OccurredAt) : IDomainEvent;
public record ProjectStatusChangedEvent(ProjectId ProjectId, ProjectStatus OldStatus, ProjectStatus NewStatus, DateTime OccurredAt) : IDomainEvent;
public record ProjectBudgetExceededEvent(ProjectId ProjectId, BudgetType BudgetType, decimal Limit, decimal Current, DateTime OccurredAt) : IDomainEvent;
public record TeamMemberAddedEvent(ProjectId ProjectId, UserId UserId, ProjectRole Role, DateTime OccurredAt) : IDomainEvent;
public record TeamMemberRemovedEvent(ProjectId ProjectId, UserId UserId, DateTime OccurredAt) : IDomainEvent;
```

---

## 3. Time Tracking Context

### TimeEntry Aggregate

**TimeEntry** (Aggregate Root)

```csharp
public class TimeEntry : AggregateRoot
{
    // Identity
    public TimeEntryId Id { get; private set; }
    public UserId UserId { get; private set; }
    public ProjectId ProjectId { get; private set; }

    // Time Data
    public DateTime Date { get; private set; }
    public TimeRange? TimeRange { get; private set; } // For manual entries with start/end
    public TimeSpan Duration { get; private set; }

    // Details
    public string Description { get; private set; }
    public string? Notes { get; private set; }
    private List<Tag> _tags;
    public IReadOnlyList<Tag> Tags => _tags.AsReadOnly();

    // Classification
    public bool IsBillable { get; private set; }
    public TimeEntrySource Source { get; private set; } // Timer, Manual, Import, etc.

    // Approval
    public ApprovalStatus ApprovalStatus { get; private set; }
    public ApprovalInfo? ApprovalInfo { get; private set; }

    // State
    public TimeEntryStatus Status { get; private set; }
    public bool IsLocked { get; private set; }

    // Audit
    public DateTime CreatedAt { get; private set; }
    public DateTime? UpdatedAt { get; private set; }
    public DateTime? DeletedAt { get; private set; }

    // Factory Methods
    public static TimeEntry CreateFromTimer(
        UserId userId,
        ProjectId projectId,
        DateTime startedAt,
        DateTime stoppedAt,
        string description);

    public static TimeEntry CreateManual(
        UserId userId,
        ProjectId projectId,
        DateTime date,
        TimeRange timeRange,
        string description);

    public static TimeEntry CreateWithDuration(
        UserId userId,
        ProjectId projectId,
        DateTime date,
        TimeSpan duration,
        string description);

    // Domain Methods - Modification
    public void UpdateDescription(string description);
    public void UpdateTimeRange(TimeRange timeRange);
    public void UpdateDuration(TimeSpan duration);
    public void UpdateDate(DateTime date);
    public void UpdateNotes(string? notes);
    public void ToggleBillable();
    public void SetBillable(bool isBillable);

    // Domain Methods - Tags
    public void AddTag(Tag tag);
    public void RemoveTag(Tag tag);
    public void ClearTags();

    // Domain Methods - Approval
    public void SubmitForApproval();
    public void Approve(UserId approverId, string? comment = null);
    public void Reject(UserId approverId, string reason);
    public void Unapprove(UserId unapprovedBy, string reason);

    // Domain Methods - Lifecycle
    public void Delete();
    public void Restore();
    public void Lock(string reason);
    public void Unlock();

    // Business Rules
    public bool CanBeEdited() => !IsLocked && Status == TimeEntryStatus.Active;
    public bool CanBeDeleted() => !IsLocked && ApprovalStatus != ApprovalStatus.Approved;
    public bool CanBeApproved() => ApprovalStatus == ApprovalStatus.Submitted;
    public Money CalculateAmount(Money hourlyRate);

    // Domain Invariants
    private void EnsureNotLocked();
    private void EnsureDateNotInFuture();
    private void EnsureDurationPositive();
    private void EnsureDescriptionNotEmpty();
}
```

### Timer Aggregate

**Timer** (Aggregate Root)

```csharp
public class Timer : AggregateRoot
{
    // Identity
    public TimerId Id { get; private set; }
    public UserId UserId { get; private set; }

    // Timer Data
    public ProjectId ProjectId { get; private set; }
    public string? Description { get; private set; }
    public DateTime StartedAt { get; private set; }
    public DateTime LastSyncedAt { get; private set; }
    public bool IsRunning { get; private set; }

    // Idle Tracking (Phase 3)
    public IdleDetectionStatus? IdleStatus { get; private set; }

    // Calculated Properties
    public TimeSpan CurrentDuration => DateTime.UtcNow - StartedAt;
    public TimeSpan ActiveDuration => CalculateActiveDuration();

    // Factory Methods
    public static Timer Start(UserId userId, ProjectId projectId, string? description = null);

    // Domain Methods
    public void UpdateDescription(string description);
    public void UpdateProject(ProjectId projectId);
    public void Sync(); // Sync with server to prevent data loss
    public void RecordIdleTime(DateTime idleStartedAt);
    public void DiscardIdleTime(DateTime idleStartedAt);
    public TimeEntry Stop(DateTime stoppedAt);

    // Business Rules
    private void EnsureNotExceedingMaximumDuration();
    private TimeSpan CalculateActiveDuration();
}
```

### Value Objects

```csharp
public record TimeEntryId(Guid Value);
public record TimerId(Guid Value);

public record TimeRange
{
    public TimeOnly StartTime { get; init; }
    public TimeOnly EndTime { get; init; }

    public TimeRange(TimeOnly startTime, TimeOnly endTime)
    {
        if (endTime <= startTime)
            throw new DomainException("End time must be after start time");
        StartTime = startTime;
        EndTime = endTime;
    }

    public TimeSpan Duration => EndTime - StartTime;

    public bool OverlapsWith(TimeRange other);
}

public record Tag
{
    public string Name { get; init; }
    public Color? Color { get; init; }

    public Tag(string name, Color? color = null)
    {
        if (string.IsNullOrWhiteSpace(name) || name.Length > 30)
            throw new DomainException("Invalid tag name");
        Name = name.Trim();
        Color = color;
    }
}

public record ApprovalInfo
{
    public UserId ApprovedBy { get; init; }
    public DateTime ApprovedAt { get; init; }
    public string? Comment { get; init; }
}

public record IdleDetectionStatus
{
    public DateTime IdleStartedAt { get; init; }
    public TimeSpan IdleDuration { get; init; }
    public bool IsResolved { get; init; }
}
```

### Enumerations

```csharp
public enum TimeEntryStatus
{
    Active = 0,
    Deleted = 1
}

public enum TimeEntrySource
{
    Timer = 0,
    Manual = 1,
    Import = 2,
    Recurring = 3,
    CalendarSync = 4
}

public enum ApprovalStatus
{
    NotSubmitted = 0,
    Submitted = 1,
    Approved = 2,
    Rejected = 3
}
```

### Domain Events

```csharp
public record TimerStartedEvent(TimerId TimerId, UserId UserId, ProjectId ProjectId, DateTime StartedAt, DateTime OccurredAt) : IDomainEvent;
public record TimerStoppedEvent(TimerId TimerId, TimeEntryId TimeEntryId, TimeSpan Duration, DateTime OccurredAt) : IDomainEvent;

public record TimeEntryCreatedEvent(TimeEntryId TimeEntryId, UserId UserId, ProjectId ProjectId, TimeSpan Duration, DateTime OccurredAt) : IDomainEvent;
public record TimeEntryUpdatedEvent(TimeEntryId TimeEntryId, DateTime OccurredAt) : IDomainEvent;
public record TimeEntryDeletedEvent(TimeEntryId TimeEntryId, UserId UserId, DateTime OccurredAt) : IDomainEvent;

public record TimeEntrySubmittedForApprovalEvent(TimeEntryId TimeEntryId, UserId UserId, DateTime OccurredAt) : IDomainEvent;
public record TimeEntryApprovedEvent(TimeEntryId TimeEntryId, UserId ApprovedBy, DateTime OccurredAt) : IDomainEvent;
public record TimeEntryRejectedEvent(TimeEntryId TimeEntryId, UserId RejectedBy, string Reason, DateTime OccurredAt) : IDomainEvent;
```

---

## 4. Reporting & Invoicing Context

### Report

```csharp
public class Report
{
    public ReportId Id { get; private set; }
    public UserId UserId { get; private set; }
    public string Name { get; private set; }
    public ReportConfiguration Configuration { get; private set; }
    public ReportResult? Result { get; private set; }
    public DateTime CreatedAt { get; private set; }

    public static Report Create(UserId userId, string name, ReportConfiguration configuration);
    public void Execute(); // Generate report
}

public record ReportConfiguration
{
    public ReportType Type { get; init; }
    public ReportFilters Filters { get; init; }
    public ReportGrouping Grouping { get; init; }
    public List<string> Columns { get; init; }
}
```

### ExportJob Aggregate

```csharp
public class ExportJob : AggregateRoot
{
    public ExportJobId Id { get; private set; }
    public UserId UserId { get; private set; }
    public ExportFormat Format { get; private set; }
    public ReportConfiguration Configuration { get; private set; }
    public ExportStatus Status { get; private set; }
    public string? FileUrl { get; private set; }
    public long? FileSizeBytes { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }
    public DateTime ExpiresAt { get; private set; }
    public string? ErrorMessage { get; private set; }
    public int DownloadCount { get; private set; }

    public static ExportJob Create(UserId userId, ExportFormat format, ReportConfiguration configuration);

    public void MarkAsProcessing();
    public void MarkAsCompleted(string fileUrl, long fileSize);
    public void MarkAsFailed(string errorMessage);
    public void IncrementDownloadCount();
    public bool IsExpired() => DateTime.UtcNow > ExpiresAt;
}
```

### Invoice Aggregate (Phase 3)

```csharp
public class Invoice : AggregateRoot
{
    public InvoiceId Id { get; private set; }
    public InvoiceNumber Number { get; private set; }
    public ClientId ClientId { get; private set; }
    public UserId IssuedBy { get; private set; }

    public DateTime InvoiceDate { get; private set; }
    public DateTime DueDate { get; private set; }
    public DateRange ServicePeriod { get; private set; }

    private List<InvoiceLineItem> _lineItems;
    public IReadOnlyList<InvoiceLineItem> LineItems => _lineItems.AsReadOnly();

    public Money Subtotal { get; private set; }
    public TaxAmount Tax { get; private set; }
    public Money Total { get; private set; }

    public InvoiceStatus Status { get; private set; }
    public PaymentInfo? PaymentInfo { get; private set; }

    public DateTime CreatedAt { get; private set; }

    public static Invoice CreateFromTimeEntries(
        ClientId clientId,
        UserId issuedBy,
        List<TimeEntryId> timeEntryIds,
        DateTime invoiceDate,
        DateTime dueDate);

    public void AddLineItem(string description, decimal quantity, Money unitPrice);
    public void Finalize(); // Locks invoice and time entries
    public void MarkAsPaid(DateTime paidDate, string? reference);
    public void Cancel(string reason);
}
```

---

## Domain Services

### TimeEntryValidationService

```csharp
public class TimeEntryValidationService
{
    public ValidationResult ValidateTimeEntry(TimeEntry timeEntry, List<TimeEntry> existingEntries);
    public bool HasOverlap(TimeEntry entry, List<TimeEntry> existingEntries);
    public bool ExceedsMaximumDuration(TimeEntry entry);
}
```

### ProjectBudgetService

```csharp
public class ProjectBudgetService
{
    public BudgetUtilization CalculateUtilization(Project project, List<TimeEntry> timeEntries);
    public bool IsBudgetExceeded(Project project, List<TimeEntry> timeEntries);
    public void CheckBudgetThresholds(Project project, List<TimeEntry> timeEntries);
}
```

### ReportGenerationService

```csharp
public class ReportGenerationService
{
    public ReportResult GenerateReport(ReportConfiguration configuration);
    public Task<string> ExportToExcel(ReportResult report, ExportTemplate template);
    public Task<string> ExportToPdf(ReportResult report, ExportTemplate template);
}
```

---

## Repository Interfaces

```csharp
// Identity Context
public interface IUserRepository
{
    Task<User?> GetByIdAsync(UserId id);
    Task<User?> GetByEmailAsync(Email email);
    Task AddAsync(User user);
    Task UpdateAsync(User user);
    Task<bool> IsEmailUniqueAsync(Email email);
}

// Client & Project Context
public interface IClientRepository
{
    Task<Client?> GetByIdAsync(ClientId id);
    Task<List<Client>> GetAllActiveAsync();
    Task AddAsync(Client client);
    Task UpdateAsync(Client client);
    Task<bool> IsNameUniqueAsync(string name);
}

public interface IProjectRepository
{
    Task<Project?> GetByIdAsync(ProjectId id);
    Task<List<Project>> GetActiveProjectsAsync();
    Task<List<Project>> GetByClientIdAsync(ClientId clientId);
    Task AddAsync(Project project);
    Task UpdateAsync(Project project);
    Task<bool> IsNameUniqueForClientAsync(string name, ClientId clientId);
}

// Time Tracking Context
public interface ITimeEntryRepository
{
    Task<TimeEntry?> GetByIdAsync(TimeEntryId id);
    Task<List<TimeEntry>> GetByUserAndDateRangeAsync(UserId userId, DateTime startDate, DateTime endDate);
    Task<List<TimeEntry>> GetByProjectIdAsync(ProjectId projectId);
    Task AddAsync(TimeEntry timeEntry);
    Task UpdateAsync(TimeEntry timeEntry);
    Task DeleteAsync(TimeEntry timeEntry);
}

public interface ITimerRepository
{
    Task<Timer?> GetRunningTimerForUserAsync(UserId userId);
    Task AddAsync(Timer timer);
    Task UpdateAsync(Timer timer);
    Task DeleteAsync(Timer timer);
}

// Reporting Context
public interface IExportJobRepository
{
    Task<ExportJob?> GetByIdAsync(ExportJobId id);
    Task<List<ExportJob>> GetByUserIdAsync(UserId userId);
    Task AddAsync(ExportJob job);
    Task UpdateAsync(ExportJob job);
}
```

---

## Ubiquitous Language Dictionary

| Term | Definition |
|------|------------|
| **Time Entry** | A record of time spent on a project task |
| **Timer** | A running stopwatch tracking time in real-time |
| **Billable** | Time that can be invoiced to a client |
| **Non-Billable** | Time not charged to clients (internal, overhead) |
| **Hourly Rate** | Price per hour for project work (Stundensatz) |
| **Project** | A work engagement for a client |
| **Client** | A customer or organization |
| **Aggregate** | A cluster of domain objects treated as a unit |
| **Aggregate Root** | The entry point to an aggregate |
| **Value Object** | An immutable object without identity |
| **Domain Event** | Something that happened in the domain |
| **Bounded Context** | A boundary within which a model is defined |
| **Approval** | Manager review and acceptance of time entries |
| **Timesheet** | Collection of time entries for a period |
| **Export** | Generation of Excel or PDF report |
| **Budget** | Maximum hours or amount for a project |
| **Utilization** | Percentage of budget consumed |

---

**Document Owner**: Development Team
**Reviewers**: Architecture Team, Domain Experts
**Approval Status**: Pending
