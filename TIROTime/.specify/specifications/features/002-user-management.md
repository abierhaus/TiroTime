# Feature Specification: User Management
**Specification ID**: 002
**Version**: 2.0
**Status**: Draft
**Parent**: [001 - TiroTime Overview](../001-tirotime-overview.md)

## Overview

User Management provides authentication, authorization, and user profile management capabilities for the TiroTime application. This module ensures secure access control and manages user identity throughout the system.

**Technology**: This module uses **ASP.NET Core Identity** for user authentication and authorization, providing enterprise-grade security features including password hashing, account lockout, email confirmation, and token management out of the box.

## User Roles

### Administrator
- Full system access
- Manage all users, clients, and projects
- View all time entries
- Configure system settings
- Export any data

### Manager
- Manage assigned projects and clients
- View team time entries
- Approve/reject time entries
- Export team reports
- Cannot modify system settings

### Employee
- Manage own time entries
- View assigned projects
- View own reports
- Cannot manage users or system settings

## Functional Requirements

### FR-UM-001: User Registration
**Priority**: High
**Phase**: MVP

Users can register for a new account with the following information:
- Email address (unique, validated)
- Full name (first and last name)
- Password (minimum 12 characters, complexity requirements)
- Terms of service acceptance

**Business Rules**:
- Email must be unique in the system
- Password must meet security requirements:
  - Minimum 12 characters
  - At least one uppercase letter
  - At least one lowercase letter
  - At least one number
  - At least one special character
- Email verification required before full access
- First registered user becomes Administrator
- Subsequent users default to Employee role

**Acceptance Criteria**:
- Given a valid email and password, when user registers, then account is created
- Given an existing email, when user registers, then error message is displayed
- Given weak password, when user registers, then validation error is shown
- Given successful registration, when account created, then verification email is sent

### FR-UM-002: User Login
**Priority**: High
**Phase**: MVP

Users can log in with email and password.

**Authentication Flow**:
1. User enters email and password
2. System validates credentials
3. System checks email verification status
4. System generates JWT access token (15 min expiry)
5. System generates JWT refresh token (7 days expiry)
6. System returns tokens and user profile

**Business Rules**:
- Lock account after 5 failed login attempts (15 minutes)
- Email must be verified to log in
- Session expires after 15 minutes of inactivity
- Support "Remember Me" option (30 days)

**Acceptance Criteria**:
- Given valid credentials, when user logs in, then access is granted
- Given invalid credentials, when user logs in, then error is displayed
- Given 5 failed attempts, when user tries again, then account is locked
- Given unverified email, when user logs in, then verification prompt is shown

### FR-UM-003: Email Verification
**Priority**: High
**Phase**: MVP

Users must verify email address before full system access.

**Verification Flow**:
1. System sends email with verification link
2. Link contains time-limited token (24 hours)
3. User clicks link
4. System validates token
5. System marks email as verified
6. User is redirected to login or dashboard

**Business Rules**:
- Verification link expires after 24 hours
- User can request new verification email
- Maximum 3 verification emails per hour

### FR-UM-004: Password Reset
**Priority**: High
**Phase**: MVP

Users can reset forgotten passwords.

**Reset Flow**:
1. User requests password reset with email
2. System sends reset link (token valid 1 hour)
3. User clicks link and enters new password
4. System validates token and updates password
5. All existing sessions are invalidated
6. User logs in with new password

**Business Rules**:
- Reset token expires after 1 hour
- Reset token is single-use
- Old password cannot be reused (last 5 passwords)
- Maximum 3 reset requests per hour

### FR-UM-005: User Profile Management
**Priority**: Medium
**Phase**: MVP

Users can view and update their profile information.

**Editable Fields**:
- First name, last name
- Email (requires re-verification)
- Phone number
- Profile picture
- Language preference
- Timezone
- Date/time format preference
- Working hours (start/end time)
- Weekly working days

**Business Rules**:
- Email change requires verification of new email
- Profile picture max size: 5MB
- Supported image formats: JPG, PNG, WebP

### FR-UM-006: Password Change
**Priority**: Medium
**Phase**: MVP

Logged-in users can change their password.

**Change Flow**:
1. User enters current password
2. User enters new password (twice)
3. System validates current password
4. System validates new password requirements
5. System updates password
6. System sends confirmation email

