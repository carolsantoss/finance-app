using System.Text.Json.Serialization;

namespace FinanceApp.Shared.DTOs
{
    public class LoginRequest
    {
        public string Email { get; set; } = string.Empty;
        public string Senha { get; set; } = string.Empty;
        public bool RememberMe { get; set; } = false;
    }

    public class RegisterRequest
    {
        public string NomeUsuario { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Senha { get; set; } = string.Empty;
        public string? ReferralCode { get; set; }
    }

    public class LoginResponse
    {
        [JsonPropertyName("token")]
        public string? Token { get; set; }

        [JsonPropertyName("nomeUsuario")]
        public string? NomeUsuario { get; set; }

        [JsonPropertyName("email")]
        public string? Email { get; set; }

        [JsonPropertyName("isAdmin")]
        public bool IsAdmin { get; set; } = false;

        [JsonPropertyName("requiresTwoFactor")]
        public bool RequiresTwoFactor { get; set; } = false;

        [JsonPropertyName("referralCode")]
        public string? ReferralCode { get; set; }

        [JsonPropertyName("referralCount")]
        public int ReferralCount { get; set; }
    }

    public class ForgotPasswordRequest
    {
        public string Email { get; set; } = string.Empty;
    }

    public class ResetPasswordRequest
    {
        public string Token { get; set; } = string.Empty;
        public string NewPassword { get; set; } = string.Empty;
    }

    public class TwoFactorSetupResponse
    {
        public string Secret { get; set; } = string.Empty;
        public string QrCodeUri { get; set; } = string.Empty;
    }

    public class TwoFactorRequest
    {
        public string? Email { get; set; }
        public string Code { get; set; } = string.Empty;
    }
}
