<script setup lang="ts">
import { ref, onMounted, h } from 'vue';
import { usePlanningStore } from '../stores/planning';
import { useCategoryStore } from '../stores/category';
import { useWalletStore } from '../stores/wallet';
import { Plus, Trash2, Edit2, RefreshCw, Calendar } from 'lucide-vue-next';

// ... (keep constants)
const planning = usePlanningStore();
const categoryStore = useCategoryStore();
const walletStore = useWalletStore();

const isModalOpen = ref(false);
const editingItem = ref<any>(null);

const form = ref({
    descricao: '',
    valor: '',
    tipo: 'Saída',
    categoryId: '',
    walletId: '',
    creditCardId: '',
    frequencia: 'Mensal',
    dataInicio: new Date().toISOString().split('T')[0],
    dataFim: '',
    ativo: true
});

onMounted(async () => {
    await Promise.all([
        planning.fetchRecurringTransactions(),
        categoryStore.fetchCategories(),
        walletStore.fetchAll()
    ]);
});

// ... (keep openModal, save, toggleStatus, formatCurrency) 


const openModal = (item: any = null) => {
    editingItem.value = item;
    if (item) {
        form.value = {
            descricao: item.descricao,
            valor: item.valor.toString(),
            tipo: item.tipo,
            categoryId: item.categoryId,
            walletId: item.walletId,
            creditCardId: item.creditCardId,
            frequencia: item.frequencia,
            dataInicio: item.dataInicio.split('T')[0],
            dataFim: item.dataFim ? item.dataFim.split('T')[0] : '',
            ativo: item.ativo
        };
    } else {
        form.value = {
            descricao: '',
            valor: '',
            tipo: 'Saída',
            categoryId: '',
            walletId: '',
            creditCardId: '',
            frequencia: 'Mensal',
            dataInicio: new Date().toISOString().split('T')[0],
            dataFim: '',
            ativo: true
        };
    }
    isModalOpen.value = true;
};

const save = async () => {
    if (!form.value.descricao || !form.value.valor) return;

    const payload = {
        descricao: form.value.descricao,
        valor: parseFloat(form.value.valor),
        tipo: form.value.tipo,
        categoryId: form.value.categoryId || null,
        walletId: form.value.walletId || null,
        creditCardId: form.value.creditCardId || null,
        frequencia: form.value.frequencia,
        dataInicio: new Date(form.value.dataInicio),
        dataFim: form.value.dataFim ? new Date(form.value.dataFim) : null,
        ativo: form.value.ativo
    };

    if (editingItem.value) {
        await planning.updateRecurringTransaction(editingItem.value.id, payload);
    } else {
        await planning.createRecurringTransaction(payload);
    }
    isModalOpen.value = false;
};

const toggleStatus = async (item: any) => {
    // Only update the status
    await planning.updateRecurringTransaction(item.id, {
        ...item,
        ativo: !item.ativo,
        categoryId: item.categoryId || null, // Ensure explicit null
        walletId: item.walletId || null,
        creditCardId: item.creditCardId || null,
        dataInicio: new Date(item.dataInicio),
        dataFim: item.dataFim ? new Date(item.dataFim) : null
    });
};

const formatCurrency = (value: number) => {
    return new Intl.NumberFormat('pt-BR', { style: 'currency', currency: 'BRL' }).format(value);
};
</script>

