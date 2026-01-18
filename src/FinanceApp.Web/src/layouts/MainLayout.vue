<script setup lang="ts">
import { ref, onMounted } from 'vue';
import { useAuthStore } from '../stores/auth';
import { useThemeStore } from '../stores/theme';
import { useNotificationStore } from '../stores/notification';
import { usePrivacyStore } from '../stores/privacy';
import NotificationDetailsModal from '../components/NotificationDetailsModal.vue';
import {
    LayoutDashboard, 
    Wallet, 
    TrendingUp, 
    LogOut, 
    Search,
    Bell,
    Settings,
    Menu,
    X,
    Plus,
    Shield,
    Tag,
    RefreshCw,
    Sun,
    Moon,
    Target,
    Package,
    FileText,
    ChevronDown,
    Users,
    Link,
    PieChart,
    BarChart3,
    Eye,
    EyeOff
} from 'lucide-vue-next';

interface MenuItem {
    label: string;
    route: string;
    icon: any;
}

interface MenuGroup {
    title?: string;
    isAdmin?: boolean;
    items: MenuItem[];
}

const auth = useAuthStore();
const theme = useThemeStore();
const isSidebarOpen = ref(false);

const menuGroups = [
    {
        title: 'Geral', // Optional title for first section
        items: [
            { label: 'Dashboard', route: '/', icon: LayoutDashboard },
            { label: 'Movimentações', route: '/extratos', icon: TrendingUp },
            { label: 'Relatórios', route: '/reports', icon: BarChart3 },
        ]
    },
    {
        title: 'Planejamento',
        items: [
            { label: 'Metas', route: '/goals', icon: Target },
            { label: 'Orçamentos', route: '/planning/budgets', icon: PieChart },
            { label: 'Recorrência', route: '/planning/recurring', icon: RefreshCw },
            { label: 'Faturas', route: '/planning/invoices', icon: FileText }
        ]
    },
    {
        title: 'Configurações',
        items: [
            { label: 'Categorias', route: '/settings/categories', icon: Tag },
            { label: 'Carteiras e Cartões', route: '/settings/wallets', icon: Wallet },
            { label: 'Perfil', route: '/perfil', icon: Settings },
        ]
    },
    {
        title: 'Administração',
        isAdmin: true,
        items: [
            { label: 'Usuários', route: '/admin/users', icon: Users },
            { label: 'Planos', route: '/admin/plans', icon: Package },
            { label: 'Integrações', route: '/admin/integrations', icon: Link },
        ]
    }
];

const collapsedSections = ref<Record<string, boolean>>({
    'Geral': false,
    'Planejamento': false,
    'Configurações': false,
    'Administração': false
});

const toggleSection = (title: string | undefined) => {
    if (!title) return;
    collapsedSections.value[title] = !collapsedSections.value[title];
};

// Notification Logic
const notificationStore = useNotificationStore();
const privacyStore = usePrivacyStore();
const showNotifications = ref(false);
const selectedNotification = ref<any>(null);
const isModalOpen = ref(false);

onMounted(() => {
    if (auth.isAuthenticated) {
        notificationStore.fetchUnreadCount();
        notificationStore.fetchNotifications();
    }
});

const toggleNotifications = () => {
    showNotifications.value = !showNotifications.value;
    if (showNotifications.value) {
        notificationStore.fetchNotifications();
    }
};

const handleNotificationClick = async (notification: any) => {
    selectedNotification.value = notification;
    isModalOpen.value = true;
    showNotifications.value = false; // Close dropdown

    if (!notification.fl_lida) {
        await notificationStore.markAsRead(notification.id_notificacao);
    }
};

const handleMarkAllRead = async () => {
    await notificationStore.markAllAsRead();
};

const stripHtml = (html: string) => {
    if (!html) return '';
    const tmp = document.createElement("DIV");
    tmp.innerHTML = html;
    return tmp.textContent || tmp.innerText || "";
};
</script>

