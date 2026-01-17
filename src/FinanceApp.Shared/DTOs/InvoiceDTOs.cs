namespace FinanceApp.Shared.DTOs
{
    public class InvoiceDTO
    {
        public int Id { get; set; }
        public string Descricao { get; set; } = string.Empty;
        public decimal Valor { get; set; }
        public int DiaVencimento { get; set; }
        public bool Ativo { get; set; }
        public DateTime? UltimoPagamento { get; set; }
        public bool PagoEsteMes { get; set; }
    }

    public class CreateInvoiceDTO
    {
        public string Descricao { get; set; } = string.Empty;
        public decimal Valor { get; set; }
        public int DiaVencimento { get; set; }
    }

    public class UpdateInvoiceDTO
    {
        public string Descricao { get; set; } = string.Empty;
        public decimal Valor { get; set; }
        public int DiaVencimento { get; set; }
        public bool Ativo { get; set; }
    }
}
