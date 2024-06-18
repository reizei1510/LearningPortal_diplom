using LearningPortal.DataBaseTables;
using Microsoft.EntityFrameworkCore;

namespace LearningPortal.Services
{
    public class VariantCreateService
    {
        private readonly ILogger<VariantCreateService> _logger;
        private readonly ApplicationContext _db;
        private readonly RecommendService _recommendService;

        public VariantCreateService(ILogger<VariantCreateService> logger, ApplicationContext db, RecommendService recommendService)
        {
            _logger = logger;
            _db = db;
            _recommendService = recommendService;
        }

        public async Task CreateVariant(User user)
        {
            List<int> reccomendDifficult = await _recommendService.CreateDificult(user);
            List<Exercise> taskList = await PickExercises(user, reccomendDifficult);
            await CombineVariant(user, taskList);
        }

        public async Task<List<Exercise>> PickExercises(User user, List<int> difficultList)
        {
            Random r = new Random();
            List<Exercise> exerciseList = new List<Exercise>();
            _logger.LogInformation($"Start creating variant for user {user.Name} with difficult {string.Join(' ', difficultList)}.");

            for (int i = 0; i < difficultList.Count; i++)
            {
                List<Exercise> exercises = await _db.Exercises
                    .Where(e => e.DifficultyLevel == difficultList[i] && e.Type == i + 1)
                    .ToListAsync();

                List<Exercise> nonSolvedExercises = exercises
                    .Where(e => !_db.UserResults
                    .Any(r => r.Variant.User == user && r.Exercise == e && r.Result == true))
                    .OrderBy(e => e.Type)
                    .ToList();

                if (nonSolvedExercises.Count == 0)
                {
                    // сделать так чтобы рекомендовались задачи меньшего уровня, если решены задачи этого и выше
                    if (difficultList[i] < 4)
                    {
                        difficultList[i]++;
                        i--;
                        continue;
                    }
                    else
                    {
                        nonSolvedExercises = exercises;
                    }
                }
                int k = r.Next(nonSolvedExercises.Count);

                Exercise exercise = nonSolvedExercises[k];
                exerciseList.Add(exercise);
            }

            return exerciseList;
        }

        public async Task CombineVariant(User user, List<Exercise> exercisesList)
        {
            int variantNum;
            Variant? lastVariant = await _db.Variants
                .OrderBy(v => v.Id)
                .LastOrDefaultAsync(v => v.User == user);

            if (lastVariant == null)
            {
                variantNum = 1;
            }
            else
            {
                variantNum = lastVariant.Num + 1;
            }

            // разобраться, почему приходится добавлять user по Id, а не напрямую, иначе ошибка с ключами
            Variant variant = new Variant()
            {
                User = await _db.Users.FirstAsync(u => u.Id == user.Id),
                Num = variantNum,
                IsSolved = false
            };
            await _db.Variants.AddAsync(variant);
            _logger.LogInformation($"Added variant {variant.Num} for user {variant.User.Name}.");

            await _db.SaveChangesAsync();

            foreach (Exercise exercise in exercisesList)
            {
                // та же проблема с добавлением exercise
                VariantComposis vc = new VariantComposis()
                {
                    Variant = variant,
                    Exercise = await _db.Exercises
                        .FirstAsync(e => e.Id == exercise.Id)
                };
                await _db.VariantComposises.AddAsync(vc);

                await _db.SaveChangesAsync();
                _logger.LogInformation($"Added exercise {exercise.Id} to variant {variant.Num} user's {variant.User.Name}.");
            }

            _logger.LogInformation($"Created variant {variant.Num} for user {variant.User.Name}.");
        }
    }
}
