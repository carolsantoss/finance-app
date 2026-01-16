using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinanceApp.Shared.Models
{
    public class Category
    {
        [Key]
        public int id_categoria { get; set; }

        public string nm_nome { get; set; } = string.Empty;

        public string nm_icone { get; set; } = string.Empty; // Name of the icon (e.g., Lucide icon name)

        public string nm_cor { get; set; } = "#FFFFFF"; // Hex Color

        public string nm_tipo { get; set; } = "Saída"; // "Entrada" or "Saída"

        public int? id_usuario { get; set; } // Null = System Default, Not Null = User Custom
    }
}
