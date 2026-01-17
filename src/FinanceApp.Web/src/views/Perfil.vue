<script setup lang="ts">
import { ref, computed, onMounted } from 'vue';
import { useRouter } from 'vue-router';
import { useAuthStore } from '../stores/auth';
import { useToastStore } from '../stores/toast';
import { 
    User, Lock, Settings, Download, LogOut, 
    Save, Key, ShieldCheck, Mail, Briefcase, Phone, Info,
    Moon, Sun, Bell, Monitor, Share2, Copy
} from 'lucide-vue-next';
import api from '../api/axios';
import QRCode from 'qrcode';

const router = useRouter();
const auth = useAuthStore();
const toast = useToastStore();

// Tabs
const activeTab = ref('general'); // general, security, preferences, data
const tabs = [
    { id: 'general', label: 'Geral', icon: User },
    { id: 'security', label: 'Segurança', icon: Lock },
    { id: 'preferences', label: 'Preferências', icon: Settings },
    { id: 'referral', label: 'Indicações', icon: Share2 },
    { id: 'data', label: 'Dados', icon: Download },
];

// Profile Data
const profileForm = ref({
    nomeUsuario: '',
    email: '',
    jobTitle: '',
    phone: '',
    bio: ''
});

// Security Data
const passwordForm = ref({
    currentPassword: '',
    newPassword: '',
    confirmNewPassword: ''
});

// 2FA Data
const isTwoFactorEnabled = ref(false);
const show2FASetup = ref(false);
const qrCodeUrl = ref('');
const twoFactorSecret = ref('');
const twoFactorCode = ref('');

// Loading States
const isLoading = ref(false);
const isSaving = ref(false);
const isVerifying2FA = ref(false);

onMounted(async () => {
    isLoading.value = true;
    try {
        await auth.fetchUser();
        // Populate form
        const user = auth.user as any; // Cast to access new fields if TS complains
        if (user) {
            profileForm.value = {
                nomeUsuario: user.nomeUsuario || '',
                email: user.email || '',
                jobTitle: user.jobTitle || '',
                phone: user.phone || '',
                bio: user.bio || ''
            };
            isTwoFactorEnabled.value = user.isTwoFactorEnabled || false;
        }
    } catch (error) {
        console.error(error);
    } finally {
        isLoading.value = false;
    }
});

// Actions
const handleUpdateProfile = async () => {
    isSaving.value = true;
    try {
        await auth.updateProfile({
            nomeUsuario: profileForm.value.nomeUsuario,
            email: profileForm.value.email,
            jobTitle: profileForm.value.jobTitle,
            phone: profileForm.value.phone,
            bio: profileForm.value.bio
        });
        toast.success('Perfil atualizado com sucesso!');
    } catch (error: any) {
        toast.error(error.response?.data || 'Erro ao atualizar perfil.');
    } finally {
        isSaving.value = false;
    }
};

const handleChangePassword = async () => {
    if (passwordForm.value.newPassword !== passwordForm.value.confirmNewPassword) {
        toast.warning('As senhas não conferem.');
        return;
    }

    isSaving.value = true;
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
        isSaving.value = false;
    }
};

// 2FA Logic
const start2FASetup = async () => {
    try {
        const response = await api.post('/auth/enable-2fa');
        const { secret, qrCodeUri } = response.data;
        twoFactorSecret.value = secret;
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
        toast.success('2FA habilitado com sucesso!');
        isTwoFactorEnabled.value = true;
        show2FASetup.value = false;
        await auth.fetchUser();
    } catch (error: any) {
        toast.error('Código inválido.');
    } finally {
        isVerifying2FA.value = false;
    }
};

const userInitial = computed(() => auth.user?.nomeUsuario?.charAt(0).toUpperCase() || 'U');

const downloadExport = async () => {
    try {
        toast.info('Gerando arquivo de exportação...');
        const response = await api.get('/export/json', { responseType: 'blob' });
        
        // Create blob link to download
        const url = window.URL.createObjectURL(new Blob([response.data]));
        const link = document.createElement('a');
        link.href = url;
        
        // Extract filename from header if present, or generate one
        const contentDisposition = response.headers['content-disposition'];
        let fileName = 'finance_export.json';
        if (contentDisposition) {
            const fileNameMatch = contentDisposition.match(/filename="?(.+)"?/);
            if (fileNameMatch && fileNameMatch.length === 2)
                fileName = fileNameMatch[1];
        }
        
        link.setAttribute('download', fileName);
        document.body.appendChild(link);
        link.click();
        link.remove();
        
        toast.success('Download iniciado!');
    } catch (error) {
        console.error(error);
        toast.error('Erro ao exportar dados.');
    }
};


