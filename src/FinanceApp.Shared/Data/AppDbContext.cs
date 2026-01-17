using Microsoft.EntityFrameworkCore;
using FinanceApp.Shared.Models;

namespace FinanceApp.Shared.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> users { get; set; }
        public DbSet<Lancamento> lancamentos { get; set; }
        public DbSet<Category> categories { get; set; }
        public DbSet<Wallet> wallets { get; set; }
        public DbSet<CreditCard> creditCards { get; set; }
        public DbSet<Budget> budgets { get; set; }
        public DbSet<RecurringTransaction> recurringTransactions { get; set; }
        public DbSet<Goal> goals { get; set; }
        public DbSet<Plan> plans { get; set; }
        public DbSet<Feature> features { get; set; }
        public DbSet<PlanFeature> planFeatures { get; set; }
        public DbSet<SystemSetting> systemSettings { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Seed Features
            modelBuilder.Entity<Feature>().HasData(
                new Feature { id_feature = 1, nm_key = "export_data", nm_label = "Exportar Dados", ds_description = "Permite exportar transações e relatórios em JSON/CSV" },
                new Feature { id_feature = 2, nm_key = "unlimited_goals", nm_label = "Metas Ilimitadas", ds_description = "Crie quantas metas financeiras desejar" },
                new Feature { id_feature = 3, nm_key = "multiple_wallets", nm_label = "Múltiplas Carteiras", ds_description = "Gerencie contas de diferentes bancos" }
            );

            // Seed Plans
            modelBuilder.Entity<Plan>().HasData(
                new Plan { id_plan = 1, nm_name = "Gratuito", nr_price = 0, fl_isDefault = true, ds_description = "Plano básico para começar" },
                new Plan { id_plan = 2, nm_name = "Premium", nr_price = 29.90m, fl_isDefault = false, ds_description = "Acesso completo a todas as funcionalidades" }
            );

            // Seed PlanFeatures (Premium gets all)
            modelBuilder.Entity<PlanFeature>().HasData(
                new PlanFeature { id_planFeature = 1, id_plan = 2, id_feature = 1 },
                new PlanFeature { id_planFeature = 2, id_plan = 2, id_feature = 2 },
                new PlanFeature { id_planFeature = 3, id_plan = 2, id_feature = 3 }
            );
        }
    }
}
