using System;
using System.Windows;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using FinanceApp.Data;
using FinanceApp.Views;

namespace FinanceApp
{
    public partial class App : Application
    {
        public static IServiceProvider Services { get; private set; }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var services = new ServiceCollection();

            services.AddDbContext<AppDbContext>(options =>
                options.UseMySql(
                    "server=localhost;port=3306;database=finance_app;user=root;password=Oliveir@1920;",
                    ServerVersion.AutoDetect(
                        "server=localhost;port=3306;database=finance_app;user=root;password=Oliveir@1920;"
                    )
                )
            );

            Services = services.BuildServiceProvider();

            using (var scope = Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                DbInitializer.Seed(db);
            }

            // Abrir Login primeiro
            var login = new LoginWindow();
            login.Show();
        }
    }
}
