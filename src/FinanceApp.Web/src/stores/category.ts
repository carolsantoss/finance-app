import { defineStore } from 'pinia';
import api from '../api/axios';

export interface Category {
    id_categoria: number;
    nm_nome: string;
    nm_icone: string;
    nm_cor: string;
    nm_tipo: 'Entrada' | 'Saída';
    id_usuario?: number | null; // null if system default
}

export const useCategoryStore = defineStore('category', {
    state: () => ({
        categories: [] as Category[],
        isLoading: false
    }),
    getters: {
        incomeCategories: (state) => state.categories.filter(c => c.nm_tipo === 'Entrada'),
        expenseCategories: (state) => state.categories.filter(c => c.nm_tipo === 'Saída'),
        systemCategories: (state) => state.categories.filter(c => c.id_usuario === null),
        userCategories: (state) => state.categories.filter(c => c.id_usuario !== null)
    },
    actions: {
        async fetchCategories() {
            this.isLoading = true;
            try {
                const response = await api.get('/categories');
                this.categories = response.data;
            } catch (error) {
                console.error('Failed to fetch categories', error);
            } finally {
                this.isLoading = false;
            }
        },
        async createCategory(category: Partial<Category>) {
            try {
                await api.post('/categories', category);
                await this.fetchCategories();
            } catch (error) {
                console.error('Failed to create category', error);
                throw error;
            }
        },
        async updateCategory(id: number, category: Partial<Category>) {
            try {
                await api.put(`/categories/${id}`, category);
                await this.fetchCategories();
            } catch (error) {
                console.error('Failed to update category', error);
                throw error;
            }
        },
        async deleteCategory(id: number) {
            try {
                await api.delete(`/categories/${id}`);
                await this.fetchCategories();
            } catch (error) {
                console.error('Failed to delete category', error);
                throw error;
            }
        }
    }
});
