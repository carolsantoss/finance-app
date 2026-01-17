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

            var data = await _context.lancamentos
                .Where(l => l.id_usuario == userId &&
                            l.nm_tipo == "Saída" &&
                            l.dt_dataLancamento.Month == month &&
                            l.dt_dataLancamento.Year == year)
                .GroupBy(l => l.Categoria.nm_nome)
                .Select(g => new
                {
                    Category = g.Key ?? "Sem Categoria",
                    Total = g.Sum(l => l.nr_valor),
                    Color = g.First().Categoria.nm_cor ?? "#ccc"
                })
                .OrderByDescending(x => x.Total)
                .ToListAsync();

            return Ok(data);
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
