using FinanceApp.Shared.Data;
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
    public class CreditCardsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CreditCardsController(AppDbContext context)
        {
            _context = context;
        }

        private int GetUserId()
        {
            return int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetCards()
        {
            var userId = GetUserId();
            var cards = await _context.creditCards
                .Include(c => c.WalletPagamento)
                .Where(c => c.id_usuario == userId)
                .ToListAsync();

            var result = new List<object>();

            foreach (var card in cards)
            {
                // Calculate Current Invoice (Simplistic: Sum of expenses in current month cycle)
                // This is a placeholder for complex logic.
                var invoice = await _context.lancamentos
                    .Where(l => l.id_credit_card == card.id_credit_card && l.nr_parcelasPagas < l.nr_parcelas)
                    .SumAsync(l => l.nr_valor); 

                result.Add(new
                {
                    card.id_credit_card,
                    card.nm_nome,
                    card.nm_bandeira,
                    card.nr_limite,
                    card.nr_dia_fechamento,
                    card.nr_dia_vencimento,
                    walletName = card.WalletPagamento?.nm_nome,
                    currentInvoice = invoice,
                    availableLimit = card.nr_limite - invoice
                });
            }

            return Ok(result);
        }

        [HttpPost]
        public async Task<ActionResult<CreditCard>> PostCard(CreditCard card)
        {
            card.id_usuario = GetUserId();
            _context.creditCards.Add(card);
            await _context.SaveChangesAsync();
            return CreatedAtAction("GetCards", new { id = card.id_credit_card }, card);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutCard(int id, CreditCard card)
        {
            if (id != card.id_credit_card) return BadRequest();
            var existing = await _context.creditCards.FindAsync(id);
            if (existing == null || existing.id_usuario != GetUserId()) return NotFound();

            existing.nm_nome = card.nm_nome;
            existing.nm_bandeira = card.nm_bandeira;
            existing.nr_limite = card.nr_limite;
            existing.nr_dia_fechamento = card.nr_dia_fechamento;
            existing.nr_dia_vencimento = card.nr_dia_vencimento;
            existing.id_wallet_pagamento = card.id_wallet_pagamento;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCard(int id)
        {
            var card = await _context.creditCards.FindAsync(id);
            if (card == null || card.id_usuario != GetUserId()) return NotFound();

            if (await _context.lancamentos.AnyAsync(l => l.id_credit_card == id))
                return BadRequest("Impossível excluir cartão com faturas.");

            _context.creditCards.Remove(card);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
