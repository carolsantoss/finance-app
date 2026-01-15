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
                ParcelasPagas = l.nr_parcelasPagas
            };
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<LancamentoResponse>>> GetLancamentos()
        {
            var userId = GetUserId();
            var lancamentos = await _context.lancamentos
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
            
            var entradas = lancamentos.Where(l => l.nm_tipo == "Entrada").Sum(l => l.nr_valor);
            var saidas = lancamentos.Where(l => l.nm_tipo == "Saída").Sum(l => l.nr_valor);

            return Ok(new DashboardSummary
            {
                Entradas = entradas,
                Saidas = saidas
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
                nr_parcelaInicial = 1 // default
            };
            
            _context.lancamentos.Add(lancamento);
            await _context.SaveChangesAsync();

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
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool LancamentoExists(int id)
        {
            return _context.lancamentos.Any(e => e.id_lancamento == id);
        }
    }
}
