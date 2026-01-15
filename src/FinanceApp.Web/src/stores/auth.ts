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
        async login(credentials: { email: string; senha: string; rememberMe: boolean }) {
            try {
                const response = await api.post('/auth/login', credentials);
                this.token = response.data.token;
                this.user = {
                    nomeUsuario: response.data.nomeUsuario,
                    email: response.data.email
                };

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
                this.user = {
                    nomeUsuario: response.data.nomeUsuario,
                    email: response.data.email
                };

                localStorage.setItem('token', this.token);
                localStorage.setItem('user', JSON.stringify(this.user));

                router.push('/');
            } catch (error) {
                console.error('Registration failed', error);
                throw error;
            }
        },
        async fetchUser() {
            try {
                const response = await api.get('/users/me');
                this.user = { ...this.user, ...response.data };
                localStorage.setItem('user', JSON.stringify(this.user));
            } catch (error) {
                console.error('Failed to fetch user profile', error);
            }
        },
        logout() {
            this.token = '';
            this.user = null;
            localStorage.removeItem('token');
            localStorage.removeItem('user');
            router.push('/login');
        },
        async updateProfile(data: { nomeUsuario: string; email: string }) {
            try {
                const response = await api.put('/users/me', data);
                this.user = { ...this.user, ...response.data };
                localStorage.setItem('user', JSON.stringify(this.user));
            } catch (error) {
                console.error('Profile update failed', error);
                throw error;
            }
        },
        async changePassword(data: { currentPassword: string; newPassword: string; confirmNewPassword: string }) {
            try {
                await api.put('/users/me/password', data);
            } catch (error) {
                console.error('Password change failed', error);
                throw error;
            }
        }
    }
});
