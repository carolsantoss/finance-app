using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinanceApp.Shared.Models
{
    public class Lancamento
    {
        [Key]
        public int id_lancamento { get; set; }

        public int id_usuario { get; set; }

        public string nm_tipo { get; set; } = string.Empty;

        public string nm_descricao { get; set; } = string.Empty;

        public decimal nr_valor { get; set; }

        public DateTime dt_dataLancamento { get; set; }

        public string nm_formaPagamento { get; set; } = string.Empty; // Debito / Credito

        public int nr_parcelas { get; set; } = 1;

        public int nr_parcelaInicial { get; set; }

        public int nr_parcelasPagas { get; set; } = 0;

        public int? id_categoria { get; set; }

        [ForeignKey("id_categoria")]
        public virtual Category? Categoria { get; set; }
    }
}
