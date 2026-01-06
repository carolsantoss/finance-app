using FinanceApp.Data;
using FinanceApp.Helpers;
using FinanceApp.Views;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Windows;

namespace FinanceApp
{
    public partial class App : Application
    {
        public static IServiceProvider Services { get; private set; }

        //protected override void OnStartup(StartupEventArgs e)
        //{
        //    base.OnStartup(e);

        //    var services = new ServiceCollection();

        //    services.AddDbContext<AppDbContext>(options =>
        //        options.UseMySql(
        //            "server=localhost;port=3306;database=finance_app;user=root;password=Oliveir@1920;",
        //            ServerVersion.AutoDetect(
        //                "server=localhost;port=3306;database=finance_app;user=root;password=Oliveir@1920;"
        //            )
        //        )
        //    );

        //    Services = services.BuildServiceProvider();

        //    var login = new LoginWindow();
        //    login.Show();
        //}

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var autoLoginAtivo = AppConfig.Configuration["AutoLogin:Ativo"] == "true";

            using var context = new AppDbContextFactory().CreateDbContext([]);

            if (autoLoginAtivo)
            {
                var email = AppConfig.Configuration["AutoLogin:Email"];
                var senha = AppConfig.Configuration["AutoLogin:Senha"];

                var usuario = context.users.FirstOrDefault(u => u.nm_email == email);

                if (usuario != null &&
                    PasswordHelper.Verify(senha, usuario.hs_senha))
                {
                    Session.UsuarioLogado = usuario;

                    new MainWindow().Show();
                    return;
                }
            }

            new LoginWindow().Show();
        }
    }
}
