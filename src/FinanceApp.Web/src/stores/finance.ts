import { defineStore } from 'pinia';
import api from '../api/axios';

export const useFinanceStore = defineStore('finance', {
    state: () => ({
        transactions: [] as any[],
        summary: { entradas: 0, saidas: 0, saldo: 0 }
    }),
    actions: {
        async fetchSummary() {
            try {
                const response = await api.get('/lancamentos/summary');
                this.summary = response.data;
            } catch (error) {
                console.error('Failed to fetch summary', error);
            }
        },
        async fetchTransactions() {
            try {
                const response = await api.get('/lancamentos');
                this.transactions = response.data;
            } catch (error) {
                console.error('Failed to fetch transactions', error);
            }
        },
        async addTransaction(transaction: any) {
            try {
                await api.post('/lancamentos', transaction);
                await this.fetchTransactions();
                await this.fetchSummary();
            } catch (error) {
                console.error('Failed to add transaction', error);
                throw error;
            }
        },
        async deleteTransaction(id: number) {
            try {
                await api.delete(`/lancamentos/${id}`);
                await this.fetchTransactions();
                await this.fetchSummary();
            } catch (error) {
                console.error('Failed to delete transaction', error);
                throw error;
            }
        }
    }
});
