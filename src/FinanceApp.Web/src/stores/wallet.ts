import { defineStore } from 'pinia';
import api from '../api/axios';

export interface Wallet {
    id_wallet: number;
    nm_nome: string;
    nm_tipo: string;
    nr_saldo_inicial: number;
    currentBalance: number;
}

export interface CreditCard {
    id_credit_card: number;
    nm_nome: string;
    nm_bandeira: string;
    nr_limite: number;
    nr_dia_fechamento: number;
    nr_dia_vencimento: number;
    walletName?: string;
    currentInvoice: number;
    availableLimit: number;
}

export const useWalletStore = defineStore('wallet', {
    state: () => ({
        wallets: [] as Wallet[],
        creditCards: [] as CreditCard[],
        isLoading: false
    }),
    getters: {
        totalBalance: (state) => state.wallets.reduce((acc, w) => acc + w.currentBalance, 0),
        totalCreditLimit: (state) => state.creditCards.reduce((acc, c) => acc + c.nr_limite, 0),
        totalCreditUsed: (state) => state.creditCards.reduce((acc, c) => acc + c.currentInvoice, 0)
    },
    actions: {
        async fetchWallets() {
            try {
                const response = await api.get('/wallets');
                this.wallets = response.data;
            } catch (error) {
                console.error('Failed to fetch wallets', error);
            }
        },
        async createWallet(wallet: Partial<Wallet>) {
            await api.post('/wallets', wallet);
            await this.fetchWallets();
        },
        async updateWallet(id: number, wallet: Partial<Wallet>) {
            await api.put(`/wallets/${id}`, wallet);
            await this.fetchWallets();
        },
        async deleteWallet(id: number) {
            await api.delete(`/wallets/${id}`);
            await this.fetchWallets();
        },

        async fetchCards() {
            try {
                const response = await api.get('/creditcards');
                this.creditCards = response.data;
            } catch (error) {
                console.error('Failed to fetch cards', error);
            }
        },
        async createCard(card: Partial<CreditCard>) {
            await api.post('/creditcards', card);
            await this.fetchCards();
        },
        async updateCard(id: number, card: Partial<CreditCard>) {
            await api.put(`/creditcards/${id}`, card);
            await this.fetchCards();
        },
        async deleteCard(id: number) {
            await api.delete(`/creditcards/${id}`);
            await this.fetchCards();
        },

        async fetchAll() {
            this.isLoading = true;
            await Promise.all([this.fetchWallets(), this.fetchCards()]);
            this.isLoading = false;
        }
    }
});
