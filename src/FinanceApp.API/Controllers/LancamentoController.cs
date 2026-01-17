using FinanceApp.Shared.Data;
using FinanceApp.Shared.DTOs;
using FinanceApp.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace FinanceApp.API.Controllers
{
    [Authorize]
    [Route("api/lancamentos")]
    [ApiController]
    public class LancamentoController : ControllerBase
    {
        private readonly AppDbContext _context;

        public LancamentoController(AppDbContext context)
        {
            _context = context;
        }

        private int GetUserId()
        {
            return int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        }

        private static LancamentoResponse ToDto(Lancamento l)
        {
            return new LancamentoResponse
            {
                Id = l.id_lancamento,
                Tipo = l.nm_tipo,
                Descricao = l.nm_descricao,
                Valor = l.nr_valor,
                DataLancamento = l.dt_dataLancamento,
                FormaPagamento = l.nm_formaPagamento,
                Parcelas = l.nr_parcelas,
                ParcelasPagas = l.nr_parcelasPagas,
                IdCategoria = l.id_categoria,
                IdWallet = l.id_wallet,
                IdCreditCard = l.id_credit_card,
                IdGoal = l.id_goal,
                CategoryName = l.Categoria?.nm_nome,
                CategoryIcon = l.Categoria?.nm_icone,
                CategoryColor = l.Categoria?.nm_cor
            };
        }

        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<LancamentoResponse>>> Search(
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endDate,
            [FromQuery] string? categories, // Comma separated IDs
            [FromQuery] string? wallets,    // Comma separated IDs
            [FromQuery] string? type)       // "Entrada" or "Saída"
        {
            var userId = GetUserId();
            var query = _context.lancamentos
                .Include(l => l.Categoria)
                .Where(l => l.id_usuario == userId)
                .AsQueryable();

            if (startDate.HasValue)
                query = query.Where(l => l.dt_dataLancamento >= startDate.Value);
            
            if (endDate.HasValue)
                query = query.Where(l => l.dt_dataLancamento <= endDate.Value);

            if (!string.IsNullOrEmpty(categories))
            {
                var catIds = categories.Split(',').Select(int.Parse).ToList();
                query = query.Where(l => catIds.Contains(l.id_categoria ?? 0));
            }

            if (!string.IsNullOrEmpty(wallets))
            {
                var walletIds = wallets.Split(',').Select(int.Parse).ToList();
                query = query.Where(l => walletIds.Contains(l.id_wallet ?? 0) || walletIds.Contains(l.id_credit_card ?? 0));
            }

            if (!string.IsNullOrEmpty(type))
                query = query.Where(l => l.nm_tipo == type);

            var result = await query.OrderByDescending(l => l.dt_dataLancamento).ToListAsync();
            return Ok(result.Select(ToDto));
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<LancamentoResponse>>> GetLancamentos()
        {
            var userId = GetUserId();
            var lancamentos = await _context.lancamentos
                .Include(l => l.Categoria)
                .Where(l => l.id_usuario == userId)
                .OrderByDescending(l => l.dt_dataLancamento)
                .ToListAsync();

            return Ok(lancamentos.Select(ToDto));
        }

        [HttpGet("summary")]
        public async Task<ActionResult<DashboardSummary>> GetSummary()
        {
            var userId = GetUserId();
            var lancamentos = await _context.lancamentos
                .Where(l => l.id_usuario == userId)
                .ToListAsync();
            
            var now = DateTime.UtcNow;
            var firstDayOfCurrentMonth = new DateTime(now.Year, now.Month, 1);
            var endOfLastMonth = firstDayOfCurrentMonth.AddSeconds(-1);

            // Current Balance (All time)
            var entradas = lancamentos.Where(l => l.nm_tipo == "Entrada").Sum(l => l.nr_valor);
            var saidas = lancamentos.Where(l => l.nm_tipo == "Saída").Sum(l => l.nr_valor);
            var currentBalance = entradas - saidas;

            // Previous Balance (Up to end of last month)
            var previousEntradas = lancamentos
                .Where(l => l.nm_tipo == "Entrada" && l.dt_dataLancamento <= endOfLastMonth)
                .Sum(l => l.nr_valor);
            var previousSaidas = lancamentos
                .Where(l => l.nm_tipo == "Saída" && l.dt_dataLancamento <= endOfLastMonth)
                .Sum(l => l.nr_valor);
            var previousBalance = previousEntradas - previousSaidas;

            // Calculate Percentage Change
            decimal percentageChange = 0;
            if (previousBalance != 0)
            {
                percentageChange = ((currentBalance - previousBalance) / Math.Abs(previousBalance)) * 100;
            }
            else if (currentBalance != 0)
            {
                 percentageChange = 100; // From 0 to something is 100% (or infinite, but 100 is safer for UI)
            }

            return Ok(new DashboardSummary
            {
                Entradas = entradas,
                Saidas = saidas,
                PercentageChange = percentageChange
            });
        }

        [HttpGet("chart")]
        public async Task<ActionResult<ChartDataResponse>> GetChartData()
        {
            var userId = GetUserId();
            var today = DateTime.Today;
            var sixMonthsAgo = today.AddMonths(-5);
            var startDate = new DateTime(sixMonthsAgo.Year, sixMonthsAgo.Month, 1);

            var lancamentos = await _context.lancamentos
                .Where(l => l.id_usuario == userId && l.dt_dataLancamento >= startDate)
                .ToListAsync();

            var monthlyData = new List<MonthlyFinancialData>();

            for (int i = 0; i < 6; i++)
            {
                var currentMonth = startDate.AddMonths(i);
                var monthName = currentMonth.ToString("MMM", new System.Globalization.CultureInfo("pt-BR"));
                
                var monthLancamentos = lancamentos
                    .Where(l => l.dt_dataLancamento.Year == currentMonth.Year && l.dt_dataLancamento.Month == currentMonth.Month)
                    .ToList();

                monthlyData.Add(new MonthlyFinancialData
                {
                    Month = char.ToUpper(monthName[0]) + monthName.Substring(1),
                    Income = monthLancamentos.Where(l => l.nm_tipo == "Entrada").Sum(l => l.nr_valor),
                    Expense = monthLancamentos.Where(l => l.nm_tipo == "Saída").Sum(l => l.nr_valor)
                });
            }

            return Ok(new ChartDataResponse
            {
                Labels = monthlyData.Select(d => d.Month).ToList(),
                IncomeData = monthlyData.Select(d => d.Income).ToList(),
                ExpenseData = monthlyData.Select(d => d.Expense).ToList()
            });
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<LancamentoResponse>> GetLancamento(int id)
        {
            var userId = GetUserId();
            var lancamento = await _context.lancamentos
                .Include(l => l.Categoria)
                .FirstOrDefaultAsync(l => l.id_lancamento == id && l.id_usuario == userId);

            if (lancamento == null)
            {
                return NotFound();
            }

            return Ok(ToDto(lancamento));
        }

        [HttpPost]
        public async Task<ActionResult<LancamentoResponse>> PostLancamento(CreateLancamentoRequest request)
        {
            var userId = GetUserId();
            
            // Validate Logic check if needed?
            // For now, trust the frontend/request.

            var lancamento = new Lancamento
            {
                id_usuario = userId,
                nm_tipo = request.Tipo,
                nm_descricao = request.Descricao,
                nr_valor = request.Valor,
                dt_dataLancamento = request.DataLancamento,
                nm_formaPagamento = request.FormaPagamento,
                nr_parcelas = request.Parcelas,
                nr_parcelasPagas = request.ParcelasPagas,
                nr_parcelaInicial = 1, // default
                id_categoria = request.IdCategoria,
                id_wallet = request.IdWallet,
                id_credit_card = request.IdCreditCard,
                id_goal = request.IdGoal
            };
            
            _context.lancamentos.Add(lancamento);

            // Update Goal Value if linked
            if (request.IdGoal.HasValue)
            {
               var goal = await _context.goals.FirstOrDefaultAsync(g => g.id_goal == request.IdGoal.Value && g.id_usuario == userId);
               if (goal != null)
               {
                   // If 'Entrada', add to goal. If 'Saida', maybe subtract? 
                   // User said "Entrada para uma Meta".
                   // Assuming only positive flow for now or based on Type.
                   // If I invest 100 in Goal, it is an Expense from Wallet? Or Income to Goal?
                   // The prompt said "Entrada para uma Meta". I will assume it increases goal value regardless.
                   // But logic dictates:
                   if (request.Tipo == "Entrada")
                   {
                      goal.nr_valorAtual += request.Valor;
                   }
                   // If user assigns 'Saida' to a Goal (e.g. spending FROM the goal saving?), should it decrease?
                   // For now, let's implement increase for 'Entrada'.
               }
            }

            await _context.SaveChangesAsync();

            // Re-fetch to include relations if needed for DTO
            // Optimization: Just map if we trust it, or null coalescing.
            // For response, let's load Category to look nice immediately.
            await _context.Entry(lancamento).Reference(l => l.Categoria).LoadAsync();

            return CreatedAtAction("GetLancamento", new { id = lancamento.id_lancamento }, ToDto(lancamento));
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutLancamento(int id, UpdateLancamentoRequest request)
        {
            var userId = GetUserId();
            var existing = await _context.lancamentos
                .FirstOrDefaultAsync(l => l.id_lancamento == id && l.id_usuario == userId);

            if (existing == null)
            {
                return NotFound();
            }

            existing.nm_tipo = request.Tipo;
            existing.nm_descricao = request.Descricao;
            existing.nr_valor = request.Valor;
            existing.dt_dataLancamento = request.DataLancamento;
            existing.nm_formaPagamento = request.FormaPagamento;
            existing.nr_parcelas = request.Parcelas;
            existing.nr_parcelasPagas = request.ParcelasPagas;
            existing.id_categoria = request.IdCategoria;
            existing.id_wallet = request.IdWallet;
            existing.id_credit_card = request.IdCreditCard;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!LancamentoExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteLancamento(int id)
        {
            var userId = GetUserId();
            var lancamento = await _context.lancamentos
                .FirstOrDefaultAsync(l => l.id_lancamento == id && l.id_usuario == userId);

            if (lancamento == null)
            {
                return NotFound();
            }

            _context.lancamentos.Remove(lancamento);
            
            // Revert Goal Value
            if (lancamento.id_goal.HasValue)
            {
                var goal = await _context.goals.FindAsync(lancamento.id_goal.Value);
                if (goal != null)
                {
                    if (lancamento.nm_tipo == "Entrada")
                    {
                        goal.nr_valorAtual -= lancamento.nr_valor;
                    }
                }
            }

            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool LancamentoExists(int id)
        {
            return _context.lancamentos.Any(e => e.id_lancamento == id);
        }
    }
}
