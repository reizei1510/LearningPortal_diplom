using LearningPortal.DataBaseTables;
using LearningPortal.Services;
using Microsoft.AspNetCore.Mvc;

namespace LearningPortal.Controllers
{
    [ApiController]
    [Route("api/save-results")]
    public class ResultsSaveController : ControllerBase
    {
        private readonly ResultsService _resultsService;

        public ResultsSaveController(ResultsService resultsService)
        {
            _resultsService = resultsService;
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] List<string> userAnswers)
        {
            User? user = HttpContext.Items["user"] as User;
            if (user == null)
            {
                return Unauthorized();
            }

            await _resultsService.SaveResults(user, userAnswers);

            return Ok();
        }
    }
}
