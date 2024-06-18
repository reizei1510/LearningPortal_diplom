using LearningPortal.DataBaseTables;
using LearningPortal.Services;
using Microsoft.EntityFrameworkCore;

namespace LearningPortal
{
    public class ApplicationContext : DbContext
    {
        public DbSet<User> Users { get; set; }
        public DbSet<Exercise> Exercises { get; set; }
        public DbSet<ExerciseFile> ExerciseFiles { get; set; }
        public DbSet<Variant> Variants { get; set; }
        public DbSet<VariantComposis> VariantComposises { get; set; }
        public DbSet<UserResult> UserResults { get; set; }
        public DbSet<Progress> Progresses { get; set; }

        public ApplicationContext(DbContextOptions<ApplicationContext> options)
            : base(options)
        {

        }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseLoggerFactory(LoggerFactory.Create(builder => builder.AddFilter((category, level) =>
            category == DbLoggerCategory.Database.Command.Name && level == LogLevel.Information)));
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }

        public override int SaveChanges()
        {
            UpdateProgress();
            return base.SaveChanges();
        }

        private void UpdateProgress()
        {
            List<UserResult> newResults = ChangeTracker.Entries<UserResult>()
                .Where(r => r.State == EntityState.Added)
                .Select(r => r.Entity)
                .ToList();

            foreach (UserResult result in newResults)
            {
                Progress progress = new Progress()
                {
                    DifficultyLevel = result.Exercise.DifficultyLevel,
                    ExerciseType = result.Exercise.Type,
                    Variant = result.Variant,
                    Result = result.Result
                };

                Progresses.Add(progress);
            }
        }

        public void Recreate(ExercisesReadService exercisesReadService, RandomizeService randomizeService)
        {
            Database.EnsureDeleted();
            Database.EnsureCreated();

            exercisesReadService.ReadData();
            randomizeService.RandomizeUsers(1);
            randomizeService.RandomizeVariants();
        }
    }
}