using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace FinanceApp.Shared.Models
{
    public class User
    {
        [Key]
        public int id_usuario { get; set; }

        public string nm_nomeUsuario { get; set; } = string.Empty;

        public string nm_email { get; set; } = string.Empty;

        public string hs_senha { get; set; } = string.Empty;

        public bool fl_admin { get; set; } = false;

        // Profile Details
        public string? nm_funcao { get; set; } // Job Title
        public string? nr_telefone { get; set; } // Phone
        public string? ds_sobre { get; set; } // Bio/About

        // Security Fields
        public bool fl_emailConfirmado { get; set; } = false;
        public string? cd_tokenConfirmacao { get; set; }
        
        public string? cd_tokenRecuperacao { get; set; }
        public DateTime? dt_expiracaoToken { get; set; }
        
        public bool fl_2faHabilitado { get; set; } = false;
        public string? cd_segredo2FA { get; set; }

        // Referral System
        public string? cd_referralCode { get; set; } // Unique Code
        
        public int? id_referrer { get; set; } // Who invited me
        
        [ForeignKey("id_referrer")]
        public virtual User? Referrer { get; set; }
        
        public int nr_indicacoes { get; set; } = 0; // Count of referrals
    }
}
