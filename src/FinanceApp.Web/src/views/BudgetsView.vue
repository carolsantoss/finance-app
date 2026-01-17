<script setup lang="ts">
import { ref, onMounted, computed } from 'vue';
import { usePlanningStore } from '../stores/planning';
import { useCategoryStore } from '../stores/category';
import { Plus, Trash2, Edit2, AlertTriangle, CheckCircle } from 'lucide-vue-next';

const planning = usePlanningStore();
const categoryStore = useCategoryStore();
const currentDate = new Date();
const selectedMonth = ref(currentDate.getMonth() + 1);
const selectedYear = ref(currentDate.getFullYear());
const isModalOpen = ref(false);
const editingBudget = ref<any>(null);

const form = ref({
    categoryId: '',
    valorLimite: '',
    alertaPorcentagem: 80
});

onMounted(async () => {
    await categoryStore.fetchCategories();
    await fetchBudgets();
});

const fetchBudgets = async () => {
    await planning.fetchBudgets(selectedMonth.value, selectedYear.value);
};

const openModal = (budget: any = null) => {
    editingBudget.value = budget;
    if (budget) {
        form.value = {
            categoryId: budget.categoryId,
            valorLimite: budget.valorLimite.toString(),
            alertaPorcentagem: budget.alertaPorcentagem
        };
    } else {
        form.value = {
            categoryId: '',
            valorLimite: '',
            alertaPorcentagem: 80
        };
    }
    isModalOpen.value = true;
};

const saveBudget = async () => {
    if (!form.value.categoryId || !form.value.valorLimite) return;

    const payload = {
        categoryId: form.value.categoryId,
        valorLimite: parseFloat(form.value.valorLimite),
        mes: selectedMonth.value,
        ano: selectedYear.value,
        alertaPorcentagem: form.value.alertaPorcentagem
    };

    await planning.createOrUpdateBudget(payload);
    isModalOpen.value = false;
};

const deleteBudget = async (id: number) => {
    if (confirm('Tem certeza que deseja excluir este orçamento?')) {
        await planning.deleteBudget(id, selectedMonth.value, selectedYear.value);
    }
};

const formatCurrency = (value: number) => {
    return new Intl.NumberFormat('pt-BR', { style: 'currency', currency: 'BRL' }).format(value);
};

const getProgressBarColor = (percentage: number) => {
    if (percentage >= 100) return 'bg-[#F75A68]'; // Red
    if (percentage >= 80) return 'bg-yellow-500';  // Yellow
    return 'bg-[#00B37E]'; // Green
};

const availableCategories = computed(() => {
    // Return categories that don't have a budget yet (unless editing)
    if (editingBudget.value) return categoryStore.categories;
    const usedIds = planning.budgets.map(b => b.categoryId);
    return categoryStore.categories.filter(c => !usedIds.includes(c.id_categoria) && c.nm_tipo === 'Saída');
});
</script>

