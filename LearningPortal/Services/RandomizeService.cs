using LearningPortal.DataBaseTables;

namespace LearningPortal.Services
{
    public class RandomizeService
    {
        private readonly ILogger<RandomizeService> _logger;
        private readonly ApplicationContext _db;
        private readonly UserService _userService;
        private readonly VariantCreateService _variantCreateService;
        private readonly ResultsService _saveResultsService;

        public RandomizeService(ILogger<RandomizeService> logger, ApplicationContext db, UserService userService, VariantCreateService variantCreateService, ResultsService saveResultsService)
        {
            _logger = logger;
            _db = db;
            _userService = userService;
            _variantCreateService = variantCreateService;
            _saveResultsService = saveResultsService;
        }

        public void RandomizeUsers(int n)
        {
            _logger.LogInformation("Start creating users.");

            for (int i = 0; i < n; i++)
            {
                string userName = Guid.NewGuid().ToString();
                string password = Guid.NewGuid().ToString();
                _userService.CreateUser(userName, password).Wait();
            }

            _logger.LogInformation("Users database has been created.");
        }

        public void RandomizeVariants()
        {
            _logger.LogInformation($"Start creating variants.");
            List<User> users = _db.Users.ToList();
            int maxDifficult = _db.Exercises
                .Max(e => e.DifficultyLevel);

            for (int d = 1; d <= maxDifficult; d++)
            {
                foreach (User u in users)
                {
                    List<int> difficultArray = Enumerable.Repeat(d, 27).ToList();

                    List<Exercise> exerciseArray =  _variantCreateService.PickExercises(u, difficultArray).Result;
                    _variantCreateService.CombineVariant(u, exerciseArray).Wait();
                    SolveRandomly(u);
                }
            }

            _logger.LogInformation("Variants database has been created.");
        }

        private void SolveRandomly(User user)
        {
            Random r = new Random();

            Variant? variant = _db.Variants
                .FirstOrDefault(v => v.User == user && v.IsSolved == false);

            if (variant != null)
            {
                _logger.LogInformation($"Start solving variant {variant.Num} by user {user.Name}.");

                List<Exercise> exercises = _db.Exercises
                    .Where(e => _db.VariantComposises
                    .Any(vc => vc.Variant == variant && vc.Exercise == e))
                    .OrderBy(e => e.Type)
                    .ToList();

                foreach (Exercise e in exercises)
                {
                    UserResult res = new UserResult()
                    {
                        Exercise = e,
                        Variant = variant,
                        Result = Convert.ToBoolean(r.Next(2)),
                        Date = DateTime.UtcNow,
                        UserAnswer = "random answer"
                    };
                    _db.UserResults.Add(res);

                    _logger.LogInformation($"Exercise {e.Type} solved by user {user.Name} with result {res.Result}.");
                }

                _db.SaveChanges();

                if (variant.Num > 1)
                {
                    _saveResultsService.CompleteProgress(user).Wait();
                }

                variant.IsSolved = true;
                _db.Variants.Update(variant);

                _db.SaveChanges();
                _logger.LogInformation($"Variant {variant.Num} was solved by user {user.Name}.");
            }
        }
    }
}
