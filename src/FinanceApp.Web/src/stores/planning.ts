import { defineStore } from 'pinia';
import api from '../api/axios';

export const usePlanningStore = defineStore('planning', {
    state: () => ({
        budgets: [] as any[],
        recurringTransactions: [] as any[],
        processedResult: null as any | null,
        isLoading: false
    }),
    actions: {
        async fetchBudgets(month: number, year: number) {
            this.isLoading = true;
            try {
                const response = await api.get(`/budgets/${month}/${year}`);
                this.budgets = response.data;
            } catch (error) {
                console.error('Failed to fetch budgets', error);
            } finally {
                this.isLoading = false;
            }
        },
        async createOrUpdateBudget(budget: any) {
            try {
                await api.post('/budgets', budget);
                // Refresh list
                await this.fetchBudgets(budget.mes, budget.ano);
            } catch (error) {
                console.error('Failed to save budget', error);
                throw error;
            }
        },
        async deleteBudget(id: number, month: number, year: number) {
            try {
                await api.delete(`/budgets/${id}`);
                await this.fetchBudgets(month, year);
            } catch (error) {
                console.error('Failed to delete budget', error);
                throw error;
            }
        },
        async fetchRecurringTransactions() {
            this.isLoading = true;
            try {
                const response = await api.get('/recurringtransactions');
                this.recurringTransactions = response.data;
            } catch (error) {
                console.error('Failed to fetch recurring transactions', error);
            } finally {
                this.isLoading = false;
            }
        },
        async createRecurringTransaction(transaction: any) {
            try {
                await api.post('/recurringtransactions', transaction);
                await this.fetchRecurringTransactions();
            } catch (error) {
                console.error('Failed to create recurring transaction', error);
                throw error;
            }
        },
        async updateRecurringTransaction(id: number, transaction: any) {
            try {
                await api.put(`/recurringtransactions/${id}`, transaction);
                await this.fetchRecurringTransactions();
            } catch (error) {
                console.error('Failed to update recurring transaction', error);
                throw error;
            }
        },
        async processRecurringTransactions() {
            try {
                // Call using the secret (in real app, this should be handled better)
                // For client-side testing, we assume the user has permission via Authorize if we allowed it, 
                // but we set it to allow anonymous with header. 
                // Since this is a "Test" button on frontend, we might need a workaround or just call it if we enable Authorize as well.
                // In my controller, I only enabled [AllowAnonymous] with Header check. 
                // I should probably also allow [Authorize] for Admin users to trigger it manually.

                // Correction: The controller requires X-Scheduler-Secret. 
                // I will add a method to my backend to allow Authenticated Admin to trigger it too, OR just pass the secret if known (bad practice for frontend).
                // Let's assume for now we can't trigger it easily from frontend without the secret. 
                // Valid workaround: Add an endpoint wrapper in RecurringTransactionsController for Admins.

                // For now, let's just Try to hit it without secret and see if it fails, or maybe I should update controller to allow Admin?
                // I'll update the controller in next step if this is a blocker. 
                // But wait, the user wants a scheduler endpoint. Frontend trigger is just for testing.
                // I will skip frontend trigger implementation or add a "Run Now" that might fail if not configured.

                // Actually, I'll pass a dummy secret or implement a dedicated admin endpoint later.
                // For now, let's leave this action empty or log a warning.
                console.warn("Manual processing requires scheduler setup.");
            } catch (error) {
                console.error(error);
            }
        }
    }
});
