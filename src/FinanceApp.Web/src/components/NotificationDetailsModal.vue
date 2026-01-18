<script setup lang="ts">
import { Dialog, DialogPanel, DialogTitle, TransitionChild, TransitionRoot } from '@headlessui/vue';
import { X, Calendar, MessageSquare, Mail } from 'lucide-vue-next';
import { computed } from 'vue';

interface Props {
    isOpen: boolean;
    notification: any; // Using any for flexibility, typing matches Store Interface
}

const props = defineProps<Props>();
const emit = defineEmits(['close']);

const formattedDate = computed(() => {
    if (!props.notification?.dt_criacao) return '';
    return new Date(props.notification.dt_criacao).toLocaleString('pt-BR', {
        day: '2-digit',
        month: '2-digit',
        year: 'numeric',
        hour: '2-digit',
        minute: '2-digit'
    });
});

const isEmail = computed(() => props.notification?.nm_tipo === 'EMAIL');
</script>

<template>
    <TransitionRoot as="template" :show="isOpen">
        <Dialog as="div" class="relative z-50" @close="emit('close')">
            <TransitionChild
                as="template"
                enter="ease-out duration-300"
                enter-from="opacity-0"
                enter-to="opacity-100"
                leave="ease-in duration-200"
                leave-from="opacity-100"
                leave-to="opacity-0"
            >
                <div class="fixed inset-0 bg-black/75 transition-opacity" />
            </TransitionChild>

            <div class="fixed inset-0 z-10 overflow-y-auto">
                <div class="flex min-h-full items-center justify-center p-4 text-center sm:p-0">
                    <TransitionChild
                        as="template"
                        enter="ease-out duration-300"
                        enter-from="opacity-0 translate-y-4 sm:translate-y-0 sm:scale-95"
                        enter-to="opacity-100 translate-y-0 sm:scale-100"
                        leave="ease-in duration-200"
                        leave-from="opacity-100 translate-y-0 sm:scale-100"
                        leave-to="opacity-0 translate-y-4 sm:translate-y-0 sm:scale-95"
                    >
                        <DialogPanel class="relative transform overflow-hidden rounded-xl bg-card text-left shadow-xl transition-all sm:my-8 sm:w-full sm:max-w-lg border border-border">
                            
                            <!-- Header -->
                            <div class="bg-card px-4 py-5 sm:px-6 border-b border-border flex justify-between items-start">
                                <div class="flex gap-3 items-center">
                                    <div class="p-2 rounded-full bg-brand/10 text-brand">
                                        <Mail v-if="isEmail" class="w-5 h-5" />
                                        <MessageSquare v-else class="w-5 h-5" />
                                    </div>
                                    <div>
                                        <DialogTitle as="h3" class="text-base font-semibold leading-6 text-text-primary">
                                            {{ notification?.nm_titulo }}
                                        </DialogTitle>
                                        <div class="flex items-center gap-2 mt-1">
                                            <Calendar class="w-3 h-3 text-text-tertiary" />
                                            <span class="text-xs text-text-secondary">{{ formattedDate }}</span>
                                        </div>
                                    </div>
                                </div>
                                <button type="button" class="text-text-tertiary hover:text-text-primary transition-colors" @click="emit('close')">
                                    <X class="w-5 h-5" />
                                </button>
                            </div>

                            <!-- Content -->
                            <div class="px-4 py-5 sm:p-6 bg-app/50 max-h-[60vh] overflow-y-auto">
                                <!-- Render HTML if it's an email type, otherwise plain text -->
                                <div v-if="isEmail" class="prose prose-invert prose-sm max-w-none text-text-secondary" v-html="notification?.ds_mensagem"></div>
                                <p v-else class="text-sm text-text-secondary whitespace-pre-wrap">{{ notification?.ds_mensagem }}</p>
                            </div>

                            <!-- Footer -->
                            <div class="bg-card px-4 py-3 sm:flex sm:flex-row-reverse sm:px-6 border-t border-border">
                                <button type="button" class="inline-flex w-full justify-center rounded-lg bg-brand px-3 py-2 text-sm font-semibold text-white shadow-sm hover:bg-brand-hover sm:ml-3 sm:w-auto transition-colors" @click="emit('close')">
                                    Fechar
                                </button>
                            </div>

                        </DialogPanel>
                    </TransitionChild>
                </div>
            </div>
        </Dialog>
    </TransitionRoot>
</template>
