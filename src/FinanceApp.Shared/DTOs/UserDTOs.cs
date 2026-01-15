using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace FinanceApp.Shared.DTOs
{
    public class UserProfileDto
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("nomeUsuario")]
        public string NomeUsuario { get; set; } = string.Empty;

        [JsonPropertyName("email")]
        public string Email { get; set; } = string.Empty;
    }

    public class UpdateProfileRequest
    {
        [Required(ErrorMessage = "O nome é obrigatório.")]
        public string NomeUsuario { get; set; } = string.Empty;
        
        [Required(ErrorMessage = "O email é obrigatório.")]
        [EmailAddress(ErrorMessage = "Email inválido.")]
        public string Email { get; set; } = string.Empty;
    }

    public class ChangePasswordRequest
    {
        [Required(ErrorMessage = "A senha atual é obrigatória.")]
        public string CurrentPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "A nova senha é obrigatória.")]
        [MinLength(6, ErrorMessage = "A nova senha deve ter pelo menos 6 caracteres.")]
        public string NewPassword { get; set; } = string.Empty;

        [Compare("NewPassword", ErrorMessage = "As senhas não conferem.")]
        public string ConfirmNewPassword { get; set; } = string.Empty;
    }
}
