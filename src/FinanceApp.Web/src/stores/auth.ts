import { defineStore } from 'pinia';
import api from '../api/axios';
import router from '../router';

export const useAuthStore = defineStore('auth', {
    state: () => ({
        token: localStorage.getItem('token') || '',
        user: JSON.parse(localStorage.getItem('user') || 'null'),
    }),
    getters: {
        isAuthenticated: (state) => !!state.token,
    },
    actions: {
        async login(credentials: any) {
            try {
                const response = await api.post('/auth/login', credentials);
                this.token = response.data.token;
                this.user = { nomeUsuario: response.data.nomeUsuario };

                localStorage.setItem('token', this.token);
                localStorage.setItem('user', JSON.stringify(this.user));

                router.push('/');
            } catch (error) {
                console.error('Login failed', error);
                throw error;
            }
        },
        async register(credentials: any) {
            try {
                const response = await api.post('/auth/register', credentials);
                this.token = response.data.token;
                this.user = { nomeUsuario: response.data.nomeUsuario };

                localStorage.setItem('token', this.token);
                localStorage.setItem('user', JSON.stringify(this.user));

                router.push('/');
            } catch (error) {
                console.error('Registration failed', error);
                throw error;
            }
        },
        logout() {
            this.token = '';
            this.user = null;
            localStorage.removeItem('token');
            localStorage.removeItem('user');
            router.push('/login');
        }
    }
});
