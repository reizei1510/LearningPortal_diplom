namespace LearningPortal.Models
{
    public class StatisticModel
    {
        public int VariantsCount { get; set; }
        public int ExercisesCount { get; set; }
        public Dictionary<int, List<int>> SolvingsCount { get; set; }
    }
}