**Business Rules**:
- Must provide current password
- New password must meet security requirements
- Cannot reuse last 5 passwords
- All other sessions are invalidated (optional)

### FR-UM-007: User Management (Admin)
**Priority**: High
**Phase**: MVP

Administrators can manage all users.

**Admin Capabilities**:
- View all users (list and details)
- Create new users
- Edit user information
- Assign/change user roles
- Activate/deactivate users
- Reset user passwords
- View user activity logs

**Business Rules**:
- Cannot delete users (data integrity)
- Can deactivate instead of delete
- Deactivated users cannot log in
- At least one Administrator must exist
- Administrators cannot demote themselves

### FR-UM-008: Role-Based Access Control
**Priority**: High
**Phase**: MVP

System enforces role-based permissions.

**Permission Matrix**:

| Action | Admin | Manager | Employee |
|--------|-------|---------|----------|
| Manage all users | ✓ | ✗ | ✗ |
| View all time entries | ✓ | Team only | Own only |
| Manage all clients | ✓ | Assigned | ✗ |
| Manage all projects | ✓ | Assigned | ✗ |
| Edit own time entries | ✓ | ✓ | ✓ |
| Delete time entries | ✓ | Team only | Own only |
| Approve time entries | ✓ | ✓ | ✗ |
| Export all reports | ✓ | Team only | Own only |
| System settings | ✓ | ✗ | ✗ |

### FR-UM-009: Session Management
**Priority**: High
**Phase**: MVP

Secure session handling with JWT tokens.

**Session Features**:
- Access token expiry: 15 minutes
- Refresh token expiry: 7 days
- Automatic token refresh
- Logout functionality (token invalidation)
- "Logout all devices" functionality

**Business Rules**:
- Tokens stored securely (HTTP-only cookies or localStorage)
- Refresh tokens must be rotated on use
- Revoked tokens stored in blacklist (Redis)
- Inactive sessions cleaned up after 7 days

### FR-UM-010: Audit Logging
**Priority**: Medium
**Phase**: Phase 2

Track security-relevant user actions.

**Logged Events**:
- Login attempts (success/failure)
- Password changes
- Password resets
- Role changes
- User activation/deactivation
- Failed authorization attempts
- Email changes

**Log Information**:
- Timestamp
- User ID
- Action type
- IP address
- User agent
- Result (success/failure)

## API Endpoints

### Authentication Endpoints

```
POST /api/v1/auth/register
POST /api/v1/auth/login
POST /api/v1/auth/logout
POST /api/v1/auth/refresh-token
POST /api/v1/auth/verify-email
POST /api/v1/auth/resend-verification
POST /api/v1/auth/forgot-password
POST /api/v1/auth/reset-password
```

### User Management Endpoints

```
GET    /api/v1/users              # List users (admin/manager)
GET    /api/v1/users/{id}         # Get user details
GET    /api/v1/users/me           # Get current user profile
POST   /api/v1/users              # Create user (admin)
PUT    /api/v1/users/{id}         # Update user (admin)
PUT    /api/v1/users/me           # Update own profile
PATCH  /api/v1/users/{id}/role    # Change user role (admin)
PATCH  /api/v1/users/{id}/status  # Activate/deactivate (admin)
PUT    /api/v1/users/me/password  # Change own password
GET    /api/v1/users/{id}/audit   # Get user audit log (admin)
```

## Data Model

### ASP.NET Core Identity Integration

This application uses **ASP.NET Core Identity** with custom extensions for domain-specific requirements.

### ApplicationUser Entity

Extends `IdentityUser<Guid>` with custom properties:

