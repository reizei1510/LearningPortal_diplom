using LearningPortal.DataBaseTables;
using LearningPortal.Models;
using LearningPortal.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace LearningPortal.Controllers
{
    [ApiController]
    [Route("api/statistic")]
    public class StatisticGetController : ControllerBase
    {
        private readonly StatisticService _statisticService;

        public StatisticGetController(StatisticService statisticService)
        {
            _statisticService = statisticService;
        }

        [Authorize]
        public async Task<IActionResult> Post([FromBody] DateModel date)
        {
            User? user = HttpContext.Items["user"] as User;
            if (user == null)
            {
                return Unauthorized();
            }

            DateTime utcDate = DateTime.Parse(date.Date).ToUniversalTime().AddDays(1).AddHours(2).AddMinutes(59).AddSeconds(59);
            StatisticModel statistic = await _statisticService.GetStatistic(user, utcDate);

            /*StatisticModel statistic = new StatisticModel()
            {
                VariantsCount = 20,
                ExercisesCount = 27,
                SolvingsCount = new Dictionary<int, List<int>>()
                {
                    { 1, new List<int>() { 3, 2, 4, 5, 4, 6, 3, 7, 6, 2, 1, 4, 5, 6, 3, 2, 3, 4, 2, 1, 5, 5, 3, 2, 2, 3, 1 } },
                    { 2, new List<int>() { 7, 8, 5, 7, 10, 9, 7, 6, 2, 8, 10, 7, 6, 5, 5, 6, 4, 5, 4, 5, 3, 2, 3, 3, 1, 1, 0 } },
                    { 3, new List<int>() { 5, 6, 8, 4, 5, 2, 7, 4, 6, 3, 4, 5, 2, 1, 4, 4, 3, 2, 3, 1, 1, 0, 0, 0, 0, 0, 0 } },
                    { 4, new List<int>() { 2, 3, 2, 4, 1, 2, 1, 3, 2, 1, 0, 0, 1, 0, 0, 0, 1, 0, 0, 0, 0, 0, 0, 0, 0, 0, 0 } }
                }
            };*/

            return Ok(statistic);
        } 
    }
}
