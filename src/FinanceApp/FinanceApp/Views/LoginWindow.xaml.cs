using FinanceApp.Data;
using FinanceApp.Models;
using FinanceApp.ViewModels;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
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
                        "server=localhost;database=financeapp;user=root;password=Oliveir@1920",
                        ServerVersion.AutoDetect(
                            "server=localhost;database=financeapp;user=root;password=Oliveir@1920"
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
                var main = new MainWindow();
                main.Show();
                this.Close();
            }
            else
            {
                MessageBox.Show("Email ou senha inválidos");
            }
        }

        private string GerarHash(string senha)
        {
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(senha));
            return Convert.ToBase64String(bytes);
        }

        private void BtnCriarConta_Click(object sender, RoutedEventArgs e)
        {
            var register = new RegisterWindow();
            register.Show();
            this.Close();
        }

    }
}
