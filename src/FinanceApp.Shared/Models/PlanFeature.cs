using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinanceApp.Shared.Models
{
    public class PlanFeature
    {
        [Key]
        public int id_planFeature { get; set; }

        public int id_plan { get; set; }
        [ForeignKey("id_plan")]
        public virtual Plan Plan { get; set; } = null!;

        public int id_feature { get; set; }
        [ForeignKey("id_feature")]
        public virtual Feature Feature { get; set; } = null!;
    }
}
