using FinanceApp.Shared.Data;
using FinanceApp.Shared.Models;
using FinanceApp.Shared.DTOs;
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
            
            // Get goals where user is Owner OR Member
            var memberGoalIds = await _context.goal_members
                .Where(gm => gm.id_usuario == userId)
                .Select(gm => gm.id_goal)
                .ToListAsync();

            var goals = await _context.goals
                .Where(g => g.id_usuario == userId || memberGoalIds.Contains(g.id_goal))
                .OrderBy(g => g.dt_prazo)
                .ToListAsync();

            return goals;
        }

        // GET: api/goals/5
        [HttpGet("{id}")]
        public async Task<ActionResult<GoalDetailDto>> GetGoal(int id)
        {
            var userId = GetUserId();
            var goal = await _context.goals
                .Include(g => g.Usuario)
                .FirstOrDefaultAsync(g => g.id_goal == id);

            if (goal == null) return NotFound();

            // Check access
            var isMember = await _context.goal_members.AnyAsync(gm => gm.id_goal == id && gm.id_usuario == userId);
            if (goal.id_usuario != userId && !isMember)
            {
                return Forbid();
            }

            // Calculate contributions
            var contributions = await _context.lancamentos
                .Where(l => l.id_goal == id)
                .GroupBy(l => l.id_usuario)
                .Select(g => new { UserId = g.Key, Total = g.Sum(x => x.nr_valor) })
                .ToListAsync();

            // Build Members DTO
            var membersDto = new List<GoalMemberDto>();

            // Add Owner
            membersDto.Add(new GoalMemberDto
            {
                UserId = goal.id_usuario,
                UserName = goal.Usuario?.nm_nomeUsuario ?? "Owner",
                Email = goal.Usuario?.nm_email ?? "",
                Role = "Owner",
                TotalContribution = contributions.FirstOrDefault(c => c.UserId == goal.id_usuario)?.Total ?? 0
            });

            // Add Other Members
            var dbMembers = await _context.goal_members
                .Include(gm => gm.Usuario)
                .Where(gm => gm.id_goal == id)
                .ToListAsync();

            foreach (var mem in dbMembers)
            {
                membersDto.Add(new GoalMemberDto
                {
                    Id = mem.id_goal_member,
                    UserId = mem.id_usuario,
                    UserName = mem.Usuario?.nm_nomeUsuario ?? "User",
                    Email = mem.Usuario?.nm_email ?? "",
                    Role = mem.nm_role,
                    TotalContribution = contributions.FirstOrDefault(c => c.UserId == mem.id_usuario)?.Total ?? 0
                });
            }

            return new GoalDetailDto
            {
                Id = goal.id_goal,
                Titulo = goal.nm_titulo,
                ValorObjetivo = goal.nr_valorObjetivo,
                ValorAtual = goal.nr_valorAtual,
                Prazo = goal.dt_prazo,
                IsOwner = goal.id_usuario == userId,
                Members = membersDto
            };
        }

        // POST: api/goals
        [HttpPost]
        public async Task<ActionResult<Goal>> PostGoal(Goal goal)
        {
            var userId = GetUserId();
            goal.id_usuario = userId;
            // Initialize updated value 
            // goal.nr_valorAtual = 0; // Usually 0 start

            _context.goals.Add(goal);
            await _context.SaveChangesAsync();
            
            // Note: Owner is not added to GoalMembers by default, but treated implicitly as Owner. 
            // Or we could add them? Let's treat implicitly as per GetGoal logic.

            return CreatedAtAction("GetGoal", new { id = goal.id_goal }, goal);
        }

        // PUT: api/goals/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutGoal(int id, Goal goal)
        {
            if (id != goal.id_goal) return BadRequest();

            var userId = GetUserId();
            var existingGoal = await _context.goals
                .FirstOrDefaultAsync(g => g.id_goal == id);

            if (existingGoal == null) return NotFound();
            
            // Only owner can edit? Or members too? Let's say Owner only for settings.
            if (existingGoal.id_usuario != userId) return Forbid();

            existingGoal.nm_titulo = goal.nm_titulo;
            existingGoal.nr_valorObjetivo = goal.nr_valorObjetivo;
            existingGoal.dt_prazo = goal.dt_prazo;
            // ValorAtual is typically updated via Transactions, but if user edits it manually? 
            // Ideally should be calculated, but legacy logic might allow edit. Keeping it sync.
            existingGoal.nr_valorAtual = goal.nr_valorAtual; 

            await _context.SaveChangesAsync();
            return NoContent();
        }

        // DELETE: api/goals/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteGoal(int id)
        {
            var userId = GetUserId();
            var goal = await _context.goals.FindAsync(id);

            if (goal == null) return NotFound();
            if (goal.id_usuario != userId) return Forbid(); // Only owner deletes

            _context.goals.Remove(goal);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // POST: api/goals/5/invite
        [HttpPost("{id}/invite")]
        public async Task<IActionResult> Invite(int id, InviteGoalRequest request)
        {
            var userId = GetUserId();
            var goal = await _context.goals.FindAsync(id);
            if (goal == null) return NotFound();
            if (goal.id_usuario != userId) return Forbid(); // Only owner invites

            var userToInvite = await _context.users.FirstOrDefaultAsync(u => u.nm_email == request.Email);
            if (userToInvite == null) return BadRequest("User not found");

            if (userToInvite.id_usuario == userId) return BadRequest("Cannot invite yourself");

            var exists = await _context.goal_members
                .AnyAsync(gm => gm.id_goal == id && gm.id_usuario == userToInvite.id_usuario);
            
            if (exists) return BadRequest("User already a member");

            _context.goal_members.Add(new GoalMember
            {
                id_goal = id,
                id_usuario = userToInvite.id_usuario,
                nm_role = "Member",
                dt_joined = DateTime.UtcNow
            });

            await _context.SaveChangesAsync();
            return Ok();
        }
    }
}
