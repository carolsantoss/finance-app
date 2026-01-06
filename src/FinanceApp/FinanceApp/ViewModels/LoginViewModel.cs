using FinanceApp.Data;
using FinanceApp.Helpers;

namespace FinanceApp.ViewModels
{
    public class LoginViewModel
    {
        public string Email { get; set; }
        public string Senha { get; set; }

        public bool Login()
        {
            using var context = new AppDbContextFactory().CreateDbContext([]);

            var user = context.users
                .FirstOrDefault(u => u.nm_email == Email);

            if (user == null)
                return false;

            return PasswordHelper.Verify(Senha, user.hs_senha);
        }
    }
}
