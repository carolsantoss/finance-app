using FinanceApp.Data;
using FinanceApp.Helpers;
using System;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace FinanceApp.ViewModels
{
    public class PerfilViewModel : INotifyPropertyChanged
    {
        #region Campos Privados

        private string _nomeUsuario = string.Empty;
        private string _email = string.Empty;
        private string _inicialNome = "U";
        private string _dataCadastro = string.Empty;

        #endregion

        #region Propriedades Públicas

        public string NomeUsuario
        {
            get => _nomeUsuario;
            set
            {
                _nomeUsuario = value;
                NotificarPropriedadeAlterada();
                AtualizarInicial();
            }
        }

        public string Email
        {
            get => _email;
            set
            {
                _email = value;
                NotificarPropriedadeAlterada();
            }
        }

        public string InicialNome
        {
            get => _inicialNome;
            set
            {
                _inicialNome = value;
                NotificarPropriedadeAlterada();
            }
        }

        public string DataCadastro
        {
            get => _dataCadastro;
            set
            {
                _dataCadastro = value;
                NotificarPropriedadeAlterada();
            }
        }

        #endregion

        #region Construtor

        public PerfilViewModel()
        {
            CarregarDadosUsuario();
        }

        #endregion

        #region Métodos Públicos

        public bool SalvarAlteracoes(string novoNome, string novoEmail)
        {
            try
            {
                var usuario = Session.UsuarioLogado;
                if (usuario == null)
                    return false;

                using var contexto = new AppDbContextFactory().CreateDbContext();
                var usuarioDB = contexto.users.FirstOrDefault(u => u.id_usuario == usuario.id_usuario);

                if (usuarioDB == null)
                    return false;

                usuarioDB.nm_nomeUsuario = novoNome;
                usuarioDB.nm_email = novoEmail;

                contexto.SaveChanges();

                // Atualiza a sessão
                Session.UsuarioLogado.nm_nomeUsuario = novoNome;
                Session.UsuarioLogado.nm_email = novoEmail;

                NomeUsuario = novoNome;
                Email = novoEmail;

                return true;
            }
            catch
            {
                return false;
            }
        }

        public bool AlterarSenha(string senhaAtual, string novaSenha)
        {
            try
            {
                var usuario = Session.UsuarioLogado;
                if (usuario == null)
                    return false;

                // Verifica a senha atual
                if (!PasswordHelper.Verify(senhaAtual, usuario.hs_senha))
                    return false;

                using var contexto = new AppDbContextFactory().CreateDbContext();
                var usuarioDB = contexto.users.FirstOrDefault(u => u.id_usuario == usuario.id_usuario);

                if (usuarioDB == null)
                    return false;

                // Criptografa a nova senha
                var novaSenhaHash = PasswordHelper.Hash(novaSenha);
                usuarioDB.hs_senha = novaSenhaHash;

                contexto.SaveChanges();

                // Atualiza a sessão
                Session.UsuarioLogado.hs_senha = novaSenhaHash;

                return true;
            }
            catch
            {
                return false;
            }
        }

        #endregion

        #region Métodos Privados

        private void CarregarDadosUsuario()
        {
            var usuario = Session.UsuarioLogado;
            if (usuario == null)
                return;

            NomeUsuario = usuario.nm_nomeUsuario;
            Email = usuario.nm_email;
            DataCadastro = $"Membro desde {DateTime.Now:MMMM 'de' yyyy}";
            AtualizarInicial();
        }

        private void AtualizarInicial()
        {
            if (!string.IsNullOrWhiteSpace(NomeUsuario))
            {
                InicialNome = NomeUsuario.Substring(0, 1).ToUpper();
            }
        }

        #endregion

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void NotificarPropriedadeAlterada([CallerMemberName] string? nomePropriedade = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nomePropriedade));
        }

        #endregion
    }
}