```csharp
public class ApplicationUser : IdentityUser<Guid>
{
    // Base Identity properties (inherited):
    // - Id (Guid)
    // - UserName
    // - Email
    // - EmailConfirmed
    // - PasswordHash
    // - SecurityStamp
    // - PhoneNumber
    // - PhoneNumberConfirmed
    // - TwoFactorEnabled
    // - LockoutEnd (DateTimeOffset?)
    // - LockoutEnabled
    // - AccessFailedCount

    // Custom TiroTime properties
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string? ProfilePictureUrl { get; set; }
    public string LanguageCode { get; set; } = "de"; // Default German
    public string TimeZone { get; set; } = "Europe/Berlin";
    public string DateFormat { get; set; } = "DD.MM.YYYY";
    public string TimeFormat { get; set; } = "24h";
    public string CurrencyCode { get; set; } = "EUR";

    // Working Hours (stored as JSON)
    public string? WorkingHoursJson { get; set; }

    // Status
    public UserStatus Status { get; set; } = UserStatus.Active;

    // Audit
    public DateTime CreatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
    public DateTime? UpdatedAt { get; set; }

    // Navigation properties
    public ICollection<IdentityUserRole<Guid>> UserRoles { get; set; } = new List<IdentityUserRole<Guid>>();
    public ICollection<IdentityUserClaim<Guid>> Claims { get; set; } = new List<IdentityUserClaim<Guid>>();

    // Computed properties
    public string FullName => $"{FirstName} {LastName}";

    // Helper methods
    public WorkingHours? GetWorkingHours()
    {
        if (string.IsNullOrEmpty(WorkingHoursJson))
            return null;
        return JsonSerializer.Deserialize<WorkingHours>(WorkingHoursJson);
    }

    public void SetWorkingHours(WorkingHours? workingHours)
    {
        WorkingHoursJson = workingHours != null
            ? JsonSerializer.Serialize(workingHours)
            : null;
    }
}
```

### ApplicationRole Entity

Extends `IdentityRole<Guid>` for custom roles:

```csharp
public class ApplicationRole : IdentityRole<Guid>
{
    // Base Identity properties (inherited):
    // - Id (Guid)
    // - Name
    // - NormalizedName
    // - ConcurrencyStamp

    public string? Description { get; set; }
    public DateTime CreatedAt { get; set; }

    // Predefined role names
    public const string Administrator = "Administrator";
    public const string Manager = "Manager";
    public const string Employee = "Employee";
}
```

### RefreshToken Entity

For JWT refresh token management:

```csharp
public class RefreshToken
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string Token { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? RevokedAt { get; set; }
    public string? RevokedByIp { get; set; }
    public string? ReplacedByToken { get; set; }
    public string CreatedByIp { get; set; } = string.Empty;

    // Navigation
    public ApplicationUser User { get; set; } = null!;

    // Helper properties
    public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
    public bool IsRevoked => RevokedAt != null;
    public bool IsActive => !IsRevoked && !IsExpired;
}
```

### Enumerations

```csharp
public enum UserStatus
{
    Active = 0,
    Inactive = 1,
    Suspended = 2
}
```

### Value Objects

```csharp
public record WorkingHours
{
    public TimeOnly StartTime { get; init; }
    public TimeOnly EndTime { get; init; }
    public DayOfWeek[] WorkingDays { get; init; }
    public decimal HoursPerDay { get; init; }
    public decimal HoursPerWeek { get; init; }

    public bool IsWorkingDay(DateTime date) => WorkingDays.Contains(date.DayOfWeek);
    public bool IsWithinWorkingHours(TimeOnly time) => time >= StartTime && time <= EndTime;
}
```

### Identity Configuration

```csharp
public static class IdentityConfiguration
{
    public static IServiceCollection AddTiroTimeIdentity(this IServiceCollection services)
    {
        services.AddIdentity<ApplicationUser, ApplicationRole>(options =>
        {
            // Password settings
            options.Password.RequireDigit = true;
            options.Password.RequireLowercase = true;
            options.Password.RequireUppercase = true;
            options.Password.RequireNonAlphanumeric = true;
            options.Password.RequiredLength = 12;
            options.Password.RequiredUniqueChars = 4;

            // Lockout settings
            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(15);
            options.Lockout.MaxFailedAccessAttempts = 5;
            options.Lockout.AllowedForNewUsers = true;

            // User settings
            options.User.RequireUniqueEmail = true;
            options.User.AllowedUserNameCharacters =
                "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789-._@+";

            // Sign-in settings
            options.SignIn.RequireConfirmedEmail = true;
            options.SignIn.RequireConfirmedPhoneNumber = false;

            // Token settings
            options.Tokens.EmailConfirmationTokenProvider = TokenOptions.DefaultEmailProvider;
            options.Tokens.PasswordResetTokenProvider = TokenOptions.DefaultEmailProvider;
        })
        .AddEntityFrameworkStores<TiroTimeDbContext>()
        .AddDefaultTokenProviders()
        .AddTokenProvider<DataProtectorTokenProvider<ApplicationUser>>("TiroTime");

        // Configure token lifetimes
        services.Configure<DataProtectionTokenProviderOptions>(options =>
        {
            options.TokenLifespan = TimeSpan.FromHours(24); // Email confirmation
        });

        services.Configure<PasswordHasherOptions>(options =>
        {
            // Use PBKDF2 with HMAC-SHA512, 100,000 iterations
            options.IterationCount = 100_000;
        });

        return services;
    }
}
```

