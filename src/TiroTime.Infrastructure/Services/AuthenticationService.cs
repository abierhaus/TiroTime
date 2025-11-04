using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TiroTime.Application.Common;
using TiroTime.Application.Interfaces;
using TiroTime.Domain.Identity;
using TiroTime.Infrastructure.Persistence;

namespace TiroTime.Infrastructure.Services;

public class AuthenticationService : IAuthenticationService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IJwtTokenService _jwtTokenService;
    private readonly ApplicationDbContext _context;

    public AuthenticationService(
        UserManager<ApplicationUser> userManager,
        IJwtTokenService jwtTokenService,
        ApplicationDbContext context)
    {
        _userManager = userManager;
        _jwtTokenService = jwtTokenService;
        _context = context;
    }

    public async Task<Result<AuthenticationResult>> LoginAsync(
        string email,
        string password,
        string ipAddress,
        CancellationToken cancellationToken = default)
    {
        var user = await _userManager.FindByEmailAsync(email);
        if (user == null)
        {
            return Result.Failure<AuthenticationResult>("Ungültige Anmeldedaten");
        }

        if (user.Status != UserStatus.Active)
        {
            return Result.Failure<AuthenticationResult>("Benutzerkonto ist nicht aktiv");
        }

        // Check if user is locked out
        if (await _userManager.IsLockedOutAsync(user))
        {
            return Result.Failure<AuthenticationResult>("Konto ist gesperrt. Bitte versuchen Sie es später erneut.");
        }

        // Check password
        var isPasswordValid = await _userManager.CheckPasswordAsync(user, password);
        if (!isPasswordValid)
        {
            // Increment failed access count
            await _userManager.AccessFailedAsync(user);
            return Result.Failure<AuthenticationResult>("Ungültige Anmeldedaten");
        }

        // Reset failed access count on successful login
        await _userManager.ResetAccessFailedCountAsync(user);

        // Update last login
        user.LastLoginAt = DateTime.UtcNow;
        await _userManager.UpdateAsync(user);

        // Get user roles
        var roles = await _userManager.GetRolesAsync(user);

        // Generate tokens
        var accessToken = _jwtTokenService.GenerateAccessToken(user.Id, user.Email!, roles);
        var refreshTokenString = _jwtTokenService.GenerateRefreshToken();

        // Save refresh token
        var refreshToken = RefreshToken.Create(
            user.Id,
            refreshTokenString,
            DateTime.UtcNow.AddDays(7),
            ipAddress);

        await _context.RefreshTokens.AddAsync(refreshToken, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        var authResult = new AuthenticationResult(
            user.Id,
            user.Email!,
            user.FirstName,
            user.LastName,
            accessToken,
            refreshTokenString,
            DateTime.UtcNow.AddMinutes(15),
            DateTime.UtcNow.AddDays(7));

        return Result.Success(authResult);
    }

    public async Task<Result<AuthenticationResult>> RefreshTokenAsync(
        string refreshToken,
        string ipAddress,
        CancellationToken cancellationToken = default)
    {
        var token = await _context.RefreshTokens
            .FirstOrDefaultAsync(t => t.Token == refreshToken, cancellationToken);

        if (token == null || !token.IsActive)
        {
            return Result.Failure<AuthenticationResult>("Ungültiges Refresh-Token");
        }

        var user = await _userManager.FindByIdAsync(token.UserId.ToString());
        if (user == null || user.Status != UserStatus.Active)
        {
            return Result.Failure<AuthenticationResult>("Benutzer nicht gefunden oder inaktiv");
        }

        // Get user roles
        var roles = await _userManager.GetRolesAsync(user);

        // Generate new tokens
        var accessToken = _jwtTokenService.GenerateAccessToken(user.Id, user.Email!, roles);
        var newRefreshTokenString = _jwtTokenService.GenerateRefreshToken();

        // Create new refresh token
        var newRefreshToken = RefreshToken.Create(
            user.Id,
            newRefreshTokenString,
            DateTime.UtcNow.AddDays(7),
            ipAddress);

        // Revoke old token
        token.Revoke(ipAddress, "Replaced by new token", newRefreshTokenString);

        await _context.RefreshTokens.AddAsync(newRefreshToken, cancellationToken);
        await _context.SaveChangesAsync(cancellationToken);

        var authResult = new AuthenticationResult(
            user.Id,
            user.Email!,
            user.FirstName,
            user.LastName,
            accessToken,
            newRefreshTokenString,
            DateTime.UtcNow.AddMinutes(15),
            DateTime.UtcNow.AddDays(7));

        return Result.Success(authResult);
    }

    public async Task<Result> RevokeTokenAsync(
        string refreshToken,
        string ipAddress,
        CancellationToken cancellationToken = default)
    {
        var token = await _context.RefreshTokens
            .FirstOrDefaultAsync(t => t.Token == refreshToken, cancellationToken);

        if (token == null || !token.IsActive)
        {
            return Result.Failure("Ungültiges Refresh-Token");
        }

        token.Revoke(ipAddress, "Revoked by user");
        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    public async Task<Result> LogoutAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        // Revoke all active refresh tokens for the user
        var tokens = await _context.RefreshTokens
            .Where(t => t.UserId == userId && t.RevokedAt == null)
            .ToListAsync(cancellationToken);

        foreach (var token in tokens)
        {
            token.Revoke("System", "User logout");
        }

        await _context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }
}
