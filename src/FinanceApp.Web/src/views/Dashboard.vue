<script setup lang="ts">
import { onMounted, ref } from 'vue';
import { useAuthStore } from '../stores/auth';
import { useFinanceStore } from '../stores/finance';
import { 
    TrendingUp, 
    Plus
} from 'lucide-vue-next';
import FinanceChart from '../components/FinanceChart.vue';

const auth = useAuthStore();
const finance = useFinanceStore();

onMounted(() => {
  finance.fetchSummary();
  finance.fetchTransactions();
});

// Formatter
const formatCurrency = (value: number) => {
    return new Intl.NumberFormat('pt-BR', { style: 'currency', currency: 'BRL' }).format(value);
};
</script>

<template>
    <div class="h-full flex flex-col overflow-hidden">
        
        <!-- Scrollable Content -->
        <div class="flex-1 overflow-y-auto p-6 space-y-6">
            <!-- Welcome Section -->
            <div class="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4">
                <div>
                    <h2 class="text-2xl font-bold">Visão Geral</h2>
                    <p class="text-gray-400">Resumo financeiro dos últimos 30 dias.</p>
                </div>
                <router-link to="/lancamento" class="flex items-center gap-2 bg-[#00875F] hover:bg-[#00B37E] text-white px-5 py-2.5 rounded-lg shadow-lg shadow-[#00875F]/20 transition-all font-medium">
                    <Plus class="w-5 h-5" />
                    Nova Transação
                </router-link>
            </div>

            <!-- Cards Grid -->
            <div class="grid grid-cols-1 md:grid-cols-3 gap-6">
                <div class="bg-[#202024] p-6 rounded-2xl border border-[#323238] shadow-xl relative overflow-hidden group">
                    <div class="absolute right-[-20px] top-[-20px] w-24 h-24 bg-[#00875F]/10 rounded-full blur-2xl group-hover:bg-[#00875F]/20 transition-all"></div>
                    <div class="relative z-10">
                        <p class="text-gray-400 font-medium mb-1">Saldo Total</p>
                        <h3 class="text-3xl font-bold text-white">{{ formatCurrency(finance.summary.saldo) }}</h3>
                        <div class="mt-4 flex items-center text-sm" :class="finance.summary.saldo >= 0 ? 'text-[#00B37E]' : 'text-[#F75A68]'">
                            <TrendingUp class="w-4 h-4 mr-1" />
                            <span>{{ finance.summary.saldo >= 0 ? '+' : '' }}12.5% vs mês anterior</span>
                        </div>
                    </div>
                </div>

                <div class="bg-[#202024] p-6 rounded-2xl border border-[#323238] shadow-xl relative overflow-hidden group">
                    <div class="absolute right-[-20px] top-[-20px] w-24 h-24 bg-[#00875F]/10 rounded-full blur-2xl group-hover:bg-[#00875F]/20 transition-all"></div>
                    <div class="relative z-10">
                        <p class="text-gray-400 font-medium mb-1">Entradas</p>
                        <h3 class="text-3xl font-bold text-white">{{ formatCurrency(finance.summary.entradas) }}</h3>
                        <div class="mt-4 flex items-center text-sm text-gray-400">
                            <span>Total recebido</span>
                        </div>
                    </div>
                </div>

                <div class="bg-[#202024] p-6 rounded-2xl border border-[#323238] shadow-xl relative overflow-hidden group">
                    <div class="absolute right-[-20px] top-[-20px] w-24 h-24 bg-[#F75A68]/10 rounded-full blur-2xl group-hover:bg-[#F75A68]/20 transition-all"></div>
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
            <div class="bg-[#202024] p-6 rounded-2xl border border-[#323238] shadow-xl">
                <div class="flex justify-between items-center mb-6">
                    <h3 class="text-lg font-bold">Fluxo de Caixa</h3>
                    <select class="bg-[#121214] border border-[#323238] text-gray-300 text-sm rounded-lg p-2 focus:ring-[#00875F] focus:border-[#00875F] outline-none">
                        <option>Últimos 6 meses</option>
                        <option>Este ano</option>
                    </select>
                </div>
                <FinanceChart />
            </div>

            <!-- Recent Transactions -->
            <div class="bg-[#202024] rounded-2xl border border-[#323238] shadow-xl overflow-hidden">
                <div class="p-6 border-b border-[#323238] flex justify-between items-center">
                    <h3 class="text-lg font-bold">Transações Recentes</h3>
                    <router-link to="/extratos" class="text-sm text-[#00B37E] hover:text-[#00875F]">Ver todas</router-link>
                </div>
                <div class="overflow-x-auto">
                    <table class="w-full text-left text-sm text-gray-400">
                        <thead class="bg-[#29292E] text-xs uppercase font-medium">
                            <tr>
                                <th class="px-6 py-4">Data</th>
                                <th class="px-6 py-4">Descrição</th>
                                <th class="px-6 py-4">Status</th>
                                <th class="px-6 py-4 text-right">Valor</th>
                            </tr>
                        </thead>
                        <tbody class="divide-y divide-[#323238]">
                            <tr v-for="item in finance.transactions.slice(0, 5)" :key="item.id_lancamento" class="hover:bg-[#29292E] transition-colors">
                                <td class="px-6 py-4">{{ new Date(item.dt_dataLancamento).toLocaleDateString() }}</td>
                                <td class="px-6 py-4 font-medium text-white">{{ item.nm_descricao }}</td>
                                <td class="px-6 py-4">
                                    <span class="px-2.5 py-0.5 rounded-full text-xs font-medium border"
                                        :class="item.nm_tipo.includes('Entrada') 
                                            ? 'bg-[#00875F]/10 text-[#00B37E] border-[#00875F]/20' 
                                            : 'bg-[#F75A68]/10 text-[#F75A68] border-[#F75A68]/20'">
                                        {{ item.nm_tipo }}
                                    </span>
                                </td>
                                <td class="px-6 py-4 text-right font-bold"
                                    :class="item.nm_tipo.includes('Entrada') ? 'text-[#00B37E]' : 'text-[#F75A68]'">
                                    {{ item.nm_tipo.includes('Saída') ? '-' : '+' }} {{ formatCurrency(item.nr_valor) }}
                                </td>
                            </tr>
                        </tbody>
                    </table>
                </div>
            </div>
        </div>
    </div>
</template>
