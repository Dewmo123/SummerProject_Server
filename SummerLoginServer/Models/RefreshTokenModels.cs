using System.ComponentModel.DataAnnotations;

namespace SummerLoginServer.Models;

public sealed class RefreshTokenOptions
{
    public const string SectionName = "RefreshToken";

    [Range(1, 365)]
    public int LifetimeDays { get; init; } = 30;
}

public sealed record RefreshTokenRequest(
    [Required] string RefreshToken);

public sealed record TokenRefreshResponse(
    string AccessToken,
    DateTime AccessTokenExpiresAt,
    string RefreshToken,
    DateTime RefreshTokenExpiresAt);

public sealed record IssuedRefreshToken(
    string Value,
    DateTime ExpiresAt);

public enum RefreshTokenStatus
{
    Success,
    Invalid,
    Expired,
    Revoked,
    Reused
}

public sealed record RefreshTokenRotationResult(
    RefreshTokenStatus Status,
    int? UserId = null,
    string? RefreshToken = null,
    DateTime? RefreshTokenExpiresAt = null);
