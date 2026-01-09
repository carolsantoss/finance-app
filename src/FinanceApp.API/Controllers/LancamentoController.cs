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
    [Route("api/[controller]")]
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

        [HttpGet]
        public async Task<ActionResult<IEnumerable<Lancamento>>> GetLancamentos()
        {
            var userId = GetUserId();
            return await _context.lancamentos
                .Where(l => l.id_usuario == userId)
                .OrderByDescending(l => l.dt_dataLancamento)
                .ToListAsync();
        }

        [HttpGet("summary")]
        public async Task<ActionResult<DashboardSummary>> GetSummary()
        {
            var userId = GetUserId();
            var lancamentos = await _context.lancamentos
                .Where(l => l.id_usuario == userId)
                .ToListAsync();

            var entradas = lancamentos.Where(l => l.nm_tipo.Contains("Entrada")).Sum(l => l.nr_valor);
            var saidas = lancamentos.Where(l => l.nm_tipo.Contains("Saída")).Sum(l => l.nr_valor);

            return Ok(new DashboardSummary
            {
                Entradas = entradas,
                Saidas = saidas
            });
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<Lancamento>> GetLancamento(int id)
        {
            var userId = GetUserId();
            var lancamento = await _context.lancamentos
                .FirstOrDefaultAsync(l => l.id_lancamento == id && l.id_usuario == userId);

            if (lancamento == null)
            {
                return NotFound();
            }

            return lancamento;
        }

        [HttpPost]
        public async Task<ActionResult<Lancamento>> PostLancamento(Lancamento lancamento)
        {
            var userId = GetUserId();
            lancamento.id_usuario = userId;
            
            _context.lancamentos.Add(lancamento);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetLancamento", new { id = lancamento.id_lancamento }, lancamento);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutLancamento(int id, Lancamento lancamento)
        {
            if (id != lancamento.id_lancamento)
            {
                return BadRequest();
            }

            var userId = GetUserId();
            var existing = await _context.lancamentos
                .FirstOrDefaultAsync(l => l.id_lancamento == id && l.id_usuario == userId);

            if (existing == null)
            {
                return NotFound();
            }

            // Simple update - in real app, might update fields individually
            _context.Entry(existing).CurrentValues.SetValues(lancamento);
            
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
