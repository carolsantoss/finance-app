using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinanceApp.Shared.Models
{
    public class RecurringTransaction
    {
        [Key]
        public int id_transacaoRecorrente { get; set; }

        public int id_usuario { get; set; }

        public string nm_descricao { get; set; } = string.Empty;

        public decimal nr_valor { get; set; }

        public string nm_tipo { get; set; } = "Saída"; // "Entrada" or "Saída"

        public int? id_categoria { get; set; }

        [ForeignKey("id_categoria")]
        public virtual Category? Categoria { get; set; }

        public int? id_wallet { get; set; } // Nullable, can be unassigned

        public int? id_credit_card { get; set; } // Nullable

        public string nm_frequencia { get; set; } = "Mensal"; // "Diário", "Semanal", "Mensal", "Anual"

        public DateTime dt_inicio { get; set; }

        public DateTime? dt_fim { get; set; } // Null means indefinitely

        public DateTime? dt_ultimaProcessamento { get; set; }

        public bool fl_ativo { get; set; } = true;
    }
}
