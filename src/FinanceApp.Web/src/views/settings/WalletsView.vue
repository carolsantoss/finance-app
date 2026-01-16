<script setup lang="ts">
import { ref, onMounted } from 'vue';
import { useWalletStore, Wallet, CreditCard } from '../../stores/wallet';
import { Wallet as WalletIcon, CreditCard as CardIcon, Plus, MoreVertical, Trash2, Edit, X, Landmark, DollarSign } from 'lucide-vue-next';

const walletStore = useWalletStore();
const isWalletModalOpen = ref(false);
const isCardModalOpen = ref(false);
const isEditing = ref(false);
const errorMsg = ref('');

// Wallet Form
const walletForm = ref<Partial<Wallet>>({
    nm_nome: '',
    nm_tipo: 'Conta Corrente',
    nr_saldo_inicial: 0
});

// Card Form
const cardForm = ref<Partial<CreditCard>>({
    nm_nome: '',
    nm_bandeira: 'Mastercard',
    nr_limite: 0,
    nr_dia_fechamento: 1,
    nr_dia_vencimento: 10
});

onMounted(() => {
    walletStore.fetchAll();
});

const formatCurrency = (val: number) => {
    return new Intl.NumberFormat('pt-BR', { style: 'currency', currency: 'BRL' }).format(val);
};

// --- Wallet Actions ---
const openCreateWallet = () => {
    isEditing.value = false;
    walletForm.value = { nm_nome: '', nm_tipo: 'Conta Corrente', nr_saldo_inicial: 0 };
    isWalletModalOpen.value = true;
};

const openEditWallet = (w: Wallet) => {
    isEditing.value = true;
    walletForm.value = { ...w };
    isWalletModalOpen.value = true;
};

const saveWallet = async () => {
    try {
        if (isEditing.value && walletForm.value.id_wallet) {
            await walletStore.updateWallet(walletForm.value.id_wallet, walletForm.value);
        } else {
            await walletStore.createWallet(walletForm.value);
        }
        isWalletModalOpen.value = false;
    } catch (e: any) {
        errorMsg.value = e.response?.data || 'Erro ao salvar carteira';
    }
};

const deleteWallet = async (id: number) => {
    if (!confirm('Tem certeza?')) return;
    try {
        await walletStore.deleteWallet(id);
    } catch (e: any) {
        alert(e.response?.data || 'Erro ao excluir');
    }
};

// --- Card Actions ---
const openCreateCard = () => {
    isEditing.value = false;
    cardForm.value = { nm_nome: '', nm_bandeira: 'Mastercard', nr_limite: 0, nr_dia_fechamento: 1, nr_dia_vencimento: 10 };
    isCardModalOpen.value = true;
};

const openEditCard = (c: CreditCard) => {
    isEditing.value = true;
    cardForm.value = { ...c };
    isCardModalOpen.value = true;
};

const saveCard = async () => {
    try {
        if (isEditing.value && cardForm.value.id_credit_card) {
            await walletStore.updateCard(cardForm.value.id_credit_card, cardForm.value);
        } else {
            await walletStore.createCard(cardForm.value);
        }
        isCardModalOpen.value = false;
    } catch (e: any) {
        errorMsg.value = e.response?.data || 'Erro ao salvar cartão';
    }
};

const deleteCard = async (id: number) => {
    if (!confirm('Tem certeza?')) return;
    try {
        await walletStore.deleteCard(id);
    } catch (e: any) {
        alert(e.response?.data || 'Erro ao excluir');
    }
};
</script>

