using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Persistence.Entities;
using SummerLoginServer.DbContexts;
using SummerLoginServer.Models;
using SummerLoginServer.Services;
using System.Security.Cryptography;
using System.Text;

namespace SummerLoginServer.Controllers;

[ApiController]
[Route("api/account")]
public sealed class AccountController(
    UserDbContext dbContext,
    GoogleService googleService,
    JwtTokenService jwtTokenService) : ControllerBase
{
    [HttpPost("login/google")]
    [EnableRateLimiting("login")]
    public async Task<IActionResult> GoogleLogin(
        GoogleLoginRequest request,
        CancellationToken cancellationToken)
    {
        GoogleUserInfo? googleUser = await googleService.VerifyIdTokenAsync(
            request.IdToken,
            cancellationToken);
        if (googleUser is null)
            return Unauthorized(new { message = "Invalid Google ID Token" });

        User? user = await dbContext.Users.SingleOrDefaultAsync(
            candidate => candidate.Provider == LoginProvider.Google &&
                         candidate.ProviderUserId == googleUser.Subject,
            cancellationToken);

        if (user is null)
        {
            user = new User
            {
                Username = CreateInitialUsername(googleUser.Subject),
                Provider = LoginProvider.Google,
                ProviderUserId = googleUser.Subject,
                CreatedAt = DateTime.UtcNow
            };
            dbContext.Users.Add(user);

            try
            {
                await dbContext.SaveChangesAsync(cancellationToken);
            }
            catch (DbUpdateException)
            {
                // 동일 계정의 동시 최초 로그인은 unique index가 한 요청만 허용한다.
                dbContext.ChangeTracker.Clear();
                user = await dbContext.Users.SingleOrDefaultAsync(
                    candidate => candidate.Provider == LoginProvider.Google &&
                                 candidate.ProviderUserId == googleUser.Subject,
                    cancellationToken);
                if (user is null)
                    throw;
            }
        }

        IssuedToken token = jwtTokenService.CreateAccessToken(user);
        return Ok(new GoogleLoginResponse(user.Id, user.Username, token.Value, token.ExpiresAt));
    }

    // 사용자 요청에 따라 개발용 토큰 엔드포인트는 유지한다.
    [HttpGet("test")]
    [EnableRateLimiting("login")]
    public async Task<IActionResult> TestLogin(CancellationToken cancellationToken)
    {
        User? user = await dbContext.Users.SingleOrDefaultAsync(
            candidate => candidate.Username == "Developer",
            cancellationToken);
        if (user is null)
            return NotFound("개발자는 없습니다.");

        IssuedToken token = jwtTokenService.CreateAccessToken(user);
        return Ok(new GoogleLoginResponse(user.Id, user.Username, token.Value, token.ExpiresAt));
    }

    private static string CreateInitialUsername(string subject)
    {
        byte[] hash = SHA256.HashData(Encoding.UTF8.GetBytes(subject));
        return $"google_{Convert.ToHexString(hash.AsSpan(0, 12)).ToLowerInvariant()}";
    }
}
