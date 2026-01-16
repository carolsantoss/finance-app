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
    public class WalletsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public WalletsController(AppDbContext context)
        {
            _context = context;
        }

        private int GetUserId()
        {
            return int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        }

        // GET: api/wallets
        [HttpGet]
        public async Task<ActionResult<IEnumerable<object>>> GetWallets()
        {
            var userId = GetUserId();
            var wallets = await _context.wallets
                .Where(w => w.id_usuario == userId)
                .ToListAsync();

            var result = new List<object>();

            foreach (var wallet in wallets)
            {
                // Calculate Balance
                // Balance = Initial Balance + Income - Expense (Where id_wallet == wallet.id)
                // Note: Credit Transactions don't affect balance until paid (which would be a separate transaction)
                
                var income = await _context.lancamentos
                    .Where(l => l.id_wallet == wallet.id_wallet && l.nm_tipo == "Entrada")
                    .SumAsync(l => l.nr_valor);

                var expense = await _context.lancamentos
                    .Where(l => l.id_wallet == wallet.id_wallet && l.nm_tipo == "Saída")
                    .SumAsync(l => l.nr_valor);

                result.Add(new
                {
                    wallet.id_wallet,
                    wallet.nm_nome,
                    wallet.nm_tipo,
                    wallet.nr_saldo_inicial,
                    currentBalance = wallet.nr_saldo_inicial + income - expense
                });
            }

            return Ok(result);
        }

        [HttpPost]
        public async Task<ActionResult<Wallet>> PostWallet(Wallet wallet)
        {
            wallet.id_usuario = GetUserId();
            _context.wallets.Add(wallet);
            await _context.SaveChangesAsync();
            return CreatedAtAction("GetWallets", new { id = wallet.id_wallet }, wallet);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutWallet(int id, Wallet wallet)
        {
            if (id != wallet.id_wallet) return BadRequest();
            
            var existing = await _context.wallets.FindAsync(id);
            if (existing == null || existing.id_usuario != GetUserId()) return NotFound();

            existing.nm_nome = wallet.nm_nome;
            existing.nm_tipo = wallet.nm_tipo;
            existing.nr_saldo_inicial = wallet.nr_saldo_inicial;

            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteWallet(int id)
        {
            var wallet = await _context.wallets.FindAsync(id);
            if (wallet == null || wallet.id_usuario != GetUserId()) return NotFound();

            // Check dependencies
            if (await _context.lancamentos.AnyAsync(l => l.id_wallet == id))
                return BadRequest("Não é possível excluir carteira com lançamentos vinculados.");

            _context.wallets.Remove(wallet);
            await _context.SaveChangesAsync();
            return NoContent();
        }
    }
}