const referralLink = computed(() => {
    const code = (auth.user as any)?.referralCode || '';
    return `${window.location.origin}/register?ref=${code}`;
});

const copyReferralLink = () => {
    navigator.clipboard.writeText(referralLink.value);
    toast.success('Link copiado!');
};

// Preferences
const preferences = ref({
    darkMode: true,
    notifications: false
});

const applyTheme = () => {
    if (preferences.value.darkMode) {
        document.documentElement.classList.remove('light');
    } else {
        document.documentElement.classList.add('light');
    }
};

// Load preferences from localStorage
const storedPrefs = localStorage.getItem('preferences');
if (storedPrefs) {
    preferences.value = JSON.parse(storedPrefs);
    applyTheme(); // Apply on load
}

const togglePreference = (key: 'darkMode' | 'notifications') => {
    try {
        preferences.value[key] = !preferences.value[key];
        localStorage.setItem('preferences', JSON.stringify(preferences.value));
        
        if (key === 'darkMode') {
            applyTheme();
            toast.success(preferences.value.darkMode ? 'Modo Escuro ativado.' : 'Modo Claro ativado.');
        } else if (key === 'notifications') {
            toast.success(`Notificações ${preferences.value.notifications ? 'ativadas' : 'desativadas'}.`);
        }
    } catch (e) {
        console.error('Error toggling preference:', e);
        alert('Erro ao alterar preferência');
    }
};
</script>

