<script setup lang="ts">
import { useToastStore } from '../../stores/toast';
import type { ToastType } from '../../stores/toast';
import { X, CheckCircle, AlertCircle, Info, AlertTriangle } from 'lucide-vue-next';

const store = useToastStore();

const icons: Record<ToastType, any> = {
    success: CheckCircle,
    error: AlertCircle,
    info: Info,
    warning: AlertTriangle
};

const colors: Record<ToastType, string> = {
    success: 'bg-[#121214] border-[#00875F] text-white',
    error: 'bg-[#121214] border-[#F75A68] text-white',
    info: 'bg-[#121214] border-blue-500 text-white',
    warning: 'bg-[#121214] border-yellow-500 text-white'
};

const iconColors: Record<ToastType, string> = {
    success: 'text-[#00B37E]',
    error: 'text-[#F75A68]',
    info: 'text-blue-500',
    warning: 'text-yellow-500'
};
</script>

<template>
    <div class="fixed top-4 right-4 z-50 flex flex-col gap-2 pointer-events-none">
        <transition-group name="toast">
            <div 
                v-for="toast in store.toasts" 
                :key="toast.id"
                class="pointer-events-auto flex items-center gap-3 min-w-[300px] p-4 rounded-lg shadow-xl border-l-4 transform transition-all duration-300"
                :class="colors[toast.type]"
            >
                <component :is="icons[toast.type]" class="w-5 h-5" :class="iconColors[toast.type]" />
                <p class="text-sm font-medium flex-1">{{ toast.message }}</p>
                <button @click="store.remove(toast.id)" class="text-gray-400 hover:text-white transition-colors">
                    <X class="w-4 h-4" />
                </button>
            </div>
        </transition-group>
    </div>
</template>

<style scoped>
.toast-enter-active,
.toast-leave-active {
    transition: all 0.3s ease;
}

.toast-enter-from {
    opacity: 0;
    transform: translateX(30px);
}

.toast-leave-to {
    opacity: 0;
    transform: translateY(-30px);
}
</style>
