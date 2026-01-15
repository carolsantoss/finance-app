<script setup lang="ts">
import { ref } from 'vue';
import { useRouter } from 'vue-router';
import { useAuthStore } from '../stores/auth';
import { ArrowLeft, User, Lock, Save, FileText, Key } from 'lucide-vue-next';

const router = useRouter();
const auth = useAuthStore();

const form = ref({
    nomeUsuario: auth.user?.nomeUsuario || '',
    email: auth.user?.email || '', // Assuming email is in user object, otherwise mock or fetch
    currentPassword: '',
    newPassword: ''
});

const isSaving = ref(false);

const handleSave = async () => {
    isSaving.value = true;
    try {
        await new Promise(r => setTimeout(r, 1000)); // Mock API delay
        // Call API to update profile
        alert('Perfil atualizado com sucesso!');
        form.value.currentPassword = '';
        form.value.newPassword = '';
    } catch {
        alert('Erro ao atualizar perfil.');
    } finally {
        isSaving.value = false;
    }
};
</script>

<template>
    <div class="flex flex-col h-full bg-[#121214] p-6 space-y-6 overflow-hidden items-center justify-start">
        <!-- Cards Container -->
        <div class="w-full max-w-4xl grid grid-cols-1 gap-6">
            
            <!-- User Banner -->
            <div class="bg-[#202024] p-6 rounded-lg border border-[#323238] flex items-center gap-4">
                <div class="w-16 h-16 rounded-full bg-[#00875F] flex items-center justify-center text-2xl font-bold text-white shadow-lg">
                    {{ form.nomeUsuario.charAt(0).toUpperCase() }}
                </div>
                 <div>
                    <h2 class="text-xl font-bold text-white">{{ form.nomeUsuario }}</h2>
                    <p class="text-sm text-gray-400">{{ form.email }}</p>
                    <span class="text-xs text-[#00875F] mt-1 block font-medium">Membro desde janeiro de 2026</span>
                </div>
            </div>

            <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
                <!-- Personal Info -->
                 <div class="bg-[#202024] p-6 rounded-lg border border-[#323238]">
                     <h3 class="text-lg font-bold text-white mb-4 flex items-center gap-2">
                        <FileText class="w-4 h-4 text-white" />
                        Informações Pessoais
                    </h3>
                    
                    <div class="space-y-4">
                        <div class="space-y-1.5">
                            <label class="text-xs text-gray-400 font-medium">Nome completo</label>
                            <input 
                                v-model="form.nomeUsuario"
                                type="text"
                                class="w-full bg-[#121214] border border-[#323238] rounded p-3 text-white focus:outline-none focus:border-[#00875F]" 
                            />
                        </div>
                         <div class="space-y-1.5">
                            <label class="text-xs text-gray-400 font-medium">E-mail</label>
                            <input 
                                v-model="form.email"
                                type="email"
                                class="w-full bg-[#121214] border border-[#323238] rounded p-3 text-white focus:outline-none focus:border-[#00875F] opacity-70 cursor-not-allowed" 
                                disabled
                            />
                        </div>
                    </div>

                    <button @click="handleSave" :disabled="isSaving" class="w-full mt-6 bg-[#00875F] hover:bg-[#00B37E] text-white font-bold py-3 rounded transition-colors flex items-center justify-center gap-2">
                         <Save class="w-4 h-4" />
                         Salvar Alterações
                    </button>
                 </div>

                 <!-- Security -->
                 <div class="bg-[#202024] p-6 rounded-lg border border-[#323238]">
                    <h3 class="text-lg font-bold text-white mb-4 flex items-center gap-2">
                        <Lock class="w-4 h-4 text-white" />
                        Segurança
                    </h3>

                    <div class="space-y-4">
                         <div class="space-y-1.5">
                            <label class="text-xs text-gray-400 font-medium">Senha atual</label>
                            <input 
                                v-model="form.currentPassword"
                                type="password"
                                class="w-full bg-[#121214] border border-[#323238] rounded p-3 text-white focus:outline-none focus:border-[#00875F]" 
                                placeholder="••••••••"
                            />
                        </div>
                        <div class="space-y-1.5">
                            <label class="text-xs text-gray-400 font-medium">Nova senha</label>
                            <input 
                                v-model="form.newPassword"
                                type="password"
                                class="w-full bg-[#121214] border border-[#323238] rounded p-3 text-white focus:outline-none focus:border-[#00875F]" 
                                placeholder="••••••••"
                            />
                        </div>
                        <div class="space-y-1.5">
                            <label class="text-xs text-gray-400 font-medium">Confirmar nova senha</label>
                            <input 
                                type="password"
                                class="w-full bg-[#121214] border border-[#323238] rounded p-3 text-white focus:outline-none focus:border-[#00875F]" 
                                placeholder="••••••••"
                            />
                        </div>
                    </div>

                     <button class="w-full mt-6 bg-[#29292E] hover:bg-[#323238] text-white font-bold py-3 rounded transition-colors flex items-center justify-center gap-2 border border-[#323238]">
                         <Key class="w-4 h-4" />
                         Alterar Senha
                    </button>

                 </div>
            </div>
        </div>
    </div>
</template>
