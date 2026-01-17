<script setup lang="ts">
import { ref, computed, onMounted } from 'vue';
import { useRouter } from 'vue-router';
import { useAuthStore } from '../stores/auth';
import { useToastStore } from '../stores/toast';
import { ArrowLeft, User, Lock, Save, FileText, Key, LogOut, ShieldCheck, QrCode as QrIcon, CheckCircle } from 'lucide-vue-next';
import api from '../api/axios';
import QRCode from 'qrcode';

const router = useRouter();
const auth = useAuthStore();
const toast = useToastStore();

const isTwoFactorEnabled = ref(false);
const show2FASetup = ref(false);
const qrCodeUrl = ref('');
const twoFactorSecret = ref('');
const twoFactorCode = ref('');
const isVerifying2FA = ref(false);

onMounted(async () => {
    await auth.fetchUser();
    // Update local form with fetched data
    profileForm.value.nomeUsuario = auth.user?.nomeUsuario || '';
    profileForm.value.email = auth.user?.email || '';
    
    // Check 2FA status (assuming it's in user object now)
    // We cast to any because TS might not know about isTwoFactorEnabled yet if we didn't update interface
    // But API returns it as IsTwoFactorEnabled (mapped from fl_2faHabilitado)
    // However, fetchUser updates `this.user` which is stored in local storage.
    // We need to ensure fetchUser retrieves the new field.
    if ((auth.user as any)?.isTwoFactorEnabled) {
        isTwoFactorEnabled.value = true;
    }
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

// 2FA Logic
const start2FASetup = async () => {
    try {
        const response = await api.post('/auth/enable-2fa');
        const { secret, qrCodeUri } = response.data;
        twoFactorSecret.value = secret;
        
        // Generate QR Code Image
        qrCodeUrl.value = await QRCode.toDataURL(qrCodeUri);
        show2FASetup.value = true;
    } catch (error) {
        toast.error('Erro ao iniciar configuração de 2FA.');
    }
};

const confirm2FA = async () => {
    isVerifying2FA.value = true;
    try {
        await api.post('/auth/confirm-2fa', { code: twoFactorCode.value });
        toast.success('Autenticação de Dois Fatores habilitada com sucesso!');
        isTwoFactorEnabled.value = true;
        show2FASetup.value = false;
        // Refresh user to persist state
        await auth.fetchUser();
    } catch (error: any) {
        toast.error(error.response?.data || 'Código inválido.');
    } finally {
        isVerifying2FA.value = false;
    }
};

// Computed to display user initial
const userInitial = computed(() => {
    return auth.user?.nomeUsuario?.charAt(0).toUpperCase() || 'U';
});
</script>

<template>
    <div class="flex flex-col h-full bg-app p-6 space-y-6 overflow-y-auto items-center justify-start">
        <!-- Cards Container -->
        <div class="w-full max-w-4xl grid grid-cols-1 gap-6 pb-6">
            
            <!-- User Banner -->
            <div class="bg-card p-6 rounded-lg border border-border flex flex-col sm:flex-row items-center gap-6 relative overflow-hidden">
                <!-- Background Decoration -->
                <div class="absolute right-0 top-0 h-full w-1/3 bg-gradient-to-l from-brand/10 to-transparent pointer-events-none"></div>

                <div class="w-20 h-20 rounded-full bg-brand flex items-center justify-center text-3xl font-bold text-white shadow-lg border-4 border-app z-10">
                    {{ userInitial }}
                </div>
                 <div class="text-center sm:text-left z-10">
                    <h2 class="text-2xl font-bold text-text-primary">{{ auth.user?.nomeUsuario }}</h2>
                    <p class="text-text-secondary">{{ auth.user?.email }}</p>
                    <span class="text-xs text-brand mt-2 inline-flex items-center gap-1 bg-brand/10 px-2 py-1 rounded border border-brand/20">
                        <User class="w-3 h-3" />
                        Conta Ativa
                    </span>
                </div>
            </div>

            <div class="grid grid-cols-1 lg:grid-cols-2 gap-6">
                <!-- Personal Info -->
                 <div class="bg-card p-6 rounded-lg border border-border h-fit">
                     <h3 class="text-lg font-bold text-text-primary mb-6 flex items-center gap-2 pb-4 border-b border-border">
                        <FileText class="w-5 h-5 text-brand" />
                        Informações Pessoais
                    </h3>
                    
                    <form @submit.prevent="handleUpdateProfile" class="space-y-5">
                        <div class="space-y-2">
                            <label class="text-xs text-text-secondary font-medium uppercase tracking-wider">Nome completo</label>
                            <input 
                                v-model="profileForm.nomeUsuario"
                                type="text"
                                required
                                class="w-full bg-input border border-border rounded-md p-3 text-text-primary focus:outline-none focus:border-brand transition-colors" 
                            />
                        </div>
                         <div class="space-y-2">
                            <label class="text-xs text-text-secondary font-medium uppercase tracking-wider">E-mail</label>
                            <input 
                                v-model="profileForm.email"
                                type="email"
                                required
                                class="w-full bg-input border border-border rounded-md p-3 text-text-primary focus:outline-none focus:border-brand transition-colors" 
                            />
                        </div>

                        <button type="submit" :disabled="isSavingProfile" class="w-full mt-2 bg-brand hover:bg-brand-hover text-white font-bold py-3 rounded-md transition-all flex items-center justify-center gap-2 shadow-lg shadow-brand/20 disabled:opacity-50 disabled:cursor-not-allowed">
                             <Save class="w-4 h-4" />
                             <span v-if="isSavingProfile">Salvando...</span>
                             <span v-else>Salvar Alterações</span>
                        </button>
                    </form>
                 </div>

                 <!-- Security -->
                 <div class="bg-card p-6 rounded-lg border border-border h-fit">
                    <h3 class="text-lg font-bold text-text-primary mb-6 flex items-center gap-2 pb-4 border-b border-border">
                        <Lock class="w-5 h-5 text-danger" />
                        Segurança
                    </h3>

                    <!-- 2FA Section -->
                    <div class="mb-8 p-4 bg-hover rounded-lg border border-border">
                        <div class="flex justify-between items-start mb-4">
                            <div>
                                <h4 class="font-bold text-text-primary flex items-center gap-2">
                                    <ShieldCheck class="w-4 h-4 text-brand" />
                                    Autenticação em Dois Fatores (2FA)
                                </h4>
                                <p class="text-xs text-text-secondary mt-1">Adicione uma camada extra de segurança à sua conta.</p>
                            </div>
                            <span v-if="isTwoFactorEnabled" class="px-2 py-1 bg-[#00875F]/20 text-[#00B37E] text-xs font-bold rounded border border-[#00875F]/30">HABILITADO</span>
                            <span v-else class="px-2 py-1 bg-gray-500/20 text-gray-400 text-xs font-bold rounded border border-gray-500/30">DESABILITADO</span>
                        </div>

                        <div v-if="!isTwoFactorEnabled && !show2FASetup">
                            <button @click="start2FASetup" class="w-full py-2 bg-brand/10 text-brand border border-brand/20 hover:bg-brand/20 rounded-md text-sm font-bold transition-colors">
                                Configurar 2FA
                            </button>
                        </div>

                        <div v-if="show2FASetup" class="animate-in fade-in zoom-in duration-300">
                             <div class="flex flex-col items-center gap-4 bg-card p-4 rounded border border-border mb-4">
                                <img :src="qrCodeUrl" alt="QR Code" class="w-32 h-32 rounded bg-white p-2" />
                                <div class="text-center">
                                    <p class="text-xs text-text-secondary mb-1">Escaneie com Google Authenticator ou Authy</p>
                                    <p class="text-xs font-mono bg-black/30 px-2 py-1 rounded text-text-tertiary">{{ twoFactorSecret }}</p>
                                </div>
                             </div>
                             
                             <div class="space-y-2">
                                <label class="text-xs text-text-secondary">Código de verificação</label>
                                <div class="flex gap-2">
                                    <input v-model="twoFactorCode" type="text" placeholder="000000" maxlength="6" class="flex-1 bg-input border border-border rounded px-3 py-2 text-text-primary text-center font-mono focus:border-brand outline-none" />
                                    <button @click="confirm2FA" :disabled="isVerifying2FA || twoFactorCode.length < 6" class="bg-brand hover:bg-brand-hover text-white px-4 rounded font-bold disabled:opacity-50">
                                        {{ isVerifying2FA ? '...' : 'OK' }}
                                    </button>
                                </div>
                             </div>
                        </div>
                    </div>

                    <form @submit.prevent="handleChangePassword" class="space-y-5">
                         <div class="space-y-2">
                            <label class="text-xs text-text-secondary font-medium uppercase tracking-wider">Senha atual</label>
                            <input 
                                v-model="passwordForm.currentPassword"
                                type="password"
                                required
                                class="w-full bg-input border border-border rounded-md p-3 text-text-primary focus:outline-none focus:border-danger transition-colors" 
                                placeholder="••••••••"
                            />
                        </div>
                        <div class="space-y-2">
                            <label class="text-xs text-text-secondary font-medium uppercase tracking-wider">Nova senha</label>
                            <input 
                                v-model="passwordForm.newPassword"
                                type="password"
                                required
                                class="w-full bg-input border border-border rounded-md p-3 text-text-primary focus:outline-none focus:border-danger transition-colors" 
                                placeholder="••••••••"
                            />
                        </div>
                        <div class="space-y-2">
                            <label class="text-xs text-text-secondary font-medium uppercase tracking-wider">Confirmar nova senha</label>
                            <input 
                                v-model="passwordForm.confirmNewPassword"
                                type="password"
                                required
                                class="w-full bg-input border border-border rounded-md p-3 text-text-primary focus:outline-none focus:border-danger transition-colors" 
                                placeholder="••••••••"
                            />
                        </div>

                         <button type="submit" :disabled="isChangingPassword" class="w-full mt-2 bg-hover hover:bg-hover/80 text-text-primary font-bold py-3 rounded-md transition-all flex items-center justify-center gap-2 border border-border hover:border-danger group disabled:opacity-50 disabled:cursor-not-allowed">
                             <Key class="w-4 h-4 text-text-tertiary group-hover:text-danger transition-colors" />
                             <span v-if="isChangingPassword" class="group-hover:text-danger transition-colors">Alterando...</span>
                             <span v-else class="group-hover:text-danger transition-colors">Alterar Senha</span>
                        </button>
                    </form>
                 </div>
            </div>

            <!-- Danger Zone -->
            <div class="bg-card p-6 rounded-lg border border-border/50 mt-4 opacity-80 hover:opacity-100 transition-opacity">
                <div class="flex flex-col sm:flex-row items-center justify-between gap-4">
                    <div>
                        <h4 class="text-text-primary font-bold">Sair do Sistema</h4>
                        <p class="text-sm text-text-secondary">Encerrar sua sessão atual neste dispositivo.</p>
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
