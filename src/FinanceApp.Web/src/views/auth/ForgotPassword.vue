<script setup lang="ts">
import { ref } from 'vue';
import { useRouter } from 'vue-router';
import { useAuthStore } from '../../stores/auth'; // We might need a new action here or just call api directly
import api from '../../api/axios';
import { ArrowLeft, Mail } from 'lucide-vue-next';

const router = useRouter();
const email = ref('');
const isSubmitting = ref(false);
const message = ref('');
const error = ref('');

const handleSubmit = async () => {
    isSubmitting.value = true;
    message.value = '';
    error.value = '';

    try {
        await api.post('/auth/forgot-password', { email: email.value });
        message.value = 'Se o email estiver cadastrado, você receberá um link de redefinição.';
    } catch (e) {
        // Even on error we might show success message to prevent enumeration, or generic error
        message.value = 'Se o email estiver cadastrado, você receberá um link de redefinição.';
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
                    <Mail class="text-white w-6 h-6" />
                </div>
                <h1 class="text-2xl font-bold text-text-primary mb-2">Recuperar Senha</h1>
                <p class="text-text-secondary">Informe seu email para receber o link de redefinição.</p>
            </div>

            <form @submit.prevent="handleSubmit" class="space-y-6">
                <div>
                    <label class="block text-sm font-medium text-text-secondary mb-1">Email</label>
                    <div class="relative">
                        <div class="absolute inset-y-0 left-0 pl-3 flex items-center pointer-events-none">
                            <Mail class="h-5 w-5 text-text-tertiary" />
                        </div>
                        <input 
                            v-model="email"
                            type="email" 
                            required
                            class="w-full bg-input border border-border rounded-lg pl-10 pr-4 py-3 text-text-primary placeholder-text-tertiary focus:ring-2 focus:ring-brand focus:border-brand outline-none transition-all"
                            placeholder="seu@email.com"
                        />
                    </div>
                </div>

                <div v-if="message" class="p-4 bg-brand/10 border border-brand/20 rounded-lg">
                    <p class="text-sm text-brand text-center">{{ message }}</p>
                </div>

                <button 
                    type="submit" 
                    :disabled="isSubmitting"
                    class="w-full bg-brand hover:bg-brand-hover text-white font-bold py-3 rounded-lg transition-all shadow-lg hover:shadow-brand/20 disabled:opacity-50 disabled:cursor-not-allowed flex items-center justify-center"
                >
                    <span v-if="isSubmitting" class="w-5 h-5 border-2 border-white/20 border-t-white rounded-full animate-spin mr-2"></span>
                    {{ isSubmitting ? 'Enviando...' : 'Enviar Link' }}
                </button>
            </form>

            <div class="mt-6 text-center">
                <router-link to="/login" class="text-sm text-text-secondary hover:text-brand flex items-center justify-center gap-2 transition-colors">
                    <ArrowLeft class="w-4 h-4" /> Voltar para Login
                </router-link>
            </div>
        </div>
    </div>
</template>
