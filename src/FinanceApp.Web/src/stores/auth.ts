import { defineStore } from 'pinia';
import api from '../api/axios';
import router from '../router';

interface User {
    id: number;
    nomeUsuario: string;
    email: string;
    isAdmin: boolean;
    jobTitle?: string;
    phone?: string;
    bio?: string;
    isTwoFactorEnabled?: boolean;
    referralCode?: string;
    referralCount?: number;
}

export const useAuthStore = defineStore('auth', {
    state: () => ({
        token: localStorage.getItem('token') || '',
        user: JSON.parse(localStorage.getItem('user') || 'null') as User | null,
    }),
    getters: {
        isAuthenticated: (state) => !!state.token,
    },
    actions: {
        async login(credentials: { email: string; senha: string; rememberMe: boolean }) {
            try {
                const response = await api.post('/auth/login', credentials);

                if (response.data.requiresTwoFactor) {
                    return response.data; // Component handles 2FA step
                }

                this.token = response.data.token;
                this.user = {
                    id: response.data.id,
                    nomeUsuario: response.data.nomeUsuario,
                    email: response.data.email,
                    isAdmin: response.data.isAdmin,
                    jobTitle: response.data.jobTitle,
                    phone: response.data.phone,
                    bio: response.data.bio,
                    isTwoFactorEnabled: response.data.isTwoFactorEnabled,
                    referralCode: response.data.referralCode,
                    referralCount: response.data.referralCount
                };

                localStorage.setItem('token', this.token);
                localStorage.setItem('user', JSON.stringify(this.user));

                router.push('/');
                return response.data;
            } catch (error) {
                console.error('Login failed', error);
                throw error;
            }
        },
        async login2FA(data: { email: string; code: string }) {
            try {
                const response = await api.post('/auth/login-2fa', data);

                this.token = response.data.token;
                this.user = {
                    id: response.data.id,
                    nomeUsuario: response.data.nomeUsuario,
                    email: response.data.email,
                    isAdmin: response.data.isAdmin,
                    jobTitle: response.data.jobTitle,
                    phone: response.data.phone,
                    bio: response.data.bio,
                    isTwoFactorEnabled: response.data.isTwoFactorEnabled,
                    referralCode: response.data.referralCode,
                    referralCount: response.data.referralCount
                };

                localStorage.setItem('token', this.token);
                localStorage.setItem('user', JSON.stringify(this.user));

                router.push('/');
                return response.data;
            } catch (error) {
                console.error('2FA Login failed', error);
                throw error;
            }
        },
        async register(userData: any) {
            try {
                // userData includes referralCode if present
                await api.post('/auth/register', userData);
                // After register, redirect to login
                router.push('/login');
            } catch (error) {
                console.error('Registration failed', error);
                throw error;
            }
        },
        async fetchUser() {
            try {
                const response = await api.get('/users/me');
                this.user = response.data;
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
        async updateProfile(data: Partial<User>) {
            try {
                const response = await api.put(`/users/${(this.user as any)?.id || 'me'}`, data);
                // Note: The API endpoint logic might need adjustment. 
                // Controller has [HttpPut("{id}")] for UpdateUser and [HttpPut("me/password")]
                // We should probably rely on UpdateUser(id, DTO). 
                // Wait, UsersController has UpdateUser at PUT /api/users/{id}
                // But typically for self-update we might want a specific endpoint or use ID.
                // Let's check UsersController. It has GetMe but not UpdateMe (except password).
                // Ah, UpdateUser is PUT /api/users/{id}. 
                // Since user is logged in, we can get ID from store.

                // Correction: The users/me endpoint was GET only.
                // We need to use PUT /api/users/{id}.

                this.user = { ...this.user, ...data } as User;
                localStorage.setItem('user', JSON.stringify(this.user));

                // Ideally backend returns updated user.
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
        },
        async validateReferral(code: string) {
            try {
                const response = await api.get(`/auth/validate-referral/${code}`);
                return response.data; // { valid: true, referrerName: '...' }
            } catch (error) {
                // If 404, it means invalid
                throw error;
            }
        }
    }
});