## Security Requirements

### SEC-UM-001: Password Security (ASP.NET Core Identity)
- **Password Hashing**: ASP.NET Core Identity uses PBKDF2 with HMAC-SHA512, 100,000 iterations by default
- **Salt**: Automatic unique salt per password
- **Minimum length**: 12 characters (configured in IdentityOptions)
- **Complexity**: Requires uppercase, lowercase, digit, and special character
- **Password History**: Implement custom validation to prevent reusing last 5 passwords
- **Never log or transmit**: Passwords never logged or transmitted in plain text

**Implementation**:
```csharp
// ASP.NET Core Identity handles hashing automatically
var result = await _userManager.CreateAsync(user, password);

// For password change with history validation
public class PasswordHistoryValidator<TUser> : IPasswordValidator<TUser>
    where TUser : ApplicationUser
{
    public async Task<IdentityResult> ValidateAsync(
        UserManager<TUser> manager,
        TUser user,
        string password)
    {
        // Check against previous password hashes
        var previousHashes = await GetLastPasswordHashes(user.Id, 5);
        var passwordHasher = manager.PasswordHasher;

        foreach (var oldHash in previousHashes)
        {
            if (passwordHasher.VerifyHashedPassword(user, oldHash, password)
                == PasswordVerificationResult.Success)
            {
                return IdentityResult.Failed(new IdentityError
                {
                    Description = "Cannot reuse recent passwords"
                });
            }
        }

        return IdentityResult.Success;
    }
}
```

### SEC-UM-002: Token Security
- **JWT Access Tokens**: Short-lived (15 minutes), signed with HMAC-SHA256
- **Refresh Tokens**: Stored in database, long-lived (7 days), rotated on use
- **Token Signing**: Use strong secret key (256-bit minimum) or RS256 for production
- **Token Claims**: Include user ID, email, roles, and expiration
- **Refresh Token Rotation**: Each refresh invalidates old token and issues new one
- **Token Revocation**: Revoked tokens stored in database, checked on refresh
- **Blacklist**: Use Redis for fast token blacklist lookup (optional optimization)

**JWT Configuration**:
```csharp
services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = configuration["Jwt:Issuer"],
        ValidAudience = configuration["Jwt:Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(configuration["Jwt:SecretKey"])),
        ClockSkew = TimeSpan.Zero
    };
});
```

### SEC-UM-003: Account Lockout (ASP.NET Core Identity)
- **Lockout Duration**: 15 minutes after max failed attempts
- **Max Failed Attempts**: 5 attempts (configured in IdentityOptions)
- **Automatic Unlocking**: Account automatically unlocks after lockout period
- **Manual Unlock**: Admins can manually unlock accounts
- **Lockout Tracking**: ASP.NET Core Identity tracks `AccessFailedCount` and `LockoutEnd`

**Implementation**: Built into Identity via `UserManager`:
```csharp
// Handled automatically by SignInManager
var result = await _signInManager.PasswordSignInAsync(
    email,
    password,
    isPersistent: false,
    lockoutOnFailure: true); // Enables lockout

// Check lockout status
if (result.IsLockedOut)
{
    return BadRequest("Account is locked. Try again in 15 minutes.");
}

// Manual unlock by admin
await _userManager.SetLockoutEndDateAsync(user, null);
```

### SEC-UM-004: Email Confirmation (ASP.NET Core Identity)
- **Email Tokens**: ASP.NET Core Identity generates secure, time-limited tokens
- **Token Lifetime**: 24 hours (configurable via DataProtectionTokenProviderOptions)
- **Token Format**: Encrypted and tamper-proof
- **Single Use**: Tokens are validated and invalidated after use
- **Rate Limiting**: Maximum 3 verification emails per hour (custom implementation)

