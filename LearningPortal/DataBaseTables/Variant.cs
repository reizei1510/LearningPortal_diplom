using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace LearningPortal.DataBaseTables
{
    public class Variant
    {
        [Key]
        public int Id { get; set; }
        public int UserId { get; set; }
        [ForeignKey("UserId")]
        public User User { get; set; }
        public int Num { get; set; }
        public bool IsSolved { get; set; }
        public List<VariantComposis> VariantComposises { get; set; } = new();
        public List<UserResult> UserResults { get; set; } = new();
        public List<Progress> Progresses { get; set; } = new();
    }
}