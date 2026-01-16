<script setup lang="ts">
import { ref } from 'vue';
import { useAuthStore } from '../stores/auth';
import { User, Lock, ArrowRight, Wallet } from 'lucide-vue-next';

const auth = useAuthStore();
const email = ref('');
const password = ref('');
const rememberMe = ref(false);
const isLoading = ref(false);

const handleSubmit = async () => {
    isLoading.value = true;
    try {
        // Simulate network delay for effect
        await new Promise(r => setTimeout(r, 800));
        await auth.login({ email: email.value, senha: password.value, rememberMe: rememberMe.value });
    } catch (error) {
        alert('Erro na autenticação. Tente novamente.');
    } finally {
        isLoading.value = false;
    }
};
</script>

<template>
    <div class="min-h-screen bg-[#121214] text-gray-100 flex items-center justify-center p-4 font-sans">
        <div class="w-full max-w-[400px] bg-[#202024] p-8 rounded-lg shadow-xl relative animation-fade-in border border-[#323238]">
            
            <!-- Logo Section -->
            <div class="flex flex-col items-center mb-8">
                <div class="w-16 h-16 rounded-full bg-[#121214] border border-[#323238] flex items-center justify-center mb-4">
                    <Wallet class="w-8 h-8 text-gray-100" />
                </div>
                <h1 class="text-2xl font-bold text-white mb-1">Finance App</h1>
                <p class="text-gray-400 text-sm">Seu controle financeiro inteligente</p>
            </div>

            <div class="mb-6">
                <h2 class="text-xl font-bold text-white flex items-center gap-2">
                    Bem-vindo de volta! 
                    <span class="text-xl">👋</span>
                </h2>
                <p class="text-gray-400 text-sm mt-1">Entre com suas credenciais para continuar</p>
            </div>

            <!-- Form -->
            <form @submit.prevent="handleSubmit" class="space-y-4">
                <div class="space-y-1.5">
                    <label class="text-sm font-medium text-gray-200">E-mail</label>
                    <input 
                        v-model="email"
                        type="email" 
                        class="w-full bg-[#121214] border border-[#323238] rounded-md px-4 py-3 text-sm text-white focus:outline-none focus:border-[#00875F] transition-colors placeholder-gray-500"
                        required
                    />
                </div>
                
                <div class="space-y-1.5">
                    <label class="text-sm font-medium text-gray-200">Senha</label>
                    <div class="relative">
                        <input 
                            v-model="password"
                            type="password" 
                            class="w-full bg-[#121214] border border-[#323238] rounded-md pl-4 pr-10 py-3 text-sm text-white focus:outline-none focus:border-[#00875F] transition-colors placeholder-gray-500"
                            required
                        />
                        <button type="button" class="absolute right-3 top-3 text-gray-500 hover:text-gray-300">
                             <!-- Eye Icon placeholder if needed, or just keep simple -->
                             <Lock class="w-4 h-4" />
                        </button>
                    </div>
                </div>

                <div class="flex items-center justify-between">
                    <div class="flex items-center">
                        <input 
                            id="remember-me" 
                            v-model="rememberMe"
                            type="checkbox" 
                            class="h-4 w-4 text-[#00875F] focus:ring-[#00875F] border-[#323238] rounded bg-[#121214]" 
                        />
                        <label for="remember-me" class="ml-2 block text-sm text-gray-400">
                            Lembrar por 30 dias
                        </label>
                    </div>
                </div>

                <button 
                    type="submit"
                    :disabled="isLoading"
                    class="w-full bg-[#00875F] hover:bg-[#00B37E] text-white font-bold py-3 rounded-md transition-all duration-300 mt-4 disabled:opacity-50 disabled:cursor-not-allowed"
                >
                    <span v-if="isLoading">Entrando...</span>
                    <span v-else>Entrar na conta</span>
                </button>
            </form>

            <div class="mt-8 flex items-center justify-between text-sm text-gray-400 relative">
                <div class="absolute inset-0 flex items-center" aria-hidden="true">
                    <div class="w-full border-t border-[#323238]"></div>
                </div>
                <div class="relative flex justify-center w-full">
                    <span class="bg-[#202024] px-2 text-xs uppercase text-gray-500">OU</span>
                </div>
            </div>

            <!-- Registration Link Removed for Public Access -->
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
