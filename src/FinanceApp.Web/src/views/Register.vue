<script setup lang="ts">
import { ref } from 'vue';
import { useAuthStore } from '../stores/auth';
import { useRouter, useRoute } from 'vue-router';
import { User, Lock, Wallet, Mail, FileText, Share2 } from 'lucide-vue-next';

const auth = useAuthStore();
const router = useRouter();
const route = useRoute();

const name = ref('');
const email = ref('');
const password = ref('');
const confirmPassword = ref('');
const referralCode = ref('');
const isLoading = ref(false);

// Auto-fill from URL
const validationState = ref<'loading' | 'valid' | 'invalid'>('loading');
const referrerName = ref('');
const validationError = ref('');

// Auto-fill from URL and Validate
if (route.query.ref) {
    referralCode.value = route.query.ref as string;
    validateReferralCode(referralCode.value);
} else {
    validationState.value = 'invalid';
}

async function validateReferralCode(code: string) {
    validationState.value = 'loading';
    try {
        const result = await auth.validateReferral(code);
        if (result && result.valid) {
            validationState.value = 'valid';
            referrerName.value = result.referrerName;
        } else {
            validationState.value = 'invalid';
            referralCode.value = '';
        }
    } catch (error) {
        validationState.value = 'invalid';
        referralCode.value = ''; // Clear code to trigger "Missing Invite" state, or we can use validationState to show specific error
        validationError.value = 'O código de convite é inválido.';
    }
}

const handleSubmit = async () => {
    if (password.value !== confirmPassword.value) {
        alert('As senhas não coincidem.');
        return;
    }

    isLoading.value = true;
    try {
        await new Promise(r => setTimeout(r, 800)); // Simulate delay
        await auth.register({ 
            nomeUsuario: name.value, 
            email: email.value, 
            senha: password.value,
            referralCode: referralCode.value
        });
        // Start with auto-login or redirect to login? 
        // Specification says "Navegação para login" implied, but store.register usually logs in.
        // The store logic redirects to '/' after register.
    } catch (error) {
        alert('Erro ao criar conta. Tente novamente.');
    } finally {
        isLoading.value = false;
    }
};
</script>

