using FinanceApp.Data;
using FinanceApp.Helpers;
using FinanceApp.Models;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;

namespace FinanceApp.Views
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        #region Campos Privados

        private decimal _saldoMesAtual;
        private decimal _totalEntradas;
        private decimal _totalSaidas;
        private string _nomeUsuario = "Usuário";

        #endregion

        #region Propriedades Públicas

        public string NomeUsuario
        {
            get => _nomeUsuario;
            set
            {
                _nomeUsuario = value;
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

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            CarregarNomeUsuario();
            AtualizarDados();
        }

        #endregion

        #region Eventos

        private void BtnNovoLancamento_Click(object sender, RoutedEventArgs e)
        {
            var tela = new LancamentoWindow();
            tela.ShowDialog();

            AtualizarDados();
        }

        private void BtnExtratos_Click(object sender, RoutedEventArgs e)
        {
            var tela = new ExtratosWindow();
            tela.ShowDialog();

            AtualizarDados();
        }

        private void BtnPerfil_Click(object sender, RoutedEventArgs e)
        {
            var tela = new PerfilWindow();
            tela.ShowDialog();

            // Atualiza o nome do usuário caso tenha sido alterado
            CarregarNomeUsuario();
        }

        private void BtnSair_Click(object sender, RoutedEventArgs e)
        {
            Session.UsuarioLogado = null;
            var login = new LoginWindow();
            login.Show();
            Close();
        }

        #endregion

        #region Métodos

        private void CarregarNomeUsuario()
        {
            var usuarioLogado = Session.UsuarioLogado;
            if (usuarioLogado != null)
            {
                NomeUsuario = usuarioLogado.nm_nomeUsuario;
            }
        }

        private void AtualizarDados()
        {
            var usuarioLogado = Session.UsuarioLogado;
            if (usuarioLogado == null)
                return;

            using var contexto = new AppDbContextFactory().CreateDbContext();

            var mesAtual = DateTime.Now.Month;
            var anoAtual = DateTime.Now.Year;

            // Busca todos os lançamentos do usuário
            var todosLancamentos = contexto.lancamentos
                .Where(l => l.id_usuario == usuarioLogado.id_usuario)
                .ToList();

            // Expandir lançamentos parcelados
            var lancamentosExpandidos = ExpandirLancamentosParcelados(todosLancamentos);

            // Filtra apenas os lançamentos do mês atual
            var lancamentosMesAtual = lancamentosExpandidos
                .Where(l => l.dt_dataLancamento.Month == mesAtual &&
                           l.dt_dataLancamento.Year == anoAtual)
                .ToList();

            // Calcula totais do mês
            TotalEntradas = lancamentosMesAtual
                .Where(l => l.nm_tipo.Contains("Entrada"))
                .Sum(l => l.nr_valor);

            TotalSaidas = lancamentosMesAtual
                .Where(l => l.nm_tipo.Contains("Saída"))
                .Sum(l => l.nr_valor);

            SaldoMesAtual = TotalEntradas - TotalSaidas;
        }

        private List<Lancamento> ExpandirLancamentosParcelados(List<Lancamento> lancamentos)
        {
            var lancamentosExpandidos = new List<Lancamento>();

            foreach (var lancamento in lancamentos)
            {
                // Só expande se tiver mais de 1 parcela total E se parcelaInicial <= total
                if (lancamento.nr_parcelas > 1 && lancamento.nr_parcelaInicial <= lancamento.nr_parcelas)
                {
                    int parcelasRestantes = ObterQuantidadeParcelasRestantes(lancamento);
                    
                    // Cria apenas as parcelas restantes
                    for (int i = 1; i <= parcelasRestantes; i++)
                    {
                        var parcelaVirtual = CriarParcelaVirtual(lancamento, i);
                        lancamentosExpandidos.Add(parcelaVirtual);
                    }
                }
                else
                {
                    // Lançamento sem parcelamento ou parcela única
                    lancamentosExpandidos.Add(lancamento);
                }
            }

            return lancamentosExpandidos;
        }

        private Lancamento CriarParcelaVirtual(Lancamento lancamentoOriginal, int numeroParcela)
        {
            // Calcula o número real da parcela baseado na parcela inicial
            int numeroParcelaReal = lancamentoOriginal.nr_parcelaInicial + numeroParcela - 1;
            
            // CORREÇÃO: Divide pelo TOTAL de parcelas, não pelas restantes
            decimal valorPorParcela = lancamentoOriginal.nr_valor / lancamentoOriginal.nr_parcelas;
            
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