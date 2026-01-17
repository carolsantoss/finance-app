<script setup lang="ts">
import { ref, computed, onMounted, watch } from 'vue';
import { useFinanceStore } from '../stores/finance';
import { useCategoryStore } from '../stores/category';
import { useWalletStore } from '../stores/wallet';
import { useToastStore } from '../stores/toast';
import { X, DollarSign, Layers, Wallet, CreditCard } from 'lucide-vue-next';
import * as LucideIcons from 'lucide-vue-next';

const emit = defineEmits(['close', 'success']);
const finance = useFinanceStore();
const categoryStore = useCategoryStore();
const walletStore = useWalletStore();
const toast = useToastStore();

const form = ref({
    nm_tipo: 'Saída',
    nm_descricao: '',
    nr_valor: 0,
    dt_dataLancamento: new Date().toISOString().split('T')[0],
    nm_formaPagamento: 'Débito',
    nr_parcelas: 1,
    nr_parcelaInicial: 1,
    id_categoria: null as number | null,
    id_wallet: null as number | null,
    id_credit_card: null as number | null
});

const isProcessing = ref(false);
const isCredit = computed(() => form.value.nm_formaPagamento === 'Crédito');

// If Debit -> Show Wallets. If Credit -> Show Cards.
const showWallets = computed(() => form.value.nm_formaPagamento === 'Débito');

// Watcher to auto-select first option if available to improve UX
watch(() => form.value.nm_formaPagamento, (val) => {
    if (val === 'Débito' && walletStore.wallets.length > 0) {
        form.value.id_wallet = walletStore.wallets[0].id_wallet;
        form.value.id_credit_card = null;
    } else if (val === 'Crédito' && walletStore.creditCards.length > 0) {
        form.value.id_credit_card = walletStore.creditCards[0].id_credit_card;
        form.value.id_wallet = null;
    }
});

const filteredCategories = computed(() => {
    return categoryStore.categories.filter(c => c.nm_tipo === form.value.nm_tipo);
});

const getIcon = (name: string) => {
    // @ts-ignore
    return LucideIcons[name] || LucideIcons.Tag;
};

// Currency Mask Logic
const valorDisplay = ref('');

const formatCurrencyInput = (event: Event) => {
    const input = event.target as HTMLInputElement;
    let value = input.value.replace(/\D/g, '');
    
    if (!value) {
        form.value.nr_valor = 0;
        valorDisplay.value = '';
        return;
    }

    const numericValue = parseInt(value) / 100;
    form.value.nr_valor = numericValue;
    
    valorDisplay.value = new Intl.NumberFormat('pt-BR', {
        style: 'currency',
        currency: 'BRL'
    }).format(numericValue);
};

onMounted(async () => {
    categoryStore.fetchCategories();
    await walletStore.fetchAll();
    
    // Auto-select first wallet if exists
    if (walletStore.wallets.length > 0) {
        form.value.id_wallet = walletStore.wallets[0].id_wallet;
    }
});

const handleSubmit = async () => {
    if (!form.value.id_categoria) {
        toast.error('Selecione uma categoria.');
        return;
    }

    // Validation for wallet removed as per user request

    if (isCredit.value && !form.value.id_credit_card) {
        toast.error('Selecione um cartão de crédito.');
        return;
    }

    isProcessing.value = true;
    try {
        const payload = {
            tipo: form.value.nm_tipo,
            descricao: form.value.nm_descricao,
            valor: form.value.nr_valor,
            data: form.value.dt_dataLancamento,
            formaPagamento: form.value.nm_formaPagamento,
            parcelas: form.value.nr_parcelas,
            parcelasPagas: 0,
            id_categoria: form.value.id_categoria,
            id_wallet: form.value.id_wallet,
            id_credit_card: form.value.id_credit_card
        };
        await finance.addTransaction(payload);
        toast.success('Lançamento salvo com sucesso!');
        emit('success');
        emit('close');
        
        // Reset specific fields but keep others for UX
        form.value.nr_valor = 0;
        valorDisplay.value = '';
        form.value.nm_descricao = '';
        form.value.id_categoria = null;
    } catch (error: any) {
        if (error.response?.data?.errors) {
            const errors = error.response.data.errors;
            Object.keys(errors).forEach(key => {
                const messages = errors[key];
                messages.forEach((msg: string) => toast.error(`${msg}`));
            });
        } else {
            toast.error(error.response?.data?.title || 'Erro ao salvar lançamento.');
        }
    } finally {
        isProcessing.value = false;
    }
};

