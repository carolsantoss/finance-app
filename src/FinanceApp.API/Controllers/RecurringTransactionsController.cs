using FinanceApp.Shared.Data;
using FinanceApp.Shared.DTOs;
using FinanceApp.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace FinanceApp.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RecurringTransactionsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public RecurringTransactionsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/recurringtransactions
        [HttpGet]
        [Authorize]
        public async Task<ActionResult<IEnumerable<RecurringTransactionDTO>>> GetRecurringTransactions()
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

            var recurring = await _context.recurringTransactions
                .Include(r => r.Categoria)
                .Where(r => r.id_usuario == userId)
                .ToListAsync();

            return Ok(recurring.Select(r => new RecurringTransactionDTO
            {
                Id = r.id_transacaoRecorrente,
                Descricao = r.nm_descricao,
                Valor = r.nr_valor,
                Tipo = r.nm_tipo,
                CategoryId = r.id_categoria,
                CategoryName = r.Categoria?.nm_nome,
                WalletId = r.id_wallet,
                CreditCardId = r.id_credit_card,
                Frequencia = r.nm_frequencia,
                DataInicio = r.dt_inicio,
                DataFim = r.dt_fim,
                UltimaProcessamento = r.dt_ultimaProcessamento,
                Ativo = r.fl_ativo
            }));
        }

        // POST: api/recurringtransactions
        [HttpPost]
        [Authorize]
        public async Task<ActionResult<RecurringTransaction>> CreateRecurringTransaction(CreateRecurringTransactionDTO request)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

            var recurring = new RecurringTransaction
            {
                id_usuario = userId,
                nm_descricao = request.Descricao,
                nr_valor = request.Valor,
                nm_tipo = request.Tipo,
                id_categoria = request.CategoryId,
                id_wallet = request.WalletId,
                id_credit_card = request.CreditCardId,
                nm_frequencia = request.Frequencia,
                dt_inicio = request.DataInicio,
                dt_fim = request.DataFim,
                fl_ativo = true
            };

            _context.recurringTransactions.Add(recurring);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetRecurringTransactions), new { id = recurring.id_transacaoRecorrente }, recurring);
        }

        // PUT: api/recurringtransactions/{id}
        [HttpPut("{id}")]
        [Authorize]
        public async Task<IActionResult> UpdateRecurringTransaction(int id, [FromBody] RecurringTransactionDTO request)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var recurring = await _context.recurringTransactions.FirstOrDefaultAsync(r => r.id_transacaoRecorrente == id && r.id_usuario == userId);

            if (recurring == null) return NotFound();

            recurring.nm_descricao = request.Descricao;
            recurring.nr_valor = request.Valor;
            recurring.nm_tipo = request.Tipo;
            recurring.id_categoria = request.CategoryId;
            recurring.id_wallet = request.WalletId;
            recurring.id_credit_card = request.CreditCardId;
            recurring.nm_frequencia = request.Frequencia;
            recurring.dt_inicio = request.DataInicio;
            recurring.dt_fim = request.DataFim;
            recurring.fl_ativo = request.Ativo;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // POST: api/recurringtransactions/process
        // This endpoint checks ALL recurring transactions system-wide and generates Lancamentos if due.
        // Should be restricted to Admin or a specific API Key.
        [HttpPost("process")]
        [AllowAnonymous] // We will check a specific header "X-Scheduler-Secret"
        public async Task<IActionResult> ProcessRecurringTransactions([FromHeader(Name = "X-Scheduler-Secret")] string? secret)
        {
            // Simple security check for the scheduler
            var envSecret = Environment.GetEnvironmentVariable("SCHEDULER_SECRET") ?? "DefaultSecret123";
            if (secret != envSecret)
            {
                return Unauthorized("Invalid Scheduler Secret");
            }

            var activeRecurring = await _context.recurringTransactions
                .Where(r => r.fl_ativo)
                .ToListAsync();

            int processedCount = 0;
            var today = DateTime.Today;

            foreach (var r in activeRecurring)
            {
                // Check if expired
                if (r.dt_fim.HasValue && r.dt_fim.Value < today) continue;

                var nextDueDate = CalculateNextDueDate(r);

                // If due date is today or in the past, process it
                if (nextDueDate <= today)
                {
                    // Create Lancamento
                    var lancamento = new Lancamento
                    {
                        id_usuario = r.id_usuario,
                        nm_descricao = r.nm_descricao + " (Recorrente)",
                        nr_valor = r.nr_valor,
                        nm_tipo = r.nm_tipo,
                        dt_dataLancamento = nextDueDate, // Create with the actual due date
                        nm_formaPagamento = r.id_credit_card.HasValue ? "Crédito" : "Débito",
                        nr_parcelas = 1,
                        nr_parcelaInicial = 1,
                        nr_parcelasPagas = r.nm_tipo == "Saída" ? 0 : 1, // Income is usually paid immediately? Or follow logic.
                        id_categoria = r.id_categoria,
                        id_wallet = r.id_wallet,
                        id_credit_card = r.id_credit_card
                    };

                    _context.lancamentos.Add(lancamento);
                    
                    // Update last processed date
                    // If we processed it for Date X, update LastProcessed to Date X
                    r.dt_ultimaProcessamento = nextDueDate;
                    processedCount++;
                }
            }

            if (processedCount > 0)
            {
                await _context.SaveChangesAsync();
            }

            return Ok(new { Message = "Processing completed", ProcessedCount = processedCount });
        }

        private DateTime CalculateNextDueDate(RecurringTransaction r)
        {
            if (!r.dt_ultimaProcessamento.HasValue)
            {
                return r.dt_inicio;
            }

            var last = r.dt_ultimaProcessamento.Value;

            return r.nm_frequencia switch
            {
                "Diário" => last.AddDays(1),
                "Semanal" => last.AddDays(7),
                "Mensal" => last.AddMonths(1),
                "Anual" => last.AddYears(1),
                _ => last.AddMonths(1)
            };
        }
    }
}