**Implementation**:
```csharp
// Generate token
var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);

// Send email with confirmation link
var confirmationLink = $"{baseUrl}/auth/verify-email?userId={user.Id}&token={WebUtility.UrlEncode(token)}";

// Verify token
var result = await _userManager.ConfirmEmailAsync(user, token);
```

### SEC-UM-005: Rate Limiting
- **Login Attempts**: 5 per 15 minutes per IP (via Identity lockout + custom IP tracking)
- **Registration**: 3 per hour per IP (custom middleware)
- **Password Reset**: 3 per hour per email (custom service)
- **Email Verification**: 3 per hour per email (custom service)

**Implementation** (using AspNetCoreRateLimit or custom middleware):
```csharp
services.AddMemoryCache();
services.Configure<IpRateLimitOptions>(configuration.GetSection("IpRateLimiting"));
services.AddSingleton<IRateLimitConfiguration, RateLimitConfiguration>();
services.AddInMemoryRateLimiting();
```

### SEC-UM-006: Data Protection
- **PII Encryption**: Sensitive data encrypted at rest using ASP.NET Core Data Protection
- **Email Storage**: Store email in both hashed (for uniqueness) and encrypted forms
- **Phone Numbers**: Encrypted if stored
- **Log Masking**: Never log passwords, tokens, or full email addresses
- **GDPR Compliance**:
  - Right to be forgotten (soft delete + data anonymization)
  - Data export functionality
  - Consent tracking
- **Token Security**: Password reset and email confirmation tokens cryptographically secure

**Data Protection Configuration**:
```csharp
services.AddDataProtection()
    .PersistKeysToDbContext<TiroTimeDbContext>()
    .SetApplicationName("TiroTime");

// Encrypt sensitive fields
public class EncryptedFieldConverter : ValueConverter<string, string>
{
    public EncryptedFieldConverter(IDataProtector protector)
        : base(
            v => protector.Protect(v),
            v => protector.Unprotect(v))
    {
    }
}
```

## Validation Rules

### Email Validation
- Valid email format (RFC 5322)
- Maximum length: 254 characters
- Case-insensitive uniqueness check
- Block disposable email domains (optional)

### Password Validation
- Minimum length: 12 characters
- Maximum length: 128 characters
- At least one uppercase letter
- At least one lowercase letter
- At least one digit
- At least one special character
- Not in common password list (optional)

### Name Validation
- First name: 1-50 characters
- Last name: 1-50 characters
- Allow Unicode characters (international names)
- Trim whitespace

## Error Handling

### Common Errors

| Error Code | HTTP Status | Message |
|------------|-------------|---------|
| USER_001 | 400 | Invalid email format |
| USER_002 | 400 | Email already exists |
| USER_003 | 400 | Password does not meet requirements |
| USER_004 | 401 | Invalid credentials |
| USER_005 | 401 | Email not verified |
| USER_006 | 403 | Account is locked |
| USER_007 | 404 | User not found |
| USER_008 | 400 | Invalid or expired token |
| USER_009 | 429 | Too many requests, please try again later |
| USER_010 | 403 | Insufficient permissions |

## Testing Requirements

### Unit Tests
- Password strength validation (custom validators)
- Email validation
- WorkingHours value object logic
- Role-based permission checks
- Token generation and validation logic
- Refresh token rotation logic

### Integration Tests
- **ASP.NET Core Identity Integration**:
  - User registration with Identity
  - Login with SignInManager
  - Email confirmation flow
  - Password reset flow
  - Account lockout behavior
  - Role assignment and retrieval
- **JWT Token Flow**:
  - Access token generation
  - Refresh token generation and rotation
  - Token validation and expiration
  - Token revocation
- **Database Operations**:
  - Identity tables CRUD operations
  - RefreshToken repository operations

### E2E Tests
- Complete registration and login journey (with Identity)
- Password reset journey (with email tokens)
- Profile update journey
- Admin user management journey
- Multi-device logout (token revocation)

## Performance Requirements

- Login: < 500ms (95th percentile)
- Token refresh: < 200ms
- User profile retrieval: < 100ms
- User list (paginated): < 300ms
- Concurrent logins: 100 per second

## Dependencies

### NuGet Packages

