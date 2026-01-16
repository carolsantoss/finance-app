<script setup lang="ts">
import { ref, onMounted } from 'vue';
import { 
    Users, 
    UserPlus, 
    Trash2, 
    Edit, 
    Check, 
    X,
    Shield
} from 'lucide-vue-next';
import api from '../../api/axios';

interface User {
    id: number;
    nomeUsuario: string;
    email: string;
    isAdmin: boolean;
}

const users = ref<User[]>([]);
const isLoading = ref(false);
const isModalOpen = ref(false);
const isEditing = ref(false);
const errorMessage = ref('');

// Form state
const form = ref({
    id: 0,
    nomeUsuario: '',
    email: '',
    senha: '',
    isAdmin: false
});

const fetchUsers = async () => {
    isLoading.value = true;
    try {
        const response = await api.get('/users');
        users.value = response.data;
    } catch (error) {
        console.error('Failed to fetch users', error);
    } finally {
        isLoading.value = false;
    }
};

const openCreateModal = () => {
    isEditing.value = false;
    form.value = { id: 0, nomeUsuario: '', email: '', senha: '', isAdmin: false };
    isModalOpen.value = true;
    errorMessage.value = '';
};

const openEditModal = (user: User) => {
    isEditing.value = true;
    form.value = { 
        id: user.id, 
        nomeUsuario: user.nomeUsuario, 
        email: user.email, 
        senha: '', // Don't fill password
        isAdmin: user.isAdmin 
    };
    isModalOpen.value = true;
    errorMessage.value = '';
};

const closeModal = () => {
    isModalOpen.value = false;
};

const saveUser = async () => {
    try {
        if (isEditing.value) {
            await api.put(`/users/${form.value.id}`, form.value);
        } else {
            await api.post('/users', form.value);
        }
        await fetchUsers();
        closeModal();
    } catch (error: any) {
        errorMessage.value = error.response?.data || 'Erro ao salvar usuário';
    }
};

const deleteUser = async (id: number) => {
    if (!confirm('Tem certeza que deseja excluir este usuário?')) return;
    
    try {
        await api.delete(`/users/${id}`);
        await fetchUsers();
    } catch (error: any) {
        alert(error.response?.data || 'Erro ao excluir usuário');
    }
};

onMounted(() => {
    fetchUsers();
});
</script>

