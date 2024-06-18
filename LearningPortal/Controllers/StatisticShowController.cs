using LearningPortal.DataBaseTables;
using Microsoft.AspNetCore.Mvc;

namespace LearningPortal.Controllers
{
    [ApiController]
    [Route("statistic")]
    public class StatisticShowController : ControllerBase
    {
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            User? user = HttpContext.Items["user"] as User;
            if (user == null || user.Password == "")
            {
                return Unauthorized();
            }

            Console.WriteLine($"Statistic page loaded.");
            string path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/html/statistic.html");

            return PhysicalFile(path, "text/html");
        }
    }
}
