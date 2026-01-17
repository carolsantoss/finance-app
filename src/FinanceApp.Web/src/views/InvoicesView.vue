<script setup lang="ts">
import { ref, onMounted } from 'vue';
import api from '../api/axios';
import { Plus, Trash2, Edit2, CheckCircle, AlertCircle, FileText, Calendar } from 'lucide-vue-next';

const invoices = ref<any[]>([]);
const isLoading = ref(false);
const isModalOpen = ref(false);
const editingItem = ref<any>(null);

const form = ref({
    descricao: '',
    valor: 0,
    diaVencimento: 5 as number
});

const valorDisplay = ref('');

const formatCurrencyInput = (event: Event) => {
    const input = event.target as HTMLInputElement;
    let value = input.value.replace(/\D/g, '');
    if (!value) {
        form.value.valor = 0;
        valorDisplay.value = '';
        return;
    }
    const numericValue = parseInt(value) / 100;
    form.value.valor = numericValue;
    valorDisplay.value = new Intl.NumberFormat('pt-BR', { style: 'currency', currency: 'BRL' }).format(numericValue);
};

const formatCurrency = (val: number) => new Intl.NumberFormat('pt-BR', { style: 'currency', currency: 'BRL' }).format(val);

const fetchInvoices = async () => {
    isLoading.value = true;
    try {
        const response = await api.get('/invoices');
        invoices.value = response.data;
    } catch (error) {
        console.error('Error fetching invoices:', error);
    } finally {
        isLoading.value = false;
    }
};

const openModal = (item: any = null) => {
    editingItem.value = item;
    if (item) {
        form.value = {
            descricao: item.descricao,
            valor: item.valor,
            diaVencimento: item.diaVencimento
        };
        valorDisplay.value = formatCurrency(item.valor);
    } else {
        form.value = { descricao: '', valor: 0, diaVencimento: 10 };
        valorDisplay.value = '';
    }
    isModalOpen.value = true;
};

const save = async () => {
    if (!form.value.descricao || !form.value.valor) return;
    try {
        if (editingItem.value) {
            await api.put(`/invoices/${editingItem.value.id}`, { ...form.value, ativo: true });
        } else {
            await api.post('/invoices', form.value);
        }
        await fetchInvoices();
        isModalOpen.value = false;
    } catch (error) {
        alert('Erro ao salvar fatura.');
    }
};

const markAsPaid = async (item: any) => {
    if (!confirm(`Marcar ${item.descricao} como paga este mês?`)) return;
    try {
        await api.put(`/invoices/${item.id}/pay`);
        await fetchInvoices();
    } catch (error) {
        alert('Erro ao marcar como paga.');
    }
};

const deleteInvoice = async (item: any) => {
    if (!confirm('Tem certeza?')) return;
    try {
        await api.delete(`/invoices/${item.id}`);
        await fetchInvoices();
    } catch (error) {
        alert('Erro ao excluir.');
    }
};

onMounted(fetchInvoices);

const getStatus = (item: any) => {
    if (item.pagoEsteMes) return { label: 'PAGO', color: 'text-green-500', bg: 'bg-green-500/10' };
    
    // Check due date
    const today = new Date();
    const dueDay = item.diaVencimento;
    
    // Simple logic: if today's day > dueDay -> Overdue (if not paid)
    // Note: This is simple. Ideally compare full dates.
    if (today.getDate() > dueDay) return { label: 'ATRASADO', color: 'text-red-500', bg: 'bg-red-500/10' };
    if (today.getDate() === dueDay) return { label: 'VENCE HOJE', color: 'text-orange-500', bg: 'bg-orange-500/10' };
    
    return { label: 'A VENCER', color: 'text-blue-500', bg: 'bg-blue-500/10' };
};
</script>

