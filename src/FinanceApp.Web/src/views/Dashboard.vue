<script setup lang="ts">
import { onMounted, ref } from 'vue';
import { useAuthStore } from '../stores/auth';
import { useFinanceStore } from '../stores/finance';

const auth = useAuthStore();
const finance = useFinanceStore();

const showForm = ref(false);
const newTransaction = ref({
  nm_tipo: 'Entrada',
  nm_descricao: '',
  nr_valor: 0,
  dt_dataLancamento: new Date().toISOString().split('T')[0]
});

onMounted(() => {
  finance.fetchSummary();
  finance.fetchTransactions();
});

const handleAdd = async () => {
  await finance.addTransaction(newTransaction.value);
  showForm.value = false;
  newTransaction.value = { nm_tipo: 'Entrada', nm_descricao: '', nr_valor: 0, dt_dataLancamento: new Date().toISOString().split('T')[0] };
};

const handleDelete = async (id: number) => {
  if(confirm('Tem certeza?')) {
    await finance.deleteTransaction(id);
  }
};
</script>

<template>
  <div class="container mx-auto px-4 py-8">
    <!-- Header -->
    <div class="flex justify-between items-center mb-8">
      <h1 class="text-3xl font-bold text-gray-800">FinanceApp</h1>
      <div class="flex items-center gap-4">
        <span>Olá, {{ auth.user?.nomeUsuario }}</span>
        <button @click="auth.logout" class="px-4 py-2 bg-red-500 text-white rounded hover:bg-red-600">Sair</button>
      </div>
    </div>

    <!-- Summary Cards -->
    <div class="grid grid-cols-1 md:grid-cols-3 gap-6 mb-8">
      <div class="bg-white p-6 rounded-lg shadow-md border-l-4 border-green-500">
        <h3 class="text-gray-500 text-sm font-semibold">Entradas</h3>
        <p class="text-2xl font-bold text-green-600">R$ {{ finance.summary.entradas.toFixed(2) }}</p>
      </div>
      <div class="bg-white p-6 rounded-lg shadow-md border-l-4 border-red-500">
        <h3 class="text-gray-500 text-sm font-semibold">Saídas</h3>
        <p class="text-2xl font-bold text-red-600">R$ {{ finance.summary.saidas.toFixed(2) }}</p>
      </div>
      <div class="bg-white p-6 rounded-lg shadow-md border-l-4 border-blue-500">
        <h3 class="text-gray-500 text-sm font-semibold">Saldo</h3>
        <p class="text-2xl font-bold text-blue-600">R$ {{ finance.summary.saldo.toFixed(2) }}</p>
      </div>
    </div>

    <!-- Actions -->
    <div class="mb-6">
        <button @click="showForm = !showForm" class="px-4 py-2 bg-blue-600 text-white rounded hover:bg-blue-700">
            {{ showForm ? 'Fechar' : 'Nova Transação' }}
        </button>
    </div>

    <!-- Form -->
    <div v-if="showForm" class="bg-white p-6 rounded-lg shadow-md mb-8">
        <form @submit.prevent="handleAdd" class="grid grid-cols-1 md:grid-cols-2 gap-4">
            <div>
                <label class="block text-sm font-medium text-gray-700">Tipo</label>
                <select v-model="newTransaction.nm_tipo" class="mt-1 block w-full rounded-md border-gray-300 shadow-sm border p-2">
                    <option value="Entrada">Entrada</option>
                    <option value="Saída">Saída</option>
                </select>
            </div>
            <div>
                <label class="block text-sm font-medium text-gray-700">Descrição</label>
                <input v-model="newTransaction.nm_descricao" type="text" required class="mt-1 block w-full rounded-md border-gray-300 shadow-sm border p-2" />
            </div>
            <div>
                <label class="block text-sm font-medium text-gray-700">Valor</label>
                <input v-model.number="newTransaction.nr_valor" type="number" step="0.01" required class="mt-1 block w-full rounded-md border-gray-300 shadow-sm border p-2" />
            </div>
            <div>
                <label class="block text-sm font-medium text-gray-700">Data</label>
                <input v-model="newTransaction.dt_dataLancamento" type="date" required class="mt-1 block w-full rounded-md border-gray-300 shadow-sm border p-2" />
            </div>
            <div class="md:col-span-2">
                <button type="submit" class="w-full px-4 py-2 bg-green-600 text-white rounded hover:bg-green-700">Salvar</button>
            </div>
        </form>
    </div>

    <!-- Transactions List -->
    <div class="bg-white shadow-md rounded-lg overflow-hidden">
      <table class="min-w-full divide-y divide-gray-200">
        <thead class="bg-gray-50">
          <tr>
            <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Data</th>
            <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Descrição</th>
            <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Tipo</th>
            <th class="px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider">Valor</th>
            <th class="px-6 py-3 text-right text-xs font-medium text-gray-500 uppercase tracking-wider">Ações</th>
          </tr>
        </thead>
        <tbody class="bg-white divide-y divide-gray-200">
          <tr v-for="item in finance.transactions" :key="item.id_lancamento">
            <td class="px-6 py-4 whitespace-nowrap">{{ new Date(item.dt_dataLancamento).toLocaleDateString() }}</td>
            <td class="px-6 py-4">{{ item.nm_descricao }}</td>
            <td class="px-6 py-4">
                <span :class="item.nm_tipo.includes('Entrada') ? 'text-green-600 bg-green-100' : 'text-red-600 bg-red-100'" class="px-2 inline-flex text-xs leading-5 font-semibold rounded-full">
                    {{ item.nm_tipo }}
                </span>
            </td>
            <td class="px-6 py-4 font-bold" :class="item.nm_tipo.includes('Entrada') ? 'text-green-600' : 'text-red-600'">
                R$ {{ item.nr_valor.toFixed(2) }}
            </td>
            <td class="px-6 py-4 whitespace-nowrap text-right text-sm font-medium">
                <button @click="handleDelete(item.id_lancamento)" class="text-red-600 hover:text-red-900">Excluir</button>
            </td>
          </tr>
        </tbody>
      </table>
    </div>
  </div>
</template>
