<script setup lang="ts">
import { ref } from 'vue';
import { useAuthStore } from '../stores/auth';
import { useThemeStore } from '../stores/theme';
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
    Moon
} from 'lucide-vue-next';

const auth = useAuthStore();
const theme = useThemeStore();
const isSidebarOpen = ref(false);

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

            <nav id="sidebar-nav" class="flex-1 px-4 py-4 space-y-2">
                <router-link to="/" class="flex items-center gap-3 px-4 py-3 rounded-lg transition-colors"
                :class="$route.path === '/' ? 'bg-brand/10 text-brand border border-brand/20' : 'text-text-secondary hover:text-text-primary hover:bg-hover'">
                    <LayoutDashboard class="w-5 h-5" />
                    <span class="font-medium">Dashboard</span>
                </router-link>
                <router-link to="/extratos" class="flex items-center gap-3 px-4 py-3 rounded-lg transition-colors"
                :class="$route.path.includes('/extratos') ? 'bg-brand/10 text-brand border border-brand/20' : 'text-text-secondary hover:text-text-primary hover:bg-hover'">
                    <TrendingUp class="w-5 h-5" />
                    <span>Extratos</span>
                </router-link>
                <router-link to="/reports" class="flex items-center gap-3 px-4 py-3 rounded-lg transition-colors"
                :class="$route.path.includes('/reports') ? 'bg-brand/10 text-brand border border-brand/20' : 'text-text-secondary hover:text-text-primary hover:bg-hover'">
                    <TrendingUp class="w-5 h-5 rotate-180" />
                    <span>Relatórios</span>
                </router-link>
                <router-link to="/perfil" class="flex items-center gap-3 px-4 py-3 rounded-lg transition-colors"
                :class="$route.path.includes('/perfil') ? 'bg-brand/10 text-brand border border-brand/20' : 'text-text-secondary hover:text-text-primary hover:bg-hover'">
                    <Settings class="w-5 h-5" />
                    <span>Perfil</span>
                </router-link>
                <router-link to="/settings/categories" class="flex items-center gap-3 px-4 py-3 rounded-lg transition-colors"
                :class="$route.path.includes('/categories') ? 'bg-brand/10 text-brand border border-brand/20' : 'text-text-secondary hover:text-text-primary hover:bg-hover'">
                    <Tag class="w-5 h-5" />
                    <span>Categorias</span>
                </router-link>
                <router-link to="/settings/wallets" class="flex items-center gap-3 px-4 py-3 rounded-lg transition-colors"
                :class="$route.path.includes('/wallets') ? 'bg-brand/10 text-brand border border-brand/20' : 'text-text-secondary hover:text-text-primary hover:bg-hover'">
                    <Wallet class="w-5 h-5" />
                    <span>Carteiras e Cartões</span>
                </router-link>

                <div class="px-4 pt-4 pb-2">
                    <p class="text-xs font-bold text-text-tertiary uppercase tracking-wider">Planejamento</p>
                </div>

                <router-link to="/planning/budgets" class="flex items-center gap-3 px-4 py-3 rounded-lg transition-colors"
                :class="$route.path.includes('/budgets') ? 'bg-brand/10 text-brand border border-brand/20' : 'text-text-secondary hover:text-text-primary hover:bg-hover'">
                    <TrendingUp class="w-5 h-5" />
                    <span>Orçamentos</span>
                </router-link>
                <router-link to="/planning/recurring" class="flex items-center gap-3 px-4 py-3 rounded-lg transition-colors"
                :class="$route.path.includes('/recurring') ? 'bg-brand/10 text-brand border border-brand/20' : 'text-text-secondary hover:text-text-primary hover:bg-hover'">
                    <RefreshCw class="w-5 h-5" />
                    <span>Recorrência</span>
                </router-link>
                <!-- Admin Link -->
                <router-link v-if="auth.user?.isAdmin" to="/admin/users" class="flex items-center gap-3 px-4 py-3 rounded-lg transition-colors"
                :class="$route.path.includes('/admin') ? 'bg-brand/10 text-brand border border-brand/20' : 'text-text-secondary hover:text-text-primary hover:bg-hover'">
                    <Shield class="w-5 h-5" />
                    <span>Administração</span>
                </router-link>
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

                    <button class="relative text-text-secondary hover:text-text-primary transition-colors">
                        <Bell class="w-5 h-5" />
                        <span class="absolute top-0 right-0 w-2 h-2 bg-danger rounded-full"></span>
                    </button>
                    <div class="flex items-center gap-3 pl-4 border-l border-border">
                        <router-link to="/perfil" class="text-right hidden sm:block cursor-pointer hover:opacity-80 transition-opacity">
                            <p class="text-sm font-medium text-text-primary">{{ auth.user?.nomeUsuario }}</p>
                            <p class="text-xs text-text-secondary">Premium</p>
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
              <nav class="space-y-4">
                <router-link to="/" class="block text-text-secondary hover:text-brand" @click="isSidebarOpen = false">Dashboard</router-link>
                <router-link to="/extratos" class="block text-text-secondary hover:text-brand" @click="isSidebarOpen = false">Extratos</router-link>
                <router-link to="/reports" class="block text-text-secondary hover:text-brand" @click="isSidebarOpen = false">Relatórios</router-link>
                <router-link to="/perfil" class="block text-text-secondary hover:text-brand" @click="isSidebarOpen = false">Perfil</router-link>
                <router-link to="/settings/categories" class="block text-text-secondary hover:text-brand" @click="isSidebarOpen = false">Categorias</router-link>
                <router-link to="/settings/wallets" class="block text-text-secondary hover:text-brand" @click="isSidebarOpen = false">Carteiras</router-link>
                <div class="flex justify-between items-center text-text-secondary">
                    <span>Tema</span>
                    <button @click="theme.toggleTheme()" class="p-2">
                        <Moon v-if="theme.isDark" class="w-5 h-5" />
                        <Sun v-else class="w-5 h-5" />
                    </button>
                </div>
                <router-link v-if="auth.user?.isAdmin" to="/admin/users" class="block text-text-secondary hover:text-brand" @click="isSidebarOpen = false">Administração</router-link>
                <button @click="auth.logout" class="block text-danger mt-8">Sair</button>
            </nav>
        </aside>

    </div>
</template>
```
