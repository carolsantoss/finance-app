using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinanceApp.Models
{
    public class Lancamento
    {
        [Key]
        public int hs_id_lancamento { get; set; }

        public decimal hs_valor { get; set; }

        public DateTime hs_data { get; set; }

        public string hs_tipo { get; set; } = string.Empty;

        public string hs_descricao { get; set; } = string.Empty;

        public int hs_id_user { get; set; }

        [ForeignKey(nameof(hs_id_user))]
        public User user { get; set; } = null!;
    }
}
