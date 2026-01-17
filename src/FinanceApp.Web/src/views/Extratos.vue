<script setup lang="ts">
import { ref, onMounted, computed } from 'vue';
import { useRouter } from 'vue-router';
import { useCategoryStore } from '../stores/category';
import { useWalletStore } from '../stores/wallet';
import { useFinanceStore } from '../stores/finance';
import { ArrowLeft, Trash2, Filter, Calendar, FileText, ArrowDownCircle, ArrowUpCircle, DollarSign, RefreshCw } from 'lucide-vue-next';

const router = useRouter();
const finance = useFinanceStore();
const categoryStore = useCategoryStore();
const walletStore = useWalletStore();

const currentYear = new Date().getFullYear();
const currentMonth = new Date().getMonth() + 1;

const filters = ref({
    month: currentMonth,
    year: currentYear,
    type: 'Todos',
    categoryId: null as number | null,
    walletId: null as number | null
});

const isProcessing = ref(false);

onMounted(async () => {
    await Promise.all([
        finance.fetchTransactions(),
        categoryStore.fetchCategories(),
        walletStore.fetchAll()
    ]);
});

const filteredTransactions = computed(() => {
    return finance.transactions.filter(t => {
        const d = new Date(t.data);
        const matchMonth = d.getMonth() + 1 === filters.value.month;
        const matchYear = d.getFullYear() === filters.value.year;
        const matchType = filters.value.type === 'Todos' || t.tipo === filters.value.type;
        const matchCategory = !filters.value.categoryId || t.categoryId === filters.value.categoryId;
        const matchWallet = !filters.value.walletId || t.walletId === filters.value.walletId;
        
        return matchMonth && matchYear && matchType && matchCategory && matchWallet;
    });
});

const totalEntradas = computed(() => filteredTransactions.value.filter(t => t.tipo === 'Entrada').reduce((acc, t) => acc + t.valor, 0));
const totalSaidas = computed(() => filteredTransactions.value.filter(t => t.tipo === 'Saída').reduce((acc, t) => acc + t.valor, 0));
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
        <div class="bg-[#202024] p-4 rounded-md border border-[#323238] flex flex-wrap items-end gap-4">
            <div class="flex-1 min-w-[150px] space-y-1">
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

            <!-- Advanced Filters -->
             <div class="flex-1 min-w-[200px] space-y-1">
                <label class="text-xs text-gray-400">Categoria</label>
                <select v-model="filters.categoryId" class="w-full bg-[#121214] border border-[#323238] rounded px-3 py-2 text-white focus:border-[#00875F] outline-none">
                    <option :value="null">Todas</option>
                    <option v-for="cat in categoryStore.categories" :key="cat.id_categoria" :value="cat.id_categoria">{{ cat.nm_nome }}</option>
                </select>
            </div>
             <div class="flex-1 min-w-[200px] space-y-1">
                <label class="text-xs text-gray-400">Carteira</label>
                <select v-model="filters.walletId" class="w-full bg-[#121214] border border-[#323238] rounded px-3 py-2 text-white focus:border-[#00875F] outline-none">
                    <option :value="null">Todas</option>
                    <option v-for="w in walletStore.wallets" :key="w.id_wallet" :value="w.id_wallet">{{ w.nm_nome }}</option>
                </select>
            </div>

            <button @click="finance.fetchTransactions()" class="bg-[#29292E] hover:bg-[#323238] text-white font-bold px-4 py-2 rounded transition-colors flex items-center gap-2 h-[42px]" title="Recarregar">
                <RefreshCw class="w-4 h-4" />
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
                    <tr v-for="item in filteredTransactions" :key="item.id" class="hover:bg-[#29292E] transition-colors">
                        <td class="px-6 py-4 text-[#C4C4CC]">{{ new Date(item.data).toLocaleDateString() }}</td>
                        <td class="px-6 py-4 font-medium text-gray-100">{{ item.descricao }}</td>
                        <td class="px-6 py-4 text-center">
                             <span class="px-2 py-1 rounded text-xs font-bold"
                                :class="item.tipo === 'Entrada' ? 'bg-[#00B37E] text-white' : 'bg-[#F75A68] text-white'">
                                {{ item.tipo }}
                            </span>
                        </td>
                         <td class="px-6 py-4 text-[#C4C4CC]">{{ item.formaPagamento }}</td>
                        <td class="px-6 py-4 font-bold" :class="item.tipo === 'Entrada' ? 'text-[#00B37E]' : 'text-[#F75A68]'">
                            {{ formatCurrency(item.valor) }}
                        </td>
                         <td class="px-6 py-4 text-center text-[#C4C4CC]">
                            <span v-if="item.formaPagamento === 'Crédito'">
                                {{ item.parcelas }}x
                            </span>
                            <span v-else>-</span>
                        </td>
                        <td class="px-6 py-4 text-center">
                             <button @click="handleDelete(item.id)" class="text-gray-500 hover:text-red-400 transition-colors p-1" title="Excluir">
                                <Trash2 class="w-4 h-4" />
                            </button>
                        </td>
                    </tr>
                </tbody>
             </table>
        </div>
    </div>
</template>
