using LearningPortal.DataBaseTables;
using LearningPortal.Models;
using LearningPortal.Services;
using Microsoft.AspNetCore.Mvc;

namespace LearningPortal.Controllers
{
    [ApiController]
    [Route("api/get-results")]
    public class ResultsGetController : ControllerBase
    {
        private readonly ResultsService _resultsService;

        public ResultsGetController(ResultsService resultsService)
        {
            _resultsService = resultsService;
        }

        [HttpGet("{variantNum:int:min(1)}")]
        public async Task<IActionResult> Get(int variantNum)
        {
            User? user = HttpContext.Items["user"] as User;
            if (user == null)
            {
                return Unauthorized();
            }

            List<AnswerModel> answers = await _resultsService.GetResults(user, variantNum);

            return Ok(answers);
        }
    }
}
