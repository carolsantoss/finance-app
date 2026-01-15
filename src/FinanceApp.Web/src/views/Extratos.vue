<script setup lang="ts">
import { ref, onMounted, computed } from 'vue';
import { useRouter } from 'vue-router';
import { useFinanceStore } from '../stores/finance';
import { ArrowLeft, Trash2, Filter, Calendar, FileText, ArrowDownCircle, ArrowUpCircle, DollarSign, RefreshCw } from 'lucide-vue-next';

const router = useRouter();
const finance = useFinanceStore();

const currentYear = new Date().getFullYear();
const currentMonth = new Date().getMonth() + 1;

const filters = ref({
    month: currentMonth,
    year: currentYear,
    type: 'Todos'
});

const isProcessing = ref(false);

onMounted(() => {
    // In a real app, we might pass filters to the API. 
    // For now, we fetch all and filter client-side or assume store handles it.
    finance.fetchTransactions();
});

const filteredTransactions = computed(() => {
    return finance.transactions.filter(t => {
        const d = new Date(t.dt_dataLancamento);
        const matchMonth = d.getMonth() + 1 === filters.value.month;
        const matchYear = d.getFullYear() === filters.value.year;
        const matchType = filters.value.type === 'Todos' || t.nm_tipo === filters.value.type;
        return matchMonth && matchYear && matchType;
    });
});

const totalEntradas = computed(() => filteredTransactions.value.filter(t => t.nm_tipo === 'Entrada').reduce((acc, t) => acc + t.nr_valor, 0));
const totalSaidas = computed(() => filteredTransactions.value.filter(t => t.nm_tipo === 'Saída').reduce((acc, t) => acc + t.nr_valor, 0));
const saldo = computed(() => totalEntradas.value - totalSaidas.value);

const formatCurrency = (val: number) => new Intl.NumberFormat('pt-BR', { style: 'currency', currency: 'BRL' }).format(val);

const handleDelete = async (id: number) => {
    if(!confirm('Tem certeza que deseja excluir este lançamento?')) return;
    
    isProcessing.value = true;
    try {
        await finance.deleteTransaction(id);
        // Refresh? Store usually is reactive.
    } catch {
        alert('Erro ao excluir.');
    } finally {
        isProcessing.value = false;
    }
};
</script>

