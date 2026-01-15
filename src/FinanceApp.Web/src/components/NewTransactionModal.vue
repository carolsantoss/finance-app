<script setup lang="ts">
import { ref, computed } from 'vue';
import { useFinanceStore } from '../stores/finance';
import { useToastStore } from '../stores/toast';
import { X, DollarSign, Layers } from 'lucide-vue-next';

const emit = defineEmits(['close', 'success']);
const finance = useFinanceStore();
const toast = useToastStore();

const form = ref({
    nm_tipo: 'Saída',
    nm_descricao: '',
    nr_valor: 0,
    dt_dataLancamento: new Date().toISOString().split('T')[0],
    nm_formaPagamento: 'Débito',
    nr_parcelas: 1,
    nr_parcelaInicial: 1
});

const isProcessing = ref(false);
const isCredit = computed(() => form.value.nm_formaPagamento === 'Crédito');

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

const handleSubmit = async () => {
    isProcessing.value = true;
    try {
        const payload = {
            tipo: form.value.nm_tipo,
            descricao: form.value.nm_descricao,
            valor: form.value.nr_valor,
            data: form.value.dt_dataLancamento,
            formaPagamento: form.value.nm_formaPagamento,
            parcelas: form.value.nr_parcelas,
            parcelasPagas: 0
        };
        await finance.addTransaction(payload);
        toast.success('Lançamento salvo com sucesso!');
        emit('success');
        emit('close');
        
        // Reset form
        form.value.nr_valor = 0;
        valorDisplay.value = '';
        form.value.nm_descricao = '';
    } catch (error: any) {
        if (error.response?.data?.errors) {
            const errors = error.response.data.errors;
            Object.keys(errors).forEach(key => {
                const messages = errors[key];
                messages.forEach((msg: string) => toast.error(`${key}: ${msg}`));
            });
        } else {
            toast.error(error.response?.data?.title || 'Erro ao salvar lançamento.');
        }
    } finally {
        isProcessing.value = false;
    }
};

// Reset form when needed or simple close logic
const close = () => {
    emit('close');
};
</script>

<template>
    <div class="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/80 backdrop-blur-sm animate-in fade-in duration-200">
        <div class="w-full max-w-2xl bg-[#202024] rounded-2xl shadow-2xl border border-[#323238] overflow-hidden animate-in zoom-in-95 duration-200 relative">
            
            <!-- Close Button -->
            <button @click="close" class="absolute top-4 right-4 p-2 text-gray-400 hover:text-white rounded-lg hover:bg-[#323238] transition-colors">
                <X class="w-5 h-5" />
            </button>

            <div class="p-8">
                <!-- Header -->
                <div class="flex items-center gap-4 mb-8">
                    <div class="p-2 bg-[#121214] border border-[#323238] rounded-full">
                            <DollarSign class="w-6 h-6 text-white" />
                    </div>
                    <div>
                        <h2 class="text-xl font-bold text-white">Novo Lançamento</h2>
                        <p class="text-sm text-gray-400">Registre suas entradas e saídas</p>
                    </div>
                </div>

                <!-- Form -->
                <form @submit.prevent="handleSubmit" class="space-y-6">
                    <!-- Row 1 -->
                    <div class="grid grid-cols-1 md:grid-cols-2 gap-4">
                        <div class="space-y-2">
                                <label class="text-xs font-medium text-gray-400 uppercase">Tipo</label>
                                <div class="flex rounded-md bg-[#121214] border border-[#323238] p-1">
                                    <button 
                                        type="button" 
                                        @click="form.nm_tipo = 'Entrada'"
                                        class="flex-1 py-2 text-sm font-medium rounded transition-all"
                                        :class="form.nm_tipo === 'Entrada' ? 'bg-[#00875F] text-white' : 'text-gray-400 hover:text-white'"
                                    >Entrada</button>
                                    <button 
                                        type="button" 
                                        @click="form.nm_tipo = 'Saída'"
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

                    <!-- Row 2 -->
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
                        <select 
                            v-model="form.nm_formaPagamento"
                            class="w-full bg-[#121214] border border-[#323238] rounded-md px-4 py-2.5 text-white focus:outline-none focus:border-[#00875F]"
                        >
                            <option value="Débito">Débito / Dinheiro</option>
                            <option value="Crédito">Crédito</option>
                        </select>
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

                    <div class="flex gap-4 mt-8">
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
