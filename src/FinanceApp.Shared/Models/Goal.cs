using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinanceApp.Shared.Models
{
    public class Goal
    {
        [Key]
        public int id_goal { get; set; }

        public string nm_titulo { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,2)")]
        public decimal nr_valorObjetivo { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal nr_valorAtual { get; set; }

        public DateTime? dt_prazo { get; set; }

        public int id_usuario { get; set; }

        [ForeignKey("id_usuario")]
        public User? Usuario { get; set; }
    }
}