<template>
    <div class="h-full flex flex-col p-6 space-y-6 overflow-hidden">
        <!-- Header -->
        <div class="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4">
            <div>
                <h2 class="text-2xl font-bold text-white flex items-center gap-2">
                    <Shield class="w-6 h-6 text-[#00875F]" />
                    Administração de Usuários
                </h2>
                <p class="text-gray-400">Gerencie o acesso e permissões do sistema.</p>
            </div>
            <button @click="openCreateModal" class="flex items-center gap-2 bg-[#00875F] hover:bg-[#00B37E] text-white px-5 py-2.5 rounded-lg shadow-lg shadow-[#00875F]/20 transition-all font-medium">
                <UserPlus class="w-5 h-5" />
                Novo Usuário
            </button>
        </div>

        <!-- Content -->
        <div class="flex-1 bg-[#202024] rounded-2xl border border-[#323238] shadow-xl overflow-hidden flex flex-col">
            <div class="p-6 border-b border-[#323238]">
                <h3 class="text-lg font-bold text-white">Usuários Cadastrados</h3>
            </div>
            
            <div class="overflow-x-auto flex-1">
                <table class="w-full text-left text-sm text-gray-400">
                    <thead class="bg-[#29292E] text-xs uppercase font-medium">
                        <tr>
                            <th class="px-6 py-4">ID</th>
                            <th class="px-6 py-4">Nome</th>
                            <th class="px-6 py-4">Email</th>
                            <th class="px-6 py-4 text-center">Admin</th>
                            <th class="px-6 py-4 text-right">Ações</th>
                        </tr>
                    </thead>
                    <tbody class="divide-y divide-[#323238]">
                        <tr v-if="isLoading">
                            <td colspan="5" class="px-6 py-8 text-center text-gray-400">Carregando usuários...</td>
                        </tr>
                        <tr v-else-if="users.length === 0">
                            <td colspan="5" class="px-6 py-8 text-center text-gray-400">Nenhum usuário encontrado.</td>
                        </tr>
                        <tr v-for="user in users" :key="user.id" class="hover:bg-[#29292E] transition-colors">
                            <td class="px-6 py-4 text-gray-500">#{{ user.id }}</td>
                            <td class="px-6 py-4 font-medium text-white">{{ user.nomeUsuario }}</td>
                            <td class="px-6 py-4">{{ user.email }}</td>
                            <td class="px-6 py-4 text-center">
                                <span v-if="user.isAdmin" class="inline-flex items-center px-2.5 py-0.5 rounded-full text-xs font-medium bg-[#00875F]/10 text-[#00B37E] border border-[#00875F]/20">
                                    Sim
                                </span>
                                <span v-else class="text-gray-600">-</span>
                            </td>
                            <td class="px-6 py-4 text-right">
                                <div class="flex items-center justify-end gap-2">
                                    <button @click="openEditModal(user)" class="p-2 hover:bg-[#323238] rounded-lg text-gray-400 hover:text-white transition-colors" title="Editar">
                                        <Edit class="w-4 h-4" />
                                    </button>
                                    <button @click="deleteUser(user.id)" class="p-2 hover:bg-[#F75A68]/10 rounded-lg text-gray-400 hover:text-[#F75A68] transition-colors" title="Excluir">
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
        <div class="bg-[#202024] w-full max-w-md rounded-2xl border border-[#323238] shadow-2xl p-6">
            <div class="flex justify-between items-center mb-6">
                <h3 class="text-xl font-bold text-white">{{ isEditing ? 'Editar Usuário' : 'Novo Usuário' }}</h3>
                <button @click="closeModal" class="text-gray-400 hover:text-white transition-colors">
                    <X class="w-6 h-6" />
                </button>
            </div>

            <form @submit.prevent="saveUser" class="space-y-4">
                <div v-if="errorMessage" class="p-3 bg-red-500/10 border border-red-500/20 rounded-lg text-red-500 text-sm">
                    {{ errorMessage }}
                </div>

                <div class="space-y-1.5">
                    <label class="text-sm font-medium text-gray-200">Nome de Usuário</label>
                    <input v-model="form.nomeUsuario" type="text" required class="w-full bg-[#121214] border border-[#323238] rounded-lg px-4 py-2.5 text-white focus:outline-none focus:border-[#00875F] transition-colors" />
                </div>

                <div class="space-y-1.5">
                    <label class="text-sm font-medium text-gray-200">Email</label>
                    <input v-model="form.email" type="email" required class="w-full bg-[#121214] border border-[#323238] rounded-lg px-4 py-2.5 text-white focus:outline-none focus:border-[#00875F] transition-colors" />
                </div>

                <div class="space-y-1.5">
                    <label class="text-sm font-medium text-gray-200">Senha {{ isEditing ? '(deixe em branco para manter)' : '' }}</label>
                    <input v-model="form.senha" type="password" :required="!isEditing" class="w-full bg-[#121214] border border-[#323238] rounded-lg px-4 py-2.5 text-white focus:outline-none focus:border-[#00875F] transition-colors" />
                </div>

                 <div class="flex items-center space-x-3 pt-2">
                    <input id="isAdmin" v-model="form.isAdmin" type="checkbox" class="h-4 w-4 text-[#00875F] focus:ring-[#00875F] border-[#323238] rounded bg-[#121214]">
                    <label for="isAdmin" class="text-sm font-medium text-gray-200">Administrador</label>
                </div>

                <div class="pt-4 flex justify-end gap-3">
                    <button type="button" @click="closeModal" class="px-4 py-2 text-sm font-medium text-gray-300 hover:text-white transition-colors">
                        Cancelar
                    </button>
                    <button type="submit" class="bg-[#00875F] hover:bg-[#00B37E] text-white px-5 py-2 rounded-lg font-medium transition-colors">
                        Salvar
                    </button>
                </div>
            </form>
        </div>
    </div>
</template>
