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
            
            // Adiciona os meses ao ComboBox
            AdicionarMesesAoFiltro();
            
            // Define o filtro padrão para "Este mês" antes de carregar
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

        private void AdicionarMesesAoFiltro()
        {
            var culturaPortugues = new CultureInfo("pt-BR");
            var dataAtual = DateTime.Now;

            // Adiciona os últimos 12 meses
            for (int i = 0; i < 12; i++)
            {
                var mes = dataAtual.AddMonths(-i);
                var nomeMes = culturaPortugues.DateTimeFormat.GetMonthName(mes.Month);
                var nomeMesCapitalizado = char.ToUpper(nomeMes[0]) + nomeMes.Substring(1);
                
                var item = new ComboBoxItem
                {
                    Content = $"{nomeMesCapitalizado}/{mes.Year}",
                    Tag = mes // Armazena a data no Tag para usar depois
                };
                
                cbFiltroPeriodo.Items.Add(item);
            }
        }

        private void DefinirFiltroPadrao()
        {
            // Seleciona "Este mês" como padrão
            foreach (ComboBoxItem item in cbFiltroPeriodo.Items)
            {
                if (item.Content.ToString() == "Este mês")
                {
                    cbFiltroPeriodo.SelectedItem = item;
                    break;
                }
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

            // Expandir lançamentos parcelados
            var lancamentosExpandidos = ExpandirLancamentosParcelados(lancamentos);
            
            // Aplicar filtro de período DEPOIS de expandir as parcelas
            var lancamentosFiltrados = AplicarFiltroPeriodoEmLista(lancamentosExpandidos);

            // Calcular saldo do período filtrado
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
            // Calcula o número real da parcela baseado na parcela inicial
            int numeroParcelaReal = lancamentoOriginal.nr_parcelaInicial + numeroParcela - 1;
            
            return new Lancamento
            {
                id_lancamento = lancamentoOriginal.id_lancamento * 1000 + numeroParcela,
                id_usuario = lancamentoOriginal.id_usuario,
                nm_descricao = $"{lancamentoOriginal.nm_descricao} ({numeroParcelaReal}/{lancamentoOriginal.nr_parcelas})",
                nm_tipo = lancamentoOriginal.nm_tipo,
                nm_formaPagamento = lancamentoOriginal.nm_formaPagamento,
                nr_valor = lancamentoOriginal.nr_valor / ObterQuantidadeParcelasRestantes(lancamentoOriginal),
                nr_parcelas = lancamentoOriginal.nr_parcelas,
                nr_parcelaInicial = lancamentoOriginal.nr_parcelaInicial,
                dt_dataLancamento = lancamentoOriginal.dt_dataLancamento.AddMonths(numeroParcela - 1)
            };
        }

        private int ObterQuantidadeParcelasRestantes(Lancamento lancamento)
        {
            // Calcula quantas parcelas faltam (de parcelaInicial até o total)
            return lancamento.nr_parcelas - lancamento.nr_parcelaInicial + 1;
        }

        private List<Lancamento> ExpandirLancamentosParcelados(List<Lancamento> lancamentos)
        {
            var lancamentosExpandidos = new List<Lancamento>();

            foreach (var lancamento in lancamentos)
            {
                int parcelasRestantes = ObterQuantidadeParcelasRestantes(lancamento);

                if (parcelasRestantes > 1)
                {
                    // Cria apenas as parcelas restantes
                    for (int i = 1; i <= parcelasRestantes; i++)
                    {
                        var parcelaVirtual = CriarParcelaVirtual(lancamento, i);
                        lancamentosExpandidos.Add(parcelaVirtual);
                    }
                }
                else
                {
                    // Lançamento sem parcelamento
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
            if (cbFiltroPeriodo == null || cbFiltroPeriodo.SelectedItem == null)
                return lancamentos;

            var itemSelecionado = cbFiltroPeriodo.SelectedItem as ComboBoxItem;
            var periodoSelecionado = itemSelecionado?.Content.ToString();
            var dataAtual = DateTime.Now;

            // Verifica se é um mês específico (tem Tag)
            if (itemSelecionado?.Tag is DateTime mesEspecifico)
            {
                return lancamentos.Where(l => 
                    l.dt_dataLancamento.Month == mesEspecifico.Month &&
                    l.dt_dataLancamento.Year == mesEspecifico.Year).ToList();
            }

            // Filtros rápidos
            return periodoSelecionado switch
            {
                "Hoje" => lancamentos.Where(l => l.dt_dataLancamento.Date == dataAtual.Date).ToList(),
                "Esta semana" => lancamentos.Where(l => l.dt_dataLancamento >= dataAtual.AddDays(-7)).ToList(),
                "Este mês" => lancamentos.Where(l => l.dt_dataLancamento.Month == dataAtual.Month &&
                                                     l.dt_dataLancamento.Year == dataAtual.Year).ToList(),
                "Este ano" => lancamentos.Where(l => l.dt_dataLancamento.Year == dataAtual.Year).ToList(),
                _ => lancamentos
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