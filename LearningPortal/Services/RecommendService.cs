using LearningPortal.DataBaseTables;
using LearningPortal.Models;
using Microsoft.EntityFrameworkCore;

namespace LearningPortal.Services
{
    public class RecommendService
    {
        private readonly ILogger<RecommendService> _logger;
        private readonly ApplicationContext _db;

        public RecommendService(ILogger<RecommendService> logger, ApplicationContext db)
        {
            _logger = logger;
            _db = db;
        }

        public async Task<List<int>> CreateDificult(User user)
        {
            _logger.LogInformation($"Trying to create difficult for user {user.Name}.");

            List<int> reccomendDifficult = new List<int>();

            int maxExerciseType = await _db.Exercises
                .MaxAsync(e => e.Type);

            reccomendDifficult = Enumerable.Repeat(0, maxExerciseType).ToList();

            List<VariantComposis> solvedExercises = await _db.VariantComposises
                .Include(vc => vc.Exercise)
                .Where(vc => vc.Variant.User == user)
                .ToListAsync();

            if (solvedExercises.Count > 0)
            {
                int maxSolvedDifficult = solvedExercises
                    .Max(vc => vc.Exercise.DifficultyLevel);

                int maxExerciseDifficult = await _db.Exercises
                    .MaxAsync(e => e.DifficultyLevel);

                for (int d = maxSolvedDifficult; d >= 1; d--)
                {
                    List<ProgressModel> progresses = await GetUserProgresses(user, d);
                    if (progresses.Count != 0)
                    {
                        ProgressModel lastProgress = progresses.Last();
                        _logger.LogInformation($"Last progress of user {user.Name} for difficult {d} is {string.Join(' ', lastProgress.Results)}.");
                        if (d == maxExerciseDifficult)
                        {
                            for (int i = 0; i < lastProgress.Results.Count; i++)
                            {
                                if (lastProgress.Results[i] == 1)
                                {
                                    reccomendDifficult[i] = d;
                                }
                            }
                        }
                        else
                        {
                            List<SimilarVariantModel> similarVariants = await FindSimilarVariants(user, lastProgress, d);
                            foreach (SimilarVariantModel sv in similarVariants)
                            {
                                Variant similarVariant = await _db.Variants
                                    .Include(Variant => Variant.User)
                                    .FirstAsync(v => v == sv.Variant);

                                List<Progress> nextToSimilarProgress = await _db.Progresses
                                    .Where(p => p.Variant.User == similarVariant.User && p.Variant.Num == similarVariant.Num + 1 && p.DifficultyLevel == d + 1)
                                    .ToListAsync();

                                if (nextToSimilarProgress == null || sv.Similarity == 1)
                                {
                                    continue;
                                }

                                _logger.LogInformation($"The most similar user for user {user.Name} is {similarVariant.User.Name} with similarity {sv.Similarity}.");

                                foreach (Progress p in nextToSimilarProgress)
                                {
                                    if (p.Result == true && reccomendDifficult[p.ExerciseType - 1] == 0)
                                    {
                                        reccomendDifficult[p.ExerciseType - 1] = p.DifficultyLevel;
                                    }
                                }

                                break;
                            }
                        }

                        if (!reccomendDifficult.Contains(0))
                        {
                            break;
                        }
                    }
                }
            }

            for (int i = 0; i < reccomendDifficult.Count; i++)
            {
                if (reccomendDifficult[i] == 0)
                {
                    reccomendDifficult[i] = 1;
                }
            }

            _logger.LogInformation($"Recommend difficult for user {user.Name} is {string.Join(' ', reccomendDifficult)}.");

            return reccomendDifficult;
        }

        private async Task<List<ProgressModel>> GetUserProgresses(User user, int d)
        {
            _logger.LogInformation($"Trying get user's {user.Name} progress fot difficult {d}.");

            List<ProgressModel> progresses = new List<ProgressModel>();
            List<Progress> allProgresses = await _db.Progresses
                .Include(p => p.Variant)
                .ThenInclude(v => v.User)
                .Where(p => p.Variant.User == user && p.DifficultyLevel == d)
                .OrderBy(p => p.Variant.Num)
                .ToListAsync();

            if (allProgresses.Count > 0)
            {
                _logger.LogInformation($"User's {user.Name} progress for difficult {d} contains {allProgresses.Count} results.");
                foreach (Progress progress in allProgresses)
                {
                    if (!progresses.Any(p => p.Variant == progress.Variant))
                    {
                        progresses.Add(new ProgressModel()
                        {
                            Variant = progress.Variant,
                            Results = new List<int>()
                        });
                    }
                    progresses.First(p => p.Variant == progress.Variant).Results.Add(progress.Result ? 1 : 0);
                }
            }

            return progresses;
        }

        private async Task<List<SimilarVariantModel>> FindSimilarVariants(User user, ProgressModel progress, int d)
        {
            List<SimilarVariantModel> similarVariants = new List<SimilarVariantModel>();

            List<User> users = await _db.Users.ToListAsync();
            foreach (User u in users)
            {
                if (u.Name != user.Name)
                {
                    List<ProgressModel> progresses = await GetUserProgresses(u, d);
                    foreach (ProgressModel p in progresses)
                    {
                        similarVariants.Add(new SimilarVariantModel()
                        {
                            Variant = p.Variant,
                            Similarity = JaccardIndex(progress.Results, p.Results)
                        });
                    }
                }
            }

            return similarVariants.OrderByDescending(v => v.Similarity).ToList();
        }

        private float JaccardIndex(List<int> array1, List<int> array2)
        {
            float a = array1.Count;
            float b = array2.Count;
            float c = array1.Zip(array2, (a, b) => a == b? 1 : 0).Sum();

            return c / (a + b - c);
        }
    }
}
