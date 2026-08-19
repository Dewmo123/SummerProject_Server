using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using Persistence.Entities;
using Persistence.Extensions;
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace SummerLoginServer.Services
{
    public sealed record IssuedTokenProto(string Value, DateTime ExpiresAt);

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

            var signingKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtOptions.SigningKey));
            _signingCredentials = new SigningCredentials(
                signingKey,
                SecurityAlgorithms.HmacSha256);
        }
        public IssuedTokenProto CreateAccessToken(UserModel user)
        {
            var expiresAt = DateTime.UtcNow.Add(_accessTokenLifetime);
            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim("username", user.Username),
                new Claim("provider", user.Provider.ToString())
            };

            var jwt = new JwtSecurityToken(
                _issuer,
                _audience,
                claims,
                notBefore: DateTime.UtcNow,
                expires: expiresAt,
                signingCredentials: _signingCredentials);

            return new IssuedTokenProto(new JwtSecurityTokenHandler().WriteToken(jwt), expiresAt);
        }
    }
}
