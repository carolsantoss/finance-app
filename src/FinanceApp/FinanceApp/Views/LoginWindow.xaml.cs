using FinanceApp.Data;
using FinanceApp.Helpers;
using FinanceApp.ViewModels;
using Microsoft.EntityFrameworkCore;
using System.Windows;

namespace FinanceApp.Views
{
    public partial class LoginWindow : Window
    {
        private readonly AppDbContext _context;

        public LoginWindow()
        {
            InitializeComponent();

            _context = new AppDbContext(
                new DbContextOptionsBuilder<AppDbContext>()
                    .UseMySql(
                        "server=localhost;database=finance_app;user=root;password=Oliveir@1920",
                        ServerVersion.AutoDetect(
                            "server=localhost;database=finance_app;user=root;password=Oliveir@1920"
                        )
                    ).Options
            );
        }

        private void Login_Click(object sender, RoutedEventArgs e)
        {
            var vm = new LoginViewModel
            {
                Email = EmailTextBox.Text,
                Senha = PasswordBox.Password
            };

            if (vm.Login())
            {
                var user = _context.users.FirstOrDefault(u => u.nm_email == vm.Email);

                if (user != null)
                {
                    Session.UsuarioLogado = user;
                }

                var main = new MainWindow();
                main.Show();
                this.Close();
            }
            else
            {
                MessageBox.Show("Email ou senha inválidos");
            }
        }

        private void BtnCriarConta_Click(object sender, RoutedEventArgs e)
        {
            var register = new RegisterWindow();
            register.Show();
            this.Close();
        }

    }
}