<script setup lang="ts">
import { ref, onMounted } from 'vue';
import api from '../api/axios';
import { useToastStore } from '../stores/toast';
import { UserPlus, User, X, Mail } from 'lucide-vue-next';

const props = defineProps({
    goalId: {
        type: Number,
        required: true
    }
});

const emit = defineEmits(['close']);
const toast = useToastStore();
const isLoading = ref(true);
const inviteEmail = ref('');
const isInviting = ref(false);

interface Member {
    id: number;
    userId: number;
    userName: string;
    email: string;
    role: string;
    totalContribution: number;
}

interface GoalDetail {
    id: number;
    titulo: string;
    members: Member[];
}

const goalData = ref<GoalDetail | null>(null);

const fetchDetails = async () => {
    isLoading.value = true;
    try {
        const response = await api.get(`/goals/${props.goalId}`);
        goalData.value = response.data;
    } catch (error) {
        toast.error('Erro ao carregar detalhes da meta.');
        emit('close');
    } finally {
        isLoading.value = false;
    }
};

const inviteUser = async () => {
    if (!inviteEmail.value) return;
    isInviting.value = true;
    try {
        await api.post(`/goals/${props.goalId}/invite`, { email: inviteEmail.value });
        toast.success('Convite enviado com sucesso!');
        inviteEmail.value = '';
        await fetchDetails(); // Refresh list
    } catch (error: any) {
        if (error.response && error.response.data) {
             toast.error(typeof error.response.data === 'string' ? error.response.data : 'Erro ao convidar usuário.');
        } else {
             toast.error('Erro ao convidar usuário.');
        }
    } finally {
        isInviting.value = false;
    }
};

const formatCurrency = (value: number) => {
    return new Intl.NumberFormat('pt-BR', { style: 'currency', currency: 'BRL' }).format(value);
};

onMounted(() => {
    fetchDetails();
});
</script>

<template>
    <div class="fixed inset-0 z-50 flex items-center justify-center bg-black/50 backdrop-blur-sm p-4 animate-in fade-in duration-200">
        <div class="bg-card w-full max-w-md rounded-xl shadow-2xl border border-border flex flex-col max-h-[90vh]">
            <div class="p-6 border-b border-border flex justify-between items-center">
                <h2 class="text-xl font-bold text-text-primary">Compartilhar Meta</h2>
                <button @click="$emit('close')" class="text-text-secondary hover:text-text-primary text-2xl">&times;</button>
            </div>

            <div v-if="isLoading" class="p-12 flex justify-center">
                <div class="animate-spin rounded-full h-8 w-8 border-b-2 border-brand"></div>
            </div>

            <div v-else class="p-6 space-y-6 overflow-y-auto">
                <div>
                     <h3 class="font-bold text-text-primary mb-1">{{ goalData?.titulo }}</h3>
                     <p class="text-sm text-text-secondary">Gerencie quem contribui com esta meta.</p>
                </div>

                <!-- Invite Form -->
                <div class="flex gap-2">
                    <div class="relative flex-1">
                        <Mail class="w-4 h-4 absolute left-3 top-3.5 text-text-secondary" />
                        <input 
                            v-model="inviteEmail" 
                            type="email" 
                            placeholder="Email do usuário" 
                            class="w-full bg-input border border-border rounded-lg pl-10 pr-3 py-3 text-text-primary focus:border-brand outline-none" 
                            @keyup.enter="inviteUser"
                        />
                    </div>
                    <button 
                        @click="inviteUser" 
                        :disabled="isInviting || !inviteEmail"
                        class="bg-brand hover:bg-brand-hover text-white px-4 rounded-lg font-bold disabled:opacity-50 disabled:cursor-not-allowed transition-colors"
                    >
                        <UserPlus class="w-5 h-5" v-if="!isInviting" />
                        <span v-else class="animate-spin rounded-full h-4 w-4 border-b-2 border-white block"></span>
                    </button>
                </div>

                <!-- Members List -->
                <div class="space-y-3">
                    <h4 class="text-sm font-bold text-text-secondary uppercase">Membros</h4>
                    
                    <div v-for="member in goalData?.members" :key="member.id" class="flex justify-between items-center bg-hover/50 p-3 rounded-lg border border-border">
                        <div class="flex items-center gap-3">
                            <div class="bg-card p-2 rounded-full border border-border">
                                <User class="w-4 h-4 text-brand" />
                            </div>
                            <div>
                                <p class="text-sm font-bold text-text-primary">{{ member.userName }} <span v-if="member.role === 'Owner'" class="text-[10px] bg-brand/20 text-brand px-1.5 py-0.5 rounded ml-1">DONO</span></p>
                                <p class="text-xs text-text-secondary">{{ member.email }}</p>
                            </div>
                        </div>
                        <div class="text-right">
                             <p class="text-xs text-text-secondary">Contribuiu</p>
                             <p class="text-sm font-bold text-success">{{ formatCurrency(member.totalContribution) }}</p>
                        </div>
                    </div>
                </div>
            </div>
            
            <div class="p-6 border-t border-border flex justify-end">
                <button @click="$emit('close')" class="px-4 py-2 bg-hover hover:bg-opacity-80 rounded-lg text-text-primary font-bold transition-colors">Fechar</button>
            </div>
        </div>
    </div>
</template>
