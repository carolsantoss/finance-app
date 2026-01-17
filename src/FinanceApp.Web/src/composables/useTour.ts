import { driver } from "driver.js";
import "driver.js/dist/driver.css";

export function useTour() {
    const tour = driver({
        showProgress: true,
        steps: [
            { element: '#sidebar-nav', popover: { title: 'Navegação', description: 'Acesse aqui todas as funcionalidades: Dashboard, Extratos, Relatórios e Configurações.' } },
            { element: '#theme-toggle', popover: { title: 'Temas', description: 'Prefere claro ou escuro? Alterne aqui o visual do sistema.' } },
            { element: '#btn-new-transaction', popover: { title: 'Novo Lançamento', description: 'Registre suas entradas e saídas rapidamente clicando aqui.' } },
            { element: '#summary-cards', popover: { title: 'Resumo', description: 'Visualize seu saldo, receitas e despesas do mês.' } },
            { element: '#recent-transactions', popover: { title: 'Últimos Lançamentos', description: 'Acompanhe suas transações mais recentes.' } }
        ],
        onDestroyed: () => {
            localStorage.setItem('hasSeenTour', 'true');
        }
    });

    const startTour = () => {
        tour.drive();
    };

    const checkAndStartTour = () => {
        if (!localStorage.getItem('hasSeenTour')) {
            // Delay slightly to ensure DOM is ready
            setTimeout(() => {
                tour.drive();
            }, 1000);
        }
    };

    return {
        startTour,
        checkAndStartTour
    };
}
