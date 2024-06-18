using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace LearningPortal.DataBaseTables
{
    public class ExerciseFile
    {
        [Key]
        public int Id { get; set; }
        public int ExerciseId { get; set; }
        [ForeignKey("ExerciseId")]
        public Exercise Exercise { get; set; }
        public string Path { get; set; }
        public string Extension { get; set; }
    }
}