```xml
<!-- ASP.NET Core Identity -->
<PackageReference Include="Microsoft.AspNetCore.Identity.EntityFrameworkCore" Version="8.0.*" />

<!-- JWT Authentication -->
<PackageReference Include="Microsoft.AspNetCore.Authentication.JwtBearer" Version="8.0.*" />
<PackageReference Include="System.IdentityModel.Tokens.Jwt" Version="7.0.*" />

<!-- Entity Framework Core -->
<PackageReference Include="Microsoft.EntityFrameworkCore" Version="8.0.*" />
<PackageReference Include="Microsoft.EntityFrameworkCore.SqlServer" Version="8.0.*" />
<!-- OR -->
<PackageReference Include="Npgsql.EntityFrameworkCore.PostgreSQL" Version="8.0.*" />

<!-- Validation & CQRS -->
<PackageReference Include="FluentValidation.AspNetCore" Version="11.3.*" />
<PackageReference Include="MediatR" Version="12.0.*" />

<!-- Rate Limiting (Optional) -->
<PackageReference Include="AspNetCoreRateLimit" Version="5.0.*" />

<!-- Data Protection (for encryption) -->
<PackageReference Include="Microsoft.AspNetCore.DataProtection.EntityFrameworkCore" Version="8.0.*" />
```

### Database Tables Created by Identity

ASP.NET Core Identity creates the following tables:

- **AspNetUsers**: User accounts
- **AspNetRoles**: Application roles
- **AspNetUserRoles**: User-role assignments (many-to-many)
- **AspNetUserClaims**: User claims
- **AspNetUserLogins**: External login providers (OAuth)
- **AspNetUserTokens**: Authentication tokens
- **AspNetRoleClaims**: Role-based claims
- **DataProtectionKeys**: Encryption keys for Data Protection API

### Additional Custom Tables

- **RefreshTokens**: JWT refresh token storage and tracking

## Implementation Guide

### 1. Setup Identity in Program.cs

```csharp
// Program.cs
var builder = WebApplication.CreateBuilder(args);

// Add DbContext
builder.Services.AddDbContext<TiroTimeDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add ASP.NET Core Identity
builder.Services.AddTiroTimeIdentity(); // Extension method from IdentityConfiguration

// Add JWT Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(/* JWT config */);

// Add Authorization
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAdminRole",
        policy => policy.RequireRole(ApplicationRole.Administrator));
    options.AddPolicy("RequireManagerOrAdmin",
        policy => policy.RequireRole(ApplicationRole.Manager, ApplicationRole.Administrator));
});

var app = builder.Build();

// Seed default roles and admin user
await SeedDatabase(app);

app.UseAuthentication();
app.UseAuthorization();

app.Run();
```

### 2. Database Context Configuration

```csharp
public class TiroTimeDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, Guid>
{
    public TiroTimeDbContext(DbContextOptions<TiroTimeDbContext> options)
        : base(options)
    {
    }

    // Custom entities
    public DbSet<RefreshToken> RefreshTokens { get; set; }
    public DbSet<Client> Clients { get; set; }
    public DbSet<Project> Projects { get; set; }
    public DbSet<TimeEntry> TimeEntries { get; set; }

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder); // Must call to configure Identity tables

        // Configure ApplicationUser
        builder.Entity<ApplicationUser>(entity =>
        {
            entity.Property(e => e.FirstName).HasMaxLength(50).IsRequired();
            entity.Property(e => e.LastName).HasMaxLength(50).IsRequired();
            entity.Property(e => e.LanguageCode).HasMaxLength(10).IsRequired();
            entity.Property(e => e.TimeZone).HasMaxLength(50).IsRequired();
            entity.Property(e => e.CurrencyCode).HasMaxLength(3).IsRequired();
        });

        // Configure RefreshToken
        builder.Entity<RefreshToken>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Token).IsUnique();
            entity.HasOne(e => e.User)
                .WithMany()
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Apply other configurations...
    }
}
```

### 3. Authentication Service Implementation

