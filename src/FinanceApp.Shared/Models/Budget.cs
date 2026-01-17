using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinanceApp.Shared.Models
{
    public class Budget
    {
        [Key]
        public int id_budget { get; set; }

        public int id_usuario { get; set; }

        public int id_categoria { get; set; }

        [ForeignKey("id_categoria")]
        public virtual Category? Categoria { get; set; }

        public decimal nr_valorLimite { get; set; }

        public int nr_mes { get; set; }

        public int nr_ano { get; set; }

        public int nr_alertaPorcentagem { get; set; } = 80; // Default alert at 80% usage
    }
}
