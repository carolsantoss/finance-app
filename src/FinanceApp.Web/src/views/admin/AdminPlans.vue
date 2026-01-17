<script setup lang="ts">
import { ref, onMounted } from 'vue';
import { 
    Package, 
    Plus, 
    Trash2, 
    Edit, 
    Check, 
    X, 
    Star 
} from 'lucide-vue-next';
import api from '../../api/axios';

interface Feature {
    id: number;
    key: string;
    label: string;
    description: string;
}

interface Plan {
    id: number;
    name: string;
    description: string;
    price: number;
    isDefault: boolean;
    features: Feature[];
}

const plans = ref<Plan[]>([]);
const systemFeatures = ref<Feature[]>([]);
const isLoading = ref(false);
const isModalOpen = ref(false);
const isEditing = ref(false);
const errorMessage = ref('');

// Form state
const form = ref({
    id: 0,
    name: '',
    description: '',
    price: 0,
    isDefault: false,
    featureIds: [] as number[]
});

const fetchPlans = async () => {
    isLoading.value = true;
    try {
        const response = await api.get('/plans');
        plans.value = response.data;
    } catch (error) {
        console.error('Failed to fetch plans', error);
    } finally {
        isLoading.value = false;
    }
};

const fetchFeatures = async () => {
    try {
        const response = await api.get('/plans/features');
        systemFeatures.value = response.data;
    } catch (error) {
        console.error('Failed to fetch features', error);
    }
};

const openCreateModal = () => {
    isEditing.value = false;
    form.value = { 
        id: 0, 
        name: '', 
        description: '', 
        price: 0, 
        isDefault: false,
        featureIds: [] 
    };
    isModalOpen.value = true;
    errorMessage.value = '';
};

const openEditModal = (plan: Plan) => {
    isEditing.value = true;
    form.value = { 
        id: plan.id, 
        name: plan.name, 
        description: plan.description, 
        price: plan.price, 
        isDefault: plan.isDefault,
        featureIds: plan.features.map(f => f.id)
    };
    isModalOpen.value = true;
    errorMessage.value = '';
};

const closeModal = () => {
    isModalOpen.value = false;
};

const savePlan = async () => {
    try {
        if (isEditing.value) {
            await api.put(`/plans/${form.value.id}`, form.value);
        } else {
            await api.post('/plans', form.value);
        }
        await fetchPlans();
        closeModal();
    } catch (error: any) {
        errorMessage.value = error.response?.data || 'Erro ao salvar plano';
    }
};

const deletePlan = async (id: number) => {
    if (!confirm('Tem certeza que deseja excluir este plano?')) return;
    
    try {
        await api.delete(`/plans/${id}`);
        await fetchPlans();
    } catch (error: any) {
        alert(error.response?.data || 'Erro ao excluir plano');
    }
};

const toggleFeature = (featureId: number) => {
    const index = form.value.featureIds.indexOf(featureId);
    if (index === -1) {
        form.value.featureIds.push(featureId);
    } else {
        form.value.featureIds.splice(index, 1);
    }
};

onMounted(() => {
    fetchPlans();
    fetchFeatures();
});
</script>

