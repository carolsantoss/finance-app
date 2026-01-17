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

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
        }
    }
}
