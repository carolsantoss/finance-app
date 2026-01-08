<script setup lang="ts">
import { onMounted, ref, computed } from 'vue';
import { useAuthStore } from '../stores/auth';
import { useFinanceStore } from '../stores/finance';
import { 
    LayoutDashboard, 
    Wallet, 
    TrendingUp, 
    TrendingDown, 
    LogOut, 
    Plus, 
    Search,
    Bell,
    Settings,
    Menu,
    X
} from 'lucide-vue-next';
import FinanceChart from '../components/FinanceChart.vue';

const auth = useAuthStore();
const finance = useFinanceStore();

const isSidebarOpen = ref(false);
const showForm = ref(false);
const newTransaction = ref({
  nm_tipo: 'Entrada',
  nm_descricao: '',
  nr_valor: 0,
  dt_dataLancamento: new Date().toISOString().split('T')[0]
});

onMounted(() => {
  finance.fetchSummary();
  finance.fetchTransactions();
});

const handleAdd = async () => {
    // Add logic here
    await finance.addTransaction(newTransaction.value);
    showForm.value = false;
};

// Formatter
const formatCurrency = (value: number) => {
    return new Intl.NumberFormat('pt-BR', { style: 'currency', currency: 'BRL' }).format(value);
};
</script>

<template>
    <div class="flex h-screen bg-gray-900 text-gray-100 font-sans overflow-hidden">
        
        <!-- Sidebar (Desktop) -->
        <aside class="hidden md:flex flex-col w-64 bg-gray-800 border-r border-gray-700">
            <div class="p-6 flex items-center gap-3">
                <div class="w-10 h-10 rounded-xl bg-gradient-to-tr from-blue-500 to-purple-600 flex items-center justify-center shadow-lg">
                    <Wallet class="text-white w-6 h-6" />
                </div>
                <h1 class="text-2xl font-bold tracking-tight">Finance</h1>
            </div>

            <nav class="flex-1 px-4 py-4 space-y-2">
                <a href="#" class="flex items-center gap-3 px-4 py-3 bg-blue-600/10 text-blue-400 rounded-lg border border-blue-600/20">
                    <LayoutDashboard class="w-5 h-5" />
                    <span class="font-medium">Dashboard</span>
                </a>
                <a href="#" class="flex items-center gap-3 px-4 py-3 text-gray-400 hover:text-white hover:bg-gray-700/50 rounded-lg transition-colors">
                    <TrendingUp class="w-5 h-5" />
                    <span>Receitas</span>
                </a>
                <a href="#" class="flex items-center gap-3 px-4 py-3 text-gray-400 hover:text-white hover:bg-gray-700/50 rounded-lg transition-colors">
                    <TrendingDown class="w-5 h-5" />
                    <span>Despesas</span>
                </a>
                <a href="#" class="flex items-center gap-3 px-4 py-3 text-gray-400 hover:text-white hover:bg-gray-700/50 rounded-lg transition-colors">
                    <Settings class="w-5 h-5" />
                    <span>Ajustes</span>
                </a>
            </nav>

            <div class="p-4 border-t border-gray-700">
                <button @click="auth.logout" class="flex items-center gap-3 px-4 py-3 w-full text-red-400 hover:bg-red-500/10 rounded-lg transition-colors">
                    <LogOut class="w-5 h-5" />
                    <span>Sair</span>
                </button>
            </div>
        </aside>

        <!-- Main Content -->
        <main class="flex-1 flex flex-col overflow-hidden relative">
            <!-- Header -->
            <header class="h-16 bg-gray-800/50 backdrop-blur-md border-b border-gray-700 flex items-center justify-between px-6 z-20">
                <div class="flex items-center gap-4">
                    <button class="md:hidden text-gray-400" @click="isSidebarOpen = true">
                        <Menu class="w-6 h-6" />
                    </button>
                    <div class="relative hidden sm:block">
                        <Search class="absolute left-3 top-2.5 text-gray-500 w-4 h-4" />
                        <input type="text" placeholder="Buscar..." class="bg-gray-900 border border-gray-700 rounded-full pl-10 pr-4 py-2 text-sm focus:border-blue-500 focus:outline-none w-64 transition-all" />
                    </div>
                </div>
                <div class="flex items-center gap-4">
                    <button class="relative text-gray-400 hover:text-white transition-colors">
                        <Bell class="w-5 h-5" />
                        <span class="absolute top-0 right-0 w-2 h-2 bg-red-500 rounded-full"></span>
                    </button>
                    <div class="flex items-center gap-3 pl-4 border-l border-gray-700">
                        <div class="text-right hidden sm:block">
                            <p class="text-sm font-medium text-white">{{ auth.user?.nomeUsuario }}</p>
                            <p class="text-xs text-gray-400">Premium</p>
                        </div>
                        <div class="w-9 h-9 rounded-full bg-gradient-to-br from-gray-700 to-gray-600 border border-gray-500"></div>
                    </div>
                </div>
            </header>

            <!-- Scrollable Content -->
            <div class="flex-1 overflow-y-auto p-6 space-y-6">
                <!-- Welcome Section -->
                <div class="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4">
                    <div>
                        <h2 class="text-2xl font-bold">Visão Geral</h2>
                        <p class="text-gray-400">Resumo financeiro dos últimos 30 dias.</p>
                    </div>
                    <button @click="showForm = true" class="flex items-center gap-2 bg-blue-600 hover:bg-blue-700 text-white px-5 py-2.5 rounded-lg shadow-lg shadow-blue-500/20 transition-all font-medium">
                        <Plus class="w-5 h-5" />
                        Nova Transação
                    </button>
                </div>

                <!-- Cards Grid -->
                <div class="grid grid-cols-1 md:grid-cols-3 gap-6">
                    <div class="bg-gray-800 p-6 rounded-2xl border border-gray-700 shadow-xl relative overflow-hidden group">
                        <div class="absolute right-[-20px] top-[-20px] w-24 h-24 bg-blue-500/20 rounded-full blur-2xl group-hover:bg-blue-500/30 transition-all"></div>
                        <div class="relative z-10">
                            <p class="text-gray-400 font-medium mb-1">Saldo Total</p>
                            <h3 class="text-3xl font-bold text-white">{{ formatCurrency(finance.summary.saldo) }}</h3>
                            <div class="mt-4 flex items-center text-sm text-green-400">
                                <TrendingUp class="w-4 h-4 mr-1" />
                                <span>+12.5% vs mês anterior</span>
                            </div>
                        </div>
                    </div>

                    <div class="bg-gray-800 p-6 rounded-2xl border border-gray-700 shadow-xl relative overflow-hidden group">
                        <div class="absolute right-[-20px] top-[-20px] w-24 h-24 bg-green-500/20 rounded-full blur-2xl group-hover:bg-green-500/30 transition-all"></div>
                        <div class="relative z-10">
                            <p class="text-gray-400 font-medium mb-1">Entradas</p>
                            <h3 class="text-3xl font-bold text-white">{{ formatCurrency(finance.summary.entradas) }}</h3>
                            <div class="mt-4 flex items-center text-sm text-gray-400">
                                <span>Total recebido</span>
                            </div>
                        </div>
                    </div>

                    <div class="bg-gray-800 p-6 rounded-2xl border border-gray-700 shadow-xl relative overflow-hidden group">
                        <div class="absolute right-[-20px] top-[-20px] w-24 h-24 bg-red-500/20 rounded-full blur-2xl group-hover:bg-red-500/30 transition-all"></div>
                        <div class="relative z-10">
                            <p class="text-gray-400 font-medium mb-1">Saídas</p>
                            <h3 class="text-3xl font-bold text-white">{{ formatCurrency(finance.summary.saidas) }}</h3>
                             <div class="mt-4 flex items-center text-sm text-gray-400">
                                <span>Total gasto</span>
                            </div>
                        </div>
                    </div>
                </div>

                <!-- Charts Section -->
                <div class="bg-gray-800 p-6 rounded-2xl border border-gray-700 shadow-xl">
                    <div class="flex justify-between items-center mb-6">
                        <h3 class="text-lg font-bold">Fluxo de Caixa</h3>
                        <select class="bg-gray-900 border border-gray-700 text-gray-300 text-sm rounded-lg p-2 focus:ring-blue-500">
                            <option>Últimos 6 meses</option>
                            <option>Este ano</option>
                        </select>
                    </div>
                    <FinanceChart />
                </div>

                <!-- Recent Transactions -->
                <div class="bg-gray-800 rounded-2xl border border-gray-700 shadow-xl overflow-hidden">
                    <div class="p-6 border-b border-gray-700">
                        <h3 class="text-lg font-bold">Transações Recentes</h3>
                    </div>
                    <div class="overflow-x-auto">
                        <table class="w-full text-left text-sm text-gray-400">
                            <thead class="bg-gray-900/50 text-xs uppercase font-medium">
                                <tr>
                                    <th class="px-6 py-4">Data</th>
                                    <th class="px-6 py-4">Descrição</th>
                                    <th class="px-6 py-4">Status</th>
                                    <th class="px-6 py-4 text-right">Valor</th>
                                </tr>
                            </thead>
                            <tbody class="divide-y divide-gray-700">
                                <tr v-for="item in finance.transactions" :key="item.id_lancamento" class="hover:bg-gray-700/30 transition-colors">
                                    <td class="px-6 py-4">{{ new Date(item.dt_dataLancamento).toLocaleDateString() }}</td>
                                    <td class="px-6 py-4 font-medium text-white">{{ item.nm_descricao }}</td>
                                    <td class="px-6 py-4">
                                        <span class="px-2.5 py-0.5 rounded-full text-xs font-medium border"
                                            :class="item.nm_tipo.includes('Entrada') 
                                                ? 'bg-green-500/10 text-green-400 border-green-500/20' 
                                                : 'bg-red-500/10 text-red-400 border-red-500/20'">
                                            {{ item.nm_tipo }}
                                        </span>
                                    </td>
                                    <td class="px-6 py-4 text-right font-bold"
                                        :class="item.nm_tipo.includes('Entrada') ? 'text-green-400' : 'text-red-400'">
                                        {{ item.nm_tipo.includes('Saída') ? '-' : '+' }} {{ formatCurrency(item.nr_valor) }}
                                    </td>
                                </tr>
                            </tbody>
                        </table>
                    </div>
                </div>
            </div>
        </main>
        
        <!-- Mobile Sidebar Overlay (Simple Implementation) -->
        <div v-if="isSidebarOpen" class="fixed inset-0 bg-black/50 z-40 md:hidden" @click="isSidebarOpen = false"></div>
        <aside v-if="isSidebarOpen" class="fixed inset-y-0 left-0 w-64 bg-gray-800 z-50 p-6 md:hidden shadow-2xl transition-transform">
             <div class="flex justify-between items-center mb-8">
                 <h2 class="text-xl font-bold">Menu</h2>
                 <button @click="isSidebarOpen = false"><X class="w-6 h-6" /></button>
             </div>
             <!-- Mobile Nav Links -->
              <nav class="space-y-4">
                <a href="#" class="block text-gray-300">Dashboard</a>
                <a href="#" class="block text-gray-300">Receitas</a>
                <a href="#" class="block text-gray-300">Despesas</a>
                <button @click="auth.logout" class="block text-red-400 mt-8">Sair</button>
            </nav>
        </aside>

    </div>
</template>
