using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinanceApp.Shared.Models
{
    public class Wallet
    {
        [Key]
        public int id_wallet { get; set; }

        public string nm_nome { get; set; } = string.Empty;

        public string nm_tipo { get; set; } = "Conta Corrente"; // Conta Corrente, Carteira, Poupança, Investimento

        [Column(TypeName = "decimal(18,2)")]
        public decimal nr_saldo_inicial { get; set; } = 0;

        public int id_usuario { get; set; }
        
        [ForeignKey("id_usuario")]
        public virtual User? Usuario { get; set; }
    }
}
