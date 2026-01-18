using System;
using System.Collections.Generic;

namespace FinanceApp.Shared.DTOs
{
    public class InviteGoalRequest
    {
        public string Email { get; set; } = string.Empty;
    }

    public class GoalMemberDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Role { get; set; } = string.Empty;
        public decimal TotalContribution { get; set; }
    }

    public class GoalDetailDto
    {
        public int Id { get; set; }
        public string Titulo { get; set; } = string.Empty;
        public decimal ValorObjetivo { get; set; }
        public decimal ValorAtual { get; set; }
        public DateTime? Prazo { get; set; }
        public bool IsOwner { get; set; }
        public List<GoalMemberDto> Members { get; set; } = new List<GoalMemberDto>();
    }
}
