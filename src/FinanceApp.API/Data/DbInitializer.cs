using FinanceApp.Shared.Data;
using FinanceApp.Shared.Helpers;
using FinanceApp.Shared.Models;

namespace FinanceApp.API.Data
{
    public static class DbInitializer
    {
        public static void Seed(AppDbContext context)
        {
            // Ensure database is created
            context.Database.EnsureCreated();

            // Check if user already exists
            if (context.users.Any(u => u.nm_email == "kauavitorioof@gmail.com"))
            {
                return; // User already exists
            }

            var user = new User
            {
                nm_nomeUsuario = "Kaua Lima",
                nm_email = "kauavitorioof@gmail.com",
                hs_senha = PasswordHelper.Hash("123456") // Default password
            };

            context.users.Add(user);
            context.SaveChanges();
        }
    }
}
