using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using FinanceApp.Data;

namespace FinanceApp.ViewModels
{
    public class MainViewModel : INotifyPropertyChanged
    {
        private decimal _entradas;
        private decimal _saidas;

        public decimal Entradas
        {
            get => _entradas;
            set
            {
                _entradas = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(Saldo));
            }
        }

        public decimal Saidas
        {
            get => _saidas;
            set
            {
                _saidas = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(Saldo));
            }
        }

        public decimal Saldo => Entradas - Saidas;

        public MainViewModel()
        {
            CarregarDados();
        }

        private void CarregarDados()
        {
            using var context = new AppDbContextFactory().CreateDbContext([]);

            Entradas = context.lancamentos
                              .Where(l => l.hs_tipo == "Entrada")
                              .Select(l => (decimal?)l.hs_valor)
                              .Sum() ?? 0;

            Saidas = context.lancamentos
                            .Where(l => l.hs_tipo == "Saida")
                            .Select(l => (decimal?)l.hs_valor)
                            .Sum() ?? 0;

        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
