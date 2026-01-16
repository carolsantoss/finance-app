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

            // Seed Categories
            if (!context.categories.Any())
            {
                var categories = new List<Category>
                {
                    // Despesas (Expense)
                    new Category { nm_nome = "Alimentação", nm_icone = "Utensils", nm_cor = "#F75A68", nm_tipo = "Saída", id_usuario = null },
                    new Category { nm_nome = "Transporte", nm_icone = "Car", nm_cor = "#F75A68", nm_tipo = "Saída", id_usuario = null },
                    new Category { nm_nome = "Lazer", nm_icone = "PartyPopper", nm_cor = "#F75A68", nm_tipo = "Saída", id_usuario = null },
                    new Category { nm_nome = "Saúde", nm_icone = "HeartPulse", nm_cor = "#F75A68", nm_tipo = "Saída", id_usuario = null },
                    new Category { nm_nome = "Educação", nm_icone = "GraduationCap", nm_cor = "#F75A68", nm_tipo = "Saída", id_usuario = null },
                    new Category { nm_nome = "Moradia", nm_icone = "Home", nm_cor = "#F75A68", nm_tipo = "Saída", id_usuario = null },
                    new Category { nm_nome = "Outros", nm_icone = "MoreHorizontal", nm_cor = "#F75A68", nm_tipo = "Saída", id_usuario = null },

                    // Receitas (Income)
                    new Category { nm_nome = "Salário", nm_icone = "Banknote", nm_cor = "#00B37E", nm_tipo = "Entrada", id_usuario = null },
                    new Category { nm_nome = "Investimentos", nm_icone = "TrendingUp", nm_cor = "#00B37E", nm_tipo = "Entrada", id_usuario = null },
                    new Category { nm_nome = "Freelance", nm_icone = "Laptop", nm_cor = "#00B37E", nm_tipo = "Entrada", id_usuario = null },
                    new Category { nm_nome = "Presente", nm_icone = "Gift", nm_cor = "#00B37E", nm_tipo = "Entrada", id_usuario = null }
                };

                context.categories.AddRange(categories);
                context.SaveChanges();
            }

            // Check if user already exists
            if (context.users.Any(u => u.nm_email == "kauavitorioof@gmail.com"))
            {
                return; // User already exists
            }

            var user = new User
            {
                nm_nomeUsuario = "Kaua Lima",
                nm_email = "kauavitorioof@gmail.com",
                hs_senha = PasswordHelper.Hash("123456"), // Default password
                fl_admin = true // Ensure admin access for dev
            };

            context.users.Add(user);
            context.SaveChanges();

            // Ensure Default Wallet for this New User
            if (!context.wallets.Any(w => w.id_usuario == user.id_usuario))
            {
                context.wallets.Add(new Wallet
                {
                    nm_nome = "Conta Principal",
                    nm_tipo = "Conta Corrente",
                    nr_saldo_inicial = 0,
                    id_usuario = user.id_usuario
                });
                context.SaveChanges();
            }
        }
    }
}
