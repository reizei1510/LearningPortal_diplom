using LearningPortal.DataBaseTables;
using LearningPortal.Models;
using Microsoft.EntityFrameworkCore;

namespace LearningPortal.Services
{
    public class ResultsService
    {
        private readonly ILogger<ResultsService> _logger;
        private readonly ApplicationContext _db;

        public ResultsService(ILogger<ResultsService> logger, ApplicationContext db)
        {
            _logger = logger;
            _db = db;
        }

        public async Task SaveResults(User user, List<string> answers)
        {
            Variant? variantToSave = await _db.Variants
                .FirstOrDefaultAsync(v => v.User == user && v.IsSolved == false);

            if (variantToSave != null)
            {
                List<Exercise> exercises = await _db.Exercises
                    .Where(e => _db.VariantComposises
                    .Any(vc => vc.Exercise == e && vc.Variant == variantToSave))
                    .OrderBy(e => e.Type)
                    .ToListAsync();

                int i = 0;
                foreach (Exercise e in exercises)
                {
                    UserResult result = new UserResult()
                    {
                        Exercise = e,
                        Variant = variantToSave,
                        Date = DateTime.UtcNow,
                        UserAnswer = answers[i],
                        Result = e.Answer.Replace('\n', ' ').Trim() == answers[i]
                    };
                    await _db.UserResults.AddAsync(result);
                    i++;
                }

                _db.SaveChanges();

                if (variantToSave.Num > 1)
                {
                    await CompleteProgress(user);
                }

                variantToSave.IsSolved = true;
                _db.Variants.Update(variantToSave);
                await _db.SaveChangesAsync();

                _logger.LogInformation($"User's {user.Name} results saved succesfully.");
            }
            else
            {
                _logger.LogWarning($"User {user.Name} has not unsolved variant.");
            }
        }

        public async Task CompleteProgress(User user)
        {
            Variant? lastVariant = await _db.Variants
                    .FirstOrDefaultAsync(v => v.User == user && v.IsSolved == false);

            if (lastVariant != null)
            {
                List<int> lastVariantDifficults = await _db.VariantComposises
                    .Where(vc => vc.Variant == lastVariant)
                    .Select(vc => vc.Exercise.DifficultyLevel)
                    .Distinct()
                    .ToListAsync();

                foreach (int d in lastVariantDifficults)
                {
                    int maxExerciseType = await _db.VariantComposises
                        .Where(vc => vc.Variant == lastVariant)
                        .MaxAsync(vc => vc.Exercise.Type);

                    for (int t = 1; t <= maxExerciseType; t++)
                    {
                        Progress? progress = await _db.Progresses
                            .FirstOrDefaultAsync(p => p.Variant == lastVariant && p.ExerciseType == t && p.DifficultyLevel == d);

                        if (progress == null)
                        {
                            Progress? lastProgress = await _db.Progresses
                                .OrderBy(p => p.Id)
                                .LastOrDefaultAsync(p => p.Variant.User == user && p.DifficultyLevel == d && p.ExerciseType == t);

                            Progress newProgress = new Progress()
                            {
                                DifficultyLevel = d,
                                ExerciseType = t,
                                Variant = lastVariant,
                                Result = lastProgress != null ? lastProgress.Result : false
                            };

                            await _db.Progresses.AddAsync(newProgress);
                        }
                    }
                }

                await _db.SaveChangesAsync();
                _logger.LogInformation($"User's {user.Name} progress has been updated.");
            }
            else
            {
                _logger.LogWarning($"User's {user.Name} progress is already updated.");
            }
        }

        public async Task<List<AnswerModel>> GetResults(User user, int variantNum)
        {
            List<string> rightAnswers = await _db.VariantComposises
                    .Where(vc => vc.Variant.User == user && vc.Variant.Num == variantNum)
                    .OrderBy(vc => vc.Exercise.Type)
                    .Select(vc => vc.Exercise.Answer.Replace('\n', ' ').Trim())
                    .ToListAsync();

            List<string> userAnswers = await _db.UserResults
                    .Where(r => r.Variant.User == user && r.Variant.Num == variantNum)
                    .OrderBy(r => r.Exercise.Type)
                    .Select(r => r.UserAnswer)
                    .ToListAsync();

            List<AnswerModel> answers = rightAnswers
                .Zip(userAnswers, (r, u) => new AnswerModel() { rightAnswer = r, userAnswer = u })
                .ToList();

            return answers;
        }
    }
}