<template>
    <div class="min-h-screen bg-[#121214] text-gray-100 flex items-center justify-center p-4 font-sans">
        <div class="w-full max-w-[500px] bg-[#202024] p-8 rounded-lg shadow-xl relative animation-fade-in border border-[#323238]">
            
            <div class="flex justify-between items-start mb-8">
                <div>
                    <h1 class="text-2xl font-bold text-white mb-1 flex items-center gap-2">
                        Criar Conta
                        <FileText class="w-6 h-6 text-white" />
                    </h1>
                    <p class="text-gray-400 text-sm">Preencha os dados abaixo para começar</p>
                </div>
            </div>



                <div v-if="validationState === 'loading'" class="py-12 text-center">
                     <p class="text-gray-400">Verificando convite...</p>
                </div>

                <div v-else-if="validationState === 'invalid'" class="text-center py-8 animate-in fade-in">
                    <div class="bg-red-500/10 text-red-500 p-4 rounded-lg border border-red-500/20 mb-6">
                        <Share2 class="w-12 h-12 mx-auto mb-2 opacity-50" />
                        <h3 class="font-bold text-lg">Convite Inválido ou Necessário</h3>
                        <p class="text-sm opacity-90">{{ validationError || 'O cadastro é exclusivo para convidados com link válido.' }}</p>
                    </div>
                    <p class="text-gray-400 text-sm mb-6">Você precisa de um link de convite válido para criar uma conta.</p>
                    <router-link to="/login" class="inline-block bg-[#202024] border border-[#323238] rounded-md px-6 py-2 text-sm font-bold text-white hover:bg-[#323238] transition-colors">
                        Voltar para Login
                    </router-link>
                </div>

                <!-- Form -->
                <form v-else @submit.prevent="handleSubmit" class="space-y-4 animate-in fade-in">
                    
                    <div v-if="referrerName" class="bg-[#00875F]/10 border border-[#00875F]/20 rounded-md p-3 mb-4 flex items-center gap-3">
                         <div class="w-8 h-8 rounded-full bg-[#00875F]/20 flex items-center justify-center text-[#00875F]">
                            <User class="w-4 h-4" />
                         </div>
                         <div>
                             <p class="text-xs text-gray-400">Você foi convidado por</p>
                             <p class="text-sm font-bold text-[#00875F]">{{ referrerName }}</p>
                         </div>
                    </div>

                    <div class="grid grid-cols-2 gap-4">
                        <div class="space-y-1.5">
                            <label class="text-sm font-medium text-gray-200">Nome completo</label>
                            <input 
                                v-model="name"
                                type="text" 
                                class="w-full bg-[#121214] border border-[#323238] rounded-md px-4 py-3 text-sm text-white focus:outline-none focus:border-[#00875F] transition-colors"
                                required
                            />
                        </div>
                        <div class="space-y-1.5">
                            <label class="text-sm font-medium text-gray-200">E-mail</label>
                            <input 
                                v-model="email"
                                type="email" 
                                class="w-full bg-[#121214] border border-[#323238] rounded-md px-4 py-3 text-sm text-white focus:outline-none focus:border-[#00875F] transition-colors"
                                required
                            />
                        </div>
                    </div>

                <div v-if="referralCode" class="space-y-1.5 animate-in fade-in slide-in-from-top-2">
                    <label class="text-sm font-medium text-gray-200 flex items-center gap-1">
                        <Share2 class="w-3 h-3 text-[#00875F]" /> 
                        Código de Convite (Incluso)
                    </label>
                    <input 
                        v-model="referralCode"
                        type="text" 
                        class="w-full bg-[#121214]/50 border border-[#00875F]/30 rounded-md px-4 py-3 text-sm text-[#00875F] font-bold outline-none cursor-not-allowed"
                        readonly
                    />
                </div>

                <div class="grid grid-cols-2 gap-4">
                    <div class="space-y-1.5">
                         <label class="text-sm font-medium text-gray-200">Senha</label>
                         <div class="relative">
                            <input 
                                v-model="password"
                                type="password" 
                                class="w-full bg-[#121214] border border-[#323238] rounded-md px-4 py-3 text-sm text-white focus:outline-none focus:border-[#00875F] transition-colors"
                                required
                            />
                            <button type="button" class="absolute right-3 top-3 text-gray-500 hover:text-gray-300">
                                <Lock class="w-4 h-4" />
                            </button>
                        </div>
                    </div>
                    <div class="space-y-1.5">
                         <label class="text-sm font-medium text-gray-200">Confirmar senha</label>
                         <div class="relative">
                            <input 
                                v-model="confirmPassword"
                                type="password" 
                                class="w-full bg-[#121214] border border-[#323238] rounded-md px-4 py-3 text-sm text-white focus:outline-none focus:border-[#00875F] transition-colors"
                                required
                            />
                            <button type="button" class="absolute right-3 top-3 text-gray-500 hover:text-gray-300">
                                <Lock class="w-4 h-4" />
                            </button>
                        </div>
                    </div>
                </div>

                <button 
                    type="submit"
                    :disabled="isLoading"
                    class="w-full bg-[#00875F] hover:bg-[#00B37E] text-white font-bold py-3 rounded-md transition-all duration-300 mt-6 disabled:opacity-50 disabled:cursor-not-allowed"
                >
                    <span v-if="isLoading">Processando...</span>
                    <span v-else>Criar minha conta</span>
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

            <div class="mt-6 flex justify-center text-sm">
                <span class="text-gray-400">Já tem uma conta?</span>
                <router-link to="/login" class="ml-2 text-[#00B37E] font-bold hover:underline">
                    Fazer login
                </router-link>
            </div>
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
