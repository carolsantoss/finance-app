using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinanceApp.Shared.Models
{
    public class CreditCard
    {
        [Key]
        public int id_credit_card { get; set; }

        public string nm_nome { get; set; } = string.Empty;

        public string nm_bandeira { get; set; } = "Mastercard"; // Visa, Mastercard, Elo, Amex

        [Column(TypeName = "decimal(18,2)")]
        public decimal nr_limite { get; set; } = 0;

        public int nr_dia_fechamento { get; set; } = 1;

        public int nr_dia_vencimento { get; set; } = 10;

        public int id_usuario { get; set; }

        [ForeignKey("id_usuario")]
        public virtual User? Usuario { get; set; }

        public int? id_wallet_pagamento { get; set; } // Default wallet to pay the bill

        [ForeignKey("id_wallet_pagamento")]
        public virtual Wallet? WalletPagamento { get; set; }
    }
}
