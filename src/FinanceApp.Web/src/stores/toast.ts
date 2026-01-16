import { defineStore } from 'pinia';
import { ref } from 'vue';

export type ToastType = 'success' | 'error' | 'info' | 'warning';

export interface ToastMessage {
    id: number;
    message: string;
    type: ToastType;
}

export const useToastStore = defineStore('toast', () => {
    const toasts = ref<ToastMessage[]>([]);
    let nextId = 1;

    const add = (message: string, type: ToastType = 'info', duration = 3000) => {
        const id = nextId++;
        const toast = { id, message, type };
        toasts.value.push(toast);

        if (duration > 0) {
            setTimeout(() => {
                remove(id);
            }, duration);
        }
    };

    const remove = (id: number) => {
        toasts.value = toasts.value.filter(t => t.id !== id);
    };

    const success = (message: string, duration?: number) => add(message, 'success', duration);
    const error = (message: string, duration?: number) => add(message, 'error', duration);
    const info = (message: string, duration?: number) => add(message, 'info', duration);
    const warning = (message: string, duration?: number) => add(message, 'warning', duration);

    return {
        toasts,
        add,
        remove,
        success,
        error,
        info,
        warning
    };
});
