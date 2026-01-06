using FinanceApp.Data;
using FinanceApp.Helpers;
using FinanceApp.Models;

namespace FinanceApp.ViewModels
{
    public class RegisterViewModel
    {
        public string Nome { get; set; }
        public string Email { get; set; }
        public string Senha { get; set; }

        public void Registrar()
        {
            using var context = new AppDbContextFactory().CreateDbContext([]);

            if (context.users.Any(u => u.nm_email == Email))
                throw new Exception("E-mail já cadastrado.");

            var user = new User
            {
                nm_nomeUsuario = Nome,
                nm_email = Email,
                hs_senha = PasswordHelper.Hash(Senha)
            };

            context.users.Add(user);
            context.SaveChanges();
        }
    }
}
