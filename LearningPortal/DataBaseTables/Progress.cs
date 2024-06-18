using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace LearningPortal.DataBaseTables
{
    public class Progress
    {
        [Key]
        public int Id { get; set; }
        public int DifficultyLevel { get; set; }
        public int ExerciseType { get; set; }
        public int VariantId { get; set; }
        [ForeignKey("VariantId")]
        public Variant Variant { get; set; }
        public bool Result { get; set; }
    }
}