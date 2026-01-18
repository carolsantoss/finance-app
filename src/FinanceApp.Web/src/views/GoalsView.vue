<script setup lang="ts">
import { ref, onMounted, computed } from 'vue';
import api from '../api/axios';
import { useToastStore } from '../stores/toast';
import { useAuthStore } from '../stores/auth';
import { Plus, Target, Calendar, DollarSign, Trash2, Edit2, TrendingUp, Users } from 'lucide-vue-next';
import GoalShareModal from '../components/GoalShareModal.vue';

interface Goal {
    id_goal: number;
    nm_titulo: string;
    nr_valorObjetivo: number;
    nr_valorAtual: number;
    dt_prazo: string | null;
    id_usuario: number;
}

const auth = useAuthStore();
const goals = ref<Goal[]>([]);
const isLoading = ref(true);
const toast = useToastStore();
const showModal = ref(false);
const showShareModal = ref(false);
const selectedShareGoalId = ref(0);
const isSaving = ref(false);
const isEditing = ref(false);

const form = ref({
    id_goal: 0,
    nm_titulo: '',
    nr_valorObjetivo: 0,
    nr_valorAtual: 0,
    dt_prazo: ''
});

const fetchGoals = async () => {
    isLoading.value = true;
    try {
        const response = await api.get('/goals');
        goals.value = response.data;
    } catch (error) {
        toast.error('Erro ao carregar metas.');
    } finally {
        isLoading.value = false;
    }
};

const openNewGoalModal = () => {
    isEditing.value = false;
    form.value = { id_goal: 0, nm_titulo: '', nr_valorObjetivo: 0, nr_valorAtual: 0, dt_prazo: '' };
    showModal.value = true;
};

const openShareModal = (goal: Goal) => {
    selectedShareGoalId.value = goal.id_goal;
    showShareModal.value = true;
};

const openEditModal = (goal: Goal) => {
    isEditing.value = true;
    form.value = {
        id_goal: goal.id_goal,
        nm_titulo: goal.nm_titulo,
        nr_valorObjetivo: goal.nr_valorObjetivo,
        nr_valorAtual: goal.nr_valorAtual,
        dt_prazo: goal.dt_prazo ? goal.dt_prazo.split('T')[0] : ''
    };
    showModal.value = true;
};

const saveGoal = async () => {
    isSaving.value = true;
    try {
        const payload = {
            id_goal: form.value.id_goal,
            nm_titulo: form.value.nm_titulo,
            nr_valorObjetivo: form.value.nr_valorObjetivo,
            nr_valorAtual: form.value.nr_valorAtual,
            dt_prazo: form.value.dt_prazo || null
        };

        if (isEditing.value) {
            await api.put(`/goals/${form.value.id_goal}`, payload);
            toast.success('Meta atualizada com sucesso!');
        } else {
            await api.post('/goals', payload);
            toast.success('Meta criada com sucesso!');
        }
        showModal.value = false;
        fetchGoals();
    } catch (error) {
        toast.error('Erro ao salvar meta.');
    } finally {
        isSaving.value = false;
    }
};

const deleteGoal = async (id: number) => {
    if (!confirm('Tem certeza que deseja excluir esta meta?')) return;
    try {
        await api.delete(`/goals/${id}`);
        toast.success('Meta excluída.');
        fetchGoals();
    } catch (error) {
        toast.error('Erro ao excluir meta.');
    }
};

const calculateProgress = (current: number, target: number) => {
    if (target <= 0) return 0;
    const p = (current / target) * 100;
    return Math.min(p, 100).toFixed(0);
};

onMounted(fetchGoals);
</script>

