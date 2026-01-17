using System.ComponentModel.DataAnnotations;

namespace FinanceApp.Shared.DTOs
{
    public class BudgetDTO
    {
        public int Id { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public decimal ValorLimite { get; set; }
        public decimal ValorGasto { get; set; } // Calculated value
        public int Mes { get; set; }
        public int Ano { get; set; }
        public int AlertaPorcentagem { get; set; }
    }

    public class CreateBudgetDTO
    {
        [Required]
        public int CategoryId { get; set; }
        [Required]
        public decimal ValorLimite { get; set; }
        [Required]
        public int Mes { get; set; }
        [Required]
        public int Ano { get; set; }
        public int AlertaPorcentagem { get; set; } = 80;
    }

    public class RecurringTransactionDTO
    {
        public int Id { get; set; }
        public string Descricao { get; set; } = string.Empty;
        public decimal Valor { get; set; }
        public string Tipo { get; set; } = string.Empty;
        public int? CategoryId { get; set; }
        public string? CategoryName { get; set; }
        public int? WalletId { get; set; }
        public int? CreditCardId { get; set; }
        public string Frequencia { get; set; } = string.Empty;
        public DateTime DataInicio { get; set; }
        public DateTime? DataFim { get; set; }
        public DateTime? UltimaProcessamento { get; set; }
        public bool Ativo { get; set; }
        public int? RecurrenceDay { get; set; }
        public int? DayOfWeek { get; set; }
    }

    public class CreateRecurringTransactionDTO
    {
        [Required]
        public string Descricao { get; set; } = string.Empty;
        [Required]
        public decimal Valor { get; set; }
        [Required]
        public string Tipo { get; set; } = "Saída";
        public int? CategoryId { get; set; }
        public int? WalletId { get; set; }
        public int? CreditCardId { get; set; }
        [Required]
        public string Frequencia { get; set; } = "Mensal";
        [Required]
        public DateTime DataInicio { get; set; }
        public DateTime? DataFim { get; set; }
        public int? RecurrenceDay { get; set; }
        public int? DayOfWeek { get; set; }
    }
}
