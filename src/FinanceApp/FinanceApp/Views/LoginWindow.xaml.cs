using FinanceApp.Data;
using FinanceApp.Models;
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
            var email = EmailTextBox.Text;
            var senha = PasswordBox.Password;

            var senhaHash = GerarHash(senha);

            var user = _context.users
                .FirstOrDefault(u => u.hs_email == email && u.hs_password_hash == senhaHash);

            if (user == null)
            {
                MessageBox.Show("Email ou senha inválidos");
                return;
            }

            // Login OK
            var mainWindow = new MainWindow(user);
            mainWindow.Show();
            this.Close();
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
