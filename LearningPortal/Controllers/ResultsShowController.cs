using LearningPortal.DataBaseTables;
using LearningPortal.Services;
using Microsoft.AspNetCore.Mvc;

namespace LearningPortal.Controllers
{
    [ApiController]
    [Route("results")]
    public class ResultsShowController : ControllerBase
    {
        private readonly VariantCountService _variantCountService;

        public ResultsShowController(VariantCountService variantCountService)
        {
            _variantCountService = variantCountService;
        }
    
        [HttpGet("{variantNum:int:min(1)}")]
        public async Task<IActionResult> Get(int variantNum)
        {
            User? user = HttpContext.Items["user"] as User;
            if (user == null)
            {
                return Unauthorized();
            }

            if (await _variantCountService.CheckSolvedVariants(user) >= variantNum)
            {
                Console.WriteLine($"Results page loaded.");
                string path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/html/results.html");

                return PhysicalFile(path, "text/html");
            }
            else
            {
                return NotFound();
            }
        }
    }
}
