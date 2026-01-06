using FinanceApp.Data;
using FinanceApp.Helpers;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Linq;

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
            var user = Session.UsuarioLogado;
            if (user == null) return;

            using var context = new AppDbContextFactory().CreateDbContext([]);

            Entradas = context.lancamentos
                .Where(l => l.id_usuario == user.id_usuario && l.nm_tipo == "Entrada")
                .Sum(l => l.nr_valor);

            Saidas = context.lancamentos
                .Where(l => l.id_usuario == user.id_usuario && l.nm_tipo == "Saida")
                .Sum(l => l.nr_valor);
        }

        public event PropertyChangedEventHandler? PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string name = null!)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}