<template>
    <div class="p-6 h-full overflow-y-auto space-y-6">
        <div class="flex justify-between items-center">
            <div>
                <h2 class="text-2xl font-bold">Lançamentos Recorrentes</h2>
                <p class="text-gray-400">Automatize suas contas a pagar e receber.</p>
            </div>
            <button @click="openModal()" class="bg-[#00875F] hover:bg-[#00B37E] text-white px-4 py-2 rounded-lg flex items-center gap-2">
                <Plus class="w-5 h-5" /> Novo
            </button>
        </div>

        <div v-if="planning.isLoading" class="text-center py-10">
            <p class="text-gray-400">Carregando...</p>
        </div>

        <div v-else class="space-y-4">
             <div v-for="item in planning.recurringTransactions" :key="item.id" class="bg-[#202024] p-4 rounded-xl border border-[#323238] flex flex-col md:flex-row justify-between items-center gap-4 transition-all hover:border-[#00875F]/50">
                <div class="flex items-center gap-4 flex-1">
                    <div class="p-3 rounded-lg" :class="item.ativo ? 'bg-[#00875F]/10 text-[#00875F]' : 'bg-[#29292E] text-gray-500'">
                        <RefreshCw class="w-6 h-6" />
                    </div>
                    <div>
                        <h3 class="font-bold flex items-center gap-2">
                             {{ item.descricao }}
                             <span v-if="!item.ativo" class="text-xs bg-[#29292E] px-2 py-0.5 rounded text-gray-500">Inativo</span>
                        </h3>
                        <div class="text-sm text-gray-400 flex flex-wrap gap-2 items-center mt-1">
                             <span class="flex items-center gap-1"><Calendar class="w-3 h-3" /> {{ item.frequencia }}</span>
                             <span v-if="item.categoryName" class="px-2 py-0.5 bg-[#121214] rounded-full text-xs">{{ item.categoryName }}</span>
                        </div>
                    </div>
                </div>

                <div class="text-right flex flex-col items-end">
                    <span class="font-bold" :class="item.tipo === 'Entrada' ? 'text-[#00B37E]' : 'text-[#F75A68]'">
                        {{ item.tipo === 'Entrada' ? '+' : '-' }} {{ formatCurrency(item.valor) }}
                    </span>
                    <span class="text-xs text-gray-500">Próx: {{ item.ultimaProcessamento ? 'Processado' : 'Aguardando' }}</span>
                </div>

                <div class="flex gap-2 border-l border-[#323238] pl-4">
                    <button @click="toggleStatus(item)" class="p-2 hover:bg-[#29292E] rounded text-gray-400" :class="item.ativo ? 'hover:text-[#F75A68]' : 'hover:text-[#00B37E]'" title="Ativar/Desativar">
                        <component :is="item.ativo ? (props: any) => h(Trash2, props) : (props: any) => h(RefreshCw, props)" class="w-4 h-4" /> 
                         <!-- Simplification for toggle icon: usually a power button or pause -->
                         <div class="text-xs font-bold">{{ item.ativo ? 'Desativar' : 'Ativar' }}</div>
                    </button>
                     <button @click="openModal(item)" class="p-2 hover:bg-[#29292E] rounded text-gray-400 hover:text-white" title="Editar">
                        <Edit2 class="w-4 h-4" />
                    </button>
                </div>
            </div>
             <div v-if="planning.recurringTransactions.length === 0" class="text-center py-10 bg-[#202024] rounded-2xl border border-[#323238]">
                <p class="text-gray-400">Nenhuma transação recorrente configurada.</p>
            </div>
        </div>

        <!-- Modal -->
         <div v-if="isModalOpen" class="fixed inset-0 bg-black/50 flex items-center justify-center z-50 p-4">
            <div class="bg-[#202024] rounded-2xl p-6 w-full max-w-lg border border-[#323238] shadow-2xl max-h-[90vh] overflow-y-auto">
                <h3 class="text-xl font-bold mb-6">{{ editingItem ? 'Editar Recorrência' : 'Nova Recorrência' }}</h3>
                
                <div class="space-y-4">
                    <div class="grid grid-cols-2 gap-4">
                        <div class="col-span-2">
                             <label class="block text-sm font-medium text-gray-400 mb-1">Descrição</label>
                             <input v-model="form.descricao" type="text" class="w-full bg-[#121214] border border-[#323238] rounded-lg p-3 text-white focus:border-[#00875F] outline-none">
                        </div>
                        <div>
                            <label class="block text-sm font-medium text-gray-400 mb-1">Valor (R$)</label>
                            <input v-model="form.valor" type="number" step="0.01" class="w-full bg-[#121214] border border-[#323238] rounded-lg p-3 text-white focus:border-[#00875F] outline-none">
                        </div>
                        <div>
                             <label class="block text-sm font-medium text-gray-400 mb-1">Tipo</label>
                             <div class="flex bg-[#121214] rounded-lg p-1 border border-[#323238]">
                                <button @click="form.tipo = 'Entrada'" class="flex-1 py-2 rounded-md transition-colors text-sm font-medium" :class="form.tipo === 'Entrada' ? 'bg-[#00875F] text-white' : 'text-gray-400 hover:text-white'">Entrada</button>
                                <button @click="form.tipo = 'Saída'" class="flex-1 py-2 rounded-md transition-colors text-sm font-medium" :class="form.tipo === 'Saída' ? 'bg-[#F75A68] text-white' : 'text-gray-400 hover:text-white'">Saída</button>
                             </div>
                        </div>
                    </div>

                    <div class="grid grid-cols-2 gap-4">
                        <div>
                            <label class="block text-sm font-medium text-gray-400 mb-1">Frequência</label>
                             <select v-model="form.frequencia" class="w-full bg-[#121214] border border-[#323238] rounded-lg p-3 text-white focus:border-[#00875F] outline-none">
                                <option>Diário</option>
                                <option>Semanal</option>
                                <option>Mensal</option>
                                <option>Anual</option>
                            </select>
                        </div>
                        <div>
                            <label class="block text-sm font-medium text-gray-400 mb-1">Início</label>
                            <input v-model="form.dataInicio" type="date" class="w-full bg-[#121214] border border-[#323238] rounded-lg p-3 text-white focus:border-[#00875F] outline-none">
                        </div>
                    </div>

                    <div>
                        <label class="block text-sm font-medium text-gray-400 mb-1">Categoria</label>
                         <select v-model="form.categoryId" class="w-full bg-[#121214] border border-[#323238] rounded-lg p-3 text-white focus:border-[#00875F] outline-none">
                            <option value="">Sem categoria</option>
                            <option v-for="cat in categoryStore.categories.filter(c => c.nm_tipo === form.tipo)" :key="cat.id_categoria" :value="cat.id_categoria">{{ cat.nm_nome }}</option>
                        </select>
                    </div>

                     <!-- Optional: Wallet/Card selection -->
                     <div class="grid grid-cols-2 gap-4" v-if="form.tipo === 'Saída'">
                         <div>
                            <label class="block text-sm font-medium text-gray-400 mb-1">Cartão de Crédito</label>
                            <select v-model="form.creditCardId" class="w-full bg-[#121214] border border-[#323238] rounded-lg p-3 text-white focus:border-[#00875F] outline-none">
                                <option value="">Nenhum</option>
                                <option v-for="card in walletStore.creditCards" :key="card.id_credit_card" :value="card.id_credit_card">{{ card.nm_nome }}</option>
                            </select>
                        </div>
                         <div>
                            <label class="block text-sm font-medium text-gray-400 mb-1">Carteira</label>
                            <select v-model="form.walletId" class="w-full bg-[#121214] border border-[#323238] rounded-lg p-3 text-white focus:border-[#00875F] outline-none">
                                <option value="">Nenhuma</option>
                                <option v-for="w in walletStore.wallets" :key="w.id_wallet" :value="w.id_wallet">{{ w.nm_nome }}</option>
                            </select>
                        </div>
                     </div>
                </div>

                <div class="flex gap-3 mt-8">
                    <button @click="isModalOpen = false" class="flex-1 px-4 py-3 rounded-lg bg-[#29292E] hover:bg-[#323238] text-white transition-colors">Cancelar</button>
                    <button @click="save" class="flex-1 px-4 py-3 rounded-lg bg-[#00875F] hover:bg-[#00B37E] text-white font-bold transition-colors">Salvar</button>
                </div>
            </div>
        </div>
    </div>
</template>