<template>
    <div class="h-full flex overflow-hidden bg-app">
        <!-- Sidebar Navigation -->
        <aside class="w-64 bg-card border-r border-border flex flex-col hidden lg:flex">
            <div class="p-6 border-b border-border">
                <h2 class="text-xl font-bold text-text-primary">Configurações</h2>
                <p class="text-xs text-text-secondary">Gerencie sua conta</p>
            </div>
            
            <nav class="flex-1 p-4 space-y-1">
                <button 
                    v-for="tab in tabs" 
                    :key="tab.id"
                    @click="activeTab = tab.id"
                    :class="[
                        'w-full flex items-center gap-3 px-4 py-3 rounded-md text-sm font-medium transition-all duration-200',
                        activeTab === tab.id 
                            ? 'bg-brand/10 text-brand border border-brand/20 shadow-sm' 
                            : 'text-text-secondary hover:bg-hover hover:text-text-primary'
                    ]"
                >
                    <component :is="tab.icon" class="w-5 h-5" />
                    {{ tab.label }}
                </button>
            </nav>

            <div class="p-4 border-t border-border">
                <button @click="auth.logout" class="w-full flex items-center gap-3 px-4 py-3 text-red-400 hover:bg-red-500/10 rounded-md transition-colors text-sm font-medium">
                    <LogOut class="w-5 h-5" />
                    Sair da Conta
                </button>
            </div>
        </aside>

        <!-- Main Content -->
        <main class="flex-1 overflow-y-auto p-8 relative">
            <div class="max-w-3xl mx-auto space-y-8">
                
                <!-- Profile Header -->
                <div class="flex items-center gap-6 mb-8">
                    <div class="w-24 h-24 rounded-full bg-brand flex items-center justify-center text-4xl font-bold text-white shadow-xl ring-4 ring-app">
                        {{ userInitial }}
                    </div>
                    <div>
                        <h1 class="text-2xl font-bold text-text-primary">{{ auth.user?.nomeUsuario }}</h1>
                        <p class="text-text-secondary">{{ auth.user?.email }}</p>
                        <div class="flex items-center gap-2 mt-2">
                            <span class="px-2 py-0.5 rounded-full bg-brand/10 text-brand text-xs font-bold border border-brand/20 uppercase">{{ (auth.user as any)?.planName || 'Gratuito' }}</span>
                            <span v-if="isTwoFactorEnabled" class="px-2 py-0.5 rounded-full bg-green-500/10 text-green-500 text-xs font-bold border border-green-500/20">Seguro</span>
                        </div>
                    </div>
                </div>

                <!-- Tabs (Mobile Only) -->
                <div class="flex lg:hidden overflow-x-auto pb-4 gap-2 border-b border-border mb-6">
                     <button 
                        v-for="tab in tabs" 
                        :key="tab.id"
                        @click="activeTab = tab.id"
                        :class="[
                            'flex items-center gap-2 px-4 py-2 rounded-full text-sm font-medium whitespace-nowrap',
                            activeTab === tab.id 
                                ? 'bg-brand text-white' 
                                : 'bg-card text-text-secondary border border-border'
                        ]"
                    >
                        {{ tab.label }}
                    </button>
                </div>

                <!-- Content Area -->
                <div class="bg-card rounded-xl border border-border shadow-sm overflow-hidden min-h-[500px]">
                    
                    <!-- General Tab -->
                    <div v-if="activeTab === 'general'" class="p-8 animate-in fade-in slide-in-from-bottom-4 duration-300">
                        <div class="flex items-center justify-between mb-6">
                            <div>
                                <h3 class="text-lg font-bold text-text-primary">Informações Pessoais</h3>
                                <p class="text-sm text-text-secondary">Atualize seus dados de perfil público.</p>
                            </div>
                            <button @click="handleUpdateProfile" :disabled="isSaving" class="bg-brand hover:bg-brand-hover text-white px-4 py-2 rounded-md font-bold text-sm flex items-center gap-2 transition-all">
                                <Save class="w-4 h-4" />
                                {{ isSaving ? 'Salvando...' : 'Salvar' }}
                            </button>
                        </div>

                        <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
                            <div class="space-y-2">
                                <label class="text-xs font-bold text-text-secondary uppercase tracking-wider flex items-center gap-1">
                                    <User class="w-3 h-3" /> Nome Completo
                                </label>
                                <input v-model="profileForm.nomeUsuario" type="text" class="w-full bg-input border border-border rounded-lg p-3 text-text-primary focus:ring-2 focus:ring-brand/20 focus:border-brand outline-none transition-all" />
                            </div>

                            <div class="space-y-2">
                                <label class="text-xs font-bold text-text-secondary uppercase tracking-wider flex items-center gap-1">
                                    <Mail class="w-3 h-3" /> Email
                                </label>
                                <input v-model="profileForm.email" type="email" class="w-full bg-input border border-border rounded-lg p-3 text-text-primary focus:ring-2 focus:ring-brand/20 focus:border-brand outline-none transition-all" />
                            </div>

                            <div class="space-y-2">
                                <label class="text-xs font-bold text-text-secondary uppercase tracking-wider flex items-center gap-1">
                                    <Briefcase class="w-3 h-3" /> Cargo / Função
                                </label>
                                <input v-model="profileForm.jobTitle" type="text" placeholder="Ex: Desenvolvedor Senior" class="w-full bg-input border border-border rounded-lg p-3 text-text-primary focus:ring-2 focus:ring-brand/20 focus:border-brand outline-none transition-all" />
                            </div>

                            <div class="space-y-2">
                                <label class="text-xs font-bold text-text-secondary uppercase tracking-wider flex items-center gap-1">
                                    <Phone class="w-3 h-3" /> Telefone
                                </label>
                                <input v-model="profileForm.phone" type="text" placeholder="(00) 00000-0000" class="w-full bg-input border border-border rounded-lg p-3 text-text-primary focus:ring-2 focus:ring-brand/20 focus:border-brand outline-none transition-all" />
                            </div>

                            <div class="col-span-1 md:col-span-2 space-y-2">
                                <label class="text-xs font-bold text-text-secondary uppercase tracking-wider flex items-center gap-1">
                                    <Info class="w-3 h-3" /> Sobre Mim
                                </label>
                                <textarea v-model="profileForm.bio" rows="4" placeholder="Escreva um pouco sobre você..." class="w-full bg-input border border-border rounded-lg p-3 text-text-primary focus:ring-2 focus:ring-brand/20 focus:border-brand outline-none transition-all resize-none"></textarea>
                            </div>
                        </div>
                    </div>

                    <!-- Security Tab -->
                    <div v-if="activeTab === 'security'" class="p-8 animate-in fade-in slide-in-from-bottom-4 duration-300">
                        <h3 class="text-lg font-bold text-text-primary mb-2">Segurança da Conta</h3>
                        <p class="text-sm text-text-secondary mb-8">Gerencie sua senha e autenticação de dois fatores.</p>

                        <!-- 2FA -->
                        <div class="p-6 rounded-xl border" :class="isTwoFactorEnabled ? 'bg-green-500/5 border-green-500/20' : 'bg-input border-border'">
                            <div class="flex justify-between items-start">
                                <div class="flex gap-4">
                                    <div class="p-3 rounded-lg" :class="isTwoFactorEnabled ? 'bg-green-500/10 text-green-500' : 'bg-card text-text-secondary'">
                                        <ShieldCheck class="w-6 h-6" />
                                    </div>
                                    <div>
                                        <h4 class="font-bold text-text-primary">Autenticação em Dois Fatores (2FA)</h4>
                                        <p class="text-sm text-text-secondary max-w-md mt-1">Proteja sua conta solicitando um código do seu app autenticador ao fazer login.</p>
                                    </div>
                                </div>
                                <div v-if="isTwoFactorEnabled" class="flex items-center gap-2 text-green-500 font-bold text-sm bg-green-500/10 px-3 py-1 rounded-full border border-green-500/20">
                                    Ativo
                                </div>
                                <button v-else @click="start2FASetup" class="px-4 py-2 bg-brand text-white rounded-md font-bold text-sm hover:bg-brand-hover transition-colors">
                                    Ativar Agora
                                </button>
                            </div>

                            <!-- Setup Area -->
                            <div v-if="show2FASetup && !isTwoFactorEnabled" class="mt-6 pt-6 border-t border-border flex flex-col md:flex-row gap-8 items-center animate-in fade-in">
                                <div class="bg-white p-2 rounded-lg shadow-sm">
                                    <img :src="qrCodeUrl" class="w-32 h-32" />
                                </div>
                                <div class="flex-1 space-y-4 w-full">
                                    <div>
                                        <p class="text-text-primary font-medium mb-1">1. Escaneie o QR Code</p>
                                        <p class="text-xs text-text-secondary">Use o Google Authenticator ou Authy.</p>
                                    </div>
                                    <div>
                                        <p class="text-text-primary font-medium mb-2">2. Digite o código</p>
                                        <div class="flex gap-2">
                                            <input v-model="twoFactorCode" type="text" placeholder="000000" maxlength="6" class="w-32 bg-card border border-border rounded px-3 py-2 text-center font-mono focus:border-brand outline-none" />
                                            <button @click="confirm2FA" class="px-4 py-2 bg-text-primary text-app rounded font-bold hover:bg-text-secondary">Verificar</button>
                                        </div>
                                    </div>
                                </div>
                            </div>
                        </div>

                        <!-- Reset Password -->
                        <div class="mt-8 pt-8 border-t border-border">
                            <h4 class="font-bold text-text-primary mb-4 flex items-center gap-2">
                                <Key class="w-4 h-4 text-brand" /> Alterar Senha
                            </h4>
                            <form @submit.prevent="handleChangePassword" class="space-y-4 max-w-md">
                                <input v-model="passwordForm.currentPassword" type="password" placeholder="Senha Atual" class="w-full bg-input border border-border rounded-lg p-3 text-text-primary focus:border-brand outline-none transition-all" />
                                <input v-model="passwordForm.newPassword" type="password" placeholder="Nova Senha" class="w-full bg-input border border-border rounded-lg p-3 text-text-primary focus:border-brand outline-none transition-all" />
                                <input v-model="passwordForm.confirmNewPassword" type="password" placeholder="Confirmar Nova Senha" class="w-full bg-input border border-border rounded-lg p-3 text-text-primary focus:border-brand outline-none transition-all" />
                                <button type="submit" :disabled="isSaving" class="px-6 py-2 bg-card border border-border hover:border-brand hover:text-brand text-text-secondary rounded-md font-bold transition-all w-full">
                                    Atualizar Senha
                                </button>
                            </form>
                        </div>
                    </div>

                    <!-- Preferences Tab (Mock) -->
                    <div v-if="activeTab === 'preferences'" class="p-8 animate-in fade-in slide-in-from-bottom-4 duration-300">
                        <h3 class="text-lg font-bold text-text-primary mb-6">Preferências do Sistema</h3>
                        
                        <div class="space-y-6">
                            <div class="flex items-center justify-between p-4 bg-input rounded-lg border border-border">
                                <div class="flex items-center gap-4">
                                    <Moon class="w-5 h-5 text-brand" />
                                    <div>
                                        <h4 class="font-bold text-text-primary">Modo Escuro</h4>
                                        <p class="text-xs text-text-secondary">Alternar entre tema claro e escuro</p>
                                    </div>
                                </div>
                                <div @click="togglePreference('darkMode')" class="w-12 h-6 rounded-full relative cursor-pointer transition-colors" :class="preferences.darkMode ? 'bg-brand' : 'bg-gray-600'">
                                    <div class="absolute top-1 w-4 h-4 bg-white rounded-full shadow-sm transition-all" :class="preferences.darkMode ? 'right-1' : 'left-1'"></div>
                                </div>
                            </div>

                            <div class="flex items-center justify-between p-4 bg-input rounded-lg border border-border">
                                <div class="flex items-center gap-4">
                                    <div class="p-2 bg-blue-500/10 rounded-lg text-blue-500">
                                        <Bell class="w-5 h-5" />
                                    </div>
                                    <div>
                                        <h4 class="font-bold text-text-primary">Notificações</h4>
                                        <p class="text-xs text-text-secondary">Receber alertas via email</p>
                                    </div>
                                </div>
                                <div @click="togglePreference('notifications')" class="w-12 h-6 rounded-full relative cursor-pointer transition-colors" :class="preferences.notifications ? 'bg-brand' : 'bg-gray-600'">
                                    <div class="absolute top-1 w-4 h-4 bg-white rounded-full shadow-sm transition-all" :class="preferences.notifications ? 'right-1' : 'left-1'"></div>
                                </div>
                            </div>
                        </div>
                    </div>

                    <!-- Referral Tab -->
                    <div v-if="activeTab === 'referral'" class="p-8 animate-in fade-in slide-in-from-bottom-4 duration-300">
                        <h3 class="text-lg font-bold text-text-primary mb-2">Programa de Indicação</h3>
                        <p class="text-sm text-text-secondary mb-8">Convide amigos e ganhe recompensas no futuro!</p>

                        <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
                            <!-- Link Card -->
                            <div class="p-6 bg-gradient-to-br from-brand/10 to-transparent border border-brand/20 rounded-xl">
                                <h4 class="font-bold text-brand mb-4 flex items-center gap-2">
                                    <Share2 class="w-5 h-5" /> Seu Link de Convite
                                </h4>
                                <div class="flex gap-2">
                                    <input 
                                        readonly 
                                        :value="referralLink" 
                                        class="flex-1 bg-black/20 border border-brand/20 rounded-md px-3 py-2 text-sm text-text-primary"
                                    />
                                    <button @click="copyReferralLink" class="p-2 bg-brand text-white rounded-md hover:bg-brand-hover transition-colors">
                                        <Copy class="w-4 h-4" />
                                    </button>
                                </div>
                                <p class="text-xs text-text-secondary mt-3">Envie este link para seus amigos criarem uma conta.</p>
                            </div>

                            <!-- Link Stats -->
                            <div class="p-6 bg-card border border-border rounded-xl flex flex-col items-center justify-center text-center">
                                <span class="text-4xl font-bold text-white mb-2">{{ (auth.user as any)?.referralCount || 0 }}</span>
                                <span class="text-sm text-text-secondary font-medium uppercase tracking-wider">Amigos Indicados</span>
                            </div>
                        </div>
                    </div>

                     <!-- Data Tab (Mock) -->
                     <div v-if="activeTab === 'data'" class="p-8 animate-in fade-in slide-in-from-bottom-4 duration-300">
                        <h3 class="text-lg font-bold text-text-primary mb-6">Seus Dados</h3>
                        
                        <div class="bg-input rounded-xl p-6 text-center border-2 border-dashed border-border hover:border-brand/50 transition-colors cursor-pointer group">
                            <Download class="w-12 h-12 text-text-tertiary mx-auto mb-4 group-hover:text-brand transition-colors" />
                            <h4 class="font-bold text-text-primary">Exportar Dados Completos</h4>
                            <p class="text-sm text-text-secondary mb-4">Baixe um arquivo .JSON com todas as suas transações e histórico.</p>
                            <button @click="downloadExport" class="px-4 py-2 bg-card border border-border rounded-md text-sm font-bold hover:bg-brand hover:text-white hover:border-brand transition-all">
                                Solicitar Exportação
                            </button>
                        </div>
                    </div>

                </div>
            </div>
        </main>
    </div>
</template>
