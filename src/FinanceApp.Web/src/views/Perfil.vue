<script setup lang="ts">
import { ref, computed, onMounted } from 'vue';
import { useRouter } from 'vue-router';
import { useAuthStore } from '../stores/auth';
import { useToastStore } from '../stores/toast';
import { ArrowLeft, User, Lock, Save, FileText, Key, LogOut } from 'lucide-vue-next';

const router = useRouter();
const auth = useAuthStore();
const toast = useToastStore();

onMounted(async () => {
    await auth.fetchUser();
    // Update local form with fetched data
    profileForm.value.nomeUsuario = auth.user?.nomeUsuario || '';
    profileForm.value.email = auth.user?.email || '';
});

// Profile Form
const profileForm = ref({
    nomeUsuario: auth.user?.nomeUsuario || '',
    email: auth.user?.email || ''
});

// Password Form
const passwordForm = ref({
    currentPassword: '',
    newPassword: '',
    confirmNewPassword: ''
});

const isSavingProfile = ref(false);
const isChangingPassword = ref(false);

const handleUpdateProfile = async () => {
    isSavingProfile.value = true;
    try {
        await auth.updateProfile({
            nomeUsuario: profileForm.value.nomeUsuario,
            email: profileForm.value.email
        });
        toast.success('Perfil atualizado com sucesso!');
    } catch (error: any) {
        console.error('Erro detalhado:', error);
        toast.error(error.response?.data || 'Erro ao atualizar perfil.');
    } finally {
        isSavingProfile.value = false;
    }
};

const handleChangePassword = async () => {
    if (passwordForm.value.newPassword !== passwordForm.value.confirmNewPassword) {
        toast.warning('As senhas não conferem.');
        return;
    }

    isChangingPassword.value = true;
    try {
        await auth.changePassword({
            currentPassword: passwordForm.value.currentPassword,
            newPassword: passwordForm.value.newPassword,
            confirmNewPassword: passwordForm.value.confirmNewPassword
        });
        toast.success('Senha alterada com sucesso!');
        passwordForm.value = { currentPassword: '', newPassword: '', confirmNewPassword: '' };
    } catch (error: any) {
        toast.error(error.response?.data || 'Erro ao alterar senha.');
    } finally {
        isChangingPassword.value = false;
    }
};

// Computed to display user initial
const userInitial = computed(() => {
    return auth.user?.nomeUsuario?.charAt(0).toUpperCase() || 'U';
});
</script>

