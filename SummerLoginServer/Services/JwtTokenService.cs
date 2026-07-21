using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SummerLoginServer.Entities;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SummerLoginServer.Services
{
    public sealed record IssuedToken(string Value, DateTime ExpiresAt);
    public sealed class JwtOptions
    {
        public static readonly string SectionName = "Jwt";
        [Required]
        public string Issuer { get; init; } = string.Empty;

        [Required]
        public string Audience { get; init; } = string.Empty;

        [Required]
        [MinLength(32)]
        public string SigningKey { get; init; } = string.Empty;

        [Range(1,1440)]
        public int AccessTokenMinutes { get; init; } = 15;
    }
    public class JwtTokenService
    {
        private readonly string _issuer;
        private readonly string _audience;
        private readonly TimeSpan _accessTokenLifetime;
        private readonly SigningCredentials _signingCredentials;
        public JwtTokenService(IOptions<JwtOptions> options)
        {
            var jwtOptions = options.Value;

            _issuer = jwtOptions.Issuer;
            _audience = jwtOptions.Audience;
            _accessTokenLifetime = TimeSpan.FromMinutes(jwtOptions.AccessTokenMinutes);

            var signingKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtOptions.SigningKey));

            _signingCredentials = new SigningCredentials(signingKey, SecurityAlgorithms.HmacSha256);
        }
        public IssuedToken CreateAccessToken(User user)
        {
            var expiresAt = DateTime.Now.Add(_accessTokenLifetime);
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim("username", user.Username),
                new Claim("provider", user.Provider.ToString())
            };

            var jwt = new JwtSecurityToken(
                _issuer,     //private 키를 생성한 사람? (로그인 서버)
                _audience,   //public 토큰을 사용한 대상 (게임 서버)
                claims,
                notBefore: DateTime.UtcNow,
                expires: expiresAt,
                signingCredentials: _signingCredentials);

            return new IssuedToken(new JwtSecurityTokenHandler().WriteToken(jwt), expiresAt);
        }
    }
}
