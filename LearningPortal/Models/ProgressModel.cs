using LearningPortal.DataBaseTables;

namespace LearningPortal.Models
{
    public class ProgressModel
    {
        public Variant Variant { get; set; }
        public List<int> Results { get; set; }
    }
}
