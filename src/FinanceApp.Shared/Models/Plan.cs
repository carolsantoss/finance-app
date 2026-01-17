using System.ComponentModel.DataAnnotations;

namespace FinanceApp.Shared.Models
{
    public class Plan
    {
        [Key]
        public int id_plan { get; set; }

        [Required]
        public string nm_name { get; set; } = string.Empty;

        public string? ds_description { get; set; }

        public decimal nr_price { get; set; } = 0;

        public bool fl_isDefault { get; set; } = false;

        public virtual ICollection<PlanFeature> PlanFeatures { get; set; } = new List<PlanFeature>();
    }
}
