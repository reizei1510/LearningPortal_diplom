using LearningPortal.DataBaseTables;
using Microsoft.EntityFrameworkCore;

namespace LearningPortal.Services
{
    public class VariantCountService
    {
        private readonly ILogger<VariantCountService> _logger;
        private readonly ApplicationContext _db;

        public VariantCountService(ILogger<VariantCountService> logger, ApplicationContext db)
        {
            _logger = logger;
            _db = db;
        }

        public async Task<int> CheckSolvedVariants(User user)
        {
            List<Variant> solvedVariants = await _db.Variants
                    .Where(v => v.User == user && v.IsSolved == true)
                    .ToListAsync();

            return solvedVariants.Count();
        }
    }
}