<template>
    <div class="flex flex-col h-full bg-[#121214] p-6 space-y-6 overflow-hidden">
        <!-- Summary Cards -->
        <div class="grid grid-cols-1 md:grid-cols-3 gap-4">
            <div class="bg-[#202024] p-6 rounded-md border border-[#323238] flex flex-col justify-between">
                <div class="flex items-center gap-2 text-gray-400 mb-2">
                    <ArrowDownCircle class="w-4 h-4" />
                    <span class="text-xs uppercase font-bold">Entradas do Mês</span>
                </div>
                <p class="text-2xl font-bold text-[#00B37E]">{{ formatCurrency(totalEntradas) }}</p>
            </div>
            <div class="bg-[#202024] p-6 rounded-md border border-[#323238] flex flex-col justify-between">
                 <div class="flex items-center gap-2 text-gray-400 mb-2">
                    <ArrowUpCircle class="w-4 h-4" />
                    <span class="text-xs uppercase font-bold">Saídas do Mês</span>
                </div>
                <p class="text-2xl font-bold text-[#F75A68]">{{ formatCurrency(totalSaidas) }}</p>
            </div>
             <div class="bg-[#202024] p-6 rounded-md border border-[#323238] flex flex-col justify-between">
                 <div class="flex items-center gap-2 text-gray-400 mb-2">
                    <DollarSign class="w-4 h-4" />
                    <span class="text-xs uppercase font-bold">Saldo do Mês</span>
                </div>
                <p class="text-2xl font-bold" :class="saldo >= 0 ? 'text-[#00B37E]' : 'text-[#F75A68]'">
                    {{ formatCurrency(saldo) }}
                </p>
            </div>
        </div>

        <!-- Filters -->
        <div class="bg-[#202024] p-4 rounded-md border border-[#323238] flex items-end gap-4">
            <div class="flex-1 space-y-1">
                <label class="text-xs text-gray-400">Tipo</label>
                <select v-model="filters.type" class="w-full bg-[#121214] border border-[#323238] rounded px-3 py-2 text-white focus:border-[#00875F] outline-none">
                    <option value="Todos">Todos</option>
                    <option value="Entrada">Entrada</option>
                    <option value="Saída">Saída</option>
                </select>
            </div>
             <div class="w-40 space-y-1">
                <label class="text-xs text-gray-400">Mês</label>
                <select v-model="filters.month" class="w-full bg-[#121214] border border-[#323238] rounded px-3 py-2 text-white focus:border-[#00875F] outline-none">
                    <option v-for="m in 12" :key="m" :value="m">{{ new Date(0, m-1).toLocaleString('pt-BR', { month: 'long' }) }}</option>
                </select>
            </div>
             <div class="w-32 space-y-1">
                <label class="text-xs text-gray-400">Ano</label>
                <input type="number" v-model="filters.year" class="w-full bg-[#121214] border border-[#323238] rounded px-3 py-2 text-white focus:border-[#00875F] outline-none" />
            </div>
            <button @click="finance.fetchTransactions()" class="bg-[#00875F] hover:bg-[#00B37E] text-white font-bold px-6 py-2 rounded transition-colors flex items-center gap-2 h-[42px]">
                <RefreshCw class="w-4 h-4" />
                Atualizar
            </button>
        </div>

        <!-- Table -->
        <div class="flex-1 overflow-auto bg-[#202024] rounded-md border border-[#323238]">
             <table class="w-full text-left text-sm text-gray-400">
                <thead class="bg-[#29292E] text-xs uppercase font-medium text-gray-300 sticky top-0">
                    <tr>
                        <th class="px-6 py-4">Data</th>
                        <th class="px-6 py-4">Descrição</th>
                        <th class="px-6 py-4 text-center">Tipo</th>
                        <th class="px-6 py-4">Pagamento</th>
                        <th class="px-6 py-4">Valor</th>
                        <th class="px-6 py-4 text-center">Parcelas</th>
                        <th class="px-6 py-4 text-center">Ações</th>
                    </tr>
                </thead>
                <tbody class="divide-y divide-[#323238]">
                    <tr v-if="filteredTransactions.length === 0">
                         <td colspan="7" class="px-6 py-8 text-center text-gray-500">Nenhum lançamento encontrado.</td>
                    </tr>
                    <tr v-for="item in filteredTransactions" :key="item.id_lancamento" class="hover:bg-[#29292E] transition-colors">
                        <td class="px-6 py-4 text-[#C4C4CC]">{{ new Date(item.dt_dataLancamento).toLocaleDateString() }}</td>
                        <td class="px-6 py-4 font-medium text-gray-100">{{ item.nm_descricao }}</td>
                        <td class="px-6 py-4 text-center">
                             <span class="px-2 py-1 rounded text-xs font-bold"
                                :class="item.nm_tipo === 'Entrada' ? 'bg-[#00B37E] text-white' : 'bg-[#F75A68] text-white'">
                                {{ item.nm_tipo }}
                            </span>
                        </td>
                         <td class="px-6 py-4 text-[#C4C4CC]">{{ item.nm_formaPagamento }}</td>
                        <td class="px-6 py-4 font-bold" :class="item.nm_tipo === 'Entrada' ? 'text-[#00B37E]' : 'text-[#F75A68]'">
                            {{ formatCurrency(item.nr_valor) }}
                        </td>
                         <td class="px-6 py-4 text-center text-[#C4C4CC]">
                            <span v-if="item.nm_formaPagamento === 'Crédito'">
                                {{ item.nr_parcelaInicial }}/{{ item.nr_parcelas }}
                            </span>
                            <span v-else>-</span>
                        </td>
                        <td class="px-6 py-4 text-center">
                             <button @click="handleDelete(item.id_lancamento)" class="text-gray-500 hover:text-red-400 transition-colors p-1" title="Excluir">
                                <Trash2 class="w-4 h-4" />
                            </button>
                        </td>
                    </tr>
                </tbody>
             </table>
        </div>
    </div>
</template>
