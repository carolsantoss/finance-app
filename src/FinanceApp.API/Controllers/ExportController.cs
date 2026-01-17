using FinanceApp.Shared.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace FinanceApp.API.Controllers
{
    [Authorize]
    [Route("api/export")]
    [ApiController]
    public class ExportController : ControllerBase
    {
        private readonly AppDbContext _context;

        public ExportController(AppDbContext context)
        {
            _context = context;
        }

        private int GetUserId()
        {
            return int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        }

        [HttpGet("json")]
        public async Task<IActionResult> ExportJson()
        {
            var userId = GetUserId();
            
            // Fetch User Data
            var user = await _context.users.FindAsync(userId);
            
            // Fetch Related Data
            var transactions = await _context.lancamentos
                .Include(l => l.Categoria)
                .Where(l => l.id_usuario == userId)
                .ToListAsync();

            var goals = await _context.goals
                .Where(g => g.id_usuario == userId)
                .ToListAsync();

            var budgets = await _context.budgets
                .Where(b => b.id_usuario == userId)
                .ToListAsync();

            var exportData = new
            {
                UserProfile = new
                {
                    user.nm_nomeUsuario,
                    user.nm_email,
                    user.nm_funcao,
                    user.nr_telefone,
                    user.ds_sobre
                },
                Transactions = transactions,
                Goals = goals,
                Budgets = budgets,
                ExportedAt = DateTime.UtcNow
            };

            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                ReferenceHandler = ReferenceHandler.IgnoreCycles
            };

            var json = JsonSerializer.Serialize(exportData, options);
            var bytes = System.Text.Encoding.UTF8.GetBytes(json);

            var fileName = $"finance_export_{userId}_{DateTime.Now:yyyyMMdd}.json";
            return File(bytes, "application/json", fileName);
        }
    }
}
