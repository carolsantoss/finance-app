<script setup lang="ts">
import { ref, onMounted, computed } from 'vue';
import { useCategoryStore, Category } from '../../stores/category';
import { 
    Tag, 
    Plus, 
    Trash2, 
    Edit, 
    Lock,
    Utensils, Car, PartyPopper, HeartPulse, GraduationCap, Home, MoreHorizontal, Banknote, TrendingUp, Laptop, Gift // Import standard icons for display mapping if needed or use dynamic component
} from 'lucide-vue-next';
import * as LucideIcons from 'lucide-vue-next'; // Dynamic import for icons

const categoryStore = useCategoryStore();
const isModalOpen = ref(false);
const isEditing = ref(false);
const errorMessage = ref('');
const activeTab = ref<'Saída' | 'Entrada'>('Saída');

// Dynamic Icon Component wrapper
const getIcon = (name: string) => {
    // @ts-ignore
    return LucideIcons[name] || LucideIcons.Tag;
};

const form = ref<Partial<Category>>({
    id_categoria: 0,
    nm_nome: '',
    nm_icone: 'Tag', // default
    nm_cor: '#F75A68',
    nm_tipo: 'Saída'
});

// Available selection options
const availableIcons = ['Utensils', 'Car', 'PartyPopper', 'HeartPulse', 'GraduationCap', 'Home', 'ShoppingBag', 'Smartphone', 'Wifi', 'Zap', 'Gift', 'Briefcase', 'DollarSign', 'Coffee', 'Music', 'Book', 'Smile', 'Tag'];
const availableColors = ['#F75A68', '#00B37E', '#AB222E', '#00875F', '#FBA94C', '#115D33', '#8257e5', '#323238'];

const filteredCategories = computed(() => {
    return categoryStore.categories.filter(c => c.nm_tipo === activeTab.value);
});

const openCreateModal = () => {
    isEditing.value = false;
    form.value = { 
        id_categoria: 0, 
        nm_nome: '', 
        nm_icone: 'Tag', 
        nm_cor: activeTab.value === 'Saída' ? '#F75A68' : '#00B37E', 
        nm_tipo: activeTab.value 
    };
    isModalOpen.value = true;
    errorMessage.value = '';
};

const openEditModal = (category: Category) => {
    if (category.id_usuario === null) return; // Guard against editing system categories
    isEditing.value = true;
    form.value = { ...category };
    isModalOpen.value = true;
    errorMessage.value = '';
};

const closeModal = () => {
    isModalOpen.value = false;
};

const saveCategory = async () => {
    try {
        if (isEditing.value && form.value.id_categoria) {
            await categoryStore.updateCategory(form.value.id_categoria, form.value);
        } else {
            await categoryStore.createCategory(form.value);
        }
        closeModal();
    } catch (error: any) {
        errorMessage.value = error.response?.data || 'Erro ao salvar categoria';
    }
};

const deleteCategory = async (id: number) => {
    if (!confirm('Tem certeza? Isso pode falhar se houver transações vinculadas.')) return;
    try {
        await categoryStore.deleteCategory(id);
    } catch (error: any) {
        alert(error.response?.data || 'Erro ao excluir categoria');
    }
};

onMounted(() => {
    categoryStore.fetchCategories();
});
</script>

