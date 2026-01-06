using FinanceApp.Data;
using FinanceApp.Helpers;
using FinanceApp.Models;
using System.Globalization;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace FinanceApp.Views
{
    public partial class LancamentoWindow : Window
    {
        #region Constantes

        private const string PADRAO_APENAS_NUMEROS = "[^0-9]+";
        private const string FORMATO_MOEDA_BRASILEIRA = "N2";
        private const string CULTURA_PORTUGUES_BRASIL = "pt-BR";

        #endregion

        #region Campos Privados

        private bool _estaAtualizandoValor = false;

        #endregion

        #region Construtor

        public LancamentoWindow()
        {
            InitializeComponent();
            InicializarCampos();
        }

        #endregion

        #region Métodos de Inicialização

        private void InicializarCampos()
        {
            dpData.SelectedDate = DateTime.Now;
        }

        #endregion

        #region Eventos de Interface
        private void cbPagamento_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ExibirOcultarPainelParcelas();
        }

        private void txtValor_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !EhTextoNumerico(e.Text);
        }

        private void txtValor_TextChanged(object sender, TextChangedEventArgs e)
        {
            FormatarCampoValor(sender as TextBox);
        }

        private void txtParcelas_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !EhTextoNumerico(e.Text);
        }

        private void BtnSalvarLancamento_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidarUsuarioAutenticado())
                return;

            if (!ValidarCamposObrigatorios())
                return;

            var valorLancamento = ObterValorFormatado();
            if (valorLancamento == null)
                return;

            SalvarLancamentoNoBanco(valorLancamento.Value);
            ExibirMensagemSucesso();
            FecharJanela();
        }

        #endregion

        #region Métodos de Validação

        private bool ValidarUsuarioAutenticado()
        {
            if (Session.UsuarioLogado == null)
            {
                ExibirMensagemErro("Usuário não autenticado.");
                return false;
            }
            return true;
        }

        private bool ValidarCamposObrigatorios()
        {
            if (cbTipo.SelectedItem == null ||
                string.IsNullOrWhiteSpace(txtDescricao.Text) ||
                string.IsNullOrWhiteSpace(txtValor.Text) ||
                dpData.SelectedDate == null)
            {
                ExibirMensagemErro("Preencha todos os campos obrigatórios.");
                return false;
            }
            return true;
        }

        private bool EhTextoNumerico(string texto)
        {
            var regex = new Regex(PADRAO_APENAS_NUMEROS);
            return !regex.IsMatch(texto);
        }

        #endregion

        #region Métodos de Formatação

        private void FormatarCampoValor(TextBox? campoTexto)
        {
            if (_estaAtualizandoValor || campoTexto == null)
                return;

            _estaAtualizandoValor = true;

            try
            {
                var textoSemFormatacao = RemoverFormatacaoMonetaria(campoTexto.Text);

                if (TentarConverterParaNumero(textoSemFormatacao, out long valorNumerico))
                {
                    AplicarFormatacaoMonetaria(campoTexto, valorNumerico);
                }
            }
            finally
            {
                _estaAtualizandoValor = false;
            }
        }

        private string RemoverFormatacaoMonetaria(string texto)
        {
            return texto.Replace(".", "").Replace(",", "");
        }

        private bool TentarConverterParaNumero(string texto, out long valor)
        {
            valor = 0;
            return !string.IsNullOrEmpty(texto) && long.TryParse(texto, out valor);
        }

        private void AplicarFormatacaoMonetaria(TextBox campoTexto, long valorNumerico)
        {
            var valorDecimal = valorNumerico / 100m;
            var posicaoCursorAnterior = campoTexto.SelectionStart;
            var tamanhoAnterior = campoTexto.Text.Length;

            var culturaPortugues = CultureInfo.GetCultureInfo(CULTURA_PORTUGUES_BRASIL);
            campoTexto.Text = valorDecimal.ToString(FORMATO_MOEDA_BRASILEIRA, culturaPortugues);

            AjustarPosicaoCursor(campoTexto, posicaoCursorAnterior, tamanhoAnterior);
        }

        private void AjustarPosicaoCursor(TextBox campoTexto, int posicaoAnterior, int tamanhoAnterior)
        {
            var tamanhoNovo = campoTexto.Text.Length;
            var diferencaTamanho = tamanhoNovo - tamanhoAnterior;
            campoTexto.SelectionStart = Math.Max(0, posicaoAnterior + diferencaTamanho);
        }

        private decimal? ObterValorFormatado()
        {
            var textoValor = txtValor.Text.Replace(".", "").Replace(",", ".");

            if (!decimal.TryParse(textoValor, NumberStyles.Any, CultureInfo.InvariantCulture, out decimal valor))
            {
                ExibirMensagemErro("Valor inválido. Digite apenas números.");
                return null;
            }

            return valor;
        }

        #endregion

        #region Métodos de Interface (UI)

        private void ExibirOcultarPainelParcelas()
        {
            var itemSelecionado = cbPagamento.SelectedItem as ComboBoxItem;
            var ehCredito = itemSelecionado?.Content?.ToString()?.Contains("Crédito") == true;

            if (ehCredito)
            {
                PainelParcelas.Visibility = Visibility.Visible;
            }
            else
            {
                PainelParcelas.Visibility = Visibility.Collapsed;
                LimparCampoParcelas();
            }
        }

        private void LimparCampoParcelas()
        {
            txtParcelas.Text = string.Empty;
        }

        #endregion

        #region Métodos de Persistência

        private void SalvarLancamentoNoBanco(decimal valor)
        {
            using var contexto = new AppDbContextFactory().CreateDbContext();

            var novoLancamento = CriarNovoLancamento(valor);

            contexto.lancamentos.Add(novoLancamento);
            contexto.SaveChanges();
        }

        private Lancamento CriarNovoLancamento(decimal valor)
        {
            var itemTipoSelecionado = (ComboBoxItem)cbTipo.SelectedItem;

            return new Lancamento
            {
                nm_tipo = itemTipoSelecionado.Content.ToString(),
                nm_descricao = txtDescricao.Text,
                nr_valor = valor,
                dt_dataLancamento = dpData.SelectedDate!.Value,
                id_usuario = Session.UsuarioLogado!.id_usuario
            };
        }

        #endregion

        #region Métodos de Mensagens

        private void ExibirMensagemErro(string mensagem)
        {
            MessageBox.Show(mensagem, "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void ExibirMensagemSucesso()
        {
            MessageBox.Show("Lançamento salvo com sucesso!", "Sucesso", MessageBoxButton.OK, MessageBoxImage.Information);
        }

        #endregion

        #region Métodos de Navegação

        private void FecharJanela()
        {
            Close();
        }

        #endregion
    }
}