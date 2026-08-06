namespace Persistence.Entities;

public sealed class RefreshToken
{
    public Guid Id { get; set; }

    public int UserId { get; set; }

    public User User { get; set; } = null!;

    public Guid FamilyId { get; set; }

    public byte[] TokenHash { get; set; } = null!;
    public DateTime CreatedAt { get; set; }

    public DateTime ExpiresAt { get; set; }

    public DateTime? UsedAt { get; set; }

    public DateTime? RevokedAt { get; set; }

    public string? RevokeReason { get; set; }

    public Guid? ReplacedByTokenId { get; set; }
}
