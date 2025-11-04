# Phase 1: Authentication & Basic Infrastructure Tasks
**Phase**: 1 - Authentication
**Duration**: Weeks 2-3
**Status**: Pending
**Prerequisites**: Phase 0 completed

## Overview
Implement complete user authentication system using ASP.NET Core Identity, including registration, login, password reset, and user profile management with JWT token support.

---

## Task Checklist

### 1. Authentication Service Implementation
**Priority**: Critical | **Estimated Time**: 4 hours

- [ ] Create `Application/Identity/Services/IAuthenticationService.cs`
- [ ] Create `Application/Identity/Services/AuthenticationService.cs`
- [ ] Implement `RegisterAsync` method
- [ ] Implement `LoginAsync` method
- [ ] Implement `ConfirmEmailAsync` method
- [ ] Implement `ForgotPasswordAsync` method
- [ ] Implement `ResetPasswordAsync` method
- [ ] Implement `ChangePasswordAsync` method

**Key Methods**:
```csharp
Task<Result<AuthenticationResponse>> RegisterAsync(RegisterRequest request);
Task<Result<AuthenticationResponse>> LoginAsync(LoginRequest request);
Task<Result> ConfirmEmailAsync(Guid userId, string token);
Task<Result> ForgotPasswordAsync(string email);
Task<Result> ResetPasswordAsync(ResetPasswordRequest request);
Task<Result> ChangePasswordAsync(ChangePasswordRequest request);
```

**Acceptance Criteria**:
- All methods use `UserManager` and `SignInManager`
- Proper error handling with Result pattern
- Email confirmation generates token
- Password reset generates token
- Account lockout after 5 failed attempts

---

### 2. JWT Token Service
**Priority**: Critical | **Estimated Time**: 3 hours

- [ ] Create `Application/Identity/Services/IJwtTokenService.cs`
- [ ] Create `Infrastructure/Identity/JwtTokenService.cs`
- [ ] Implement `GenerateAccessToken` method
- [ ] Implement `GenerateRefreshToken` method
- [ ] Implement `ValidateToken` method
- [ ] Implement `GetPrincipalFromExpiredToken` method
- [ ] Create `Infrastructure/Persistence/Repositories/RefreshTokenRepository.cs`

**Key Methods**:
```csharp
string GenerateAccessToken(ApplicationUser user, IList<string> roles);
Task<RefreshToken> GenerateRefreshTokenAsync(Guid userId, string ipAddress);
Task<Result<ClaimsPrincipal>> ValidateTokenAsync(string token);
Task<Result<AuthenticationResponse>> RefreshTokenAsync(string refreshToken, string ipAddress);
Task RevokeTokenAsync(string token, string ipAddress);
```

**Acceptance Criteria**:
- Access token expires in 15 minutes
- Refresh token expires in 7 days
- Tokens include user ID, email, and roles
- Refresh token rotation implemented
- Revoked tokens tracked in database

---

### 3. Email Service (Console Implementation)
**Priority**: High | **Estimated Time**: 2 hours

- [ ] Create `Application/Common/Interfaces/IEmailService.cs`
- [ ] Create `Infrastructure/Services/EmailService.cs` (console output for MVP)
- [ ] Implement `SendEmailConfirmationAsync` method
- [ ] Implement `SendPasswordResetAsync` method
- [ ] Implement `SendPasswordChangedNotificationAsync` method

**Console Implementation** (production will use SMTP):
```csharp
public async Task SendEmailConfirmationAsync(string email, Guid userId, string token)
{
    var encodedToken = WebUtility.UrlEncode(token);
    var confirmLink = $"https://localhost:5001/Account/ConfirmEmail?userId={userId}&token={encodedToken}";

    _logger.LogInformation("=== EMAIL CONFIRMATION ===");
    _logger.LogInformation("To: {Email}", email);
    _logger.LogInformation("Link: {Link}", confirmLink);
    _logger.LogInformation("========================");

    await Task.CompletedTask;
}
```

**Acceptance Criteria**:
- Email content logged to console
- Links include proper tokens
- Works for confirmation and password reset

---

### 4. DTOs and Request/Response Models
**Priority**: High | **Estimated Time**: 2 hours

- [ ] Create `Application/Identity/Models/RegisterRequest.cs`
- [ ] Create `Application/Identity/Models/LoginRequest.cs`
- [ ] Create `Application/Identity/Models/AuthenticationResponse.cs`
- [ ] Create `Application/Identity/Models/ForgotPasswordRequest.cs`
- [ ] Create `Application/Identity/Models/ResetPasswordRequest.cs`
- [ ] Create `Application/Identity/Models/ChangePasswordRequest.cs`
- [ ] Create `Application/Identity/Models/UserDto.cs`

