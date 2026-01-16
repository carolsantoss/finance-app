using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace FinanceApp.Shared.DTOs
{
    public class CreateLancamentoRequest
    {
        [Required]
        [JsonPropertyName("tipo")]
        public string Tipo { get; set; } = "Saída"; // "Entrada" or "Saída"

        [Required]
        [JsonPropertyName("descricao")]
        public string Descricao { get; set; } = string.Empty;

        [Required]
        [Range(0.01, double.MaxValue, ErrorMessage = "O valor deve ser maior que zero.")]
        [JsonPropertyName("valor")]
        public decimal Valor { get; set; }

        [Required]
        [JsonPropertyName("data")]
        public DateTime DataLancamento { get; set; }

        [Required]
        [JsonPropertyName("formaPagamento")]
        public string FormaPagamento { get; set; } = "Débito"; // "Débito", "Crédito", "Dinheiro", "Pix"

        [JsonPropertyName("parcelas")]
        public int Parcelas { get; set; } = 1;

        [JsonPropertyName("parcelasPagas")]
        public int ParcelasPagas { get; set; } = 0;

        [JsonPropertyName("id_categoria")]
        public int? IdCategoria { get; set; }

        [JsonPropertyName("id_wallet")]
        public int? IdWallet { get; set; }

        [JsonPropertyName("id_credit_card")]
        public int? IdCreditCard { get; set; }
    }

    public class UpdateLancamentoRequest : CreateLancamentoRequest
    {
    }

    public class LancamentoResponse
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("tipo")]
        public string Tipo { get; set; } = string.Empty;

        [JsonPropertyName("descricao")]
        public string Descricao { get; set; } = string.Empty;

        [JsonPropertyName("valor")]
        public decimal Valor { get; set; }

        [JsonPropertyName("data")]
        public DateTime DataLancamento { get; set; }

        [JsonPropertyName("formaPagamento")]
        public string FormaPagamento { get; set; } = string.Empty;

        [JsonPropertyName("parcelas")]
        public int Parcelas { get; set; }

        [JsonPropertyName("parcelasPagas")]
        public int ParcelasPagas { get; set; }

        [JsonPropertyName("id_categoria")]
        public int? IdCategoria { get; set; }

        [JsonPropertyName("id_wallet")]
        public int? IdWallet { get; set; }

        [JsonPropertyName("id_credit_card")]
        public int? IdCreditCard { get; set; }

        [JsonPropertyName("categoria")]
        public string? CategoryName { get; set; }

        [JsonPropertyName("categoryIcon")]
        public string? CategoryIcon { get; set; }
        
        [JsonPropertyName("categoryColor")]
        public string? CategoryColor { get; set; }
    }
}
