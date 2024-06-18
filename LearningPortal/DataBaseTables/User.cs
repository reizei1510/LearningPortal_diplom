using System.ComponentModel.DataAnnotations;

namespace LearningPortal.DataBaseTables
{
    public class User
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        public string? Password { get; set; }
        public string? Salt { get; set; }
        public string? RefreshToken { get; set; }
        public DateTime? RefreshTokenExpiration { get; set; }
        public DateTime Date { get; set; }
        public List<Variant> Variants { get; set; } = new();
        public List<Progress> Progresses { get; set; } = new();
    }
}