**Example - RegisterRequest**:
```csharp
public class RegisterRequest
{
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string ConfirmPassword { get; set; } = string.Empty;
    public bool AcceptTerms { get; set; }
}
```

**Acceptance Criteria**:
- All DTOs have proper properties
- Data annotations for basic validation
- FluentValidation validators created

---

### 5. FluentValidation Validators
**Priority**: High | **Estimated Time**: 2 hours

- [ ] Create `Application/Identity/Validators/RegisterRequestValidator.cs`
- [ ] Create `Application/Identity/Validators/LoginRequestValidator.cs`
- [ ] Create `Application/Identity/Validators/ResetPasswordRequestValidator.cs`
- [ ] Create `Application/Identity/Validators/ChangePasswordRequestValidator.cs`
- [ ] Register validators in DI container

**Example - RegisterRequestValidator**:
```csharp
public class RegisterRequestValidator : AbstractValidator<RegisterRequest>
{
    public RegisterRequestValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email ist erforderlich")
            .EmailAddress().WithMessage("Ungültige E-Mail-Adresse");

        RuleFor(x => x.FirstName)
            .NotEmpty().WithMessage("Vorname ist erforderlich")
            .MaximumLength(50);

        RuleFor(x => x.Password)
            .NotEmpty()
            .MinimumLength(12).WithMessage("Passwort muss mindestens 12 Zeichen lang sein")
            .Matches(@"[A-Z]").WithMessage("Passwort muss einen Großbuchstaben enthalten")
            .Matches(@"[a-z]").WithMessage("Passwort muss einen Kleinbuchstaben enthalten")
            .Matches(@"\d").WithMessage("Passwort muss eine Ziffer enthalten")
            .Matches(@"[\W_]").WithMessage("Passwort muss ein Sonderzeichen enthalten");

        RuleFor(x => x.ConfirmPassword)
            .Equal(x => x.Password).WithMessage("Passwörter stimmen nicht überein");

        RuleFor(x => x.AcceptTerms)
            .Equal(true).WithMessage("Sie müssen die Nutzungsbedingungen akzeptieren");
    }
}
```

**Acceptance Criteria**:
- German error messages
- Email validation
- Password complexity validation
- Password confirmation validation

---

### 6. Razor Pages - Account/Register
**Priority**: Critical | **Estimated Time**: 3 hours

- [ ] Create `Pages/Account/Register.cshtml`
- [ ] Create `Pages/Account/Register.cshtml.cs` (PageModel)
- [ ] Design registration form with Bootstrap
- [ ] Implement form validation
- [ ] Call `IAuthenticationService.RegisterAsync`
- [ ] Display success message
- [ ] Handle errors

**Page Structure**:
```html
@page
@model RegisterModel

<div class="row justify-content-center">
    <div class="col-md-6">
        <div class="card shadow-sm">
            <div class="card-header bg-primary text-white">
                <h4 class="mb-0">Registrierung</h4>
            </div>
            <div class="card-body">
                <form method="post">
                    <!-- Email -->
                    <div class="mb-3">
                        <label asp-for="Input.Email" class="form-label"></label>
                        <input asp-for="Input.Email" class="form-control" />
                        <span asp-validation-for="Input.Email" class="text-danger"></span>
                    </div>

                    <!-- First Name, Last Name, Password, Confirm Password -->
                    <!-- ... -->

                    <button type="submit" class="btn btn-primary w-100">Registrieren</button>
                </form>
            </div>
        </div>
    </div>
</div>
```

**Acceptance Criteria**:
- Form uses Bootstrap styling
- Client-side validation works
- Server-side validation displays errors
- Success message shown after registration
- Email confirmation instructions displayed

---

### 7. Razor Pages - Account/Login
**Priority**: Critical | **Estimated Time**: 3 hours

- [ ] Create `Pages/Account/Login.cshtml`
- [ ] Create `Pages/Account/Login.cshtml.cs` (PageModel)
- [ ] Design login form with Bootstrap
- [ ] Implement "Remember Me" checkbox
- [ ] Call `IAuthenticationService.LoginAsync`
- [ ] Set authentication cookie
- [ ] Redirect to home page on success
- [ ] Display lockout message if applicable

**Page Features**:
- Email and password fields
- Remember me checkbox
- "Forgot password?" link
- "Register" link
- Error message display
- Blue theme styling

**Acceptance Criteria**:
- Login works with valid credentials
- Error shown for invalid credentials
- Lockout message after 5 failed attempts
- Remember me extends session
- Redirects to return URL after login

---

### 8. Razor Pages - Account/ForgotPassword
**Priority**: High | **Estimated Time**: 2 hours