<template>
    <div class="p-6 h-full overflow-y-auto space-y-6">
        <div class="flex justify-between items-center">
            <div>
                <h2 class="text-2xl font-bold">Orçamentos</h2>
                <p class="text-gray-400">Defina limites para suas categorias.</p>
            </div>
            <div class="flex gap-4 items-center">
                 <select v-model="selectedMonth" @change="fetchBudgets" class="bg-[#202024] border border-[#323238] rounded-lg p-2 text-white">
                    <option v-for="m in 12" :key="m" :value="m">{{ new Date(0, m-1).toLocaleString('pt-BR', { month: 'long' }) }}</option>
                </select>
                <select v-model="selectedYear" @change="fetchBudgets" class="bg-[#202024] border border-[#323238] rounded-lg p-2 text-white">
                    <option v-for="y in [2024, 2025, 2026]" :key="y" :value="y">{{ y }}</option>
                </select>
                <button @click="openModal()" class="bg-[#00875F] hover:bg-[#00B37E] text-white px-4 py-2 rounded-lg flex items-center gap-2">
                    <Plus class="w-5 h-5" /> Novo
                </button>
            </div>
        </div>

        <div v-if="planning.isLoading" class="text-center py-10">
            <p class="text-gray-400">Carregando...</p>
        </div>

        <div v-else class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
            <div v-for="budget in planning.budgets" :key="budget.id" class="bg-[#202024] p-6 rounded-2xl border border-[#323238] relative group">
                <div class="flex justify-between items-start mb-4">
                    <div class="flex items-center gap-3">
                         <div class="w-10 h-10 rounded-lg bg-[#323238] flex items-center justify-center">
                            <!-- Icon placeholder, could use category icon here -->
                            <span class="font-bold text-lg">{{ budget.categoryName.charAt(0) }}</span>
                        </div>
                        <div>
                            <h3 class="font-bold text-lg">{{ budget.categoryName }}</h3>
                            <p class="text-sm text-gray-400">{{ formatCurrency(budget.valorGasto) }} de {{ formatCurrency(budget.valorLimite) }}</p>
                        </div>
                    </div>
                    <div class="flex gap-2 opacity-0 group-hover:opacity-100 transition-opacity">
                        <button @click="openModal(budget)" class="p-2 hover:bg-[#29292E] rounded text-gray-400 hover:text-white">
                            <Edit2 class="w-4 h-4" />
                        </button>
                        <button @click="deleteBudget(budget.id)" class="p-2 hover:bg-[#29292E] rounded text-gray-400 hover:text-[#F75A68]">
                            <Trash2 class="w-4 h-4" />
                        </button>
                    </div>
                </div>

                <!-- Progress Bar -->
                <div class="relative w-full h-2 bg-[#121214] rounded-full overflow-hidden">
                    <div class="absolute left-0 top-0 h-full transition-all duration-500 rounded-full"
                        :class="getProgressBarColor((budget.valorGasto / budget.valorLimite) * 100)"
                        :style="{ width: Math.min((budget.valorGasto / budget.valorLimite) * 100, 100) + '%' }">
                    </div>
                </div>
                
                <div class="mt-2 flex justify-between text-xs font-medium">
                     <span :class="(budget.valorGasto / budget.valorLimite) * 100 >= 100 ? 'text-[#F75A68]' : 'text-gray-400'">
                        {{ Math.round((budget.valorGasto / budget.valorLimite) * 100) }}% usado
                     </span>
                     <span class="text-gray-500">Alerta em {{ budget.alertaPorcentagem }}%</span>
                </div>
            </div>
            
            <!-- Empty State -->
            <div v-if="planning.budgets.length === 0" class="col-span-full text-center py-10 bg-[#202024] rounded-2xl border border-[#323238]">
                <p class="text-gray-400">Nenhum orçamento definido para este mês.</p>
            </div>
        </div>

        <!-- Modal -->
        <div v-if="isModalOpen" class="fixed inset-0 bg-black/50 flex items-center justify-center z-50 p-4">
            <div class="bg-[#202024] rounded-2xl p-6 w-full max-w-md border border-[#323238] shadow-2xl">
                <h3 class="text-xl font-bold mb-6">{{ editingBudget ? 'Editar Orçamento' : 'Novo Orçamento' }}</h3>
                
                <div class="space-y-4">
                    <div>
                        <label class="block text-sm font-medium text-gray-400 mb-1">Categoria</label>
                        <select v-model="form.categoryId" :disabled="!!editingBudget" class="w-full bg-[#121214] border border-[#323238] rounded-lg p-3 text-white focus:border-[#00875F] outline-none">
                            <option value="">Selecione uma categoria</option>
                            <option v-for="cat in (editingBudget ? [categoryStore.categories.find(c => c.id_categoria == editingBudget.categoryId)] : availableCategories)" 
                                    :key="cat?.id_categoria" 
                                    :value="cat?.id_categoria">
                                {{ cat?.nm_nome }}
                            </option>
                        </select>
                    </div>

                    <div>
                        <label class="block text-sm font-medium text-gray-400 mb-1">Limite (R$)</label>
                        <input v-model="form.valorLimite" type="number" step="0.01" class="w-full bg-[#121214] border border-[#323238] rounded-lg p-3 text-white focus:border-[#00875F] outline-none" placeholder="Ex: 500.00">
                    </div>

                    <div>
                        <label class="block text-sm font-medium text-gray-400 mb-1">Alerta em (%)</label>
                        <input v-model="form.alertaPorcentagem" type="number" class="w-full bg-[#121214] border border-[#323238] rounded-lg p-3 text-white focus:border-[#00875F] outline-none">
                    </div>
                </div>

                <div class="flex gap-3 mt-8">
                    <button @click="isModalOpen = false" class="flex-1 px-4 py-3 rounded-lg bg-[#29292E] hover:bg-[#323238] text-white transition-colors">Cancelar</button>
                    <button @click="saveBudget" class="flex-1 px-4 py-3 rounded-lg bg-[#00875F] hover:bg-[#00B37E] text-white font-bold transition-colors">Salvar</button>
                </div>
            </div>
        </div>
    </div>
</template>
