using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using SummerLoginServer.DbContexts;
using SummerLoginServer.Entities;
using SummerLoginServer.Models;
using SummerLoginServer.Services;

namespace SummerLoginServer.Controllers
{
    [ApiController]
    [Route("api/account")]
    public class AccountController : ControllerBase
    {
        private readonly UserDbContext _dbContext;
        private readonly GoogleService _googleService;
        private readonly JwtTokenService _jwtTokenService;
        public AccountController(
            UserDbContext dbContext,
            GoogleService googleService,
            JwtTokenService jwtTokenService)
        {
            _dbContext = dbContext;
            _googleService = googleService;
            _jwtTokenService = jwtTokenService;
        }
        [HttpPost("login/google")]
        public async Task<ActionResult<GoogleLoginResponse>> GoogleLogin(GoogleLoginRequest request, CancellationToken cancellationToken)
        {
            var googleUser = await _googleService.VerifyIdTokenAsync(request.IdToken,
                cancellationToken);

            if (googleUser == null)
                return Unauthorized(new { message = "Invalid Google ID Token" });

            var user = await _dbContext.Users.SingleOrDefaultAsync(
                u => u.Provider == LoginProvider.Google
                && u.ProviderUserId == googleUser.Subject, cancellationToken);

            if(user == null)
            {
                user = new User
                {
                    Username = CreateInitialUsername(googleUser.Subject),
                    Provider = LoginProvider.Google,
                    ProviderUserId = googleUser.Subject,
                    CreatedAt = DateTime.UtcNow
                };
                _dbContext.Users.Add(user);
                await _dbContext.SaveChangesAsync(cancellationToken);
            }
            var token = _jwtTokenService.CreateAccessToken(user);

            return Ok(new GoogleLoginResponse(user.Id, user.Username, token.Value, token.ExpiresAt));
        }
        private static string CreateInitialUsername(string subject)
        {
            var suffix = subject.Length <= 20
                ? subject
                : subject[^20..];

            return $"google_{suffix}";
        }
    }
}