<template>
    <div class="p-6 h-full overflow-y-auto space-y-6">
        <div class="flex justify-between items-center">
            <div>
                <h2 class="text-2xl font-bold">Gerenciamento de Faturas</h2>
                <p class="text-gray-400">Controle suas contas fixas e receba alertas por e-mail.</p>
            </div>
            <button @click="openModal()" class="bg-[#00875F] hover:bg-[#00B37E] text-white px-4 py-2 rounded-lg flex items-center gap-2 transition-colors">
                <Plus class="w-5 h-5" /> Nova Fatura
            </button>
        </div>

        <div v-if="isLoading" class="text-center py-10">
            <p class="text-gray-400">Carregando...</p>
        </div>

        <div v-else class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
             <div v-for="item in invoices" :key="item.id" class="bg-[#202024] p-5 rounded-xl border border-[#323238] flex flex-col justify-between hover:border-[#00875F]/50 transition-colors group relative overflow-hidden">
                
                <!-- Status Stripe -->
                <div class="absolute top-0 left-0 w-1 h-full" :class="getStatus(item).bg.replace('/10', '')"></div>

                <div class="flex justify-between items-start mb-4 pl-3">
                    <div class="flex items-center gap-3">
                        <div class="p-2 rounded-lg bg-[#29292E] text-gray-400 group-hover:text-white transition-colors">
                            <FileText class="w-6 h-6" />
                        </div>
                        <div>
                            <h3 class="font-bold text-lg">{{ item.descricao }}</h3>
                            <div class="flex items-center gap-1 text-xs text-gray-400 mt-1">
                                <Calendar class="w-3 h-3" /> Dia {{ item.diaVencimento }}
                            </div>
                        </div>
                    </div>
                     <span class="text-xs font-bold px-2 py-1 rounded" :class="getStatus(item).color + ' ' + getStatus(item).bg">
                        {{ getStatus(item).label }}
                    </span>
                </div>

                <div class="pl-3">
                    <div class="text-2xl font-bold text-white mb-4">{{ formatCurrency(item.valor) }}</div>

                    <div class="flex gap-2">
                         <button 
                            v-if="!item.pagoEsteMes"
                            @click="markAsPaid(item)" 
                            class="flex-1 bg-[#29292E] hover:bg-[#00875F] hover:text-white text-gray-300 py-2 rounded-lg text-sm font-medium transition-colors flex items-center justify-center gap-2"
                        >
                            <CheckCircle class="w-4 h-4" /> Pagar
                        </button>
                         <button 
                            v-else 
                            disabled
                            class="flex-1 bg-green-500/10 text-green-500 py-2 rounded-lg text-sm font-medium cursor-default flex items-center justify-center gap-2"
                        >
                            <CheckCircle class="w-4 h-4" /> Pago
                        </button>

                         <button @click="openModal(item)" class="p-2 hover:bg-[#29292E] text-gray-400 rounded-lg transition-colors">
                             <Edit2 class="w-4 h-4" />
                        </button>
                        <button @click="deleteInvoice(item)" class="p-2 hover:bg-[#29292E] text-gray-400 hover:text-red-500 rounded-lg transition-colors">
                             <Trash2 class="w-4 h-4" />
                        </button>
                    </div>
                </div>
            </div>

            <div v-if="invoices.length === 0" class="col-span-full text-center py-10 bg-[#202024] rounded-2xl border border-[#323238]">
                <p class="text-gray-400">Nenhuma fatura cadastrada.</p>
            </div>
        </div>

        <!-- Modal -->
         <div v-if="isModalOpen" class="fixed inset-0 bg-black/50 flex items-center justify-center z-50 p-4">
            <div class="bg-[#202024] rounded-2xl p-6 w-full max-w-md border border-[#323238] shadow-2xl">
                <h3 class="text-xl font-bold mb-6">{{ editingItem ? 'Editar Fatura' : 'Nova Fatura' }}</h3>
                
                <div class="space-y-4">
                     <div>
                        <label class="block text-sm font-medium text-gray-400 mb-1">Descrição</label>
                        <input v-model="form.descricao" type="text" placeholder="Ex: Aluguel, Internet" class="w-full bg-[#121214] border border-[#323238] rounded-lg p-3 text-white focus:border-[#00875F] outline-none">
                    </div>
                    <div>
                        <label class="block text-sm font-medium text-gray-400 mb-1">Valor (R$)</label>
                        <input 
                            :value="valorDisplay" 
                            @input="formatCurrencyInput"
                            type="text" 
                            placeholder="R$ 0,00"
                            class="w-full bg-[#121214] border border-[#323238] rounded-lg p-3 text-white focus:border-[#00875F] outline-none"
                        >
                    </div>
                     <div>
                        <label class="block text-sm font-medium text-gray-400 mb-1">Dia de Vencimento</label>
                        <input v-model.number="form.diaVencimento" type="number" min="1" max="31" class="w-full bg-[#121214] border border-[#323238] rounded-lg p-3 text-white focus:border-[#00875F] outline-none">
                         <p class="text-xs text-gray-500 mt-1">Dia do mês que vence a fatura.</p>
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