<template>
    <div class="h-full flex flex-col p-6 space-y-6 overflow-hidden">
        <!-- Header -->
        <div class="flex flex-col sm:flex-row justify-between items-start sm:items-center gap-4">
            <div>
                <h2 class="text-2xl font-bold text-white flex items-center gap-2">
                    <Tag class="w-6 h-6 text-[#00875F]" />
                    Categorias
                </h2>
                <p class="text-gray-400">Gerencie as categorias de suas transações.</p>
            </div>
            <button @click="openCreateModal" class="flex items-center gap-2 bg-[#00875F] hover:bg-[#00B37E] text-white px-5 py-2.5 rounded-lg shadow-lg shadow-[#00875F]/20 transition-all font-medium">
                <Plus class="w-5 h-5" />
                Nova Categoria
            </button>
        </div>

        <!-- Tabs -->
        <div class="flex gap-4 border-b border-[#323238]">
            <button 
                @click="activeTab = 'Saída'"
                class="px-4 py-2 text-sm font-medium transition-colors relative"
                :class="activeTab === 'Saída' ? 'text-white' : 'text-gray-400 hover:text-gray-200'"
            >
                Despesas
                <div v-if="activeTab === 'Saída'" class="absolute bottom-0 left-0 w-full h-0.5 bg-[#F75A68]"></div>
            </button>
            <button 
                @click="activeTab = 'Entrada'"
                class="px-4 py-2 text-sm font-medium transition-colors relative"
                :class="activeTab === 'Entrada' ? 'text-white' : 'text-gray-400 hover:text-gray-200'"
            >
                Receitas
                <div v-if="activeTab === 'Entrada'" class="absolute bottom-0 left-0 w-full h-0.5 bg-[#00B37E]"></div>
            </button>
        </div>

        <!-- Content Grid -->
        <div class="flex-1 overflow-y-auto pr-2">
            <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 xl:grid-cols-4 gap-4">
                
                <!-- System Categories (Locked) -->
                <div v-for="category in filteredCategories.filter(c => c.id_usuario === null)" :key="'sys-' + category.id_categoria" 
                     class="bg-[#29292E] p-4 rounded-xl border border-[#323238] flex items-center justify-between opacity-80 hover:opacity-100 transition-opacity">
                    <div class="flex items-center gap-3">
                        <div class="w-10 h-10 rounded-full flex items-center justify-center text-white" :style="{ backgroundColor: category.nm_cor }">
                            <component :is="getIcon(category.nm_icone)" class="w-5 h-5" />
                        </div>
                        <span class="font-medium text-white">{{ category.nm_nome }}</span>
                    </div>
                    <Lock class="w-4 h-4 text-gray-500" title="Categoria do Sistema" />
                </div>

                <!-- User Categories (Editable) -->
                <div v-for="category in filteredCategories.filter(c => c.id_usuario !== null)" :key="'usr-' + category.id_categoria" 
                     class="bg-[#202024] p-4 rounded-xl border border-[#323238] flex items-center justify-between group hover:border-[#00875F] transition-colors">
                    <div class="flex items-center gap-3">
                        <div class="w-10 h-10 rounded-full flex items-center justify-center text-white" :style="{ backgroundColor: category.nm_cor }">
                            <component :is="getIcon(category.nm_icone)" class="w-5 h-5" />
                        </div>
                        <span class="font-medium text-white">{{ category.nm_nome }}</span>
                    </div>
                    <div class="flex items-center gap-2 opacity-0 group-hover:opacity-100 transition-opacity">
                        <button @click="openEditModal(category)" class="p-1.5 text-gray-400 hover:text-white rounded hover:bg-[#323238]">
                            <Edit class="w-4 h-4" />
                        </button>
                        <button @click="deleteCategory(category.id_categoria)" class="p-1.5 text-gray-400 hover:text-[#F75A68] rounded hover:bg-[#F75A68]/10">
                            <Trash2 class="w-4 h-4" />
                        </button>
                    </div>
                </div>

            </div>
        </div>
    </div>

    <!-- Modal -->
    <div v-if="isModalOpen" class="fixed inset-0 z-50 flex items-center justify-center p-4 bg-black/50 backdrop-blur-sm">
        <div class="bg-[#202024] w-full max-w-md rounded-2xl border border-[#323238] shadow-2xl p-6">
            <div class="flex justify-between items-center mb-6">
                <h3 class="text-xl font-bold text-white">{{ isEditing ? 'Editar Categoria' : 'Nova Categoria' }}</h3>
                <button @click="closeModal" class="text-gray-400 hover:text-white transition-colors">
                    <X class="w-6 h-6" /> <!-- X needs to be imported or used from LucideIcons -->
                </button>
            </div>

            <form @submit.prevent="saveCategory" class="space-y-4">
                <div v-if="errorMessage" class="p-3 bg-red-500/10 border border-red-500/20 rounded-lg text-red-500 text-sm">
                    {{ errorMessage }}
                </div>

                <div class="space-y-1.5">
                    <label class="text-sm font-medium text-gray-200">Nome</label>
                    <input v-model="form.nm_nome" type="text" required class="w-full bg-[#121214] border border-[#323238] rounded-lg px-4 py-2.5 text-white focus:outline-none focus:border-[#00875F] transition-colors" />
                </div>

                <div class="space-y-1.5">
                    <label class="text-sm font-medium text-gray-200">Ícone</label>
                    <div class="grid grid-cols-6 gap-2 bg-[#121214] p-3 rounded-lg border border-[#323238]">
                        <button 
                            type="button"
                            v-for="icon in availableIcons" 
                            :key="icon"
                            @click="form.nm_icone = icon"
                            class="w-8 h-8 rounded flex items-center justify-center transition-colors"
                            :class="form.nm_icone === icon ? 'bg-[#00875F] text-white' : 'text-gray-400 hover:text-white hover:bg-[#202024]'"
                        >
                            <component :is="getIcon(icon)" class="w-5 h-5" />
                        </button>
                    </div>
                </div>

                <div class="space-y-1.5">
                    <label class="text-sm font-medium text-gray-200">Cor</label>
                    <div class="flex gap-2 flex-wrap bg-[#121214] p-3 rounded-lg border border-[#323238]">
                        <button 
                            type="button"
                            v-for="color in availableColors" 
                            :key="color"
                            @click="form.nm_cor = color"
                            class="w-6 h-6 rounded-full border-2 transition-transform hover:scale-110"
                            :class="form.nm_cor === color ? 'border-white ring-2 ring-[#00875F]' : 'border-transparent'"
                            :style="{ backgroundColor: color }"
                        >
                        </button>
                    </div>
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
