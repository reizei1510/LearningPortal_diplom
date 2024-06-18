using LearningPortal.DataBaseTables;
using LearningPortal.Models;
using LearningPortal.Services;
using Microsoft.AspNetCore.Mvc;

namespace LearningPortal.Controllers
{
    [ApiController]
    [Route("api/refresh-token")]
    public class TokenRefreshController : ControllerBase
    {
        private readonly JwtService _jwtService;

        public TokenRefreshController(JwtService jwtService)
        {
            _jwtService = jwtService;
        }

        public async Task<IActionResult> Post([FromBody] TokenModel tokens)
        {
            User? user = HttpContext.Items["user"] as User;
            string? token = HttpContext.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
            if (user == null || token == null || Hasher.Verify(tokens.RefreshToken, user.RefreshToken, user.Salt) == false)
            {
                return Unauthorized();
            }

            TokenModel? newTokens = await _jwtService.RefreshTokens(user, token, tokens.RefreshToken);

            return Ok(newTokens);
        }
    }
}
