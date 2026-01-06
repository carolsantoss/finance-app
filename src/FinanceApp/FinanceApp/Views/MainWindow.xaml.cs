using FinanceApp.Models;
using FinanceApp.ViewModels;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace FinanceApp.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainViewModel();
        }

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

        private void AtualizarDados()
        {
            DataContext = new MainViewModel();
        }
    }
}
