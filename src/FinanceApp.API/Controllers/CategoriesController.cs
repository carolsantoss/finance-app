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
    public class CategoriesController : ControllerBase
    {
        private readonly AppDbContext _context;

        public CategoriesController(AppDbContext context)
        {
            _context = context;
        }

        private int GetUserId()
        {
            return int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        }

        // GET: api/categories
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Category>>> GetCategories()
        {
            var userId = GetUserId();
            // Return System Defaults (id_usuario == null) AND User's Custom Categories
            return await _context.categories
                .Where(c => c.id_usuario == null || c.id_usuario == userId)
                .OrderBy(c => c.id_usuario) // Defaults first
                .ThenBy(c => c.nm_nome)
                .ToListAsync();
        }

        // POST: api/categories
        [HttpPost]
        public async Task<ActionResult<Category>> PostCategory(Category category)
        {
            var userId = GetUserId();
            
            // Force user ownership
            category.id_usuario = userId;
            category.id_categoria = 0; // Reset ID for auto-increment

            _context.categories.Add(category);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetCategories", new { id = category.id_categoria }, category);
        }

        // PUT: api/categories/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> PutCategory(int id, Category category)
        {
            var userId = GetUserId();
            var existing = await _context.categories.FindAsync(id);

            if (existing == null) return NotFound();

            // Security: Only allow editing if it belongs to the user
            if (existing.id_usuario != userId)
            {
                return Forbid("Você não pode editar categorias do sistema ou de outros usuários.");
            }

            existing.nm_nome = category.nm_nome;
            existing.nm_icone = category.nm_icone;
            existing.nm_cor = category.nm_cor;
            existing.nm_tipo = category.nm_tipo;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!CategoryExists(id)) return NotFound();
                else throw;
            }

            return NoContent();
        }

        // DELETE: api/categories/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var userId = GetUserId();
            var category = await _context.categories.FindAsync(id);

            if (category == null) return NotFound();

            // Security Check
            if (category.id_usuario != userId)
            {
                return Forbid("Você não pode excluir categorias do sistema.");
            }

            // Check if used in transactions
            if (await _context.lancamentos.AnyAsync(l => l.id_categoria == id))
            {
                return BadRequest("Não é possível excluir esta categoria pois existem lançamentos vinculados a ela.");
            }

            _context.categories.Remove(category);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool CategoryExists(int id)
        {
            return _context.categories.Any(e => e.id_categoria == id);
        }
    }
}
