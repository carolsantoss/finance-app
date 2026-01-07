using FinanceApp.ViewModels;
using System.Windows;

namespace FinanceApp.Views
{
    public partial class PerfilWindow : Window
    {
        private readonly PerfilViewModel _viewModel;

        public PerfilWindow()
        {
            InitializeComponent();
            _viewModel = new PerfilViewModel();
            DataContext = _viewModel;
        }

        private void BtnFechar_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void BtnSalvar_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(TxtNome.Text))
            {
                MessageBox.Show("Por favor, preencha o nome.", "Atenção", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(TxtEmail.Text))
            {
                MessageBox.Show("Por favor, preencha o e-mail.", "Atenção", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var resultado = _viewModel.SalvarAlteracoes(TxtNome.Text, TxtEmail.Text);

            if (resultado)
            {
                MessageBox.Show("Informações atualizadas com sucesso!", "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
            {
                MessageBox.Show("Erro ao atualizar informações.", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void BtnAlterarSenha_Click(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrWhiteSpace(PwdSenhaAtual.Password))
            {
                MessageBox.Show("Por favor, digite a senha atual.", "Atenção", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (string.IsNullOrWhiteSpace(PwdNovaSenha.Password))
            {
                MessageBox.Show("Por favor, digite a nova senha.", "Atenção", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (PwdNovaSenha.Password != PwdConfirmarSenha.Password)
            {
                MessageBox.Show("As senhas não coincidem.", "Atenção", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            if (PwdNovaSenha.Password.Length < 6)
            {
                MessageBox.Show("A senha deve ter no mínimo 6 caracteres.", "Atenção", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }

            var resultado = _viewModel.AlterarSenha(PwdSenhaAtual.Password, PwdNovaSenha.Password);

            if (resultado)
            {
                MessageBox.Show("Senha alterada com sucesso!", "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
                PwdSenhaAtual.Clear();
                PwdNovaSenha.Clear();
                PwdConfirmarSenha.Clear();
            }
            else
            {
                MessageBox.Show("Senha atual incorreta.", "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}