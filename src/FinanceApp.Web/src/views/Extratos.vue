<script setup lang="ts">
import { ref, onMounted, computed } from 'vue';
import { useRouter } from 'vue-router';
import { useCategoryStore } from '../stores/category';
import { useWalletStore } from '../stores/wallet';
import { useFinanceStore } from '../stores/finance';
import { usePrivacyStore } from '../stores/privacy';
import { ArrowLeft, Trash2, Filter, Calendar, FileText, ArrowDownCircle, ArrowUpCircle, DollarSign, RefreshCw, Upload } from 'lucide-vue-next';
import ImportTransactionModal from '../components/ImportTransactionModal.vue';

const router = useRouter();
const finance = useFinanceStore();
const categoryStore = useCategoryStore();
const walletStore = useWalletStore();
const privacyStore = usePrivacyStore();

const currentYear = new Date().getFullYear();
const currentMonth = new Date().getMonth() + 1;

const filters = ref({
    month: currentMonth,
    year: currentYear,
    type: 'Todos',
    categoryId: null as number | null,
    walletId: null as number | null,
    creditCardId: null as number | null // Added creditCardId
});

const isProcessing = ref(false);
const showImportModal = ref(false);

onMounted(async () => {
    await Promise.all([
        finance.fetchTransactions(),
        categoryStore.fetchCategories(),
        walletStore.fetchAll()
    ]);
});

