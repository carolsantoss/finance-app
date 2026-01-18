<script setup lang="ts">
import { ref } from 'vue';
import { useWalletStore } from '../stores/wallet';
import { useFinanceStore } from '../stores/finance';
import { X, Upload, FileText, AlertCircle } from 'lucide-vue-next';

const props = defineProps<{
    isOpen: boolean;
}>();

const emit = defineEmits(['close', 'success']);

const walletStore = useWalletStore();
const financeStore = useFinanceStore();

const file = ref<File | null>(null);
const selectedWalletId = ref<number | null>(null);
const isProcessing = ref(false);
const error = ref('');

const handleFileChange = (event: Event) => {
    const input = event.target as HTMLInputElement;
    if (input.files && input.files.length > 0) {
        file.value = input.files[0];
        error.value = '';
    }
};

const handleSubmit = async () => {
    if (!file.value) {
        error.value = 'Selecione um arquivo CSV.';
        return;
    }
    if (!selectedWalletId.value) {
        error.value = 'Selecione uma carteira de destino.';
        return;
    }

    isProcessing.value = true;
    error.value = '';

    const formData = new FormData();
    formData.append('file', file.value);
    formData.append('walletId', selectedWalletId.value.toString());

    try {
        await financeStore.importTransactions(formData);
        emit('success');
        handleClose();
    } catch (e: any) {
        error.value = e.response?.data?.message || 'Erro ao importar arquivo.';
    } finally {
        isProcessing.value = false;
    }
};

const handleClose = () => {
    file.value = null;
    selectedWalletId.value = null;
    error.value = '';
    emit('close');
};
</script>

<template>
    <div v-if="isOpen" class="fixed inset-0 z-50 flex items-center justify-center bg-black/50 backdrop-blur-sm p-4">
        <div class="bg-card w-full max-w-md rounded-xl border border-border shadow-2xl animate-in fade-in zoom-in-95 duration-200">
            <!-- Header -->
            <div class="flex items-center justify-between p-6 border-b border-border">
                <h2 class="text-xl font-bold text-text-primary flex items-center gap-2">
                    <Upload class="w-5 h-5 text-brand" />
                    Importar Extrato
                </h2>
                <button @click="handleClose" class="text-text-tertiary hover:text-text-primary transition-colors">
                    <X class="w-5 h-5" />
                </button>
            </div>

            <!-- Body -->
            <div class="p-6 space-y-6">
                <!-- Info Alert -->
                <div class="bg-brand/10 border border-brand/20 rounded-lg p-4 flex items-start gap-3">
                    <AlertCircle class="w-5 h-5 text-brand shrink-0 mt-0.5" />
                    <div class="text-sm text-text-secondary">
                        <p class="font-bold text-brand mb-1">Formato suportado</p>
                        Importe arquivos <strong>.csv</strong> ou <strong>.txt</strong> no formato exportado pelo Internet Banking (ex: Bradesco).
                    </div>
                </div>

                <!-- Wallet Select -->
                <div class="space-y-2">
                    <label class="text-sm font-medium text-text-secondary">Carteira de Destino</label>
                    <select 
                        v-model="selectedWalletId"
                        class="w-full bg-input border border-border rounded-lg px-4 py-2.5 text-text-primary focus:border-brand focus:ring-1 focus:ring-brand outline-none transition-all"
                    >
                        <option :value="null" disabled>Selecione uma carteira...</option>
                        <option v-for="wallet in walletStore.wallets" :key="wallet.id_wallet" :value="wallet.id_wallet">
                            {{ wallet.nm_nome }}
                        </option>
                    </select>
                </div>

                <!-- File Input -->
                <div class="space-y-2">
                    <label class="text-sm font-medium text-text-secondary">Arquivo do Extrato</label>
                    <div 
                        class="border-2 border-dashed border-border rounded-xl p-8 flex flex-col items-center justify-center text-center cursor-pointer hover:border-brand/50 hover:bg-hover transition-all group"
                        @click="$refs.fileInput.click()"
                    >
                        <input 
                            ref="fileInput"
                            type="file" 
                            accept=".csv, .txt" 
                            class="hidden" 
                            @change="handleFileChange"
                        />
                        
                        <div v-if="!file" class="flex flex-col items-center gap-2">
                            <div class="w-10 h-10 rounded-full bg-input flex items-center justify-center group-hover:bg-brand/10 transition-colors">
                                <FileText class="w-5 h-5 text-text-tertiary group-hover:text-brand" />
                            </div>
                            <p class="text-sm font-medium text-text-primary">Clique para selecionar</p>
                            <p class="text-xs text-text-tertiary">CSV ou TXT até 5MB</p>
                        </div>

                        <div v-else class="flex flex-col items-center gap-2">
                             <div class="w-10 h-10 rounded-full bg-success/10 flex items-center justify-center">
                                <FileText class="w-5 h-5 text-success" />
                            </div>
                            <p class="text-sm font-medium text-text-primary truncate max-w-[200px]">{{ file.name }}</p>
                            <p class="text-xs text-success">Arquivo selecionado</p>
                        </div>
                    </div>
                </div>

                <div v-if="error" class="text-xs text-danger font-medium flex items-center gap-2 bg-danger/10 p-3 rounded-lg">
                   <AlertCircle class="w-4 h-4" /> {{ error }}
                </div>
            </div>

            <!-- Footer -->
            <div class="p-6 border-t border-border flex justify-end gap-3 bg-card-secondary/50 rounded-b-xl">
                <button 
                    @click="handleClose"
                    class="px-4 py-2 text-sm font-medium text-text-secondary hover:text-text-primary transition-colors"
                >
                    Cancelar
                </button>
                <button 
                    @click="handleSubmit"
                    :disabled="isProcessing"
                    class="px-4 py-2 bg-brand hover:bg-brand-hover text-white text-sm font-bold rounded-lg transition-colors shadow-lg hover:shadow-brand/20 disabled:opacity-50 disabled:cursor-not-allowed flex items-center gap-2"
                >
                    <span v-if="isProcessing" class="w-4 h-4 border-2 border-white/20 border-t-white rounded-full animate-spin"></span>
                    {{ isProcessing ? 'Importando...' : 'Importar Extrato' }}
                </button>
            </div>
        </div>
    </div>
</template>
