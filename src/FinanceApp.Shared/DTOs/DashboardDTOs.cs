namespace FinanceApp.Shared.DTOs
{
    public class DashboardSummary
    {
        public decimal Entradas { get; set; }
        public decimal Saidas { get; set; }
        public decimal Saldo => Entradas - Saidas;
    }
}
