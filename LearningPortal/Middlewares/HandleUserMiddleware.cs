using LearningPortal.Services;
using System.Security.Claims;

namespace LearningPortal.Middlewares
{
    public class HandleUserMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<HandleUserMiddleware> _logger;

        public HandleUserMiddleware(RequestDelegate next, ILogger<HandleUserMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context, UserService userService, JwtService jwtService)
        {
            await HandleUserAsync(context, userService, jwtService);
            await _next.Invoke(context);
        }

        private async Task HandleUserAsync(HttpContext context, UserService userService, JwtService jwtService)
        {
            string? token = context.Request.Headers["Authorization"].FirstOrDefault()?.Split(" ").Last();
            if (token != null)
            {
                _logger.LogInformation($"Got token {token}.");

                ClaimsPrincipal? principal = jwtService.ValidateToken(token);
                if (principal != null)
                {
                    context.Items["user"] = await userService.GetUserByName(principal.Identity.Name);
                }
                else
                {
                    context.Response.StatusCode = 401;
                    await context.Response.WriteAsync("Unauthorized");

                    return;
                }
            }
            else if (context.Request.Cookies.TryGetValue("anonymousId", out string? name))
            {
                _logger.LogInformation($"Got request by {name}.");

                if (await userService.GetUserByName(name) == null)
                {
                    await userService.CreateUser(name);
                }

                context.Items.Add("user", await userService.GetUserByName(name));
            }
            else
            {
                _logger.LogInformation($"Got request by new user.");

                name = Guid.NewGuid().ToString();
                _logger.LogInformation($"New user has name {name} now.");

                await userService.CreateUser(name);

                context.Response.Cookies.Append("anonymousId", name);
                context.Items.Add("user", await userService.GetUserByName(name));
            }
        }
    }
}