const filteredTransactions = computed(() => {
    const { month, year, type, categoryId, walletId, creditCardId } = filters.value;
    const result: any[] = [];

    finance.transactions.forEach(t => {
        // Base filters that don't depend on date (Optimization)
        if (type !== 'Todos' && t.tipo !== type) return;
        if (categoryId && t.id_categoria !== categoryId) return;
        if (walletId && t.id_wallet !== walletId) return;
        if (creditCardId && t.id_credit_card !== creditCardId) return;

        // Installment Logic
        const isInstallment = t.parcelas > 1 && t.formaPagamento === 'Crédito'; // Assuming only Credit has installments
        const startParcel = t.parcelaInicial || 1;
        const totalParcels = t.parcelas || 1;
        
        // If it's an installment, we check if any parcel falls in the current view
        if (isInstallment) {
            const startDate = new Date(t.data);
            const installmentValue = t.valor / totalParcels;

            // We need to find if the CURRENT view month/year corresponds to one of the parcels.
            // Calculate the month difference between View Month and Start Date
            // ViewDate (1st of month)
            const viewDate = new Date(year, month - 1, 1);
            
            // To handle days, let's strictly compare Month/Year indices.
            // Index = Year * 12 + Month
            const viewIndex = year * 12 + (month - 1);
            const startIndex = startDate.getFullYear() * 12 + startDate.getMonth();
            
            // The offset from start: 0 means same month, 1 means next month...
            const diffMonths = viewIndex - startIndex;

            // If diffMonths is negative, the view is BEFORE the purchase -> Don't show (unless backdated? assuming start date matches parcel 1 implicitly)
            // If diffMonths >= 0.
            // The parcel number corresponding to this month is: startParcel + diffMonths.
            
            const currentParcel = startParcel + diffMonths;

            if (currentParcel >= startParcel && currentParcel <= totalParcels) {
                // Determine the exact date for this installment in this month
                // Ideally keep the same Day of month, or clamp to last day.
                // But for display in "Extratos", usually just visual "Data" is fine.
                // Let's project the date to the current view month.
                // Only issue: If original was Jan 31, and current is Feb. Feb 28?
                // Simple approach: Set date to same day, or max day of month.
                const projectedDate = new Date(year, month - 1, Math.min(startDate.getDate(), new Date(year, month, 0).getDate()));

                result.push({
                    ...t,
                    id: t.id_lancamento, // Ensure ID is preserved for deletion
                    // Virtual overrides
                    data: projectedDate.toISOString(),
                    valor: installmentValue,
                    parcelaAtual: currentParcel,
                    isVirtual: true // Marker if needed
                });
            }

        } else {
            // Standard Transaction
            const d = new Date(t.data);
            if (d.getMonth() + 1 === month && d.getFullYear() === year) {
                 result.push({
                     ...t,
                     id: t.id_lancamento
                 });
            }
        }
    });

    return result.sort((a, b) => new Date(b.data).getTime() - new Date(a.data).getTime());
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
    <div class="flex flex-col h-full bg-app p-6 space-y-6 overflow-hidden">
        <!-- Summary Cards -->
        <div class="grid grid-cols-1 md:grid-cols-3 gap-4">
            <div class="bg-card p-6 rounded-md border border-border flex flex-col justify-between">
                <div class="flex items-center gap-2 text-text-secondary mb-2">
                    <ArrowDownCircle class="w-4 h-4" />
                    <span class="text-xs uppercase font-bold">Entradas do Mês</span>
                </div>
                <p class="text-2xl font-bold text-[#00B37E]" :class="{ 'blur-md': privacyStore.isHidden }">{{ formatCurrency(totalEntradas) }}</p>
            </div>
            <div class="bg-card p-6 rounded-md border border-border flex flex-col justify-between">
                 <div class="flex items-center gap-2 text-text-secondary mb-2">
                    <ArrowUpCircle class="w-4 h-4" />
                    <span class="text-xs uppercase font-bold">Saídas do Mês</span>
                </div>
                <p class="text-2xl font-bold text-[#F75A68]" :class="{ 'blur-md': privacyStore.isHidden }">{{ formatCurrency(totalSaidas) }}</p>
            </div>
             <div class="bg-card p-6 rounded-md border border-border flex flex-col justify-between">
                 <div class="flex items-center gap-2 text-text-secondary mb-2">
                    <DollarSign class="w-4 h-4" />
                    <span class="text-xs uppercase font-bold">Saldo do Mês</span>
                </div>
                <p class="text-2xl font-bold" :class="[saldo >= 0 ? 'text-[#00B37E]' : 'text-[#F75A68]', { 'blur-md': privacyStore.isHidden }]">
                    {{ formatCurrency(saldo) }}
                </p>
            </div>
        </div>

        <!-- Filters -->
        <div class="bg-card p-4 rounded-md border border-border flex flex-wrap items-end gap-4">
            <div class="flex-1 min-w-[150px] space-y-1">
                <label class="text-xs text-text-secondary">Tipo</label>
                <select v-model="filters.type" class="w-full bg-input border border-border rounded px-3 py-2 text-text-primary focus:border-brand outline-none">
                    <option value="Todos">Todos</option>
                    <option value="Entrada">Entrada</option>
                    <option value="Saída">Saída</option>
                </select>
            </div>
             <div class="w-40 space-y-1">
                <label class="text-xs text-text-secondary">Mês</label>
                <select v-model="filters.month" class="w-full bg-input border border-border rounded px-3 py-2 text-text-primary focus:border-brand outline-none">
                    <option v-for="m in 12" :key="m" :value="m">{{ new Date(0, m-1).toLocaleString('pt-BR', { month: 'long' }) }}</option>
                </select>
            </div>
             <div class="w-32 space-y-1">
                <label class="text-xs text-text-secondary">Ano</label>
                <input type="number" v-model="filters.year" class="w-full bg-input border border-border rounded px-3 py-2 text-text-primary focus:border-brand outline-none" />
            </div>

            <!-- Advanced Filters -->
             <div class="flex-1 min-w-[200px] space-y-1">
                <label class="text-xs text-text-secondary">Categoria</label>
                <select v-model="filters.categoryId" class="w-full bg-input border border-border rounded px-3 py-2 text-text-primary focus:border-brand outline-none">
                    <option :value="null">Todas</option>
                    <option v-for="cat in categoryStore.categories" :key="cat.id_categoria" :value="cat.id_categoria">{{ cat.nm_nome }}</option>
                </select>
            </div>
            <div class="flex-1 min-w-[200px] space-y-1">
                <label class="text-xs text-text-secondary">Carteira</label>
                <select v-model="filters.walletId" class="w-full bg-input border border-border rounded px-3 py-2 text-text-primary focus:border-brand outline-none">
                    <option :value="null">Todas</option>
                    <option v-for="w in walletStore.wallets" :key="w.id_wallet" :value="w.id_wallet">{{ w.nm_nome }}</option>
                </select>
            </div>
             <div class="flex-1 min-w-[200px] space-y-1">
                <label class="text-xs text-text-secondary">Cartão de Crédito</label>
                <select v-model="filters.creditCardId" class="w-full bg-input border border-border rounded px-3 py-2 text-text-primary focus:border-brand outline-none">
                    <option :value="null">Todos</option>
                    <option v-for="c in walletStore.creditCards" :key="c.id_credit_card" :value="c.id_credit_card">{{ c.nm_nome }}</option>
                </select>
            </div>

            <button @click="showImportModal = true" class="bg-brand hover:bg-brand-hover text-white font-bold px-4 py-2 rounded transition-colors flex items-center gap-2 h-[42px]" title="Importar Extrato">
                <Upload class="w-4 h-4" />
                <span class="hidden md:inline">Importar</span>
            </button>

            <button @click="finance.fetchTransactions()" class="bg-hover hover:bg-opacity-80 text-text-primary font-bold px-4 py-2 rounded transition-colors flex items-center gap-2 h-[42px]" title="Recarregar">
                <RefreshCw class="w-4 h-4" />
            </button>
        </div>

        <!-- Table -->
        <div class="flex-1 overflow-auto bg-card rounded-md border border-border">
             <table class="w-full text-left text-sm text-text-secondary">
                <thead class="bg-hover text-xs uppercase font-medium text-text-secondary sticky top-0">
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
                <tbody class="divide-y divide-border">
                    <tr v-if="filteredTransactions.length === 0">
                         <td colspan="7" class="px-6 py-8 text-center text-text-tertiary">Nenhum lançamento encontrado.</td>
                    </tr>
                    <tr v-for="item in filteredTransactions" :key="item.id" class="hover:bg-hover transition-colors">
                        <td class="px-6 py-4 text-text-secondary">{{ new Date(item.data).toLocaleDateString() }}</td>
                        <td class="px-6 py-4 font-medium text-text-primary">{{ item.descricao }}</td>
                        <td class="px-6 py-4 text-center">
                             <span class="px-2 py-1 rounded text-xs font-bold"
                                :class="item.tipo === 'Entrada' ? 'bg-[#00B37E] text-white' : 'bg-[#F75A68] text-white'">
                                {{ item.tipo }}
                            </span>
                        </td>
                         <td class="px-6 py-4 text-text-secondary">{{ item.formaPagamento }}</td>
                        <td class="px-6 py-4 font-bold" :class="[item.tipo === 'Entrada' ? 'text-[#00B37E]' : 'text-[#F75A68]', { 'blur-sm select-none': privacyStore.isHidden }]">
                            {{ formatCurrency(item.valor) }}
                        </td>
                         <td class="px-6 py-4 text-center text-text-tertiary">
                            <span v-if="item.formaPagamento === 'Crédito' && item.parcelas > 1">
                                {{ item.parcelaAtual || 1 }}/{{ item.parcelas }}
                            </span>
                            <span v-else-if="item.formaPagamento === 'Crédito'"> 
                                à vista
                            </span>
                            <span v-else>-</span>
                        </td>
                        <td class="px-6 py-4 text-center">
                             <button @click="handleDelete(item.id)" class="text-text-tertiary hover:text-danger transition-colors p-1" title="Excluir">
                                <Trash2 class="w-4 h-4" />
                            </button>
                        </td>
                    </tr>
                </tbody>
             </table>
        </div>
    </div>

    <ImportTransactionModal 
        :is-open="showImportModal" 
        @close="showImportModal = false"
        @success="finance.fetchTransactions()" 
    />
</template>
