using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinanceApp.Shared.Models
{
    public class Notification
    {
        [Key]
        public int id_notificacao { get; set; }

        public int id_usuario { get; set; }

        public string nm_titulo { get; set; } = string.Empty;

        public string ds_mensagem { get; set; } = string.Empty;

        public bool fl_lida { get; set; } = false;

        public DateTime dt_criacao { get; set; } = DateTime.UtcNow;

        public string nm_tipo { get; set; } = "SYSTEM"; // "EMAIL", "SYSTEM", "ALERT"
    }
}
