using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinanceApp.Shared.Models
{
    public class GoalMember
    {
        [Key]
        public int id_goal_member { get; set; }

        public int id_goal { get; set; }
        [ForeignKey("id_goal")]
        public Goal? Goal { get; set; }

        public int id_usuario { get; set; }
        [ForeignKey("id_usuario")]
        public User? Usuario { get; set; }

        public string nm_role { get; set; } = "Member"; // "Owner", "Member"
        public DateTime dt_joined { get; set; } = DateTime.UtcNow;
    }
}
