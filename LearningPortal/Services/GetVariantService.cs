using LearningPortal.DataBaseTables;
using LearningPortal.Models;
using Microsoft.EntityFrameworkCore;

namespace LearningPortal.Services
{
    public class GetVariantService
    {
        private readonly ILogger<GetVariantService> _logger;
        private readonly ApplicationContext _db;
        private readonly VariantCreateService _variantCreateService;

        public GetVariantService(ILogger<GetVariantService> logger, ApplicationContext db, VariantCreateService variantCreateService)
        {
            _logger = logger;
            _db = db;
            _variantCreateService = variantCreateService;
        }

        public async Task<VariantModel?> GetVariant(User user, int variantNum)
        {
            if (variantNum == 0)
            {
                Variant? unsolvedVariant = await _db.Variants
                    .FirstOrDefaultAsync(v => v.User == user && v.IsSolved == false);

                if (unsolvedVariant == null)
                {
                    await _variantCreateService.CreateVariant(user);
                    unsolvedVariant = await _db.Variants
                        .FirstAsync(v => v.User == user && v.IsSolved == false);
                }

                variantNum = unsolvedVariant.Num;
            }

            Variant? variant = await _db.Variants
                .FirstOrDefaultAsync(v => v.User == user && v.Num == variantNum);

            if (variant != null)
            {
                List<ExerciseModel> exercisesList = new List<ExerciseModel>();

                List<Exercise> exercises = await _db.Exercises
                    .Where(e => _db.VariantComposises
                    .Any(vc => vc.Variant == variant && vc.Exercise == e))
                    .OrderBy(e => e.Type)
                    .ToListAsync();

                foreach (Exercise e in exercises)
                {
                    List<FileModel> filesList = new List<FileModel>();

                    List<ExerciseFile> files = await _db.ExerciseFiles
                        .Where(f => f.Exercise == e)
                        .OrderBy(f => f.Id)
                        .ToListAsync();

                    for (int i = 0; i < files.Count; i++)
                    {
                        FileModel file = new FileModel()
                        {
                            Num = i + 1,
                            Path = files[i].Path,
                            Extension = files[i].Extension
                        };
                        filesList.Add(file);
                    }

                    ExerciseModel exercise = new ExerciseModel()
                    {
                        Num = e.Type,
                        Text = e.Text,
                        Answer = e.Answer,
                        Files = filesList
                    };
                    exercisesList.Add(exercise);
                }

                _logger.LogInformation($"Returned variant {variantNum} for user {user.Name}.");
                return new VariantModel()
                {
                    Num = variantNum,
                    Exercises = exercisesList,
                    Lvl = Math.Round(await CountVariantDifficult(variant), 2)
                };
            }
            else
            {
                _logger.LogWarning($"User {user.Name} has not variant with num {variantNum}.");
                return null;
            }
        }

        private async Task<double> CountVariantDifficult(Variant variant)
        {
            double d = await _db.Exercises
                    .Where(e => _db.VariantComposises
                    .Any(vc => vc.Variant == variant && vc.Exercise == e))
                    .Select(e => e.DifficultyLevel)
                    .AverageAsync();

            return d;
        }
    }
}
