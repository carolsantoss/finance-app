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
    <div class="min-h-screen bg-black text-white flex items-center justify-center p-4 font-sans selection:bg-white/20">
        <!-- Subtle Ambient Glow -->
        <div class="fixed inset-0 overflow-hidden pointer-events-none">
            <div class="absolute -top-[50%] -left-[50%] w-[200%] h-[200%] bg-[radial-gradient(circle_at_center,_var(--tw-gradient-stops))] from-zinc-900/20 via-black to-black opacity-40"></div>
        </div>

        <div class="w-full max-w-[360px] z-10 relative animation-fade-in">
            <!-- Minimal Header -->
            <div class="text-center mb-10">
                <div class="inline-flex items-center justify-center w-12 h-12 rounded-xl bg-zinc-900 border border-zinc-800 mb-6 shadow-xl">
                    <Wallet class="w-5 h-5 text-white/90" />
                </div>
                <h1 class="text-2xl font-semibold tracking-tight text-white mb-2">
                    {{ isRegister ? 'Criar conta' : 'Bem-vindo de volta' }}
                </h1>
                <p class="text-zinc-500 text-sm">
                    {{ isRegister ? 'Preencha os dados para começar.' : 'Digite suas credenciais para acessar.' }}
                </p>
            </div>

            <!-- Form -->
            <form @submit.prevent="handleSubmit" class="space-y-4">
                <div v-if="isRegister" class="space-y-1.5">
                    <label class="text-[11px] uppercase tracking-wider font-semibold text-zinc-500">Nome</label>
                    <div class="relative group">
                        <User class="absolute left-3 top-2.5 w-4 h-4 text-zinc-500 group-focus-within:text-white transition-colors duration-300" />
                        <input 
                            v-model="name"
                            type="text" 
                            class="w-full bg-zinc-900/50 border border-zinc-800 rounded-lg pl-9 pr-4 py-2.5 text-sm text-white placeholder-zinc-600 focus:outline-none focus:border-zinc-600 focus:bg-zinc-900 transition-all duration-300"
                            placeholder="Seu nome completo"
                            required
                        />
                    </div>
                </div>

                <div class="space-y-1.5">
                    <label class="text-[11px] uppercase tracking-wider font-semibold text-zinc-500">Email</label>
                    <div class="relative group">
                        <User class="absolute left-3 top-2.5 w-4 h-4 text-zinc-500 group-focus-within:text-white transition-colors duration-300" />
                        <input 
                            v-model="email"
                            type="email" 
                            class="w-full bg-zinc-900/50 border border-zinc-800 rounded-lg pl-9 pr-4 py-2.5 text-sm text-white placeholder-zinc-600 focus:outline-none focus:border-zinc-600 focus:bg-zinc-900 transition-all duration-300"
                            placeholder="exemplo@email.com"
                            required
                        />
                    </div>
                </div>
                
                <div class="space-y-1.5">
                    <div class="flex justify-between items-center">
                        <label class="text-[11px] uppercase tracking-wider font-semibold text-zinc-500">Senha</label>
                        <a v-if="!isRegister" href="#" class="text-[11px] text-zinc-500 hover:text-white transition-colors">Esqueceu?</a>
                    </div>
                    <div class="relative group">
                        <Lock class="absolute left-3 top-2.5 w-4 h-4 text-zinc-500 group-focus-within:text-white transition-colors duration-300" />
                        <input 
                            v-model="password"
                            type="password" 
                            class="w-full bg-zinc-900/50 border border-zinc-800 rounded-lg pl-9 pr-4 py-2.5 text-sm text-white placeholder-zinc-600 focus:outline-none focus:border-zinc-600 focus:bg-zinc-900 transition-all duration-300"
                            placeholder="••••••••"
                            required
                        />
                    </div>
                </div>

                <button 
                    type="submit"
                    :disabled="isLoading"
                    class="w-full bg-white hover:bg-zinc-200 text-black font-medium text-sm py-2.5 rounded-lg transition-all duration-300 transform active:scale-[0.98] disabled:opacity-50 disabled:cursor-not-allowed mt-2"
                >
                    <span v-if="isLoading" class="flex items-center justify-center gap-2">
                        <div class="w-3 h-3 border-2 border-zinc-400 border-t-black rounded-full animate-spin"></div>
                        Processando...
                    </span>
                    <span v-else>{{ isRegister ? 'Cadastrar' : 'Entrar' }}</span>
                </button>
            </form>

            <div class="mt-8 text-center">
                <p class="text-xs text-zinc-600">
                    {{ isRegister ? 'Já possui uma conta?' : 'Não tem uma conta?' }}
                    <button 
                        @click="isRegister = !isRegister" 
                        class="text-zinc-400 hover:text-white ml-1 font-medium transition-colors underline decoration-zinc-800 underline-offset-4 hover:decoration-white"
                    >
                        {{ isRegister ? 'Fazer login' : 'Criar agora' }}
                    </button>
                </p>
            </div>
        </div>
    </div>
</template>

<style scoped>
.animation-fade-in {
    animation: fadeIn 0.6s ease-out forwards;
}

@keyframes fadeIn {
    from { opacity: 0; transform: translateY(10px); }
    to { opacity: 1; transform: translateY(0); }
}
</style>
