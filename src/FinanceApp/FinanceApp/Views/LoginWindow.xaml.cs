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
        private bool _isPasswordVisible = false;

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

        private void TogglePasswordVisibility_Click(object sender, RoutedEventArgs e)
        {
            if (_isPasswordVisible)
            {
                // Esconder senha
                PasswordBox.Password = PasswordTextBox.Text;
                PasswordBox.Visibility = Visibility.Visible;
                PasswordTextBox.Visibility = Visibility.Collapsed;
                EyeIcon.Text = "👁️";
                _isPasswordVisible = false;
            }
            else
            {
                // Mostrar senha
                PasswordTextBox.Text = PasswordBox.Password;
                PasswordBox.Visibility = Visibility.Collapsed;
                PasswordTextBox.Visibility = Visibility.Visible;
                PasswordTextBox.Focus();
                PasswordTextBox.CaretIndex = PasswordTextBox.Text.Length;
                EyeIcon.Text = "👁️‍🗨️";
                _isPasswordVisible = true;
            }
        }

        private void Login_Click(object sender, RoutedEventArgs e)
        {
            if (Session.UsuarioLogado == null)
            {
                // Garante que pegamos a senha do campo correto
                string senha = _isPasswordVisible ? PasswordTextBox.Text : PasswordBox.Password;

                var vm = new LoginViewModel
                {
                    Email = EmailTextBox.Text,
                    Senha = senha
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
        }

        private void BtnCriarConta_Click(object sender, RoutedEventArgs e)
        {
            var register = new RegisterWindow();
            register.Show();
            this.Close();
        }
    }
}