namespace FinanceApp.Shared.DTOs
{
    public class DashboardSummary
    {
        public decimal Entradas { get; set; }
        public decimal Saidas { get; set; }
        public decimal Saldo => Entradas - Saidas;
        public decimal PercentageChange { get; set; }
    }

    public class MonthlyFinancialData
    {
        public string Month { get; set; } = string.Empty;
        public decimal Income { get; set; }
        public decimal Expense { get; set; }
    }

    public class ChartDataResponse
    {
        public List<string> Labels { get; set; } = new();
        public List<decimal> IncomeData { get; set; } = new();
        public List<decimal> ExpenseData { get; set; } = new();
    }
}