<template>
    <div class="h-full flex flex-col p-6 space-y-6 overflow-y-auto bg-app">
        <!-- Header -->
        <div class="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4">
            <div>
                <h1 class="text-2xl font-bold text-text-primary flex items-center gap-2">
                    <Target class="w-8 h-8 text-brand" />
                    Metas Financeiras
                </h1>
                <p class="text-text-secondary">Defina objetivos e acompanhe seu progresso.</p>
            </div>
            <button @click="openNewGoalModal" class="bg-brand hover:bg-brand-hover text-white px-4 py-2 rounded-lg font-bold flex items-center gap-2 transition-all shadow-lg shadow-brand/20">
                <Plus class="w-5 h-5" />
                Nova Meta
            </button>
        </div>

        <!-- Content -->
        <div v-if="isLoading" class="flex justify-center items-center h-64">
            <div class="animate-spin rounded-full h-12 w-12 border-b-2 border-brand"></div>
        </div>

        <div v-else-if="goals.length === 0" class="flex flex-col items-center justify-center h-64 text-text-secondary opacity-60">
            <Target class="w-16 h-16 mb-4" />
            <p>Nenhuma meta cadastrada.</p>
        </div>

        <div v-else class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-6">
            <div v-for="goal in goals" :key="goal.id_goal" class="bg-card border border-border rounded-xl p-6 shadow-sm hover:shadow-md transition-all group relative overflow-hidden">
                <!-- Progress Background -->
                <div class="absolute bottom-0 left-0 h-1 bg-brand transition-all" :style="{ width: calculateProgress(goal.nr_valorAtual, goal.nr_valorObjetivo) + '%' }"></div>
                
                <div class="flex justify-between items-start mb-4">
                    <div class="p-3 bg-brand/10 rounded-lg text-brand">
                        <Target class="w-6 h-6" />
                    </div>
                     <div v-if="goal.id_usuario !== auth.user?.id" class="absolute top-6 right-16 px-2 py-0.5 bg-brand text-white text-[10px] font-bold rounded uppercase">
                        Compartilhado
                    </div>
                    <div class="flex gap-2 opacity-0 group-hover:opacity-100 transition-opacity">
                         <button @click="openShareModal(goal)" class="p-2 hover:bg-hover rounded-md text-text-secondary hover:text-brand transition-colors" title="Compartilhar">
                            <Users class="w-4 h-4" />
                        </button>
                        <button @click="openEditModal(goal)" class="p-2 hover:bg-hover rounded-md text-text-secondary hover:text-brand transition-colors">
                            <Edit2 class="w-4 h-4" />
                        </button>
                        <button @click="deleteGoal(goal.id_goal)" class="p-2 hover:bg-hover rounded-md text-text-secondary hover:text-danger transition-colors">
                            <Trash2 class="w-4 h-4" />
                        </button>
                    </div>
                </div>

                <h3 class="text-lg font-bold text-text-primary mb-1">{{ goal.nm_titulo }}</h3>
                
                <div class="space-y-4 mt-4">
                    <!-- Values -->
                    <div class="flex justify-between items-end">
                        <div>
                            <p class="text-xs text-text-secondary">Guardado</p>
                            <p class="text-xl font-bold text-success">R$ {{ goal.nr_valorAtual.toLocaleString('pt-BR', { minimumFractionDigits: 2 }) }}</p>
                        </div>
                        <div class="text-right">
                             <p class="text-xs text-text-secondary">Meta</p>
                             <p class="text-sm font-bold text-text-primary">R$ {{ goal.nr_valorObjetivo.toLocaleString('pt-BR', { minimumFractionDigits: 2 }) }}</p>
                        </div>
                    </div>

                    <!-- Progress Bar -->
                    <div class="w-full bg-input rounded-full h-2.5 overflow-hidden">
                        <div class="bg-brand h-2.5 rounded-full transition-all duration-500" :style="{ width: calculateProgress(goal.nr_valorAtual, goal.nr_valorObjetivo) + '%' }"></div>
                    </div>
                    <div class="flex justify-between text-xs font-bold">
                        <span class="text-brand">{{ calculateProgress(goal.nr_valorAtual, goal.nr_valorObjetivo) }}%</span>
                        <span v-if="goal.dt_prazo" class="text-text-secondary flex items-center gap-1">
                            <Calendar class="w-3 h-3" />
                            {{ new Date(goal.dt_prazo).toLocaleDateString() }}
                        </span>
                    </div>
                </div>
            </div>
        </div>

        <!-- Modal -->
        <div v-if="showModal" class="fixed inset-0 z-50 flex items-center justify-center bg-black/50 backdrop-blur-sm p-4 animate-in fade-in duration-200">
            <div class="bg-card w-full max-w-md rounded-xl shadow-2xl border border-border flex flex-col max-h-[90vh]">
                <div class="p-6 border-b border-border flex justify-between items-center">
                    <h2 class="text-xl font-bold text-text-primary">{{ isEditing ? 'Editar Meta' : 'Nova Meta' }}</h2>
                    <button @click="showModal = false" class="text-text-secondary hover:text-text-primary text-2xl">&times;</button>
                </div>
                
                <div class="p-6 space-y-4 overflow-y-auto">
                    <div class="space-y-2">
                        <label class="text-sm font-bold text-text-secondary">Nome do Objetivo</label>
                        <input v-model="form.nm_titulo" type="text" placeholder="Ex: Viagem para Europa" class="w-full bg-input border border-border rounded-lg p-3 text-text-primary focus:border-brand outline-none" autoFocus />
                    </div>

                    <div class="grid grid-cols-2 gap-4">
                        <div class="space-y-2">
                            <label class="text-sm font-bold text-text-secondary">Valor Alvo (R$)</label>
                            <input v-model="form.nr_valorObjetivo" type="number" step="0.01" class="w-full bg-input border border-border rounded-lg p-3 text-text-primary focus:border-brand outline-none" />
                        </div>
                         <div class="space-y-2">
                            <label class="text-sm font-bold text-text-secondary">Guardado (R$)</label>
                            <input v-model="form.nr_valorAtual" type="number" step="0.01" class="w-full bg-input border border-border rounded-lg p-3 text-text-primary focus:border-brand outline-none" />
                        </div>
                    </div>

                    <div class="space-y-2">
                         <label class="text-sm font-bold text-text-secondary">Prazo (Opcional)</label>
                        <input v-model="form.dt_prazo" type="date" class="w-full bg-input border border-border rounded-lg p-3 text-text-primary focus:border-brand outline-none" />
                    </div>
                </div>

                <div class="p-6 border-t border-border flex justify-end gap-3">
                    <button @click="showModal = false" class="px-4 py-2 rounded-lg text-text-secondary hover:bg-hover font-bold">Cancelar</button>
                    <button @click="saveGoal" :disabled="isSaving" class="px-6 py-2 bg-brand text-white rounded-lg font-bold hover:bg-brand-hover disabled:opacity-50">
                        {{ isSaving ? 'Salvando...' : 'Salvar' }}
                    </button>
                </div>
            </div>
        </div>



        <GoalShareModal 
            v-if="showShareModal" 
            :goalId="selectedShareGoalId" 
            @close="showShareModal = false" 
        />

    </div>
</template>
