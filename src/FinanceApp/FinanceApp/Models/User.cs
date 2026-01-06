using System.ComponentModel.DataAnnotations;

namespace FinanceApp.Models
{
    public class User
    {
        [Key]
        public int hs_id_user { get; set; }

        public string hs_nome { get; set; } = string.Empty;

        public string hs_email { get; set; } = string.Empty;

        public string hs_password_hash { get; set; } = string.Empty;
    }
}
