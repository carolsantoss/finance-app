<script setup lang="ts">
import { ref, onMounted } from 'vue';
import api from '../../api/axios';
import { Copy, RefreshCw, Eye, EyeOff, Terminal, CheckCircle } from 'lucide-vue-next';

const isLoading = ref(false);
const token = ref('');
const showToken = ref(false);
const copied = ref(false);
const apiEndpoint = ref(`${window.location.origin}/api/recurringtransactions/process`);
const logs = ref<any[]>([]);

const fetchLogs = async () => {
    try {
        const response = await api.get('/integrations/logs');
        logs.value = response.data;
    } catch (error) {
        console.error('Error fetching logs:', error);
    }
};

const fetchToken = async () => {
    isLoading.value = true;
    try {
        const response = await api.get('/integrations/scheduler-token');
        token.value = response.data.token;
    } catch (error) {
        console.error('Error fetching token:', error);
    } finally {
        isLoading.value = false;
    }
};

const regenerateToken = async () => {
    if (!confirm('Tem certeza? Isso invalidará o token atual e parará as integrações até que seja atualizado.')) return;
    
    isLoading.value = true;
    try {
        const response = await api.post('/integrations/scheduler-token/regenerate');
        token.value = response.data.token;
        showToken.value = true;
    } catch (error) {
        console.error('Error generating token:', error);
    } finally {
        isLoading.value = false;
    }
};

const copyToken = () => {
    navigator.clipboard.writeText(token.value);
    copied.value = true;
    setTimeout(() => copied.value = false, 2000);
};

onMounted(() => {
    fetchToken();
    fetchLogs();
    // Update apiEndpoint if needed to be dynamic relative to actual API base URL,  
    // but window.location.origin is usually frontend. 
    // If API is on different port, we might need configuration. 
    // For now, assume relative path or known structure.
    // If using axios baseURL:
    if (api.defaults.baseURL) {
        // If baseURL is absolute
        if (api.defaults.baseURL.startsWith('http')) {
             apiEndpoint.value = `${api.defaults.baseURL}/recurringtransactions/process`;
        } else {
             apiEndpoint.value = `${window.location.origin}${api.defaults.baseURL}/recurringtransactions/process`;
        }
    }
});
</script>

<template>
    <div class="p-6 space-y-8">
        <div>
            <h2 class="text-2xl font-bold">Integrações do Sistema</h2>
            <p class="text-gray-400">Gerencie chaves de API e configurações externas.</p>
        </div>

        <!-- Component: Scheduler Config -->
        <div class="bg-[#202024] p-6 rounded-xl border border-[#323238]">
            <div class="flex items-start gap-4 mb-6">
                <div class="p-3 bg-blue-500/10 text-blue-500 rounded-lg">
                    <Terminal class="w-6 h-6" />
                </div>
                <div>
                    <h3 class="text-xl font-bold">Agendador de Tarefas (Jobs)</h3>
                    <p class="text-gray-400 text-sm mt-1">Configure o processamento automático de transações recorrentes.</p>
                </div>
            </div>

            <div class="space-y-6">
                 <div>
                    <label class="block text-sm font-medium text-gray-400 mb-2">Endpoint de Processamento (POST)</label>
                    <div class="flex items-center gap-2 bg-[#121214] p-3 rounded-lg border border-[#323238] font-mono text-sm text-gray-300 break-all">
                        {{ apiEndpoint }}
                    </div>
                </div>

                <div>
                    <label class="block text-sm font-medium text-gray-400 mb-2">Token de Autenticação (X-Scheduler-Secret)</label>
                    <div class="flex gap-2">
                        <div class="flex-1 flex items-center justify-between bg-[#121214] p-3 rounded-lg border border-[#323238] font-mono text-sm">
                             <span :class="showToken ? 'text-white' : 'text-gray-500 blur-sm select-none'">
                                {{ showToken ? token : '••••••••••••••••••••••••••••••••' }}
                             </span>
                             <button @click="showToken = !showToken" class="text-gray-400 hover:text-white transition-colors" title="Ver/Ocultar">
                                <component :is="showToken ? EyeOff : Eye" class="w-4 h-4" />
                            </button>
                        </div>
                        <button @click="copyToken" class="bg-[#29292E] hover:bg-[#323238] text-white px-4 rounded-lg flex items-center gap-2 transition-colors relative">
                             <component :is="copied ? CheckCircle : Copy" class="w-4 h-4" :class="copied ? 'text-green-500' : ''" />
                             <span v-if="copied" class="absolute -top-8 left-1/2 -translate-x-1/2 bg-black text-xs py-1 px-2 rounded">Copiado!</span>
                        </button>
                         <button @click="regenerateToken" class="bg-[#29292E] hover:bg-red-500/10 hover:text-red-500 text-gray-400 px-4 rounded-lg flex items-center gap-2 transition-colors" title="Gerar Novo Token">
                             <RefreshCw class="w-4 h-4" :class="{ 'animate-spin': isLoading }" />
                        </button>
                    </div>
                </div>

                <div class="bg-[#121214] p-4 rounded-lg border border-[#323238]">
                    <h4 class="font-bold text-sm text-gray-300 mb-3">Como configurar (Exemplo CURL)</h4>
                    <code class="block font-mono text-xs text-blue-300 whitespace-pre-wrap">