<template>
    <div class="p-6 space-y-8">
        <!-- Header -->
        <header class="flex flex-col md:flex-row md:items-center justify-between gap-4">
            <div>
                <h1 class="text-2xl font-bold text-white flex items-center gap-2">
                    <WalletIcon class="w-6 h-6 text-[#00875F]" />
                    Minhas Carteiras
                </h1>
                <p class="text-gray-400">Gerencie suas contas e cartões de crédito</p>
            </div>
            <div class="flex gap-2">
                <button @click="openCreateWallet" class="btn-primary flex items-center gap-2">
                    <Plus class="w-4 h-4" /> Nova Carteira
                </button>
                <button @click="openCreateCard" class="btn-secondary flex items-center gap-2">
                    <Plus class="w-4 h-4" /> Novo Cartão
                </button>
            </div>
        </header>

        <!-- Summary Cards -->
        <div class="grid grid-cols-1 md:grid-cols-3 gap-4">
            <div class="bg-[#202024] p-6 rounded-xl border border-[#323238]">
                <div class="flex items-center justify-between mb-4">
                    <span class="text-gray-400 font-medium">Saldo Total</span>
                    <DollarSign class="w-5 h-5 text-[#00B37E]" />
                </div>
                <strong class="text-2xl text-white">{{ formatCurrency(walletStore.totalBalance) }}</strong>
            </div>
            <!-- Add more summary cards if needed later -->
        </div>

        <div class="grid grid-cols-1 lg:grid-cols-2 gap-8">
            <!-- Wallets Section -->
            <section class="space-y-4">
                <h2 class="text-lg font-bold text-white flex items-center gap-2">
                    <Landmark class="w-5 h-5 text-gray-400" />
                    Contas
                </h2>
                
                <div class="space-y-3">
                    <div v-for="wallet in walletStore.wallets" :key="wallet.id_wallet" 
                        class="bg-[#29292E] p-4 rounded-xl border border-[#323238] flex justify-between items-center group relative overflow-hidden">
                        
                        <!-- Decoration -->
                        <div class="absolute left-0 top-0 bottom-0 w-1 bg-[#00875F]"></div>

                        <div>
                            <span class="text-xs text-gray-500 uppercase font-bold tracking-wider mb-1 block">{{ wallet.nm_tipo }}</span>
                            <h3 class="text-white font-medium text-lg leading-tight">{{ wallet.nm_nome }}</h3>
                            <p class="text-[#00B37E] font-bold mt-1">{{ formatCurrency(wallet.currentBalance) }}</p>
                        </div>

                        <div class="flex gap-2 opacity-0 group-hover:opacity-100 transition-opacity">
                            <button @click="openEditWallet(wallet)" class="p-2 text-gray-400 hover:text-white hover:bg-[#323238] rounded-lg"><Edit class="w-4 h-4"/></button>
                            <button @click="deleteWallet(wallet.id_wallet)" class="p-2 text-gray-400 hover:text-red-500 hover:bg-red-500/10 rounded-lg"><Trash2 class="w-4 h-4"/></button>
                        </div>
                    </div>
                </div>
            </section>

            <!-- Credit Cards Section -->
            <section class="space-y-4">
                <h2 class="text-lg font-bold text-white flex items-center gap-2">
                    <CardIcon class="w-5 h-5 text-gray-400" />
                    Cartões de Crédito
                </h2>

                <div class="grid gap-4">
                    <div v-for="card in walletStore.creditCards" :key="card.id_credit_card"
                        class="bg-gradient-to-br from-[#202024] to-[#29292E] p-5 rounded-xl border border-[#323238] relative group">
                        
                        <div class="flex justify-between items-start mb-6">
                            <div class="flex items-center gap-2">
                                <div class="w-8 h-5 bg-gray-600 rounded"></div> <!-- Abstract Chip -->
                                <span class="text-gray-400 text-xs uppercase">{{ card.nm_bandeira }}</span>
                            </div>
                            <div class="flex gap-1 opacity-0 group-hover:opacity-100 transition-opacity">
                                <button @click="openEditCard(card)" class="p-1.5 text-gray-400 hover:text-white rounded"><Edit class="w-3 h-3"/></button>
                                <button @click="deleteCard(card.id_credit_card)" class="p-1.5 text-gray-400 hover:text-red-500 rounded"><Trash2 class="w-3 h-3"/></button>
                            </div>
                        </div>

                        <h3 class="text-white font-medium text-lg tracking-wide mb-1">{{ card.nm_nome }}</h3>
                        
                        <div class="flex justify-between items-end mt-4">
                            <div>
                                <span class="text-xs text-gray-500 block">Fatura Atual</span>
                                <span class="text-[#F75A68] font-bold">{{ formatCurrency(card.currentInvoice) }}</span>
                            </div>
                            <div class="text-right">
                                <span class="text-xs text-gray-500 block">Limite Disponível</span>
                                <span class="text-[#00B37E] font-bold">{{ formatCurrency(card.availableLimit) }}</span>
                            </div>
                        </div>
                        
                        <div class="mt-3 pt-3 border-t border-[#323238] flex justify-between text-xs text-gray-500">
                             <span>Fecha dia {{ card.nr_dia_fechamento }}</span>
                             <span>Vence dia {{ card.nr_dia_vencimento }}</span>
                        </div>
                    </div>
                </div>
            </section>
        </div>
    </div>

    <!-- Create/Edit Wallet Modal -->
    <div v-if="isWalletModalOpen" class="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/50 backdrop-blur-sm">
        <div class="bg-[#202024] p-6 rounded-xl w-full max-w-sm border border-[#323238]">
            <h3 class="text-white font-bold text-lg mb-4">{{ isEditing ? 'Editar' : 'Nova' }} Carteira</h3>
            <div class="space-y-3">
                <input v-model="walletForm.nm_nome" placeholder="Nome (ex: Nubank)" class="w-full bg-[#121214] border border-[#323238] rounded p-3 text-white focus:border-[#00875F] outline-none" />
                <select v-model="walletForm.nm_tipo" class="w-full bg-[#121214] border border-[#323238] rounded p-3 text-white outline-none">
                    <option>Conta Corrente</option>
                    <option>Dinheiro</option>
                    <option>Investimento</option>
                    <option>Poupança</option>
                </select>
                <input v-if="!isEditing" v-model.number="walletForm.nr_saldo_inicial" type="number" placeholder="Saldo Inicial" class="w-full bg-[#121214] border border-[#323238] rounded p-3 text-white focus:border-[#00875F] outline-none" />
            </div>
            <div class="flex justify-end gap-2 mt-6">
                <button @click="isWalletModalOpen = false" class="px-4 py-2 text-gray-400 hover:text-white">Cancelar</button>
                <button @click="saveWallet" class="bg-[#00875F] text-white px-4 py-2 rounded hover:bg-[#00B37E]">Salvar</button>
            </div>
        </div>
    </div>

    <!-- Create/Edit Card Modal -->
    <div v-if="isCardModalOpen" class="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/50 backdrop-blur-sm">
        <div class="bg-[#202024] p-6 rounded-xl w-full max-w-sm border border-[#323238]">
            <h3 class="text-white font-bold text-lg mb-4">{{ isEditing ? 'Editar' : 'Novo' }} Cartão</h3>
            <div class="space-y-3">
                <input v-model="cardForm.nm_nome" placeholder="Nome (ex: Nubank Gold)" class="w-full bg-[#121214] border border-[#323238] rounded p-3 text-white focus:border-[#00875F] outline-none" />
                <select v-model="cardForm.nm_bandeira" class="w-full bg-[#121214] border border-[#323238] rounded p-3 text-white outline-none">
                    <option>Mastercard</option>
                    <option>Visa</option>
                    <option>Elo</option>
                    <option>Amex</option>
                    <option>Hipercard</option>
                </select>
                <input v-model.number="cardForm.nr_limite" type="number" placeholder="Limite Total" class="w-full bg-[#121214] border border-[#323238] rounded p-3 text-white focus:border-[#00875F] outline-none" />
                <div class="flex gap-2">
                    <div class="flex-1">
                        <label class="text-xs text-gray-500">Dia Fechamento</label>
                        <input v-model.number="cardForm.nr_dia_fechamento" type="number" min="1" max="31" class="w-full bg-[#121214] border border-[#323238] rounded p-3 text-white focus:border-[#00875F] outline-none" />
                    </div>
                    <div class="flex-1">
                        <label class="text-xs text-gray-500">Dia Vencimento</label>
                        <input v-model.number="cardForm.nr_dia_vencimento" type="number" min="1" max="31" class="w-full bg-[#121214] border border-[#323238] rounded p-3 text-white focus:border-[#00875F] outline-none" />
                    </div>
                </div>
            </div>
            <div class="flex justify-end gap-2 mt-6">
                <button @click="isCardModalOpen = false" class="px-4 py-2 text-gray-400 hover:text-white">Cancelar</button>
                <button @click="saveCard" class="bg-[#00875F] text-white px-4 py-2 rounded hover:bg-[#00B37E]">Salvar</button>
            </div>
        </div>
    </div>
</template>

<style scoped>
.btn-primary {
    @apply bg-[#00875F] hover:bg-[#00B37E] text-white px-4 py-2 rounded-lg font-medium transition-colors shadow-lg shadow-[#00875F]/20;
}
.btn-secondary {
    @apply bg-[#29292E] hover:bg-[#323238] text-white px-4 py-2 rounded-lg font-medium transition-colors border border-[#323238];
}
</style>
