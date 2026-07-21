using System.ComponentModel.DataAnnotations;

namespace SummerLoginServer.Models
{
    public sealed record GoogleLoginRequest([Required] string IdToken);
    public sealed record GoogleLoginResponse(int UserId, string Username, string AccessToken, DateTime ExpiresAt);

    public sealed record GoogleUserInfo(
        string Subject,
        string? Email,
        string? Name,
        string? PictureUrl);

}
