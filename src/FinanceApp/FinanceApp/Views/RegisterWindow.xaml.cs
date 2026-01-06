using FinanceApp.Data;
using FinanceApp.Helpers;
using FinanceApp.Models;
using System.Linq;
using System.Windows;

namespace FinanceApp.Views
{
    public partial class RegisterWindow : Window
    {
        #region Constantes

        private const string ICONE_OLHO_FECHADO = "👁️";
        private const string ICONE_OLHO_ABERTO = "👁️‍🗨️";

        #endregion

        #region Campos Privados

        private bool _senhaVisivel = false;
        private bool _confirmacaoSenhaVisivel = false;

        #endregion

        #region Construtor

        public RegisterWindow()
        {
            InitializeComponent();
        }

        #endregion

        #region Eventos de Visibilidade de Senha

        private void TogglePasswordVisibility_Click(object sender, RoutedEventArgs e)
        {
            AlternarVisibilidadeSenha();
        }

        private void ToggleConfirmPasswordVisibility_Click(object sender, RoutedEventArgs e)
        {
            AlternarVisibilidadeConfirmacaoSenha();
        }

        #endregion

        #region Eventos de Navegação

        private void BtnRegister_Click(object sender, RoutedEventArgs e)
        {
            RealizarCadastro();
        }

        private void BtnVoltarLogin_Click(object sender, RoutedEventArgs e)
        {
            NavegarParaLogin();
        }

        #endregion

        #region Métodos de Visibilidade de Senha

        private void AlternarVisibilidadeSenha()
        {
            if (_senhaVisivel)
            {
                EsconderSenha();
            }
            else
            {
                MostrarSenha();
            }
        }

        private void EsconderSenha()
        {
            txtSenha.Password = txtSenhaVisible.Text;
            txtSenha.Visibility = Visibility.Visible;
            txtSenhaVisible.Visibility = Visibility.Collapsed;
            EyeIcon.Text = ICONE_OLHO_FECHADO;
            _senhaVisivel = false;
        }

        private void MostrarSenha()
        {
            txtSenhaVisible.Text = txtSenha.Password;
            txtSenha.Visibility = Visibility.Collapsed;
            txtSenhaVisible.Visibility = Visibility.Visible;
            FocarCampoTextoNoFinal(txtSenhaVisible);
            EyeIcon.Text = ICONE_OLHO_ABERTO;
            _senhaVisivel = true;
        }

        private void AlternarVisibilidadeConfirmacaoSenha()
        {
            if (_confirmacaoSenhaVisivel)
            {
                EsconderConfirmacaoSenha();
            }
            else
            {
                MostrarConfirmacaoSenha();
            }
        }

        private void EsconderConfirmacaoSenha()
        {
            txtConfirmarSenha.Password = txtConfirmarSenhaVisible.Text;
            txtConfirmarSenha.Visibility = Visibility.Visible;
            txtConfirmarSenhaVisible.Visibility = Visibility.Collapsed;
            EyeIconConfirm.Text = ICONE_OLHO_FECHADO;
            _confirmacaoSenhaVisivel = false;
        }

        private void MostrarConfirmacaoSenha()
        {
            txtConfirmarSenhaVisible.Text = txtConfirmarSenha.Password;
            txtConfirmarSenha.Visibility = Visibility.Collapsed;
            txtConfirmarSenhaVisible.Visibility = Visibility.Visible;
            FocarCampoTextoNoFinal(txtConfirmarSenhaVisible);
            EyeIconConfirm.Text = ICONE_OLHO_ABERTO;
            _confirmacaoSenhaVisivel = true;
        }

        private void FocarCampoTextoNoFinal(System.Windows.Controls.TextBox campo)
        {
            campo.Focus();
            campo.CaretIndex = campo.Text.Length;
        }

        #endregion

        #region Métodos de Cadastro

        private void RealizarCadastro()
        {
            var dadosUsuario = ObterDadosFormulario();

            if (!ValidarCamposObrigatorios(dadosUsuario))
                return;

            if (!ValidarSenhasIguais(dadosUsuario.senha, dadosUsuario.confirmacaoSenha))
                return;

            if (EmailJaCadastrado(dadosUsuario.email))
                return;

            SalvarNovoUsuario(dadosUsuario);
            ExibirMensagemSucesso();
            NavegarParaLogin();
        }

        private (string nome, string email, string senha, string confirmacaoSenha) ObterDadosFormulario()
        {
            var nome = txtNome.Text;
            var email = txtEmail.Text;
            var senha = _senhaVisivel ? txtSenhaVisible.Text : txtSenha.Password;
            var confirmacaoSenha = _confirmacaoSenhaVisivel ? txtConfirmarSenhaVisible.Text : txtConfirmarSenha.Password;

            return (nome, email, senha, confirmacaoSenha);
        }

        #endregion

        #region Métodos de Validação

        private bool ValidarCamposObrigatorios((string nome, string email, string senha, string confirmacaoSenha) dados)
        {
            if (string.IsNullOrWhiteSpace(dados.nome) ||
                string.IsNullOrWhiteSpace(dados.email) ||
                string.IsNullOrWhiteSpace(dados.senha))
            {
                ExibirMensagemErro("Preencha todos os campos obrigatórios.");
                return false;
            }
            return true;
        }

        private bool ValidarSenhasIguais(string senha, string confirmacaoSenha)
        {
            if (senha != confirmacaoSenha)
            {
                ExibirMensagemErro("As senhas não conferem.");
                return false;
            }
            return true;
        }

        private bool EmailJaCadastrado(string email)
        {
            using var contexto = CriarContextoBancoDados();

            if (contexto.users.Any(usuario => usuario.nm_email == email))
            {
                ExibirMensagemErro("E-mail já cadastrado.");
                return true;
            }
            return false;
        }

        #endregion

        #region Métodos de Persistência

        private void SalvarNovoUsuario((string nome, string email, string senha, string confirmacaoSenha) dados)
        {
            using var contexto = CriarContextoBancoDados();

            var novoUsuario = CriarObjetoUsuario(dados);

            contexto.users.Add(novoUsuario);
            contexto.SaveChanges();
        }

        private User CriarObjetoUsuario((string nome, string email, string senha, string confirmacaoSenha) dados)
        {
            return new User
            {
                nm_nomeUsuario = dados.nome,
                nm_email = dados.email,
                hs_senha = PasswordHelper.Hash(dados.senha)
            };
        }

        private AppDbContext CriarContextoBancoDados()
        {
            var fabricaContexto = new AppDbContextFactory();
            return fabricaContexto.CreateDbContext();
        }

        #endregion

        #region Métodos de Mensagens

        private void ExibirMensagemErro(string mensagem)
        {
            MessageBox.Show(mensagem, "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void ExibirMensagemSucesso()
        {
            MessageBox.Show("Cadastro realizado com sucesso!", "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        #endregion

        #region Métodos de Navegação

        private void NavegarParaLogin()
        {
            var janelaLogin = new LoginWindow();
            janelaLogin.Show();
            Close();
        }

        #endregion
    }
}