using System.ComponentModel.DataAnnotations;

namespace LearningPortal.DataBaseTables
{
    public class Exercise
    {
        [Key]
        public int Id { get; set; }
        public int Type { get; set; }
        public int DifficultyLevel { get; set; }
        public string Text { get; set; }
        public string Answer { get; set; }
        public List<ExerciseFile> ExerciseFiles { get; set; } = new();
        public List<VariantComposis> VariantComposises { get; set; } = new();
        public List<UserResult> UserResults { get; set; } = new();
    }
}