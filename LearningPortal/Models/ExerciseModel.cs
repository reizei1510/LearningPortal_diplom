namespace LearningPortal.Models
{
    public class ExerciseModel
    {
        public int Num { get; set; }
        public string Text { get; set; }
        public string Answer { get; set; }
        public List<FileModel> Files { get; set; }
    }
}