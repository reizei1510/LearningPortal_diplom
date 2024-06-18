using LearningPortal.DataBaseTables;
using LearningPortal.Models;
using Microsoft.EntityFrameworkCore;

namespace LearningPortal.Services
{
    public class StatisticService
    {
        private readonly ILogger<StatisticService> _logger;
        private readonly ApplicationContext _db;

        public StatisticService(ILogger<StatisticService> logger, ApplicationContext db)
        {
            _logger = logger;
            _db = db;
        }

        public async Task<StatisticModel> GetStatistic(User user, DateTime date)
        {
            StatisticModel statistic = new StatisticModel();
            statistic.SolvingsCount = new Dictionary<int, List<int>>();

            int solvedVariantsCount = await _db.Variants
                .Where(v => v.User == user && v.IsSolved == true)
                .CountAsync();

            statistic.VariantsCount = solvedVariantsCount;

            int maxDifficult = await _db.Exercises
                .MaxAsync(e => e.DifficultyLevel);

            int maxType = await _db.Exercises
                .MaxAsync(e => e.Type);

            statistic.ExercisesCount = maxType;

            for (int d = 1; d <= maxDifficult; d++)
            {
                statistic.SolvingsCount.Add(d, Enumerable.Repeat(0, maxType).ToList());

                for (int t = 1; t <= maxType; t++)
                {
                    int solvedExercisesCount = await _db.UserResults
                        .Where(r => r.Variant.User == user && r.Date <= date && r.Exercise.DifficultyLevel == d && r.Exercise.Type == t && r.Result == true)
                        .CountAsync();

                    statistic.SolvingsCount[d][t - 1] = solvedExercisesCount;
                }
            }

            _logger.LogInformation($"Returned statistic for user {user.Name}.");
            return statistic;
        }
    }
}
