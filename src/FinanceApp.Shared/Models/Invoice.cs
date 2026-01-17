using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinanceApp.Shared.Models
{
    public class Invoice
    {
        [Key]
        public int id_invoice { get; set; }

        public int id_usuario { get; set; }
        [ForeignKey("id_usuario")]
        public User? Usuario { get; set; }

        public string nm_descricao { get; set; } = string.Empty;
        
        [Column(TypeName = "decimal(18,2)")]
        public decimal nr_valor { get; set; }

        // Day of month the invoice is due (1-31)
        public int nr_diaVencimento { get; set; }

        // When it was last paid. To check if user already paid this month.
        public DateTime? dt_ultimoPagamento { get; set; }

        public bool fl_ativo { get; set; } = true;
    }
}
