using FinanceApp.Data;
using FinanceApp.Helpers;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace FinanceApp.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        #region Campos Privados

        private decimal _entradas;
        private decimal _saidas;
        private string _nomeUsuario = "Usuário";

        #endregion

        #region Propriedades Públicas

        public decimal Entradas
        {
            get => _entradas;
            set
            {
                _entradas = value;
                NotificarPropriedadeAlterada();
                NotificarPropriedadeAlterada(nameof(Saldo));
            }
        }

        public decimal Saidas
        {
            get => _saidas;
            set
            {
                _saidas = value;
                NotificarPropriedadeAlterada();
                NotificarPropriedadeAlterada(nameof(Saldo));
            }
        }

        public decimal Saldo => Entradas - Saidas;

        public string NomeUsuario
        {
            get => _nomeUsuario;
            set
            {
                _nomeUsuario = value;
                NotificarPropriedadeAlterada();
            }
        }

        #endregion

        #region Construtor

        public MainViewModel()
        {
            CarregarDadosUsuario();
        }

        #endregion

        #region Métodos Privados

        private void CarregarDadosUsuario()
        {
            var usuarioLogado = Session.UsuarioLogado;

            if (usuarioLogado == null)
                return;

            DefinirNomeUsuario(usuarioLogado.nm_nomeUsuario);
            CarregarDadosFinanceiros(usuarioLogado.id_usuario);
        }

        private void DefinirNomeUsuario(string nome)
        {
            NomeUsuario = nome;
        }

        private void CarregarDadosFinanceiros(int idUsuario)
        {
            using var contexto = new AppDbContextFactory().CreateDbContext([]);

            Entradas = CalcularTotalEntradas(contexto, idUsuario);
            Saidas = CalcularTotalSaidas(contexto, idUsuario);
        }

        private decimal CalcularTotalEntradas(AppDbContext contexto, int idUsuario)
        {
            return contexto.lancamentos
                .Where(lancamento => lancamento.id_usuario == idUsuario &&
                                   lancamento.nm_tipo.Contains("Entrada"))
                .Sum(lancamento => lancamento.nr_valor);
        }

        private decimal CalcularTotalSaidas(AppDbContext contexto, int idUsuario)
        {
            return contexto.lancamentos
                .Where(lancamento => lancamento.id_usuario == idUsuario &&
                                   lancamento.nm_tipo.Contains("Saída"))
                .Sum(lancamento => lancamento.nr_valor);
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