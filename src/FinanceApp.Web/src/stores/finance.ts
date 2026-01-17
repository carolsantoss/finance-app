import { defineStore } from 'pinia';
import api from '../api/axios';

export const useFinanceStore = defineStore('finance', {
    state: () => ({
        transactions: [] as any[],
        summary: { entradas: 0, saidas: 0, saldo: 0, percentageChange: 0 },
        chartData: { labels: [], incomeData: [], expenseData: [] }, // Default empty state
        isLoading: false
    }),
    actions: {
        async fetchAll() {
            this.isLoading = true;
            try {
                await Promise.all([
                    this.fetchSummary(),
                    this.fetchTransactions(),
                    this.fetchChartData()
                ]);
            } catch (error) {
                console.error('Failed to fetch all data', error);
            } finally {
                this.isLoading = false;
            }
        },
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
                await this.fetchAll();
            } catch (error) {
                console.error('Failed to add transaction', error);
                throw error;
            }
        },
        async deleteTransaction(id: number) {
            try {
                await api.delete(`/lancamentos/${id}`);
                await this.fetchAll();
            } catch (error) {
                console.error('Failed to delete transaction', error);
                throw error;
            }
        },
        async fetchChartData() {
            try {
                const response = await api.get('/lancamentos/chart');
                this.chartData = response.data;
            } catch (error) {
                console.error('Failed to fetch chart data', error);
            }
        },
    }
});
