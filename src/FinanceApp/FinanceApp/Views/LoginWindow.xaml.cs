using FinanceApp.Data;
using FinanceApp.Helpers;
using FinanceApp.ViewModels;
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

            _context = new AppDbContextFactory().CreateDbContext();
        }

        private void TogglePasswordVisibility_Click(object sender, RoutedEventArgs e)
        {
            if (_isPasswordVisible)
            {
                PasswordBox.Password = PasswordTextBox.Text;
                PasswordBox.Visibility = Visibility.Visible;
                PasswordTextBox.Visibility = Visibility.Collapsed;
                EyeIcon.Text = "👁️";
                _isPasswordVisible = false;
            }
            else
            {
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