const close = () => {
    emit('close');
};
</script>

<template>
    <div class="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/80 backdrop-blur-sm animate-in fade-in duration-200">
        <div class="w-full max-w-2xl bg-[#202024] rounded-2xl shadow-2xl border border-[#323238] overflow-hidden animate-in zoom-in-95 duration-200 relative max-h-[90vh] flex flex-col">
            
            <button @click="close" class="absolute top-4 right-4 p-2 text-gray-400 hover:text-white rounded-lg hover:bg-[#323238] transition-colors z-10">
                <X class="w-5 h-5" />
            </button>

            <div class="p-8 pb-0 shrink-0">
                <div class="flex items-center gap-4 mb-6">
                    <div class="p-2 bg-[#121214] border border-[#323238] rounded-full">
                            <DollarSign class="w-6 h-6 text-white" />
                    </div>
                    <div>
                        <h2 class="text-xl font-bold text-white">Novo Lançamento</h2>
                        <p class="text-sm text-gray-400">Registre suas entradas e saídas</p>
                    </div>
                </div>
            </div>

            <div class="p-8 pt-0 overflow-y-auto custom-scrollbar">
                <form @submit.prevent="handleSubmit" class="space-y-6">
                    <!-- Row 1: Type & Description -->
                    <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
                        <div class="space-y-2">
                                <label class="text-xs font-medium text-gray-400 uppercase">Tipo</label>
                                <div class="flex rounded-md bg-[#121214] border border-[#323238] p-1">
                                    <button 
                                        type="button" 
                                        @click="form.nm_tipo = 'Entrada'; form.id_categoria = null;"
                                        class="flex-1 py-2 text-sm font-medium rounded transition-all"
                                        :class="form.nm_tipo === 'Entrada' ? 'bg-[#00875F] text-white' : 'text-gray-400 hover:text-white'"
                                    >Entrada</button>
                                    <button 
                                        type="button" 
                                        @click="form.nm_tipo = 'Saída'; form.id_categoria = null;"
                                        class="flex-1 py-2 text-sm font-medium rounded transition-all"
                                        :class="form.nm_tipo === 'Saída' ? 'bg-[#F75A68] text-white' : 'text-gray-400 hover:text-white'"
                                    >Saída</button>
                                </div>
                        </div>
                            <div class="space-y-2">
                            <label class="text-xs font-medium text-gray-400 uppercase">Descrição</label>
                            <input 
                                v-model="form.nm_descricao"
                                type="text" 
                                class="w-full bg-[#121214] border border-[#323238] rounded-md px-4 py-2.5 text-white focus:outline-none focus:border-[#00875F] placeholder-gray-600"
                                placeholder="Ex: Salário..."
                                required
                            />
                        </div>
                    </div>

                    <!-- Category Selector -->
                    <div class="space-y-2">
                        <label class="text-xs font-medium text-gray-400 uppercase">Categoria</label>
                        <div class="grid grid-cols-3 sm:grid-cols-4 gap-2 max-h-40 overflow-y-auto pr-1">
                            <button
                                type="button"
                                v-for="category in filteredCategories"
                                :key="category.id_categoria"
                                @click="form.id_categoria = category.id_categoria"
                                class="flex flex-col items-center justify-center p-3 rounded-lg border transition-all gap-2"
                                :class="form.id_categoria === category.id_categoria 
                                    ? 'bg-[#00875F]/20 border-[#00875F] text-white' 
                                    : 'bg-[#121214] border-[#323238] text-gray-400 hover:bg-[#202024] hover:border-gray-500'"
                            >
                                <component :is="getIcon(category.nm_icone)" class="w-5 h-5" :style="{ color: form.id_categoria === category.id_categoria ? 'white' : category.nm_cor }" />
                                <span class="text-xs font-medium text-center truncate w-full">{{ category.nm_nome }}</span>
                            </button>
                        </div>
                    </div>

                    <!-- Row 2: Value & Date -->
                    <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
                            <div class="space-y-2">
                            <label class="text-xs font-medium text-gray-400 uppercase">Valor (R$)</label>
                            <input 
                                :value="valorDisplay"
                                @input="formatCurrencyInput"
                                type="text"
                                class="w-full bg-[#121214] border border-[#323238] rounded-md px-4 py-2.5 text-white focus:outline-none focus:border-[#00875F]"
                                placeholder="R$ 0,00"
                                required
                            />
                        </div>
                            <div class="space-y-2">
                            <label class="text-xs font-medium text-gray-400 uppercase">Data</label>
                            <input 
                                v-model="form.dt_dataLancamento"
                                type="date" 
                                class="w-full bg-[#121214] border border-[#323238] rounded-md px-4 py-2.5 text-white focus:outline-none focus:border-[#00875F]"
                                required
                            />
                        </div>
                    </div>

                    <!-- Row 3: Payment Method -->
                    <div class="space-y-2">
                        <label class="text-xs font-medium text-gray-400 uppercase">Forma de Pagamento</label>
                        <div class="grid grid-cols-2 gap-4">
                            <select 
                                v-model="form.nm_formaPagamento"
                                class="w-full bg-[#121214] border border-[#323238] rounded-md px-4 py-2.5 text-white focus:outline-none focus:border-[#00875F]"
                            >
                                <option value="Débito">Débito / Dinheiro</option>
                                <option value="Crédito">Crédito</option>
                            </select>

                            <!-- Dynamic Select: Wallet OR Card -->
                            <div v-if="showWallets">
                                <select 
                                    v-model="form.id_wallet"
                                    class="w-full bg-[#121214] border border-[#323238] rounded-md px-4 py-2.5 text-white focus:outline-none focus:border-[#00875F]"
                                >
                                    <option :value="null">Sem carteira</option>
                                    <option v-for="w in walletStore.wallets" :key="w.id_wallet" :value="w.id_wallet">{{ w.nm_nome }}</option>
                                </select>
                            </div>
                             <div v-if="isCredit">
                                <select 
                                    v-model="form.id_credit_card"
                                    class="w-full bg-[#121214] border border-[#323238] rounded-md px-4 py-2.5 text-white focus:outline-none focus:border-[#00875F]"
                                >
                                    <option v-for="c in walletStore.creditCards" :key="c.id_credit_card" :value="c.id_credit_card">{{ c.nm_nome }}</option>
                                </select>
                            </div>
                        </div>
                    </div>

                    <!-- Installments -->
                    <div v-if="isCredit" class="p-4 bg-[#121214]/50 border border-[#323238] rounded-lg space-y-3 animate-in fade-in slide-in-from-top-2">
                        <div class="flex items-center gap-2 text-blue-400">
                            <Layers class="w-4 h-4" />
                            <span class="text-sm font-medium">Parcelamento</span>
                        </div>
                        <div class="grid grid-cols-2 gap-4">
                            <div>
                                <label class="text-xs text-gray-500 mb-1 block">Total Parcelas</label>
                                <input v-model.number="form.nr_parcelas" type="number" min="1" max="120"
                                    class="w-full bg-[#202024] border border-[#323238] rounded px-3 py-2 text-white text-sm focus:border-[#00875F] outline-none" />
                            </div>
                            <div>
                                <label class="text-xs text-gray-500 mb-1 block">Parcela Inicial</label>
                                <input v-model.number="form.nr_parcelaInicial" type="number" min="1" :max="form.nr_parcelas"
                                    class="w-full bg-[#202024] border border-[#323238] rounded px-3 py-2 text-white text-sm focus:border-[#00875F] outline-none" />
                            </div>
                        </div>
                    </div>

                    <div class="flex gap-4 mt-8 pb-4">
                        <button 
                            type="button" 
                            @click="close"
                            class="flex-1 bg-transparent border border-[#323238] hover:bg-[#323238] text-white font-bold py-3 rounded-lg transition-all"
                        >
                            Cancelar
                        </button>
                        <button 
                            type="submit" 
                            :disabled="isProcessing"
                            class="flex-1 bg-[#00875F] hover:bg-[#00B37E] text-white font-bold py-3 rounded-lg shadow-lg transition-all flex items-center justify-center gap-2 disabled:opacity-50 disabled:cursor-not-allowed"
                        >
                            <span v-if="!isProcessing">Confirmar</span>
                            <span v-else>Salvando...</span>
                        </button>
                    </div>
                </form>
            </div>
        </div>
    </div>
</template>
