<script setup lang="ts">
import { ref } from 'vue';
import { useAuthStore } from '../stores/auth';
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
    Plus
} from 'lucide-vue-next';

const auth = useAuthStore();
const isSidebarOpen = ref(false);

</script>

<template>
    <div class="flex h-screen bg-[#121214] text-gray-100 font-sans overflow-hidden">
        
        <!-- Sidebar (Desktop) -->
        <aside class="hidden md:flex flex-col w-64 bg-[#202024] border-r border-[#323238]">
            <div class="p-6 flex items-center gap-3">
                <div class="w-10 h-10 rounded-full bg-[#00875F] flex items-center justify-center shadow-lg">
                    <Wallet class="text-white w-6 h-6" />
                </div>
                <h1 class="text-2xl font-bold tracking-tight">Finance</h1>
            </div>

            <nav class="flex-1 px-4 py-4 space-y-2">
                <router-link to="/" class="flex items-center gap-3 px-4 py-3 rounded-lg transition-colors"
                :class="$route.path === '/' ? 'bg-[#00875F]/10 text-[#00875F] border border-[#00875F]/20' : 'text-gray-400 hover:text-white hover:bg-[#29292E]'">
                    <LayoutDashboard class="w-5 h-5" />
                    <span class="font-medium">Dashboard</span>
                </router-link>
                <router-link to="/extratos" class="flex items-center gap-3 px-4 py-3 rounded-lg transition-colors"
                :class="$route.path.includes('/extratos') ? 'bg-[#00875F]/10 text-[#00875F] border border-[#00875F]/20' : 'text-gray-400 hover:text-white hover:bg-[#29292E]'">
                    <TrendingUp class="w-5 h-5" />
                    <span>Extratos</span>
                </router-link>
                <router-link to="/perfil" class="flex items-center gap-3 px-4 py-3 rounded-lg transition-colors"
                :class="$route.path.includes('/perfil') ? 'bg-[#00875F]/10 text-[#00875F] border border-[#00875F]/20' : 'text-gray-400 hover:text-white hover:bg-[#29292E]'">
                    <Settings class="w-5 h-5" />
                    <span>Perfil</span>
                </router-link>
            </nav>

            <div class="p-4 border-t border-[#323238]">
                <button @click="auth.logout" class="flex items-center gap-3 px-4 py-3 w-full text-gray-400 hover:text-[#F75A68] hover:bg-[#29292E] rounded-lg transition-colors">
                    <LogOut class="w-5 h-5" />
                    <span>Sair</span>
                </button>
            </div>
        </aside>

        <!-- Main Content Wrapper -->
        <main class="flex-1 flex flex-col overflow-hidden relative">
            <!-- Mobile Header -->
            <header class="h-16 md:hidden flex items-center justify-between px-6 border-b border-[#323238] bg-[#202024]">
                 <div class="flex items-center gap-3">
                    <div class="w-8 h-8 rounded-full bg-[#00875F] flex items-center justify-center">
                        <Wallet class="text-white w-4 h-4" />
                    </div>
                    <span class="font-bold">Finance</span>
                </div>
                <button @click="isSidebarOpen = true" class="text-gray-400">
                    <Menu class="w-6 h-6" />
                </button>
            </header>

            <!-- Desktop Header -->
             <header class="hidden md:flex h-20 items-center justify-between px-8 bg-[#202024] border-b border-[#323238]">
                <div>
                     <!-- Breadcrumb or Page Title could go here, but for now we keep it dynamic or simple -->
                     <h2 class="text-2xl font-bold" v-if="$route.path === '/'">Dashboard</h2>
                     <h2 class="text-2xl font-bold" v-else-if="$route.path.includes('/extratos')">Extratos</h2>
                     <h2 class="text-2xl font-bold" v-else-if="$route.path.includes('/perfil')">Meu Perfil</h2>
                </div>

                <div class="flex items-center gap-4">
                    <button class="relative text-gray-400 hover:text-white transition-colors">
                        <Bell class="w-5 h-5" />
                        <span class="absolute top-0 right-0 w-2 h-2 bg-[#F75A68] rounded-full"></span>
                    </button>
                    <div class="flex items-center gap-3 pl-4 border-l border-[#323238]">
                        <router-link to="/perfil" class="text-right hidden sm:block cursor-pointer hover:opacity-80 transition-opacity">
                            <p class="text-sm font-medium text-white">{{ auth.user?.nomeUsuario }}</p>
                            <p class="text-xs text-gray-400">Premium</p>
                        </router-link>
                        <router-link to="/perfil" class="w-10 h-10 rounded-full bg-[#00875F] flex items-center justify-center text-white font-bold border-2 border-[#121214] overflow-hidden">
                            {{ auth.user?.nomeUsuario?.charAt(0).toUpperCase() }}
                        </router-link>
                    </div>
                </div>
            </header>

            <!-- Router View for Page Content -->
            <div class="flex-1 overflow-hidden relative">
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
        <aside v-if="isSidebarOpen" class="fixed inset-y-0 left-0 w-64 bg-[#202024] z-50 p-6 md:hidden shadow-2xl transition-transform animate-in slide-in-from-left">
             <div class="flex justify-between items-center mb-8">
                 <h2 class="text-xl font-bold">Menu</h2>
                 <button @click="isSidebarOpen = false"><X class="w-6 h-6" /></button>
             </div>
              <nav class="space-y-4">
                <router-link to="/" class="block text-gray-300 hover:text-[#00875F]" @click="isSidebarOpen = false">Dashboard</router-link>
                <router-link to="/extratos" class="block text-gray-300 hover:text-[#00875F]" @click="isSidebarOpen = false">Extratos</router-link>
                <router-link to="/perfil" class="block text-gray-300 hover:text-[#00875F]" @click="isSidebarOpen = false">Perfil</router-link>
                <button @click="auth.logout" class="block text-[#F75A68] mt-8">Sair</button>
            </nav>
        </aside>

    </div>
</template>
