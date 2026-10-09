using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using TiroTime.Application.Common;
using TiroTime.Application.Interfaces;
using TiroTime.Domain.Identity;
using TiroTime.Infrastructure.Options;
using TiroTime.Infrastructure.Persistence;

namespace TiroTime.Infrastructure.Services;

public class AuthenticationService(
    UserManager<ApplicationUser> userManager,
    IJwtTokenService jwtTokenService,
    ApplicationDbContext context,
    IOptions<JwtOptions> jwtOptions) : IAuthenticationService
{
    private readonly JwtOptions _jwt = jwtOptions.Value;

    public async Task<Result<AuthenticationResult>> LoginAsync(
        string email,
        string password,
        string ipAddress,
        CancellationToken cancellationToken = default)
    {
        var user = await userManager.FindByEmailAsync(email);
        if (user == null)
        {
            return Result.Failure<AuthenticationResult>("Ungültige Anmeldedaten");
        }

        if (user.Status != UserStatus.Active)
        {
            return Result.Failure<AuthenticationResult>("Benutzerkonto ist nicht aktiv");
        }

        if (await userManager.IsLockedOutAsync(user))
        {
            return Result.Failure<AuthenticationResult>("Konto ist gesperrt. Bitte versuchen Sie es später erneut.");
        }

        var isPasswordValid = await userManager.CheckPasswordAsync(user, password);
        if (!isPasswordValid)
        {
            await userManager.AccessFailedAsync(user);
            return Result.Failure<AuthenticationResult>("Ungültige Anmeldedaten");
        }

        await userManager.ResetAccessFailedCountAsync(user);

        user.LastLoginAt = DateTime.UtcNow;
        await userManager.UpdateAsync(user);

        return await IssueTokensAsync(user, ipAddress, replacedToken: null, cancellationToken);
    }

    public async Task<Result<AuthenticationResult>> RefreshTokenAsync(
        string refreshToken,
        string ipAddress,
        CancellationToken cancellationToken = default)
    {
        var token = await context.RefreshTokens
            .FirstOrDefaultAsync(t => t.Token == refreshToken, cancellationToken);

        if (token == null || !token.IsActive)
        {
            return Result.Failure<AuthenticationResult>("Ungültiges Refresh-Token");
        }

        var user = await userManager.FindByIdAsync(token.UserId.ToString());
        if (user == null || user.Status != UserStatus.Active)
        {
            return Result.Failure<AuthenticationResult>("Benutzer nicht gefunden oder inaktiv");
        }

        return await IssueTokensAsync(user, ipAddress, token, cancellationToken);
    }

    public async Task<Result> RevokeTokenAsync(
        string refreshToken,
        string ipAddress,
        CancellationToken cancellationToken = default)
    {
        var token = await context.RefreshTokens
            .FirstOrDefaultAsync(t => t.Token == refreshToken, cancellationToken);

        if (token == null || !token.IsActive)
        {
            return Result.Failure("Ungültiges Refresh-Token");
        }

        token.Revoke(ipAddress, "Revoked by user");
        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    public async Task<Result> LogoutAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        var tokens = await context.RefreshTokens
            .Where(t => t.UserId == userId && t.RevokedAt == null)
            .ToListAsync(cancellationToken);

        foreach (var token in tokens)
        {
            token.Revoke("System", "User logout");
        }

        await context.SaveChangesAsync(cancellationToken);

        return Result.Success();
    }

    private async Task<Result<AuthenticationResult>> IssueTokensAsync(
        ApplicationUser user,
        string ipAddress,
        RefreshToken? replacedToken,
        CancellationToken cancellationToken)
    {
        var roles = await userManager.GetRolesAsync(user);

        var accessToken = jwtTokenService.GenerateAccessToken(user.Id, user.Email!, roles);
        var refreshTokenString = jwtTokenService.GenerateRefreshToken();

        var now = DateTime.UtcNow;
        var accessTokenExpiresAt = now.AddMinutes(_jwt.AccessTokenExpirationMinutes);
        var refreshTokenExpiresAt = now.AddDays(_jwt.RefreshTokenExpirationDays);

        var refreshToken = RefreshToken.Create(user.Id, refreshTokenString, refreshTokenExpiresAt, ipAddress);

        replacedToken?.Revoke(ipAddress, "Replaced by new token", refreshTokenString);

        await context.RefreshTokens.AddAsync(refreshToken, cancellationToken);
        await context.SaveChangesAsync(cancellationToken);

        var authResult = new AuthenticationResult(
            user.Id,
            user.Email!,
            user.FirstName,
            user.LastName,
            accessToken,
            refreshTokenString,
            accessTokenExpiresAt,
            refreshTokenExpiresAt);

        return Result.Success(authResult);
    }
}
