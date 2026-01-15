<script setup lang="ts">
import { ref, computed } from 'vue';
import { useRouter } from 'vue-router';
import { useFinanceStore } from '../stores/finance';
import { ArrowLeft, Save, Calendar, DollarSign, Tag, CreditCard, Layers } from 'lucide-vue-next';
import { useToastStore } from '../stores/toast';

const router = useRouter();
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
            parcelasPagas: 0 // Default for new transaction
        };
        await finance.addTransaction(payload);
        toast.success('Lançamento salvo com sucesso!');
        router.push('/');
    } catch (error: any) {
        if (error.response?.data?.errors) {
            // Validation errors
            const errors = error.response.data.errors;
            Object.keys(errors).forEach(key => {
                const messages = errors[key];
                messages.forEach((msg: string) => toast.error(`${key}: ${msg}`));
            });
        } else {
            // Generic error
            toast.error(error.response?.data?.title || 'Erro ao salvar lançamento.');
        }
    } finally {
        isProcessing.value = false;
    }
};

const formatCurrency = (val: number) => {
    return new Intl.NumberFormat('pt-BR', { style: 'currency', currency: 'BRL' }).format(val);
};

// Simple projection preview
const parcelValue = computed(() => {
    if (form.value.nr_parcelas <= 1) return form.value.nr_valor;
    if (form.value.nr_parcelaInicial === 1) return form.value.nr_valor / form.value.nr_parcelas;
    return form.value.nr_valor; // Spec says: Else valorParcela = nr_valor (implies full value if not starting from 1?? Or remaining? Spec: "Cenário 2... Resultado: 8 parcelas restantes")
    // Spec Logic:
    // Se nr_parcelaInicial == 1: valorParcela = nr_valor / nr_parcelas
    // Senão: valorParcela = nr_valor
});
</script>

<template>
    <div class="h-screen bg-[#121214] text-gray-100 font-sans flex flex-col justify-center items-center">
        <div class="w-full max-w-4xl bg-[#202024] p-8 rounded-lg border border-[#323238] shadow-xl">
            <!-- Header -->
            <div class="flex items-center gap-4 mb-8">
                <div class="p-2 bg-[#202024] border border-[#323238] rounded-full">
                     <DollarSign class="w-6 h-6 text-white" />
                </div>
                <div>
                    <h1 class="text-2xl font-bold text-white">Novo Lançamento</h1>
                    <p class="text-sm text-gray-400">Registre suas entradas e saídas financeiras</p>
                </div>
            </div>

            <!-- Content -->
            <form @submit.prevent="handleSubmit" class="space-y-6">
                <!-- Row 1 -->
                <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
                    <div class="space-y-2">
                         <label class="text-sm font-medium text-gray-300">Tipo de lançamento</label>
                         <div class="relative">
                            <select 
                                v-model="form.nm_tipo"
                                class="w-full bg-[#121214] border border-[#323238] rounded-md px-4 py-3 text-white focus:outline-none focus:border-[#00875F] appearance-none"
                            >
                                <option value="Entrada">Entrada</option>
                                <option value="Saída">Saída</option>
                            </select>
                         </div>
                    </div>
                     <div class="space-y-2">
                        <label class="text-sm font-medium text-gray-300">Descrição</label>
                        <input 
                            v-model="form.nm_descricao"
                            type="text" 
                            class="w-full bg-[#121214] border border-[#323238] rounded-md px-4 py-3 text-white focus:outline-none focus:border-[#00875F] placeholder-gray-600"
                            placeholder="Ex: Salário, Aluguel..."
                            required
                        />
                    </div>
                </div>

                <!-- Row 2 -->
                <div class="grid grid-cols-1 md:grid-cols-2 gap-6">
                     <div class="space-y-2">
                        <label class="text-sm font-medium text-gray-300">Valor (R$)</label>
                        <input 
                            v-model.number="form.nr_valor"
                            type="number" 
                            step="0.01"
                            class="w-full bg-[#121214] border border-[#323238] rounded-md px-4 py-3 text-white focus:outline-none focus:border-[#00875F] placeholder-gray-600"
                            placeholder="0,00"
                            required
                        />
                    </div>
                     <div class="space-y-2">
                        <label class="text-sm font-medium text-gray-300">Forma de pagamento</label>
                        <select 
                            v-model="form.nm_formaPagamento"
                            class="w-full bg-[#121214] border border-[#323238] rounded-md px-4 py-3 text-white focus:outline-none focus:border-[#00875F] appearance-none"
                        >
                            <option value="Débito">Débito / Dinheiro</option>
                            <option value="Crédito">Crédito</option>
                        </select>
                    </div>
                </div>

                <!-- Row 3 -->
                <div class="space-y-2 max-w-[50%] pr-3">
                     <label class="text-sm font-medium text-gray-300">Data do lançamento</label>
                     <div class="relative">
                        <input 
                            v-model="form.dt_dataLancamento"
                            type="date" 
                            class="w-full bg-[#121214] border border-[#323238] rounded-md px-4 py-3 text-white focus:outline-none focus:border-[#00875F]"
                            required
                        />
                     </div>
                </div>

                <!-- Parcelamento (Só se for Crédito) -->
                <div v-if="isCredit" class="p-4 bg-[#121214] border border-[#323238] rounded-md space-y-4 animate-in fade-in">
                    <div class="flex items-center gap-2 text-blue-400 mb-2">
                        <Layers class="w-5 h-5" />
                        <span class="font-medium">Opções de Parcelamento</span>
                    </div>
                    
                    <div class="grid grid-cols-2 gap-4">
                        <div class="space-y-2">
                            <label class="text-xs font-medium text-gray-400">Qtd. Parcelas</label>
                            <input 
                                v-model.number="form.nr_parcelas"
                                type="number" 
                                min="1"
                                max="120"
                                class="w-full bg-[#202024] border border-[#323238] rounded px-3 py-2 text-white focus:border-[#00875F] outline-none"
                            />
                        </div>
                        <div class="space-y-2">
                            <label class="text-xs font-medium text-gray-400">Parcela Inicial</label>
                            <input 
                                v-model.number="form.nr_parcelaInicial"
                                type="number" 
                                min="1"
                                :max="form.nr_parcelas"
                                class="w-full bg-[#202024] border border-[#323238] rounded px-3 py-2 text-white focus:border-[#00875F] outline-none"
                            />
                        </div>
                    </div>
                </div>

                <!-- Submit -->
                <button 
                    type="submit" 
                    :disabled="isProcessing"
                    class="w-full bg-[#00875F] hover:bg-[#00B37E] text-white font-bold py-4 rounded-md shadow-lg transition-all flex items-center justify-center gap-2 disabled:opacity-50 disabled:cursor-not-allowed mt-8"
                >
                    <span v-if="!isProcessing" class="flex items-center gap-2">
                         <div class="w-4 h-4 border-2 border-white rounded-full" v-if="false"></div> <!-- Check icon placeholder -->
                         ✓ Salvar Lançamento
                    </span>
                    <span v-else>Salvando...</span>
                </button>
            </form>
        </div>
    </div>
</template>