<template>
    <div class="flex h-screen bg-app text-text-primary font-sans overflow-hidden">
        
        <!-- Sidebar (Desktop) -->
        <aside class="hidden md:flex flex-col w-64 bg-card border-r border-border">
            <div class="p-6 flex items-center gap-3">
                <div class="w-10 h-10 rounded-full bg-brand flex items-center justify-center shadow-lg">
                    <Wallet class="text-white w-6 h-6" />
                </div>
                <h1 class="text-2xl font-bold tracking-tight text-text-primary">Finance</h1>
            </div>

            <nav id="sidebar-nav" class="flex-1 px-4 py-4 space-y-6 overflow-y-auto">
                <div v-for="(group, index) in menuGroups" :key="index">
                    <!-- Section Header -->
                    <div v-if="group.title" 
                         @click="toggleSection(group.title)"
                         class="flex items-center justify-between px-2 mb-2 cursor-pointer group select-none">
                        <p class="text-xs font-bold text-text-tertiary uppercase tracking-wider group-hover:text-text-primary transition-colors">
                            {{ group.title }}
                        </p>
                         <ChevronDown 
                            class="w-4 h-4 text-text-tertiary transition-transform duration-200"
                            :class="{ '-rotate-90': collapsedSections[group.title] }"
                         />
                    </div>

                    <!-- Items -->
                    <div v-show="!auth.user?.isAdmin && group.isAdmin ? false : true" class="space-y-1">
                        <transition-group
                            enter-active-class="transition duration-200 ease-out"
                            enter-from-class="transform -translate-y-2 opacity-0"
                            enter-to-class="transform translate-y-0 opacity-100"
                            leave-active-class="transition duration-200 ease-in"
                            leave-from-class="transform translate-y-0 opacity-100"
                            leave-to-class="transform -translate-y-2 opacity-0"
                        >
                            <div v-if="!collapsedSections[group.title || '']" class="space-y-1">
                                <template v-for="item in group.items" :key="item.route">
                                     <router-link 
                                        v-if="!group.isAdmin || (group.isAdmin && auth.user?.isAdmin)"
                                        :to="item.route" 
                                        class="flex items-center gap-3 px-4 py-2.5 rounded-lg transition-all duration-200 text-sm"
                                        :class="$route.path === item.route || (item.route !== '/' && $route.path.startsWith(item.route)) 
                                            ? 'bg-brand/10 text-brand border border-brand/20 shadow-sm' 
                                            : 'text-text-secondary hover:text-text-primary hover:bg-hover hover:pl-5'"
                                    >
                                        <component :is="item.icon" class="w-4 h-4" />
                                        <span class="font-medium">{{ item.label }}</span>
                                    </router-link>
                                </template>
                            </div>
                        </transition-group>
                    </div>
                </div>
            </nav>

            <div class="p-4 border-t border-border">
                <button @click="auth.logout" class="flex items-center gap-3 px-4 py-3 w-full text-text-secondary hover:text-danger hover:bg-hover rounded-lg transition-colors">
                    <LogOut class="w-5 h-5" />
                    <span>Sair</span>
                </button>
            </div>
        </aside>

        <!-- Main Content Wrapper -->
        <main class="flex-1 flex flex-col overflow-hidden relative">
            <!-- Mobile Header -->
            <header class="h-16 md:hidden flex items-center justify-between px-6 border-b border-border bg-card">
                 <div class="flex items-center gap-3">
                    <div class="w-8 h-8 rounded-full bg-brand flex items-center justify-center">
                        <Wallet class="text-white w-4 h-4" />
                    </div>
                    <span class="font-bold text-text-primary">Finance</span>
                </div>
                <button @click="isSidebarOpen = true" class="text-text-secondary">
                    <Menu class="w-6 h-6" />
                </button>
            </header>

            <!-- Desktop Header -->
             <header class="hidden md:flex h-20 items-center justify-between px-8 bg-card border-b border-border">
                <div>
                     <!-- Breadcrumb or Page Title could go here, but for now we keep it dynamic or simple -->
                     <h2 class="text-2xl font-bold text-text-primary" v-if="$route.path === '/'">Dashboard</h2>
                     <h2 class="text-2xl font-bold text-text-primary" v-else-if="$route.path.includes('/extratos')">Extratos</h2>
                     <h2 class="text-2xl font-bold text-text-primary" v-else-if="$route.path.includes('/goals')">Metas</h2>
                     <h2 class="text-2xl font-bold text-text-primary" v-else-if="$route.path.includes('/reports')">Relatórios</h2>
                     <h2 class="text-2xl font-bold text-text-primary" v-else-if="$route.path.includes('/perfil')">Meu Perfil</h2>
                     <h2 class="text-2xl font-bold text-text-primary" v-else-if="$route.path.includes('/admin')">Administração</h2>
                </div>

                <div class="flex items-center gap-4">
                    <!-- Theme Toggle -->
                    <button id="theme-toggle" @click="theme.toggleTheme()" class="p-2 text-text-secondary hover:text-text-primary rounded-lg hover:bg-hover transition-colors">
                        <Moon v-if="theme.isDark" class="w-5 h-5" />
                        <Sun v-else class="w-5 h-5" />
                    </button>

                    <!-- Privacy Toggle -->
                    <button @click="privacyStore.toggle()" class="p-2 text-text-secondary hover:text-text-primary rounded-lg hover:bg-hover transition-colors" title="Modo Privacidade">
                        <EyeOff v-if="privacyStore.isHidden" class="w-5 h-5" />
                        <Eye v-else class="w-5 h-5" />
                    </button>

                    <!-- Notifications Dropdown -->
                    <div class="relative">
                        <button 
                            @click="toggleNotifications"
                            class="relative p-2 text-text-secondary hover:text-text-primary rounded-lg hover:bg-hover transition-colors outline-none"
                        >
                            <Bell class="w-5 h-5" />
                            <span v-if="notificationStore.unreadCount > 0" class="absolute top-1 right-1 w-2.5 h-2.5 bg-danger rounded-full border-2 border-card"></span>
                        </button>

                        <!-- Dropdown Panel -->
                        <div v-if="showNotifications" 
                             class="absolute right-0 mt-2 w-80 sm:w-96 bg-card border border-border rounded-xl shadow-xl z-50 overflow-hidden animate-in fade-in slide-in-from-top-2"
                        >
                            <div class="p-4 border-b border-border flex justify-between items-center">
                                <h3 class="font-bold text-text-primary">Notificações</h3>
                                <button 
                                    v-if="notificationStore.unreadCount > 0"
                                    @click="handleMarkAllRead"
                                    class="text-xs text-brand hover:text-brand-hover font-medium"
                                >
                                    Marcar todas como lidas
                                </button>
                            </div>

                            <div class="max-h-[70vh] overflow-y-auto">
                                <div v-if="notificationStore.isLoading" class="p-8 text-center text-text-tertiary">
                                    <RefreshCw class="w-6 h-6 animate-spin mx-auto mb-2" />
                                    <p class="text-xs">Carregando...</p>
                                </div>

                                <div v-else-if="notificationStore.notifications.length === 0" class="p-8 text-center text-text-tertiary">
                                    <Bell class="w-8 h-8 mx-auto mb-2 opacity-50" />
                                    <p class="text-sm">Nenhuma notificação nova</p>
                                </div>

                                <div v-else class="divide-y divide-border/50">
                                    <div 
                                        v-for="notification in notificationStore.notifications" 
                                        :key="notification.id_notificacao"
                                        class="p-4 hover:bg-hover transition-colors cursor-pointer flex gap-3 items-start"
                                        :class="{ 'bg-brand/5': !notification.fl_lida }"
                                        @click="handleNotificationClick(notification)"
                                    >
                                        <div class="mt-1">
                                            <div class="w-2 h-2 rounded-full" :class="notification.fl_lida ? 'bg-transparent' : 'bg-brand'"></div>
                                        </div>
                                        <div class="flex-1 space-y-1">
                                            <p class="text-sm font-medium text-text-primary leading-none">{{ notification.nm_titulo }}</p>
                                            <p class="text-xs text-text-secondary line-clamp-2">{{ stripHtml(notification.ds_mensagem) }}</p>
                                            <p class="text-[10px] text-text-tertiary uppercase font-bold tracking-wider">
                                                {{ new Date(notification.dt_criacao).toLocaleDateString() }}
                                            </p>
                                        </div>
                                    </div>
                                </div>
                            </div>
                        </div>
                        
                        <!-- Backdrop for clicking outside -->
                        <div v-if="showNotifications" class="fixed inset-0 z-40" @click="showNotifications = false"></div>
                    </div>
                    <div class="flex items-center gap-3 pl-4 border-l border-border">
                        <router-link to="/perfil" class="text-right hidden sm:block cursor-pointer hover:opacity-80 transition-opacity">
                            <p class="text-sm font-medium text-text-primary">{{ auth.user?.nomeUsuario }}</p>
                            <p class="text-xs text-text-secondary text-[#00B37E] font-semibold">{{ (auth.user as any)?.planName || 'Gratuito' }}</p>
                        </router-link>
                        <router-link to="/perfil" class="w-10 h-10 rounded-full bg-brand flex items-center justify-center text-white font-bold border-2 border-app overflow-hidden">
                            {{ auth.user?.nomeUsuario?.charAt(0).toUpperCase() }}
                        </router-link>
                    </div>
                </div>
            </header>

            <!-- Router View for Page Content -->
            <div class="flex-1 overflow-hidden relative bg-app">
                 <router-view />
            </div>

            <NotificationDetailsModal 
                :is-open="isModalOpen" 
                :notification="selectedNotification"
                @close="isModalOpen = false"
            />

            <!-- Mobile Quick Action (FAB) - Visible only on Dashboard usually, or globally? 
                 Let's keep it global for consistency or maybe just on Dashboard via the page itself?
                 Actually, the design had it in the content area for mobile. 
                 Let's leave it to individual pages if it's specific.
            -->

        </main>
        
        <!-- Mobile Sidebar Overlay -->
        <div v-if="isSidebarOpen" class="fixed inset-0 bg-black/50 z-40 md:hidden animate-in fade-in" @click="isSidebarOpen = false"></div>
        <aside v-if="isSidebarOpen" class="fixed inset-y-0 left-0 w-64 bg-card z-50 p-6 md:hidden shadow-2xl transition-transform animate-in slide-in-from-left">
             <div class="flex justify-between items-center mb-8">
                 <h2 class="text-xl font-bold text-text-primary">Menu</h2>
                 <button @click="isSidebarOpen = false" class="text-text-secondary"><X class="w-6 h-6" /></button>
             </div>
              <nav class="space-y-4 overflow-y-auto flex-1 h-full pb-20">
                <div v-for="(group, index) in menuGroups" :key="index">
                     <div v-if="group.title" 
                          @click="toggleSection(group.title)" 
                          class="flex justify-between items-center text-text-secondary font-bold mb-2 cursor-pointer select-none">
                         <span>{{ group.title }}</span>
                         <ChevronDown class="w-4 h-4 transition-transform" :class="{ '-rotate-90': collapsedSections[group.title] }" />
                     </div>
                     
                     <div v-show="!collapsedSections[group.title || '']" class="space-y-2 pl-2 border-l border-border/50 ml-1">
                         <template v-for="item in group.items" :key="item.route">
                             <router-link 
                                v-if="!group.isAdmin || (group.isAdmin && auth.user?.isAdmin)"
                                :to="item.route" 
                                class="block text-text-secondary hover:text-brand py-2" 
                                :class="$route.path === item.route ? 'text-brand font-medium' : ''"
                                @click="isSidebarOpen = false"
                             >
                                {{ item.label }}
                             </router-link>
                         </template>
                     </div>
                </div>
                
                <div class="border-t border-border pt-4 mt-4 space-y-4">
                    <div class="flex justify-between items-center text-text-secondary">
                        <span>Tema / Privacidade</span>
                        <div class="flex gap-2">
                             <button @click="privacyStore.toggle()" class="p-2">
                                <EyeOff v-if="privacyStore.isHidden" class="w-5 h-5" />
                                <Eye v-else class="w-5 h-5" />
                            </button>
                            <button @click="theme.toggleTheme()" class="p-2">
                                <Moon v-if="theme.isDark" class="w-5 h-5" />
                                <Sun v-else class="w-5 h-5" />
                            </button>
                        </div>
                    </div>
                    <button @click="auth.logout" class="block text-danger w-full text-left">Sair</button>
                </div>
            </nav>
        </aside>

    </div>
</template>
```
