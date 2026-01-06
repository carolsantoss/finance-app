using FinanceApp.Data;
using FinanceApp.Helpers;
using FinanceApp.Models;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Controls;

namespace FinanceApp.Views
{
    public partial class ExtratosWindow : Window, INotifyPropertyChanged
    {
        #region Campos Privados

        private ObservableCollection<LancamentoViewModel> _lancamentos = new();
        private int _totalLancamentos;

        #endregion

        #region Propriedades Públicas

        public int TotalLancamentos
        {
            get => _totalLancamentos;
            set
            {
                _totalLancamentos = value;
                NotificarPropriedadeAlterada();
            }
        }

        #endregion

        #region Construtor

        public ExtratosWindow()
        {
            InitializeComponent();
            DataContext = this;
            CarregarLancamentos();
        }

        #endregion

        #region Eventos

        private void AplicarFiltros_Click(object sender, SelectionChangedEventArgs e)
        {
            if (!IsLoaded)
                return;

            CarregarLancamentos();
        }

        private void AtualizarLista_Click(object sender, RoutedEventArgs e)
        {
            CarregarLancamentos();
        }

        private void ExcluirLancamento_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button botao && botao.Tag is int idLancamento)
            {
                ExcluirLancamento(idLancamento);
            }
        }

        #endregion

        #region Métodos de Carregamento

        private void CarregarLancamentos()
        {
            var usuarioLogado = Session.UsuarioLogado;
            if (usuarioLogado == null)
                return;

            using var contexto = new AppDbContextFactory().CreateDbContext();

            var query = contexto.lancamentos
                .Where(l => l.id_usuario == usuarioLogado.id_usuario)
                .AsQueryable();

            query = AplicarFiltroTipo(query);
            query = AplicarFiltroPeriodo(query);

            var lancamentos = query
                .OrderByDescending(l => l.dt_dataLancamento)
                .ToList();

            AtualizarListaInterface(lancamentos);
        }

        private IQueryable<Lancamento> AplicarFiltroTipo(IQueryable<Lancamento> query)
        {
            if (cbFiltroTipo == null)
                return query;

            var tipoSelecionado = (cbFiltroTipo.SelectedItem as ComboBoxItem)?.Content.ToString();

            if (tipoSelecionado != null && tipoSelecionado != "Todos")
            {
                query = query.Where(l => l.nm_tipo.Contains(tipoSelecionado));
            }

            return query;
        }

        private IQueryable<Lancamento> AplicarFiltroPeriodo(IQueryable<Lancamento> query)
        {
            if (cbFiltroPeriodo == null)
                return query;

            var periodoSelecionado = (cbFiltroPeriodo.SelectedItem as ComboBoxItem)?.Content.ToString();
            var dataAtual = DateTime.Now;

            return periodoSelecionado switch
            {
                "Hoje" => query.Where(l => l.dt_dataLancamento.Date == dataAtual.Date),
                "Esta semana" => query.Where(l => l.dt_dataLancamento >= dataAtual.AddDays(-7)),
                "Este mês" => query.Where(l => l.dt_dataLancamento.Month == dataAtual.Month &&
                                              l.dt_dataLancamento.Year == dataAtual.Year),
                "Este ano" => query.Where(l => l.dt_dataLancamento.Year == dataAtual.Year),
                _ => query
            };
        }

        private void AtualizarListaInterface(List<Lancamento> lancamentos)
        {
            _lancamentos.Clear();

            foreach (var lancamento in lancamentos)
            {
                _lancamentos.Add(new LancamentoViewModel(lancamento));
            }

            ListaLancamentos.ItemsSource = _lancamentos;
            TotalLancamentos = _lancamentos.Count;
        }

        #endregion

        #region Métodos de Exclusão

        private void ExcluirLancamento(int idLancamento)
        {
            var resultado = MessageBox.Show(
                "Tem certeza que deseja excluir este lançamento?",
                "Confirmar Exclusão",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (resultado == MessageBoxResult.Yes)
            {
                RealizarExclusao(idLancamento);
            }
        }

        private void RealizarExclusao(int idLancamento)
        {
            using var contexto = new AppDbContextFactory().CreateDbContext();

            var lancamento = contexto.lancamentos.Find(idLancamento);

            if (lancamento != null)
            {
                contexto.lancamentos.Remove(lancamento);
                contexto.SaveChanges();

                MessageBox.Show("Lançamento excluído com sucesso!", "Sucesso",
                    MessageBoxButton.OK, MessageBoxImage.Information);

                CarregarLancamentos();
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

    #region ViewModel para Lançamento

    public class LancamentoViewModel
    {
        private readonly Lancamento _lancamento;

        public LancamentoViewModel(Lancamento lancamento)
        {
            _lancamento = lancamento;
        }

        public int id_lancamento => _lancamento.id_lancamento;
        public string nm_descricao => _lancamento.nm_descricao;
        public string nm_tipo => _lancamento.nm_tipo;
        public string nm_formaPagamento => _lancamento.nm_formaPagamento;

        public string DataFormatada => _lancamento.dt_dataLancamento.ToString("dd/MM/yyyy");

        public string ValorFormatado => _lancamento.nr_valor.ToString("C2",
            CultureInfo.GetCultureInfo("pt-BR"));

        public string ParcelasTexto => _lancamento.nr_parcelas > 1
            ? $"{_lancamento.nr_parcelas}x"
            : "-";

        public string CorTipo => _lancamento.nm_tipo.Contains("Entrada")
            ? "#4CAF50"
            : "#E53935";

        public string CorValor => _lancamento.nm_tipo.Contains("Entrada")
            ? "#4CAF50"
            : "#E53935";
    }

    #endregion
}