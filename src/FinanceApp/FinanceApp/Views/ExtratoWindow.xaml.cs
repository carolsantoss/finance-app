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
        private decimal _saldoMesAtual;
        private decimal _totalEntradas;
        private decimal _totalSaidas;

        #endregion

        #region Propriedade  Publicas

        public int TotalLancamentos
        {
            get => _totalLancamentos;
            set
            {
                _totalLancamentos = value;
                NotificarPropriedadeAlterada();
            }
        }

        public decimal SaldoMesAtual
        {
            get => _saldoMesAtual;
            set
            {
                _saldoMesAtual = value;
                NotificarPropriedadeAlterada();
                NotificarPropriedadeAlterada(nameof(SaldoMesAtualFormatado));
                NotificarPropriedadeAlterada(nameof(CorSaldo));
            }
        }

        public decimal TotalEntradas
        {
            get => _totalEntradas;
            set
            {
                _totalEntradas = value;
                NotificarPropriedadeAlterada();
                NotificarPropriedadeAlterada(nameof(TotalEntradasFormatado));
            }
        }

        public decimal TotalSaidas
        {
            get => _totalSaidas;
            set
            {
                _totalSaidas = value;
                NotificarPropriedadeAlterada();
                NotificarPropriedadeAlterada(nameof(TotalSaidasFormatado));
            }
        }

        public string SaldoMesAtualFormatado => SaldoMesAtual.ToString("C2", CultureInfo.GetCultureInfo("pt-BR"));
        public string TotalEntradasFormatado => TotalEntradas.ToString("C2", CultureInfo.GetCultureInfo("pt-BR"));
        public string TotalSaidasFormatado => TotalSaidas.ToString("C2", CultureInfo.GetCultureInfo("pt-BR"));

        public string CorSaldo => SaldoMesAtual >= 0 ? "#4CAF50" : "#E53935";

        #endregion

        #region Construtor

        public ExtratosWindow()
        {
            InitializeComponent();
            DataContext = this;

            PopularAnosFiltro();
            DefinirFiltroPadrao();

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

        private void PopularAnosFiltro()
        {
            int anoAtual = DateTime.Now.Year;

            for (int ano = anoAtual - 5; ano <= anoAtual + 2; ano++)
            {
                var item = new ComboBoxItem
                {
                    Content = ano.ToString(),
                    Tag = ano
                };
                cbFiltroAno.Items.Add(item);

                if (ano == anoAtual)
                {
                    cbFiltroAno.SelectedItem = item;
                }
            }
        }

        private void DefinirFiltroPadrao()
        {
            int mesAtual = DateTime.Now.Month;

            foreach (ComboBoxItem item in cbFiltroMes.Items)
            {
                if (item.Tag != null && int.TryParse(item.Tag.ToString(), out int mesTag) && mesTag == mesAtual)
                {
                    cbFiltroMes.SelectedItem = item;
                    return;
                }
            }

            if (cbFiltroMes.Items.Count > 0)
            {
                cbFiltroMes.SelectedIndex = 0;
            }
        }

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

            var lancamentos = query
                .OrderByDescending(l => l.dt_dataLancamento)
                .ToList();

            var lancamentosExpandidos = ExpandirLancamentosParcelados(lancamentos);

            var lancamentosFiltrados = AplicarFiltroPeriodoEmLista(lancamentosExpandidos);

            CalcularSaldoPeriodo(lancamentosFiltrados);

            AtualizarListaInterface(lancamentosFiltrados);
        }

        private void CalcularSaldoPeriodo(List<Lancamento> lancamentos)
        {
            TotalEntradas = lancamentos
                .Where(l => l.nm_tipo.Contains("Entrada"))
                .Sum(l => l.nr_valor);

            TotalSaidas = lancamentos
                .Where(l => l.nm_tipo.Contains("Saída"))
                .Sum(l => l.nr_valor);

            SaldoMesAtual = TotalEntradas - TotalSaidas;
        }

        private Lancamento CriarParcelaVirtual(Lancamento lancamentoOriginal, int numeroParcela)
        {
            int numeroParcelaReal = lancamentoOriginal.nr_parcelaInicial + numeroParcela - 1;

            // Se começou da primeira parcela, divide o valor pelo total de parcelas
            // Caso contrário, cada parcela tem o valor total informado
            decimal valorPorParcela;
            if (lancamentoOriginal.nr_parcelaInicial == 1)
            {
                valorPorParcela = lancamentoOriginal.nr_valor / lancamentoOriginal.nr_parcelas;
            }
            else
            {
                valorPorParcela = lancamentoOriginal.nr_valor;
            }

            return new Lancamento
            {
                id_lancamento = lancamentoOriginal.id_lancamento * 1000 + numeroParcela,
                id_usuario = lancamentoOriginal.id_usuario,
                nm_descricao = $"{lancamentoOriginal.nm_descricao} ({numeroParcelaReal}/{lancamentoOriginal.nr_parcelas})",
                nm_tipo = lancamentoOriginal.nm_tipo,
                nm_formaPagamento = lancamentoOriginal.nm_formaPagamento,
                nr_valor = valorPorParcela,
                nr_parcelas = lancamentoOriginal.nr_parcelas,
                nr_parcelaInicial = lancamentoOriginal.nr_parcelaInicial,
                dt_dataLancamento = lancamentoOriginal.dt_dataLancamento.AddMonths(numeroParcela - 1)
            };
        }

        private int ObterQuantidadeParcelasRestantes(Lancamento lancamento)
        {
            return lancamento.nr_parcelas - lancamento.nr_parcelaInicial + 1;
        }

        private List<Lancamento> ExpandirLancamentosParcelados(List<Lancamento> lancamentos)
        {
            var lancamentosExpandidos = new List<Lancamento>();

            foreach (var lancamento in lancamentos)
            {
                if (lancamento.nr_parcelas > 1 && lancamento.nr_parcelaInicial <= lancamento.nr_parcelas)
                {
                    int parcelasRestantes = ObterQuantidadeParcelasRestantes(lancamento);

                    for (int i = 1; i <= parcelasRestantes; i++)
                    {
                        var parcelaVirtual = CriarParcelaVirtual(lancamento, i);
                        lancamentosExpandidos.Add(parcelaVirtual);
                    }
                }
                else
                {
                    lancamentosExpandidos.Add(lancamento);
                }
            }

            return lancamentosExpandidos.OrderByDescending(l => l.dt_dataLancamento).ToList();
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

        private List<Lancamento> AplicarFiltroPeriodoEmLista(List<Lancamento> lancamentos)
        {
            if (cbFiltroMes == null || cbFiltroMes.SelectedItem == null)
                return lancamentos;

            var itemSelecionado = cbFiltroMes.SelectedItem as ComboBoxItem;
            var mesSelecionadoTag = itemSelecionado?.Tag?.ToString();

            if (!string.IsNullOrEmpty(mesSelecionadoTag) && int.TryParse(mesSelecionadoTag, out int mesNumero))
            {
                int anoSelecionado = DateTime.Now.Year;

                if (cbFiltroAno?.SelectedItem is ComboBoxItem anoItem && anoItem.Tag != null)
                {
                    anoSelecionado = (int)anoItem.Tag;
                }

                return lancamentos.Where(l =>
                    l.dt_dataLancamento.Month == mesNumero &&
                    l.dt_dataLancamento.Year == anoSelecionado).ToList();
            }

            return lancamentos;
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
            // Verifica se é uma parcela virtual (ID > 1000)
            int idOriginal = idLancamento > 1000 ? idLancamento / 1000 : idLancamento;

            var resultado = MessageBox.Show(
                "Tem certeza que deseja excluir este lançamento?\n\n" +
                (idLancamento > 1000 ? "Atenção: Isso excluirá TODAS as parcelas deste lançamento!" : ""),
                "Confirmar Exclusão",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);

            if (resultado == MessageBoxResult.Yes)
            {
                RealizarExclusao(idOriginal);
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

        public string ParcelasTexto
        {
            get
            {
                var descricao = _lancamento.nm_descricao;
                var inicioParenteses = descricao.LastIndexOf('(');
                var fimParenteses = descricao.LastIndexOf(')');

                if (inicioParenteses >= 0 && fimParenteses > inicioParenteses)
                {
                    return descricao.Substring(inicioParenteses + 1, fimParenteses - inicioParenteses - 1);
                }

                return "-";
            }
        }

        public string CorTipo => _lancamento.nm_tipo.Contains("Entrada")
            ? "#4CAF50"
            : "#E53935";

        public string CorValor => _lancamento.nm_tipo.Contains("Entrada")
            ? "#4CAF50"
            : "#E53935";
    }

    #endregion
}