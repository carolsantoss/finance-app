using System.ComponentModel.DataAnnotations;

namespace FinanceApp.Shared.Models
{
    public class User
    {
        [Key]
        public int id_usuario { get; set; }

        public string nm_nomeUsuario { get; set; } = string.Empty;

        public string nm_email { get; set; } = string.Empty;

        public string hs_senha { get; set; } = string.Empty;
    }
}