- [ ] Create `Pages/Account/ForgotPassword.cshtml`
- [ ] Create `Pages/Account/ForgotPassword.cshtml.cs`
- [ ] Design simple form with email field
- [ ] Call `IAuthenticationService.ForgotPasswordAsync`
- [ ] Display success message (even if email not found - security)

**Acceptance Criteria**:
- Form accepts email address
- Success message always shown (security)
- Email logged to console with reset link
- Link includes token

---

### 9. Razor Pages - Account/ResetPassword
**Priority**: High | **Estimated Time**: 2 hours

- [ ] Create `Pages/Account/ResetPassword.cshtml`
- [ ] Create `Pages/Account/ResetPassword.cshtml.cs`
- [ ] Accept userId and token from query string
- [ ] Display password reset form
- [ ] Call `IAuthenticationService.ResetPasswordAsync`
- [ ] Redirect to login on success

**Acceptance Criteria**:
- Token validated before showing form
- Password meets complexity requirements
- Success message shown
- Redirects to login page
- Invalid token shows error

---

### 10. Razor Pages - Account/ConfirmEmail
**Priority**: High | **Estimated Time**: 2 hours

- [ ] Create `Pages/Account/ConfirmEmail.cshtml`
- [ ] Create `Pages/Account/ConfirmEmail.cshtml.cs`
- [ ] Accept userId and token from query string
- [ ] Call `IAuthenticationService.ConfirmEmailAsync`
- [ ] Display success or error message
- [ ] Provide login link on success

**Acceptance Criteria**:
- Token validated correctly
- Email marked as confirmed in database
- Success message displayed
- Login link provided
- Invalid token shows error

---

### 11. Razor Pages - Account/Profile
**Priority**: Medium | **Estimated Time**: 3 hours

- [ ] Create `Pages/Account/Profile.cshtml`
- [ ] Create `Pages/Account/Profile.cshtml.cs`
- [ ] Display user information
- [ ] Allow editing first/last name
- [ ] Allow changing password
- [ ] Allow updating working hours
- [ ] Require authentication

**Page Sections**:
- Personal Information (editable)
- Password Change (separate form)
- Working Hours Settings
- Account Information (read-only: email, role, created date)

**Acceptance Criteria**:
- Requires authentication
- Profile information editable
- Password change requires old password
- Working hours stored as JSON
- Success messages shown

---

### 12. Razor Pages - Account/Logout
**Priority**: High | **Estimated Time**: 1 hour

- [ ] Create `Pages/Account/Logout.cshtml.cs` (POST only)
- [ ] Sign out user via Identity
- [ ] Clear authentication cookie
- [ ] Redirect to home page

**Acceptance Criteria**:
- POST-only (prevents CSRF)
- Clears authentication state
- Redirects to home page

---

### 13. Middleware - Exception Handling
**Priority**: High | **Estimated Time**: 2 hours

- [ ] Create `Web/Middleware/ExceptionHandlingMiddleware.cs`
- [ ] Handle domain exceptions
- [ ] Handle validation exceptions
- [ ] Handle generic exceptions
- [ ] Log exceptions
- [ ] Return friendly error page

**Acceptance Criteria**:
- Catches all unhandled exceptions
- Logs exception details
- Shows user-friendly error page
- Development mode shows detailed errors

---

### 14. Navigation Menu Update
**Priority**: Medium | **Estimated Time**: 1 hour

- [ ] Update `_Layout.cshtml` navigation
- [ ] Add "Anmelden" link (if not authenticated)
- [ ] Add user dropdown (if authenticated)
- [ ] Add "Profil" link
- [ ] Add "Abmelden" button
- [ ] Highlight active page

**Acceptance Criteria**:
- Shows correct links based on auth state
- Displays user name when logged in
- Logout works from dropdown
- Active page highlighted

---

### 15. Authorization Policies
**Priority**: High | **Estimated Time**: 2 hours

- [ ] Configure authorization policies in `Program.cs`
- [ ] Create `RequireAdminRole` policy
- [ ] Create `RequireManagerOrAdmin` policy
- [ ] Create `RequireAuthenticatedUser` policy
- [ ] Apply `[Authorize]` attribute to pages

**Policies**:
```csharp
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("RequireAdminRole", policy =>
        policy.RequireRole("Administrator"));

    options.AddPolicy("RequireManagerOrAdmin", policy =>
        policy.RequireRole("Manager", "Administrator"));

    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
});
```

**Acceptance Criteria**:
- Policies configured correctly
- Unauthorized access redirects to login
- Role-based access enforced

---

### 16. Current User Service
**Priority**: High | **Estimated Time**: 2 hours

