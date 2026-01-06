using FinanceApp.Models;
using FinanceApp.Security;

namespace FinanceApp.Data
{
    public static class DbInitializer
    {
        public static void Seed(AppDbContext context)
        {
            if (context.users.Any())
                return;

            var user = new User
            {
                hs_nome = "Carol",
                hs_email = "carol@email.com",
                hs_password_hash = PasswordHasher.Hash("123456")
            };

            context.users.Add(user);
            context.SaveChanges();
        }
    }
}
