using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using LearningPortal.DataBaseTables;
using System.Security.Cryptography;
using LearningPortal.Models;

namespace LearningPortal.Services
{
    public class JwtService
    {
        private readonly ILogger<JwtService> _logger;
        private readonly ApplicationContext _db;

        public JwtService(ILogger<JwtService> logger, ApplicationContext db, UserService userService)
        {
            _logger = logger;
            _db = db;
        }

        public string GenerateToken(User user)
        {
            List<Claim> claims = new List<Claim> {
                new Claim(ClaimTypes.Name, user.Name)
            };

            JwtSecurityToken token = new JwtSecurityToken(
                issuer: AuthOptions.ISSUER,
                audience: AuthOptions.AUDIENCE,
                claims: claims,
                expires: DateTime.UtcNow.Add(TimeSpan.FromMinutes(10)),
                signingCredentials: new SigningCredentials(AuthOptions.GetSymmetricSecurityKey(), SecurityAlgorithms.HmacSha256)
            );

            _logger.LogInformation($"User {user.Name} got token.");

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public async Task<string> GenerateRefreshToken(User user)
        {
            byte[] randomToken = new byte[32];
            using (RandomNumberGenerator rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomToken);
            }

            string refreshToken = Convert.ToBase64String(randomToken);
            string hashedToken = Hasher.Hash(refreshToken, user.Salt);

            user.RefreshToken = hashedToken;
            user.RefreshTokenExpiration = DateTime.UtcNow.AddDays(7);
            _db.Users.Update(user);

            await _db.SaveChangesAsync();

            _logger.LogInformation($"User {user.Name} got refresh token.");

            return refreshToken;
        }

        public async Task<TokenModel?> RefreshTokens(User user, string token, string refreshToken)
        {
            string newToken = GenerateToken(user);
            string newRefreshToken = await GenerateRefreshToken(user);

            TokenModel tokens = new TokenModel()
            {
                Token = newToken,
                RefreshToken = newRefreshToken
            };

            _logger.LogInformation($"Tokens of user {user.Name} refreshed.");

            return tokens;
        }

        public ClaimsPrincipal? ValidateToken(string token, bool active = true)
        {
            JwtSecurityTokenHandler tokenHandler = new JwtSecurityTokenHandler();

            try
            {
                ClaimsPrincipal principal = tokenHandler.ValidateToken(token, new TokenValidationParameters()
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = AuthOptions.GetSymmetricSecurityKey(),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = active,
                    ClockSkew = TimeSpan.Zero
                }, out SecurityToken validatedToken);

                return principal;
            }
            catch
            {
                return null;
            }
        }
    }
}