<template>
    <div class="flex flex-col h-full bg-[#121214] p-6 space-y-6 overflow-y-auto items-center justify-start">
        <!-- Cards Container -->
        <div class="w-full max-w-4xl grid grid-cols-1 gap-6 pb-6">
            
            <!-- User Banner -->
            <div class="bg-[#202024] p-6 rounded-lg border border-[#323238] flex flex-col sm:flex-row items-center gap-6 relative overflow-hidden">
                <!-- Background Decoration -->
                <div class="absolute right-0 top-0 h-full w-1/3 bg-gradient-to-l from-[#00875F]/10 to-transparent pointer-events-none"></div>

                <div class="w-20 h-20 rounded-full bg-[#00875F] flex items-center justify-center text-3xl font-bold text-white shadow-lg border-4 border-[#121214] z-10">
                    {{ userInitial }}
                </div>
                 <div class="text-center sm:text-left z-10">
                    <h2 class="text-2xl font-bold text-white">{{ auth.user?.nomeUsuario }}</h2>
                    <p class="text-gray-400">{{ auth.user?.email }}</p>
                    <span class="text-xs text-[#00B37E] mt-2 inline-flex items-center gap-1 bg-[#00875F]/10 px-2 py-1 rounded border border-[#00875F]/20">
                        <User class="w-3 h-3" />
                        Conta Ativa
                    </span>
                </div>
            </div>

            <div class="grid grid-cols-1 lg:grid-cols-2 gap-6">
                <!-- Personal Info -->
                 <div class="bg-[#202024] p-6 rounded-lg border border-[#323238] h-fit">
                     <h3 class="text-lg font-bold text-white mb-6 flex items-center gap-2 pb-4 border-b border-[#323238]">
                        <FileText class="w-5 h-5 text-[#00B37E]" />
                        Informações Pessoais
                    </h3>
                    
                    <form @submit.prevent="handleUpdateProfile" class="space-y-5">
                        <div class="space-y-2">
                            <label class="text-xs text-gray-400 font-medium uppercase tracking-wider">Nome completo</label>
                            <input 
                                v-model="profileForm.nomeUsuario"
                                type="text"
                                required
                                class="w-full bg-[#121214] border border-[#323238] rounded-md p-3 text-white focus:outline-none focus:border-[#00875F] transition-colors" 
                            />
                        </div>
                         <div class="space-y-2">
                            <label class="text-xs text-gray-400 font-medium uppercase tracking-wider">E-mail</label>
                            <input 
                                v-model="profileForm.email"
                                type="email"
                                required
                                class="w-full bg-[#121214] border border-[#323238] rounded-md p-3 text-white focus:outline-none focus:border-[#00875F] transition-colors" 
                            />
                        </div>

                        <button type="submit" :disabled="isSavingProfile" class="w-full mt-2 bg-[#00875F] hover:bg-[#00B37E] text-white font-bold py-3 rounded-md transition-all flex items-center justify-center gap-2 shadow-lg shadow-[#00875F]/20 disabled:opacity-50 disabled:cursor-not-allowed">
                             <Save class="w-4 h-4" />
                             <span v-if="isSavingProfile">Salvando...</span>
                             <span v-else>Salvar Alterações</span>
                        </button>
                    </form>
                 </div>

                 <!-- Security -->
                 <div class="bg-[#202024] p-6 rounded-lg border border-[#323238] h-fit">
                    <h3 class="text-lg font-bold text-white mb-6 flex items-center gap-2 pb-4 border-b border-[#323238]">
                        <Lock class="w-5 h-5 text-[#F75A68]" />
                        Segurança
                    </h3>

                    <form @submit.prevent="handleChangePassword" class="space-y-5">
                         <div class="space-y-2">
                            <label class="text-xs text-gray-400 font-medium uppercase tracking-wider">Senha atual</label>
                            <input 
                                v-model="passwordForm.currentPassword"
                                type="password"
                                required
                                class="w-full bg-[#121214] border border-[#323238] rounded-md p-3 text-white focus:outline-none focus:border-[#F75A68] transition-colors" 
                                placeholder="••••••••"
                            />
                        </div>
                        <div class="space-y-2">
                            <label class="text-xs text-gray-400 font-medium uppercase tracking-wider">Nova senha</label>
                            <input 
                                v-model="passwordForm.newPassword"
                                type="password"
                                required
                                class="w-full bg-[#121214] border border-[#323238] rounded-md p-3 text-white focus:outline-none focus:border-[#F75A68] transition-colors" 
                                placeholder="••••••••"
                            />
                        </div>
                        <div class="space-y-2">
                            <label class="text-xs text-gray-400 font-medium uppercase tracking-wider">Confirmar nova senha</label>
                            <input 
                                v-model="passwordForm.confirmNewPassword"
                                type="password"
                                required
                                class="w-full bg-[#121214] border border-[#323238] rounded-md p-3 text-white focus:outline-none focus:border-[#F75A68] transition-colors" 
                                placeholder="••••••••"
                            />
                        </div>

                         <button type="submit" :disabled="isChangingPassword" class="w-full mt-2 bg-[#29292E] hover:bg-[#323238] text-white font-bold py-3 rounded-md transition-all flex items-center justify-center gap-2 border border-[#323238] hover:border-[#F75A68] group disabled:opacity-50 disabled:cursor-not-allowed">
                             <Key class="w-4 h-4 text-gray-400 group-hover:text-[#F75A68] transition-colors" />
                             <span v-if="isChangingPassword" class="group-hover:text-[#F75A68] transition-colors">Alterando...</span>
                             <span v-else class="group-hover:text-[#F75A68] transition-colors">Alterar Senha</span>
                        </button>
                    </form>
                 </div>
            </div>

            <!-- Danger Zone (Optional / Placeholder) -->
            <div class="bg-[#202024] p-6 rounded-lg border border-[#323238]/50 mt-4 opacity-80 hover:opacity-100 transition-opacity">
                <div class="flex flex-col sm:flex-row items-center justify-between gap-4">
                    <div>
                        <h4 class="text-white font-bold">Sair do Sistema</h4>
                        <p class="text-sm text-gray-400">Encerrar sua sessão atual neste dispositivo.</p>
                    </div>
                     <button @click="auth.logout" class="px-6 py-2 bg-red-500/10 text-red-400 border border-red-500/20 hover:bg-red-500/20 rounded-md font-bold transition-colors flex items-center gap-2">
                        <LogOut class="w-4 h-4" />
                        Sair
                    </button>
                </div>
            </div>
        </div>
    </div>
</template>
