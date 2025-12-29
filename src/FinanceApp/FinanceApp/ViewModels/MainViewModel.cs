using System.ComponentModel;
using System.Runtime.CompilerServices;

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

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string name = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
