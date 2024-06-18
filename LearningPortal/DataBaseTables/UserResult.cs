using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace LearningPortal.DataBaseTables
{
    public class UserResult
    {
        [Key]
        public int Id { get; set; }
        public int ExerciseId { get; set; }
        [ForeignKey("ExerciseId")]
        public Exercise Exercise { get; set; }
        public int VariantId { get; set; }
        [ForeignKey("VariantId")]
        public Variant Variant { get; set; }
        public DateTime Date { get; set; }
        public string UserAnswer { get; set; }
        public bool Result { get; set; }
    }
}