<script setup lang="ts">
import { ref } from 'vue';
import { useAuthStore } from '../stores/auth';
import { User, Lock, Wallet, ShieldCheck, ArrowLeft } from 'lucide-vue-next';

const auth = useAuthStore();
const email = ref('');
const password = ref('');
const code = ref('');
const rememberMe = ref(false);
const isLoading = ref(false);
const step = ref<'login' | '2fa'>('login');

const handleSubmit = async () => {
    isLoading.value = true;
    try {
        if (step.value === 'login') {
            const response = await auth.login({ 
                email: email.value, 
                senha: password.value, 
                rememberMe: rememberMe.value 
            });

            if (response && response.requiresTwoFactor) {
                step.value = '2fa';
                password.value = ''; // clear sensitive data
            }
        } else {
            await auth.login2FA({
                email: email.value,
                code: code.value
            });
        }
    } catch (error: any) {
        alert(error.response?.data?.message || 'Erro na autenticação. Tente novamente.');
    } finally {
        isLoading.value = false;
    }
};

const cancel2FA = () => {
    step.value = 'login';
    code.value = '';
    password.value = '';
};
</script>

<template>
    <div class="min-h-screen bg-app text-text-primary flex items-center justify-center p-4 font-sans transition-colors duration-300">
        <div class="w-full max-w-[400px] bg-card p-8 rounded-2xl shadow-2xl relative animation-fade-in border border-border">
            
            <!-- Logo Section -->
            <div class="flex flex-col items-center mb-8">
                <div class="w-16 h-16 rounded-full bg-input border border-border flex items-center justify-center mb-4 shadow-lg">
                    <Wallet v-if="step === 'login'" class="w-8 h-8 text-brand" />
                    <ShieldCheck v-else class="w-8 h-8 text-brand" />
                </div>
                <h1 class="text-2xl font-bold text-text-primary mb-1">Finance App</h1>
                <p class="text-text-secondary text-sm">Seu controle financeiro inteligente</p>
            </div>

            <div class="mb-6 text-center">
                <h2 class="text-xl font-bold text-text-primary flex items-center justify-center gap-2">
                    {{ step === 'login' ? 'Bem-vindo de volta! 👋' : 'Verificação em Duas Etapas' }}
                </h2>
                <p class="text-text-secondary text-sm mt-1">
                    {{ step === 'login' ? 'Entre com suas credenciais para continuar' : 'Digite o código do seu aplicativo autenticador' }}
                </p>
            </div>

            <!-- Form -->
            <form @submit.prevent="handleSubmit" class="space-y-4">
                
                <!-- Login Step -->
                <div v-if="step === 'login'" class="space-y-4 animate-in fade-in slide-in-from-right-4 duration-300">
                    <div class="space-y-1.5">
                        <label class="text-sm font-medium text-text-secondary">E-mail</label>
                        <input 
                            v-model="email"
                            type="email" 
                            class="w-full bg-input border border-border rounded-lg px-4 py-3 text-sm text-text-primary focus:outline-none focus:border-brand focus:ring-1 focus:ring-brand transition-all placeholder-text-tertiary"
                            placeholder="seu@email.com"
                            required
                        />
                    </div>
                    
                    <div class="space-y-1.5">
                        <div class="flex justify-between items-center">
                            <label class="text-sm font-medium text-text-secondary">Senha</label>
                            <router-link to="/forgot-password" class="text-xs text-brand hover:text-brand-hover transition-colors">Esqueceu a senha?</router-link>
                        </div>
                        <div class="relative">
                            <input 
                                v-model="password"
                                type="password" 
                                class="w-full bg-input border border-border rounded-lg pl-4 pr-10 py-3 text-sm text-text-primary focus:outline-none focus:border-brand focus:ring-1 focus:ring-brand transition-all placeholder-text-tertiary"
                                placeholder="******"
                                required
                            />
                            <div class="absolute right-3 top-3 text-text-tertiary">
                                <Lock class="w-4 h-4" />
                            </div>
                        </div>
                    </div>

                    <div class="flex items-center justify-between">
                        <div class="flex items-center">
                            <input 
                                id="remember-me" 
                                v-model="rememberMe"
                                type="checkbox" 
                                class="h-4 w-4 text-brand focus:ring-brand border-border rounded bg-input" 
                            />
                            <label for="remember-me" class="ml-2 block text-sm text-text-secondary">
                                Lembrar por 30 dias
                            </label>
                        </div>
                    </div>
                </div>

                <!-- 2FA Step -->
                <div v-else class="space-y-6 animate-in fade-in slide-in-from-right-4 duration-300">
                    <div>
                        <label class="text-sm font-medium text-text-secondary block mb-2 text-center">Código de Autenticação</label>
                        <input 
                            v-model="code"
                            type="text" 
                            inputmode="numeric"
                            class="w-full bg-input border border-border rounded-lg px-4 py-3 text-center text-2xl font-mono tracking-widest text-text-primary focus:outline-none focus:border-brand focus:ring-1 focus:ring-brand transition-all placeholder-text-tertiary"
                            placeholder="000 000"
                            required
                            maxlength="7"
                        />
                    </div>
                </div>

                <button 
                    type="submit"
                    :disabled="isLoading"
                    class="w-full bg-brand hover:bg-brand-hover text-white font-bold py-3 rounded-lg transition-all duration-300 mt-4 disabled:opacity-50 disabled:cursor-not-allowed flex items-center justify-center shadow-lg hover:shadow-brand/20"
                >
                    <span v-if="isLoading" class="w-5 h-5 border-2 border-white/20 border-t-white rounded-full animate-spin mr-2"></span>
                    {{ isLoading ? 'Verificando...' : (step === 'login' ? 'Entrar' : 'Confirmar') }}
                </button>

                <button 
                    v-if="step === '2fa'"
                    type="button"
                    @click="cancel2FA"
                    class="w-full text-text-secondary hover:text-text-primary text-sm font-medium py-2 transition-colors flex items-center justify-center gap-2"
                >
                    <ArrowLeft class="w-4 h-4" /> Voltar
                </button>
            </form>
        </div>
    </div>
</template>

<style scoped>
.animation-fade-in {
    animation: fadeIn 0.4s ease-out forwards;
}

@keyframes fadeIn {
    from { opacity: 0; transform: translateY(10px); }
    to { opacity: 1; transform: translateY(0); }
}
</style>
