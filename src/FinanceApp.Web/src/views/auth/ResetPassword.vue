<script setup lang="ts">
import { ref, onMounted } from 'vue';
import { useRoute, useRouter } from 'vue-router';
import api from '../../api/axios';
import { Lock, CheckCircle, AlertCircle } from 'lucide-vue-next';

const route = useRoute();
const router = useRouter();
const token = ref('');
const password = ref('');
const confirmPassword = ref('');
const isSubmitting = ref(false);
const message = ref('');
const error = ref('');
const success = ref(false);

onMounted(() => {
    token.value = route.query.token as string;
    if (!token.value) {
        error.value = 'Token inválido ou não fornecido.';
    }
});

const handleSubmit = async () => {
    if (password.value !== confirmPassword.value) {
        error.value = 'As senhas não coincidem.';
        return;
    }
    
    isSubmitting.value = true;
    error.value = '';
    message.value = '';

    try {
        await api.post('/auth/reset-password', { 
            token: token.value, 
            newPassword: password.value 
        });
        success.value = true;
        message.value = 'Senha redefinida com sucesso!';
        setTimeout(() => router.push('/login'), 3000);
    } catch (e: any) {
        error.value = e.response?.data?.message || 'Erro ao redefinir senha.';
    } finally {
        isSubmitting.value = false;
    }
};
</script>

<template>
    <div class="min-h-screen bg-app flex items-center justify-center p-4">
        <div class="bg-card w-full max-w-md p-8 rounded-2xl shadow-2xl border border-border animate-in fade-in zoom-in duration-300">
            <div class="mb-8 text-center">
                 <div class="w-12 h-12 bg-brand rounded-full flex items-center justify-center mx-auto mb-4 shadow-lg shadow-brand/20">
                    <Lock class="text-white w-6 h-6" />
                </div>
                <h1 class="text-2xl font-bold text-text-primary mb-2">Nova Senha</h1>
                <p class="text-text-secondary">Defina sua nova senha de acesso.</p>
            </div>

            <div v-if="success" class="text-center space-y-4">
                <CheckCircle class="w-16 h-16 text-brand mx-auto" />
                <p class="text-lg font-medium text-text-primary">{{ message }}</p>
                <p class="text-text-secondary">Redirecionando para login...</p>
            </div>

            <form v-else @submit.prevent="handleSubmit" class="space-y-6">
                <div>
                    <label class="block text-sm font-medium text-text-secondary mb-1">Nova Senha</label>
                    <div class="relative">
                        <div class="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none">
                            <Lock class="h-5 w-5 text-text-tertiary" />
                        </div>
                        <input 
                            v-model="password"
                            type="password" 
                            required
                            minlength="6"
                            class="w-full bg-input border border-border rounded-lg pl-10 pr-4 py-3 text-text-primary placeholder-text-tertiary focus:ring-2 focus:ring-brand focus:border-brand outline-none transition-all"
                            placeholder="******"
                        />
                    </div>
                </div>

                <div>
                    <label class="block text-sm font-medium text-text-secondary mb-1">Confirmar Senha</label>
                    <div class="relative">
                        <div class="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none">
                            <Lock class="h-5 w-5 text-text-tertiary" />
                        </div>
                        <input 
                            v-model="confirmPassword"
                            type="password" 
                            required
                            minlength="6"
                            class="w-full bg-input border border-border rounded-lg pl-10 pr-4 py-3 text-text-primary placeholder-text-tertiary focus:ring-2 focus:ring-brand focus:border-brand outline-none transition-all"
                            placeholder="******"
                        />
                    </div>
                </div>

                <div v-if="error" class="p-4 bg-danger/10 border border-danger/20 rounded-lg flex items-start gap-3">
                    <AlertCircle class="w-5 h-5 text-danger shrink-0 mt-0.5" />
                    <p class="text-sm text-danger">{{ error }}</p>
                </div>

                <button 
                    type="submit" 
                    :disabled="isSubmitting || !token"
                    class="w-full bg-brand hover:bg-brand-hover text-white font-bold py-3 rounded-lg transition-all shadow-lg hover:shadow-brand/20 disabled:opacity-50 disabled:cursor-not-allowed flex items-center justify-center"
                >
                    <span v-if="isSubmitting" class="w-5 h-5 border-2 border-white/20 border-t-white rounded-full animate-spin mr-2"></span>
                    {{ isSubmitting ? 'Salvando...' : 'Redefinir Senha' }}
                </button>
            </form>
        </div>
    </div>
</template>
