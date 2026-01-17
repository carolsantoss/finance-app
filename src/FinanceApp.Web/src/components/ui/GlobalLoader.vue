<script setup lang="ts">
import { useUIStore } from '../../stores/ui';
import { Wallet } from 'lucide-vue-next';

const ui = useUIStore();
</script>

<template>
    <transition name="fade">
        <div v-if="ui.isLoading" class="fixed inset-0 z-[100] flex items-center justify-center bg-[#121214]/80 backdrop-blur-sm">
            <div class="relative flex flex-col items-center gap-4">
                <!-- Glowing effect -->
                <div class="absolute inset-0 bg-[#00875F] blur-3xl opacity-20 rounded-full w-32 h-32 animate-pulse"></div>
                
                <!-- Spinner Container -->
                <div class="relative">
                    <!-- Spinning Border -->
                    <div class="w-20 h-20 border-4 border-[#202024] border-t-[#00875F] rounded-full animate-spin"></div>
                    
                    <!-- Static Logo in Center -->
                    <div class="absolute inset-0 flex items-center justify-center">
                         <div class="w-12 h-12 rounded-full bg-[#00875F] flex items-center justify-center shadow-lg transform transition-transform hover:scale-110">
                            <Wallet class="text-white w-6 h-6 animate-bounce-slow" />
                        </div>
                    </div>
                </div>

                <div class="flex flex-col items-center mt-2">
                     <p class="text-[#00875F] font-bold text-lg tracking-wider animate-pulse">PROCESSANDO</p>
                </div>
            </div>
        </div>
    </transition>
</template>

<style scoped>
.fade-enter-active,
.fade-leave-active {
  transition: opacity 0.3s ease;
}

.fade-enter-from,
.fade-leave-to {
  opacity: 0;
}

.animate-bounce-slow {
    animation: bounce 2s infinite;
}

@keyframes bounce {
    0%, 100% {
        transform: translateY(-5%);
        animation-timing-function: cubic-bezier(0.8, 0, 1, 1);
    }
    50% {
        transform: translateY(5%);
        animation-timing-function: cubic-bezier(0, 0, 0.2, 1);
    }
}
</style>
