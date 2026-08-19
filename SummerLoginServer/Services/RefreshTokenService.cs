using Microsoft.AspNetCore.WebUtilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Persistence.Entities;
using SummerLoginServer.DbContexts;
using SummerLoginServer.Models;
using System.Data;
using System.Security.Cryptography;
using System.Text;

namespace SummerLoginServer.Services;

public sealed class RefreshTokenService(UserDbContext dbContext, IOptions<RefreshTokenOptions> options)
{
    private readonly TimeSpan _lifetime = TimeSpan.FromDays(options.Value.LifetimeDays);

    public async Task<IssuedRefreshTokenProto> CreateSessionAsync(int userId, CancellationToken cancellationToken)
    {
        DateTime now = DateTime.UtcNow;
        DateTime expiresAt = now.Add(_lifetime);
        string rawToken = CreateRawToken();

        dbContext.RefreshTokens.Add(new RefreshTokenModel
        {
            Id = Guid.NewGuid(),
            UserId = userId,
            FamilyId = Guid.NewGuid(),
            TokenHash = HashToken(rawToken),
            CreatedAt = now,
            ExpiresAt = expiresAt
        });

        await dbContext.SaveChangesAsync(cancellationToken);
        return new IssuedRefreshTokenProto(rawToken, expiresAt);
    }

    public async Task<RefreshTokenRotationResultProto> RotateAsync(string rawToken, CancellationToken cancellationToken)
    {
        byte[] tokenHash = HashToken(rawToken);
        DateTime now = DateTime.UtcNow;

        RefreshTokenModel? current = await dbContext.RefreshTokens
            .AsNoTracking()
            .SingleOrDefaultAsync(
                token => token.TokenHash == tokenHash,
                cancellationToken);

        if (current is null)
            return new RefreshTokenRotationResultProto(RefreshTokenStatus.Invalid);

        if (current.UsedAt is not null)
        {
            await RevokeFamilyAsync(
                current.FamilyId,
                now,
                "refresh_token_reuse",
                cancellationToken);
            return new RefreshTokenRotationResultProto(RefreshTokenStatus.Reused);
        }

        if (current.RevokedAt is not null)
            return new RefreshTokenRotationResultProto(RefreshTokenStatus.Revoked);

        if (current.ExpiresAt <= now)
            return new RefreshTokenRotationResultProto(RefreshTokenStatus.Expired);

        string nextRawToken = CreateRawToken();
        Guid nextTokenId = Guid.NewGuid();
        byte[] nextTokenHash = HashToken(nextRawToken);

        await using var transaction = await dbContext.Database.BeginTransactionAsync(
            IsolationLevel.ReadCommitted,
            cancellationToken);

        dbContext.RefreshTokens.Add(new RefreshTokenModel
        {
            Id = nextTokenId,
            UserId = current.UserId,
            FamilyId = current.FamilyId,
            TokenHash = nextTokenHash,
            CreatedAt = now,
            // Rotation으로 세션의 절대 만료 시간이 계속 늘어나지 않도록 유지한다.
            ExpiresAt = current.ExpiresAt
        });
        await dbContext.SaveChangesAsync(cancellationToken);

        int updatedRows = await dbContext.RefreshTokens
            .Where(token =>
                token.Id == current.Id &&
                token.UsedAt == null &&
                token.RevokedAt == null &&
                token.ExpiresAt > now)
            .ExecuteUpdateAsync(
                setters => setters
                    .SetProperty(token => token.UsedAt, now)
                    .SetProperty(token => token.ReplacedByTokenId, nextTokenId),
                cancellationToken);

        if (updatedRows != 1)
        {
            await transaction.RollbackAsync(cancellationToken);
            dbContext.ChangeTracker.Clear();
            await RevokeFamilyAsync(
                current.FamilyId,
                now,
                "refresh_token_reuse",
                cancellationToken);
            return new RefreshTokenRotationResultProto(RefreshTokenStatus.Reused);
        }
        await transaction.CommitAsync(cancellationToken);

        return new RefreshTokenRotationResultProto(
            RefreshTokenStatus.Success,
            current.UserId,
            nextRawToken,
            current.ExpiresAt);
    }

    public async Task RevokeAsync(string rawToken, string reason, CancellationToken cancellationToken)
    {
        byte[] tokenHash = HashToken(rawToken);
        RefreshTokenModel? token = await dbContext.RefreshTokens
            .AsNoTracking()
            .SingleOrDefaultAsync(
                candidate => candidate.TokenHash.SequenceEqual(tokenHash),
                cancellationToken);

        if (token is null)
            return;

        await RevokeFamilyAsync(
            token.FamilyId,
            DateTime.UtcNow,
            reason,
            cancellationToken);
    }

    private Task<int> RevokeFamilyAsync(Guid familyId, DateTime revokedAt, string reason, CancellationToken cancellationToken)
    {
        return dbContext.RefreshTokens
            .Where(token =>
                token.FamilyId == familyId &&
                token.RevokedAt == null)
            .ExecuteUpdateAsync(
                setters => setters
                    .SetProperty(token => token.RevokedAt, revokedAt)
                    .SetProperty(token => token.RevokeReason, reason),
                cancellationToken);
    }

    private static string CreateRawToken()
    {
        return WebEncoders.Base64UrlEncode(RandomNumberGenerator.GetBytes(32));
    }

    private static byte[] HashToken(string rawToken)
    {
        return SHA256.HashData(Encoding.UTF8.GetBytes(rawToken));
    }
}
