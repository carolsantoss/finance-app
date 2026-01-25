using FinanceApp.Shared.Data;
using FinanceApp.Shared.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace FinanceApp.API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ReportsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ReportsController(AppDbContext context)
        {
            _context = context;
        }

        [HttpGet("expenses-by-category")]
        public async Task<ActionResult> GetExpensesByCategory([FromQuery] int month, [FromQuery] int year)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

            // Strategy: Fetch all "Saída" transactions that could possibly affect this month.
            // 1. Standard: matches Month/Year
            // 2. Installments: Started BEFORE or ON this month, and end ON or AFTER this month.
            //    Roughly: Date <= EndOfCurrentMonth.
            //    To avoid fetching ALL history, we could limit to Date >= (EndOfCurrentMonth - MaxInstallments).
            //    Let's assume max installments = 120 (10 years).

            var targetDate = new DateTime(year, month, 1);
            var endOfTargetMonth = targetDate.AddMonths(1).AddDays(-1);
            var lookbackDate = targetDate.AddMonths(-120); // 10 years lookback for installments

            // Optimize: Fetch everything in range and filter in memory for complex installment logic
            var transactions = await _context.lancamentos
                .Include(l => l.Categoria)
                .Where(l => l.id_usuario == userId &&
                            l.nm_tipo == "Saída" &&
                            l.dt_dataLancamento >= lookbackDate &&
                            l.dt_dataLancamento <= endOfTargetMonth)
                .ToListAsync();

            var categoryTotals = new Dictionary<string, (decimal Total, string Color)>();

            foreach (var t in transactions)
            {
                bool include = false;
                decimal valueToSum = 0;

                if (t.nr_parcelas <= 1 || t.nm_formaPagamento != "Crédito")
                {
                    // Standard: Must match month/year exactly
                    if (t.dt_dataLancamento.Month == month && t.dt_dataLancamento.Year == year)
                    {
                        include = true;
                        valueToSum = t.nr_valor;
                    }
                }
                else
                {
                    // Installment Logic
                    // Calculate if this specific month (targetDate) corresponds to one of the installments
                    // Installment 1 is at t.dt_dataLancamento
                    
                    // Month diff
                    var startMonthIndex = t.dt_dataLancamento.Year * 12 + t.dt_dataLancamento.Month;
                    var targetMonthIndex = year * 12 + month;
                    
                    var diff = targetMonthIndex - startMonthIndex;

                    // If diff >= 0 (Target is after start) AND diff < parcels (Target is within range)
                    if (diff >= 0 && diff < t.nr_parcelas)
                    {
                        include = true;
                        valueToSum = t.nr_valor / t.nr_parcelas;
                    }
                }

                if (include)
                {
                    var catName = t.Categoria?.nm_nome ?? "Sem Categoria";
                    var color = t.Categoria?.nm_cor ?? "#ccc";

                    if (!categoryTotals.ContainsKey(catName))
                    {
                        categoryTotals[catName] = (0, color);
                    }
                    
                    var current = categoryTotals[catName];
                    categoryTotals[catName] = (current.Total + valueToSum, current.Color);
                }
            }

            var result = categoryTotals
                .Select(kvp => new
                {
                    Category = kvp.Key,
                    Total = Math.Round(kvp.Value.Total, 2),
                    Color = kvp.Value.Color
                })
                .OrderByDescending(x => x.Total)
                .ToList();

            return Ok(result);
        }

        [HttpGet("net-worth-evolution")]
        public async Task<ActionResult> GetNetWorthEvolution()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var endDate = DateTime.Today;
            var startDate = endDate.AddMonths(-11).AddDays(1 - endDate.Day); // Last 12 months (roughly)

            // Get all transactions
            var transactions = await _context.lancamentos
                .Where(l => l.id_usuario == userId && l.dt_dataLancamento <= endDate)
                .OrderBy(l => l.dt_dataLancamento)
                .Select(l => new { l.dt_dataLancamento, l.nr_valor, l.nm_tipo })
                .ToListAsync();

            // Initial Balance (Wallets) - Need to handle this carefully.
            // Simplified approach: Sum of all time transactions up to point X.
            // Ideally should check `wallets.nr_saldo_inicial` too, but for evolution charts, 
            // relative growth is often enough, or we fetch initial balances.
            
            var initialBalances = await _context.wallets
                .Where(w => w.id_usuario == userId)
                .SumAsync(w => w.nr_saldo_inicial);

            var monthlyData = new List<object>();
            
            // Running balance calculation
            // Iterate months
            for (int i = 0; i < 12; i++)
            {
                var currentMonth = startDate.AddMonths(i);
                var endOfMonth = new DateTime(currentMonth.Year, currentMonth.Month, DateTime.DaysInMonth(currentMonth.Year, currentMonth.Month));
                
                if (endOfMonth > DateTime.Today) endOfMonth = DateTime.Today;

                // Calculate balance up to this date
                var income = transactions.Where(t => t.dt_dataLancamento <= endOfMonth && t.nm_tipo == "Entrada").Sum(t => t.nr_valor);
                var expenses = transactions.Where(t => t.dt_dataLancamento <= endOfMonth && t.nm_tipo == "Saída").Sum(t => t.nr_valor);
                
                var totalBalance = initialBalances + income - expenses;

                monthlyData.Add(new 
                {
                    Month = currentMonth.ToString("MMM/yy"),
                    Balance = totalBalance
                });
            }

            return Ok(monthlyData);
        }
    }
}
