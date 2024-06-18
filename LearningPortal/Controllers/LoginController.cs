using LearningPortal.DataBaseTables;
using LearningPortal.Models;
using LearningPortal.Services;
using Microsoft.AspNetCore.Mvc;

namespace LearningPortal.Controllers
{
    [ApiController]
    [Route("api/login")]
    public class LoginController : ControllerBase
    {
        private readonly UserService _userService;
        private readonly JwtService _jwtService;

        public LoginController(UserService userService, JwtService jwtService)
        {
            _userService = userService;
            _jwtService = jwtService;
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] LoginModel loginData)
        {
            User? user = await _userService.GetUserByName(loginData.Login);

            if (user != null)
            {
                if (user.Password == null || user.Salt == null || Hasher.Verify(loginData.Password, user.Password, user.Salt) == false)
                {
                    return Unauthorized();
                }
            }
            else if (loginData.Password.Length < 6)
            {
                return Unauthorized();
            }
            else
            {
                await _userService.CreateUser(loginData.Login, loginData.Password);
                user = await _userService.GetUserByName(loginData.Login);
            }

            string token = _jwtService.GenerateToken(user);
            string refreshToken = await _jwtService.GenerateRefreshToken(user);

            TokenModel tokens = new TokenModel()
            {
                Token = token,
                RefreshToken = refreshToken
            };

            return Ok(tokens);
        }
    }
}
