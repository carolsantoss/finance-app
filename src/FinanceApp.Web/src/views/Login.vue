<script setup lang="ts">
import { ref } from 'vue';
import { useAuthStore } from '../stores/auth';
import { User, Lock, ArrowRight } from 'lucide-vue-next';

const auth = useAuthStore();
const email = ref('');
const password = ref('');
const name = ref('');
const isRegister = ref(false);
const isLoading = ref(false);

const handleSubmit = async () => {
    isLoading.value = true;
    try {
        // Simulate network delay for effect
        await new Promise(r => setTimeout(r, 800));
        
        if (isRegister.value) {
            await auth.register({ nomeUsuario: name.value, email: email.value, senha: password.value });
        } else {
            await auth.login({ email: email.value, senha: password.value });
        }
    } catch (error) {
        alert('Erro na autenticação. Tente novamente.');
    } finally {
        isLoading.value = false;
    }
};
</script>

<template>
    <div class="min-h-screen flex items-center justify-center bg-gray-900 relative overflow-hidden font-sans">
        <!-- Abstract Animated Background -->
        <div class="absolute inset-0 z-0">
            <div class="absolute top-[-20%] left-[-10%] w-[500px] h-[500px] bg-purple-600 rounded-full mix-blend-multiply filter blur-3xl opacity-30 animate-blob"></div>
            <div class="absolute top-[-10%] right-[-10%] w-[400px] h-[400px] bg-blue-600 rounded-full mix-blend-multiply filter blur-3xl opacity-30 animate-blob animation-delay-2000"></div>
            <div class="absolute bottom-[-20%] left-[20%] w-[600px] h-[600px] bg-pink-600 rounded-full mix-blend-multiply filter blur-3xl opacity-30 animate-blob animation-delay-4000"></div>
        </div>

        <!-- Glass Card -->
        <div class="relative z-10 w-full max-w-md p-8 bg-white/10 backdrop-blur-lg border border-white/20 rounded-2xl shadow-2xl">
            <div class="text-center mb-8">
                <div class="inline-flex items-center justify-center w-16 h-16 rounded-full bg-gradient-to-tr from-blue-500 to-purple-600 mb-4 shadow-lg">
                    <Lock class="w-8 h-8 text-white" />
                </div>
                <h2 class="text-3xl font-bold text-white tracking-tight">FinanceApp</h2>
                <p class="text-gray-300 mt-2 text-sm">Gerencie suas finanças com elegância.</p>
            </div>

            <form @submit.prevent="handleSubmit" class="space-y-6">
                <div v-if="isRegister" class="relative group">
                    <User class="absolute left-3 top-3 text-gray-400 w-5 h-5 group-focus-within:text-blue-400 transition-colors" />
                    <input 
                        type="text" 
                        v-model="name" 
                        placeholder="Nome Completo" 
                        class="w-full bg-gray-800/50 border border-gray-700 text-white rounded-lg pl-10 pr-4 py-3 outline-none focus:border-blue-500 focus:ring-1 focus:ring-blue-500 transition-all placeholder-gray-500"
                        required
                    />
                </div>

                <div class="relative group">
                    <User class="absolute left-3 top-3 text-gray-400 w-5 h-5 group-focus-within:text-blue-400 transition-colors" />
                    <input 
                        type="email" 
                        v-model="email" 
                        placeholder="Seu Email" 
                        class="w-full bg-gray-800/50 border border-gray-700 text-white rounded-lg pl-10 pr-4 py-3 outline-none focus:border-blue-500 focus:ring-1 focus:ring-blue-500 transition-all placeholder-gray-500"
                        required
                    />
                </div>

                <div class="relative group">
                    <Lock class="absolute left-3 top-3 text-gray-400 w-5 h-5 group-focus-within:text-blue-400 transition-colors" />
                    <input 
                        type="password" 
                        v-model="password" 
                        placeholder="Sua Senha" 
                        class="w-full bg-gray-800/50 border border-gray-700 text-white rounded-lg pl-10 pr-4 py-3 outline-none focus:border-blue-500 focus:ring-1 focus:ring-blue-500 transition-all placeholder-gray-500"
                        required
                    />
                </div>

                <button 
                    type="submit" 
                    :disabled="isLoading"
                    class="w-full bg-gradient-to-r from-blue-600 to-purple-600 hover:from-blue-700 hover:to-purple-700 text-white font-semibold py-3 rounded-lg shadow-lg flex items-center justify-center gap-2 transition-all transform hover:scale-[1.02] disabled:opacity-50 disabled:cursor-not-allowed"
                >
                    <div v-if="isLoading" class="w-5 h-5 border-2 border-white/30 border-t-white rounded-full animate-spin"></div>
                    <span v-else>{{ isRegister ? 'Criar Conta' : 'Acessar Painel' }}</span>
                    <ArrowRight v-if="!isLoading" class="w-4 h-4" />
                </button>
            </form>

            <div class="mt-6 text-center">
                <button 
                    @click="isRegister = !isRegister"
                    class="text-sm text-gray-400 hover:text-white transition-colors"
                >
                    {{ isRegister ? 'Já possui conta? Entrar' : 'Não tem conta? Cadastre-se' }}
                </button>
            </div>
        </div>
    </div>
</template>

<style scoped>
@keyframes blob {
    0% { transform: translate(0px, 0px) scale(1); }
    33% { transform: translate(30px, -50px) scale(1.1); }
    66% { transform: translate(-20px, 20px) scale(0.9); }
    100% { transform: translate(0px, 0px) scale(1); }
}
.animate-blob {
    animation: blob 7s infinite;
}
.animation-delay-2000 {
    animation-delay: 2s;
}
.animation-delay-4000 {
    animation-delay: 4s;
}
</style>