```csharp
public interface IAuthenticationService
{
    Task<Result<AuthenticationResponse>> RegisterAsync(RegisterRequest request);
    Task<Result<AuthenticationResponse>> LoginAsync(LoginRequest request);
    Task<Result<AuthenticationResponse>> RefreshTokenAsync(string refreshToken);
    Task<Result> RevokeTokenAsync(string refreshToken);
    Task<Result> ConfirmEmailAsync(Guid userId, string token);
    Task<Result> ForgotPasswordAsync(string email);
    Task<Result> ResetPasswordAsync(ResetPasswordRequest request);
}

public class AuthenticationService : IAuthenticationService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly SignInManager<ApplicationUser> _signInManager;
    private readonly IJwtTokenGenerator _jwtTokenGenerator;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IEmailService _emailService;

    public async Task<Result<AuthenticationResponse>> RegisterAsync(RegisterRequest request)
    {
        // Check if user exists
        var existingUser = await _userManager.FindByEmailAsync(request.Email);
        if (existingUser != null)
            return Result<AuthenticationResponse>.Failure("Email already registered");

        // Create user with Identity
        var user = new ApplicationUser
        {
            UserName = request.Email,
            Email = request.Email,
            FirstName = request.FirstName,
            LastName = request.LastName,
            CreatedAt = DateTime.UtcNow
        };

        var result = await _userManager.CreateAsync(user, request.Password);
        if (!result.Succeeded)
            return Result<AuthenticationResponse>.Failure(
                string.Join(", ", result.Errors.Select(e => e.Description)));

        // Assign default role
        await _userManager.AddToRoleAsync(user, ApplicationRole.Employee);

        // Generate email confirmation token
        var emailToken = await _userManager.GenerateEmailConfirmationTokenAsync(user);

        // Send confirmation email
        await _emailService.SendEmailConfirmationAsync(user.Email, user.Id, emailToken);

        return Result<AuthenticationResponse>.Success(new AuthenticationResponse
        {
            Message = "Registration successful. Please check your email to confirm your account."
        });
    }

    public async Task<Result<AuthenticationResponse>> LoginAsync(LoginRequest request)
    {
        var user = await _userManager.FindByEmailAsync(request.Email);
        if (user == null)
            return Result<AuthenticationResponse>.Failure("Invalid credentials");

        // Check if email is confirmed
        if (!user.EmailConfirmed)
            return Result<AuthenticationResponse>.Failure("Email not confirmed");

        // Check if account is active
        if (user.Status != UserStatus.Active)
            return Result<AuthenticationResponse>.Failure("Account is inactive");

        // Attempt sign-in (handles lockout automatically)
        var result = await _signInManager.PasswordSignInAsync(
            user,
            request.Password,
            isPersistent: request.RememberMe,
            lockoutOnFailure: true);

        if (result.IsLockedOut)
            return Result<AuthenticationResponse>.Failure(
                "Account is locked due to multiple failed login attempts. Try again in 15 minutes.");

        if (!result.Succeeded)
            return Result<AuthenticationResponse>.Failure("Invalid credentials");

        // Update last login
        user.LastLoginAt = DateTime.UtcNow;
        await _userManager.UpdateAsync(user);

        // Generate tokens
        var roles = await _userManager.GetRolesAsync(user);
        var accessToken = _jwtTokenGenerator.GenerateAccessToken(user, roles);
        var refreshToken = await _jwtTokenGenerator.GenerateRefreshTokenAsync(user.Id);

        return Result<AuthenticationResponse>.Success(new AuthenticationResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken.Token,
            ExpiresAt = DateTime.UtcNow.AddMinutes(15),
            User = MapToUserDto(user, roles)
        });
    }

    public async Task<Result> ConfirmEmailAsync(Guid userId, string token)
    {
        var user = await _userManager.FindByIdAsync(userId.ToString());
        if (user == null)
            return Result.Failure("User not found");

        var result = await _userManager.ConfirmEmailAsync(user, token);
        if (!result.Succeeded)
            return Result.Failure("Invalid or expired token");

        return Result.Success();
    }
}
```

### 4. JWT Token Generator