- [ ] Create `Application/Common/Interfaces/ICurrentUserService.cs`
- [ ] Create `Infrastructure/Identity/CurrentUserService.cs`
- [ ] Implement `UserId` property
- [ ] Implement `IsAuthenticated` property
- [ ] Implement `IsInRole` method
- [ ] Register in DI container

**Interface**:
```csharp
public interface ICurrentUserService
{
    Guid? UserId { get; }
    string? Email { get; }
    bool IsAuthenticated { get; }
    bool IsInRole(string role);
}
```

**Acceptance Criteria**:
- Gets user from HttpContext
- Returns null if not authenticated
- Works in services and repositories

---

### 17. Testing - Authentication Flow
**Priority**: High | **Estimated Time**: 4 hours

- [ ] Create unit tests for AuthenticationService
- [ ] Test successful registration
- [ ] Test duplicate email registration (should fail)
- [ ] Test weak password (should fail)
- [ ] Test successful login
- [ ] Test invalid credentials (should fail)
- [ ] Test account lockout after 5 attempts
- [ ] Test email confirmation
- [ ] Test password reset

**Example Test**:
```csharp
[Fact]
public async Task RegisterAsync_ValidRequest_ReturnsSuccess()
{
    // Arrange
    var mockUserManager = MockUserManager<ApplicationUser>();
    mockUserManager.Setup(x => x.CreateAsync(It.IsAny<ApplicationUser>(), It.IsAny<string>()))
        .ReturnsAsync(IdentityResult.Success);

    var service = new AuthenticationService(mockUserManager, ...);
    var request = new RegisterRequest
    {
        Email = "test@example.com",
        FirstName = "Test",
        LastName = "User",
        Password = "Password123!",
        ConfirmPassword = "Password123!",
        AcceptTerms = true
    };

    // Act
    var result = await service.RegisterAsync(request);

    // Assert
    result.IsSuccess.Should().BeTrue();
}
```

**Acceptance Criteria**:
- All authentication flows tested
- Tests use mocked dependencies
- Tests pass consistently

---

### 18. UI Polish - Authentication Pages
**Priority**: Medium | **Estimated Time**: 2 hours

- [ ] Add loading spinners to forms
- [ ] Add toast notifications for success/error
- [ ] Improve form styling
- [ ] Add password strength indicator
- [ ] Add "show password" toggle
- [ ] Responsive design testing

**Acceptance Criteria**:
- Forms look professional
- Good UX feedback
- Works on mobile
- Accessible (WCAG Level A)

---

### 19. Documentation
**Priority**: Low | **Estimated Time**: 1 hour

- [ ] Document authentication flow
- [ ] Document default admin credentials
- [ ] Document password requirements
- [ ] Update README with login instructions

**Acceptance Criteria**:
- Clear documentation
- Default credentials documented
- Setup instructions updated

---

### 20. Integration Testing
**Priority**: High | **Estimated Time**: 3 hours

- [ ] Create integration tests with WebApplicationFactory
- [ ] Test registration endpoint
- [ ] Test login endpoint
- [ ] Test profile page (requires auth)
- [ ] Test unauthorized access redirects

**Acceptance Criteria**:
- Integration tests pass
- Full auth flow tested end-to-end
- Database in-memory or test container

---

## Deliverables

At the end of Phase 1:

✅ **Authentication System**:
- User registration with email confirmation
- Login with password
- Password reset functionality
- User profile management
- JWT token generation
- Refresh token rotation

✅ **Security Features**:
- Password hashing (PBKDF2)
- Account lockout after 5 attempts
- Email confirmation required
- Secure token generation
- Password complexity validation

✅ **UI Pages**:
- Register page
- Login page
- Forgot password page
- Reset password page
- Confirm email page
- Profile page
- Blue-themed, responsive design

✅ **Testing**:
- Unit tests for AuthenticationService
- Integration tests for auth flow
- 70%+ coverage for auth-related code

---

## Success Criteria

- [ ] All tasks completed
- [ ] User can register with email confirmation
- [ ] User can log in with credentials
- [ ] Account locks after 5 failed attempts
- [ ] Password reset works
- [ ] User profile editable
- [ ] JWT tokens generated correctly
- [ ] All tests pass
- [ ] No security vulnerabilities
- [ ] UI follows blue theme
- [ ] Responsive on mobile

---

## Time Estimate
**Total**: ~50 hours (2 weeks with 1 developer)

## Dependencies
- Phase 0 completed
- SQL Server running
- SMTP server (console output for MVP)

## Notes
- Use console logging for emails in development
- Production will use SMTP service
- Focus on security best practices
- Follow OWASP guidelines
- Test thoroughly before moving to Phase 2

---

**Phase Owner**: Development Team
**Status**: Ready to Start
**Next Phase**: Phase 2 - Client & Project Management
