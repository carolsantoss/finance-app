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
    public class BudgetsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public BudgetsController(AppDbContext context)
        {
            _context = context;
        }

        // GET: api/budgets/{month}/{year}
        [HttpGet("{month}/{year}")]
        public async Task<ActionResult<IEnumerable<BudgetDTO>>> GetBudgets(int month, int year)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

            // Fetch budgets for the user and specific month
            var budgets = await _context.budgets
                .Include(b => b.Categoria)
                .Where(b => b.id_usuario == userId && b.nr_mes == month && b.nr_ano == year)
                .ToListAsync();

            var budgetDtos = new List<BudgetDTO>();

            foreach (var b in budgets)
            {
                // Calculate used amount for this category in this month
                // Note: This considers all transactions for the category, regardless of wallet/card
                var spent = await _context.lancamentos
                    .Where(l => l.id_usuario == userId &&
                                l.id_categoria == b.id_categoria &&
                                l.dt_dataLancamento.Month == month &&
                                l.dt_dataLancamento.Year == year &&
                                l.nm_tipo == "Saída") // Only Expenses count towards budget
                    .SumAsync(l => l.nr_valor);

                budgetDtos.Add(new BudgetDTO
                {
                    Id = b.id_budget,
                    CategoryId = b.id_categoria,
                    CategoryName = b.Categoria?.nm_nome ?? "Unknown",
                    ValorLimite = b.nr_valorLimite,
                    ValorGasto = spent,
                    Mes = b.nr_mes,
                    Ano = b.nr_ano,
                    AlertaPorcentagem = b.nr_alertaPorcentagem
                });
            }

            return Ok(budgetDtos);
        }

        // POST: api/budgets
        [HttpPost]
        public async Task<ActionResult<Budget>> CreateOrUpdateBudget(CreateBudgetDTO request)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

            // Check if budget already exists
            var existingBudget = await _context.budgets
                .FirstOrDefaultAsync(b => b.id_usuario == userId && 
                                          b.id_categoria == request.CategoryId &&
                                          b.nr_mes == request.Mes &&
                                          b.nr_ano == request.Ano);

            if (existingBudget != null)
            {
                // Update
                existingBudget.nr_valorLimite = request.ValorLimite;
                existingBudget.nr_alertaPorcentagem = request.AlertaPorcentagem;
                await _context.SaveChangesAsync();
                return Ok(existingBudget);
            }
            else
            {
                // Create
                var newBudget = new Budget
                {
                    id_usuario = userId,
                    id_categoria = request.CategoryId,
                    nr_valorLimite = request.ValorLimite,
                    nr_mes = request.Mes,
                    nr_ano = request.Ano,
                    nr_alertaPorcentagem = request.AlertaPorcentagem
                };
                _context.budgets.Add(newBudget);
                await _context.SaveChangesAsync();
                return CreatedAtAction(nameof(GetBudgets), new { month = request.Mes, year = request.Ano }, newBudget);
            }
        }

        // DELETE: api/budgets/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteBudget(int id)
        {
            var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
            var budget = await _context.budgets.FirstOrDefaultAsync(b => b.id_budget == id && b.id_usuario == userId);

            if (budget == null) return NotFound();

            _context.budgets.Remove(budget);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
