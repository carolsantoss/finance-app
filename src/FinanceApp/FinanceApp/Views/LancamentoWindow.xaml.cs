using FinanceApp.Data;
using FinanceApp.Helpers;
using FinanceApp.Models;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using static FinanceApp.Helpers.Session;

namespace FinanceApp.Views
{
    public partial class LancamentoWindow : Window
    {
        public LancamentoWindow()
        {
            InitializeComponent();
            dpData.SelectedDate = DateTime.Now;
        }

        private void cbPagamento_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (cbPagamento.SelectedItem is ComboBoxItem item &&
                item.Content?.ToString() == "Crédito")
            {
                PainelParcelas.Visibility = Visibility.Visible;
            }
            else
            {
                PainelParcelas.Visibility = Visibility.Collapsed;
                txtParcelas.Text = string.Empty;
            }
        }


        private void BtnSalvarLancamento_Click(object sender, RoutedEventArgs e)
        {
            if (Session.UsuarioLogado == null)
            {
                MessageBox.Show("Usuário não autenticado.");
                return;
            }

            if (cbTipo.SelectedItem == null ||
                string.IsNullOrWhiteSpace(txtDescricao.Text) ||
                string.IsNullOrWhiteSpace(txtValor.Text) ||
                dpData.SelectedDate == null)
            {
                MessageBox.Show("Preencha todos os campos.");
                return;
            }

            if (!decimal.TryParse(txtValor.Text, out decimal valor))
            {
                MessageBox.Show("Valor inválido.");
                return;
            }

            using var context = new AppDbContextFactory().CreateDbContext();

            var lancamento = new Lancamento
            {
                nm_tipo = ((ComboBoxItem)cbTipo.SelectedItem).Content.ToString(),
                nm_descricao = txtDescricao.Text,
                nr_valor = valor,
                dt_dataLancamento = dpData.SelectedDate.Value,
                id_usuario = Session.UsuarioLogado.id_usuario
            };

            context.lancamentos.Add(lancamento);
            context.SaveChanges();

            MessageBox.Show("Lançamento salvo com sucesso!");
            this.Close();
        }
    }
}