```csharp
public interface IJwtTokenGenerator
{
    string GenerateAccessToken(ApplicationUser user, IList<string> roles);
    Task<RefreshToken> GenerateRefreshTokenAsync(Guid userId);
    ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
}

public class JwtTokenGenerator : IJwtTokenGenerator
{
    private readonly IConfiguration _configuration;
    private readonly IRefreshTokenRepository _refreshTokenRepository;

    public string GenerateAccessToken(ApplicationUser user, IList<string> roles)
    {
        var claims = new List<Claim>
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email!),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new Claim("firstName", user.FirstName),
            new Claim("lastName", user.LastName)
        };

        // Add role claims
        claims.AddRange(roles.Select(role => new Claim(ClaimTypes.Role, role)));

        var key = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(_configuration["Jwt:SecretKey"]));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

        var token = new JwtSecurityToken(
            issuer: _configuration["Jwt:Issuer"],
            audience: _configuration["Jwt:Audience"],
            claims: claims,
            expires: DateTime.UtcNow.AddMinutes(15),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }

    public async Task<RefreshToken> GenerateRefreshTokenAsync(Guid userId)
    {
        var refreshToken = new RefreshToken
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            Token = GenerateRandomToken(),
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            CreatedAt = DateTime.UtcNow,
            CreatedByIp = GetClientIpAddress()
        };

        await _refreshTokenRepository.AddAsync(refreshToken);
        return refreshToken;
    }

    private string GenerateRandomToken()
    {
        var randomBytes = new byte[64];
        using var rng = RandomNumberGenerator.Create();
        rng.GetBytes(randomBytes);
        return Convert.ToBase64String(randomBytes);
    }
}
```

### 5. Seeding Default Data

```csharp
public static async Task SeedDatabase(WebApplication app)
{
    using var scope = app.Services.CreateScope();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<ApplicationRole>>();

    // Create roles
    var roles = new[] { "Administrator", "Manager", "Employee" };
    foreach (var roleName in roles)
    {
        if (!await roleManager.RoleExistsAsync(roleName))
        {
            await roleManager.CreateAsync(new ApplicationRole
            {
                Id = Guid.NewGuid(),
                Name = roleName,
                NormalizedName = roleName.ToUpperInvariant(),
                Description = $"{roleName} role",
                CreatedAt = DateTime.UtcNow
            });
        }
    }

    // Create default admin user
    var adminEmail = "admin@tirotime.com";
    if (await userManager.FindByEmailAsync(adminEmail) == null)
    {
        var adminUser = new ApplicationUser
        {
            Id = Guid.NewGuid(),
            UserName = adminEmail,
            Email = adminEmail,
            EmailConfirmed = true,
            FirstName = "System",
            LastName = "Administrator",
            CreatedAt = DateTime.UtcNow,
            Status = UserStatus.Active
        };

        var result = await userManager.CreateAsync(adminUser, "Admin123!@#");
        if (result.Succeeded)
        {
            await userManager.AddToRoleAsync(adminUser, "Administrator");
        }
    }
}
```

## Benefits of Using ASP.NET Core Identity

✅ **Battle-tested Security**: Industry-standard password hashing (PBKDF2 with 100,000 iterations)

✅ **Account Lockout**: Built-in protection against brute-force attacks

✅ **Email Confirmation**: Secure token-based email verification

✅ **Password Reset**: Cryptographically secure password reset flow

✅ **Two-Factor Authentication**: Easy to add in Phase 3+

✅ **External Logins**: OAuth/OpenID Connect support for Google, Microsoft, etc.

✅ **Extensible**: Easy to add custom properties and validation

✅ **Well-Documented**: Extensive Microsoft documentation and community support

✅ **Performance**: Optimized queries and caching

✅ **Compliance Ready**: Helps meet security compliance requirements (OWASP, GDPR)

## Future Enhancements (Phase 3+)

- **Multi-factor authentication (TOTP, SMS)**: Built-in support via `TwoFactorEnabled`
- **OAuth/OpenID Connect integration (Google, Microsoft)**: Via `AddGoogle()`, `AddMicrosoftAccount()`
- **Single Sign-On (SSO) support**: SAML, OpenID Connect
- **Biometric authentication**: WebAuthn support
- **Device management (trusted devices)**: Custom claims and tokens
- **Advanced audit logging**: Track all Identity operations
- **User groups and team management**: Custom entities with Identity integration
- **Delegated administration**: Custom role hierarchy
- **Custom claims-based permissions**: Fine-grained authorization

---

**Document Owner**: Development Team
**Reviewers**: Security Team, Product Team
**Approval Status**: Updated for ASP.NET Core Identity
**Change Log**:
- v2.0 (2025-11-04): Updated to use ASP.NET Core Identity instead of custom implementation