<template>
    <div class="h-full flex flex-col p-6 space-y-6 overflow-hidden">
        <!-- Header -->
        <div class="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4">
            <div>
                <h2 class="text-2xl font-bold text-white flex items-center gap-2">
                    <Package class="w-6 h-6 text-[#00875F]" />
                    Planos & Assinaturas
                </h2>
                <p class="text-gray-400">Gerencie os planos de acesso e funcionalidades.</p>
            </div>
            <button @click="openCreateModal" class="flex items-center gap-2 bg-[#00875F] hover:bg-[#00B37E] text-white px-5 py-2.5 rounded-lg shadow-lg shadow-[#00875F]/20 transition-all font-medium">
                <Plus class="w-5 h-5" />
                Novo Plano
            </button>
        </div>

        <!-- Content -->
        <div class="flex-1 bg-[#202024] rounded-2xl border border-[#323238] shadow-xl overflow-hidden flex flex-col">
            <div class="p-6 border-b border-[#323238]">
                <h3 class="text-lg font-bold text-white">Planos Disponíveis</h3>
            </div>
            
            <div class="overflow-x-auto flex-1">
                <table class="w-full text-left text-sm text-gray-400">
                    <thead class="bg-[#29292E] text-xs uppercase font-medium">
                        <tr>
                            <th class="px-6 py-4">Nome</th>
                            <th class="px-6 py-4">Preço</th>
                            <th class="px-6 py-4">Funcionalidades</th>
                            <th class="px-6 py-4 text-center">Padrão</th>
                            <th class="px-6 py-4 text-right">Ações</th>
                        </tr>
                    </thead>
                    <tbody class="divide-y divide-[#323238]">
                        <tr v-if="isLoading">
                            <td colspan="5" class="px-6 py-8 text-center text-gray-400">Carregando planos...</td>
                        </tr>
                        <tr v-else-if="plans.length === 0">
                            <td colspan="5" class="px-6 py-8 text-center text-gray-400">Nenhum plano encontrado.</td>
                        </tr>
                        <tr v-for="plan in plans" :key="plan.id" class="hover:bg-[#29292E] transition-colors">
                            <td class="px-6 py-4">
                                <div class="font-bold text-white text-base">{{ plan.name }}</div>
                                <div class="text-xs opacity-70">{{ plan.description }}</div>
                            </td>
                            <td class="px-6 py-4 font-mono text-white">
                                {{ plan.price === 0 ? 'Grátis' : `R$ ${plan.price.toFixed(2)}` }}
                            </td>
                            <td class="px-6 py-4">
                                <div class="flex flex-wrap gap-1">
                                    <span v-for="feature in plan.features" :key="feature.id" class="px-2 py-0.5 bg-[#121214] border border-[#323238] rounded text-xs text-gray-300">
                                        {{ feature.label }}
                                    </span>
                                    <span v-if="plan.features.length === 0" class="text-gray-600 text-xs italic">Nenhuma</span>
                                </div>
                            </td>
                            <td class="px-6 py-4 text-center">
                                <Star v-if="plan.isDefault" class="w-5 h-5 text-yellow-500 mx-auto fill-current" />
                                <span v-else class="text-gray-600">-</span>
                            </td>
                            <td class="px-6 py-4 text-right">
                                <div class="flex items-center justify-end gap-2">
                                    <button @click="openEditModal(plan)" class="p-2 hover:bg-[#323238] rounded-lg text-gray-400 hover:text-white transition-colors" title="Editar">
                                        <Edit class="w-4 h-4" />
                                    </button>
                                    <button @click="deletePlan(plan.id)" class="p-2 hover:bg-[#F75A68]/10 rounded-lg text-gray-400 hover:text-[#F75A68] transition-colors" title="Excluir">
                                        <Trash2 class="w-4 h-4" />
                                    </button>
                                </div>
                            </td>
                        </tr>
                    </tbody>
                </table>
            </div>
        </div>
    </div>

    <!-- Modal -->
    <div v-if="isModalOpen" class="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/50 backdrop-blur-sm">
        <div class="bg-[#202024] w-full max-w-lg rounded-2xl border border-[#323238] shadow-2xl p-6 max-h-[90vh] overflow-y-auto">
            <div class="flex justify-between items-center mb-6">
                <h3 class="text-xl font-bold text-white">{{ isEditing ? 'Editar Plano' : 'Novo Plano' }}</h3>
                <button @click="closeModal" class="text-gray-400 hover:text-white transition-colors">
                    <X class="w-6 h-6" />
                </button>
            </div>

            <form @submit.prevent="savePlan" class="space-y-4">
                <div v-if="errorMessage" class="p-3 bg-red-500/10 border border-red-500/20 rounded-lg text-red-500 text-sm">
                    {{ errorMessage }}
                </div>

                <div class="space-y-1.5">
                    <label class="text-sm font-medium text-gray-200">Nome do Plano</label>
                    <input v-model="form.name" type="text" required class="w-full bg-[#121214] border border-[#323238] rounded-lg px-4 py-2.5 text-white focus:outline-none focus:border-[#00875F] transition-colors" />
                </div>

                <div class="space-y-1.5">
                    <label class="text-sm font-medium text-gray-200">Descrição</label>
                    <textarea v-model="form.description" rows="2" class="w-full bg-[#121214] border border-[#323238] rounded-lg px-4 py-2.5 text-white focus:outline-none focus:border-[#00875F] transition-colors resize-none"></textarea>
                </div>

                <div class="space-y-1.5">
                    <label class="text-sm font-medium text-gray-200">Preço (R$)</label>
                    <input v-model="form.price" type="number" step="0.01" min="0" required class="w-full bg-[#121214] border border-[#323238] rounded-lg px-4 py-2.5 text-white focus:outline-none focus:border-[#00875F] transition-colors" />
                </div>

                <div class="flex items-center space-x-3 py-2">
                    <input id="isDefault" v-model="form.isDefault" type="checkbox" class="h-4 w-4 text-[#00875F] focus:ring-[#00875F] border-[#323238] rounded bg-[#121214]">
                    <label for="isDefault" class="text-sm font-medium text-gray-200">Definir como Plano Padrão</label>
                </div>

                <div class="border-t border-[#323238] pt-4 mt-2">
                    <h4 class="text-sm font-bold text-gray-200 mb-3 block">Funcionalidades</h4>
                    <div class="grid grid-cols-1 gap-2 max-h-40 overflow-y-auto pr-1">
                        <div v-for="feature in systemFeatures" :key="feature.id" 
                            @click="toggleFeature(feature.id)"
                            class="flex items-start gap-3 p-2 rounded-lg cursor-pointer transition-colors"
                            :class="form.featureIds.includes(feature.id) ? 'bg-[#00875F]/10 border border-[#00875F]/30' : 'bg-[#121214] border border-[#323238] hover:border-gray-500'"
                        >
                            <div class="mt-0.5 w-4 h-4 rounded border flex items-center justify-center transition-colors"
                                :class="form.featureIds.includes(feature.id) ? 'bg-[#00875F] border-[#00875F]' : 'border-gray-500'"
                            >
                                <Check v-if="form.featureIds.includes(feature.id)" class="w-3 h-3 text-white" />
                            </div>
                            <div>
                                <div class="text-sm font-medium text-gray-200">{{ feature.label }}</div>
                                <div class="text-xs text-gray-500">{{ feature.description }}</div>
                            </div>
                        </div>
                    </div>
                </div>

                <div class="pt-4 flex justify-end gap-3">
                    <button type="button" @click="closeModal" class="px-4 py-2 text-sm font-medium text-gray-300 hover:text-white transition-colors">
                        Cancelar
                    </button>
                    <button type="submit" class="bg-[#00875F] hover:bg-[#00B37E] text-white px-5 py-2 rounded-lg font-medium transition-colors">
                        Salvar Plano
                    </button>
                </div>
            </form>
        </div>
    </div>
</template>
