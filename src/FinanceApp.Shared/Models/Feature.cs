using System.ComponentModel.DataAnnotations;

namespace FinanceApp.Shared.Models
{
    public class Feature
    {
        [Key]
        public int id_feature { get; set; }

        [Required]
        public string nm_key { get; set; } = string.Empty; // e.g., 'export_data'

        [Required]
        public string nm_label { get; set; } = string.Empty; // e.g., 'Exportar Dados'

        public string? ds_description { get; set; }

        public virtual ICollection<PlanFeature> PlanFeatures { get; set; } = new List<PlanFeature>();
    }
}