curl -X POST "{{ apiEndpoint }}" \
  -H "X-Scheduler-Secret: {{ token || 'SEU_TOKEN' }}"
                    </code>
                    <p class="text-xs text-gray-500 mt-3">Recomendado: Configure este comando no CRON do seu servidor para rodar diariamente à meia-noite (00:01).</p>
                </div>
            </div>
        </div>

        <!-- Component: Integration Logs -->
        <div class="bg-[#202024] p-6 rounded-xl border border-[#323238]">
            <div class="flex items-center justify-between mb-6">
                <div class="flex items-start gap-4">
                     <div class="p-3 bg-purple-500/10 text-purple-500 rounded-lg">
                        <Terminal class="w-6 h-6" />
                    </div>
                    <div>
                        <h3 class="text-xl font-bold">Logs de Execução</h3>
                        <p class="text-gray-400 text-sm mt-1">Histórico de execuções do agendador.</p>
                    </div>
                </div>
                <button @click="fetchLogs" class="p-2 bg-[#29292E] hover:bg-[#323238] rounded-lg transition-colors text-white">
                    <RefreshCw class="w-4 h-4" />
                </button>
            </div>

            <div class="overflow-x-auto">
                <table class="w-full text-left text-sm text-gray-400">
                    <thead class="bg-[#121214] text-xs uppercase font-medium text-gray-500">
                        <tr>
                            <th class="px-4 py-3 rounded-tl-lg">Data</th>
                            <th class="px-4 py-3">Status</th>
                            <th class="px-4 py-3">Mensagem</th>
                            <th class="px-4 py-3 rounded-tr-lg text-right">Detalhes</th>
                        </tr>
                    </thead>
                    <tbody class="divide-y divide-[#323238]">
                        <tr v-for="log in logs" :key="log.id_log" class="hover:bg-[#29292E]/50 transition-colors">
                            <td class="px-4 py-3 text-white">{{ new Date(log.dt_log).toLocaleString() }}</td>
                            <td class="px-4 py-3">
                                <span class="px-2 py-1 rounded text-xs font-bold" 
                                    :class="log.ds_status === 'Success' ? 'bg-green-500/10 text-green-500' : 'bg-red-500/10 text-red-500'">
                                    {{ log.ds_status }}
                                </span>
                            </td>
                            <td class="px-4 py-3">{{ log.ds_message }}</td>
                            <td class="px-4 py-3 text-right">
                                <span class="text-xs truncate max-w-[200px] inline-block" :title="log.ds_details">{{ log.ds_details }}</span>
                            </td>
                        </tr>
                        <tr v-if="logs.length === 0">
                            <td colspan="4" class="px-4 py-8 text-center text-gray-500">Nenhum log encontrado.</td>
                        </tr>
                    </tbody>
                </table>
            </div>
        </div>
    </div>
</template>
