using FinanceApp.Data;
using FinanceApp.Helpers;
using FinanceApp.Models;
using System.Linq;
using System.Windows;

namespace FinanceApp.Views
{
    public partial class RegisterWindow : Window
    {
        public RegisterWindow()
        {
            InitializeComponent();
        }

        private void BtnRegister_Click(object sender, RoutedEventArgs e)
        {
            var factory = new AppDbContextFactory();

            using var context = factory.CreateDbContext();

            var nome = txtNome.Text;
            var email = txtEmail.Text;
            var senha = txtSenha.Password;
            var confirmarSenha = txtConfirmarSenha.Password;

            if (string.IsNullOrWhiteSpace(nome) ||
                string.IsNullOrWhiteSpace(email) ||
                string.IsNullOrWhiteSpace(senha))
            {
                MessageBox.Show("Preencha todos os campos");
                return;
            }

            if (senha != confirmarSenha)
            {
                MessageBox.Show("As senhas não conferem");
                return;
            }

            if (context.users.Any(u => u.hs_email == email))
            {
                MessageBox.Show("E-mail já cadastrado");
                return;
            }

            var user = new User
            {
                hs_nome = nome,
                hs_email = email,
                hs_password_hash = PasswordHelper.Hash(senha)
            };

            context.users.Add(user);
            context.SaveChanges();

            MessageBox.Show("Cadastro realizado com sucesso!");

            var login = new LoginWindow();
            login.Show();
            this.Close();
        }

        private void BtnVoltarLogin_Click(object sender, RoutedEventArgs e)
        {
            var login = new LoginWindow();
            login.Show();
            this.Close();
        }
    }
}
