using FinanceApp.Shared.Data;
using FinanceApp.Shared.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace FinanceApp.API.Controllers
{
    [Authorize]
    [Route("api/goals")]
    [ApiController]
    public class GoalsController : ControllerBase
    {
        private readonly AppDbContext _context;

        public GoalsController(AppDbContext context)
        {
            _context = context;
        }

        private int GetUserId()
        {
            return int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        }

        // GET: api/goals
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Goal>>> GetGoals()
        {
            var userId = GetUserId();
            return await _context.goals
                .Where(g => g.id_usuario == userId)
                .OrderBy(g => g.dt_prazo)
                .ToListAsync();
        }

        // GET: api/goals/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Goal>> GetGoal(int id)
        {
            var userId = GetUserId();
            var goal = await _context.goals
                .FirstOrDefaultAsync(g => g.id_goal == id && g.id_usuario == userId);

            if (goal == null)
            {
                return NotFound();
            }

            return goal;
        }

        // POST: api/goals
        [HttpPost]
        public async Task<ActionResult<Goal>> PostGoal(Goal goal)
        {
            var userId = GetUserId();
            goal.id_usuario = userId;

            _context.goals.Add(goal);
            await _context.SaveChangesAsync();

            return CreatedAtAction("GetGoal", new { id = goal.id_goal }, goal);
        }

        // PUT: api/goals/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutGoal(int id, Goal goal)
        {
            if (id != goal.id_goal)
            {
                return BadRequest();
            }

            var userId = GetUserId();
            var existingGoal = await _context.goals
                .FirstOrDefaultAsync(g => g.id_goal == id && g.id_usuario == userId);

            if (existingGoal == null)
            {
                return NotFound();
            }

            // Update fields
            existingGoal.nm_titulo = goal.nm_titulo;
            existingGoal.nr_valorObjetivo = goal.nr_valorObjetivo;
            existingGoal.nr_valorAtual = goal.nr_valorAtual;
            existingGoal.dt_prazo = goal.dt_prazo;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
               throw;
            }

            return NoContent();
        }

        // DELETE: api/goals/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteGoal(int id)
        {
            var userId = GetUserId();
            var goal = await _context.goals
                .FirstOrDefaultAsync(g => g.id_goal == id && g.id_usuario == userId);

            if (goal == null)
            {
                return NotFound();
            }

            _context.goals.Remove(goal);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}
