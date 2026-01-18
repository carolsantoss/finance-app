import { defineStore } from 'pinia';
import { ref } from 'vue';
import axios from 'axios';

export interface Notification {
    id_notificacao: number;
    nm_titulo: string;
    ds_mensagem: string;
    fl_lida: boolean;
    nm_tipo: string;
    dt_criacao: string;
}

export const useNotificationStore = defineStore('notification', () => {
    const notifications = ref<Notification[]>([]);
    const unreadCount = ref(0);
    const isLoading = ref(false);

    // Using relative URL to let Vite proxy handle it or assume same origin
    // Using relative URL to let Vite proxy handle it or assume same origin
    const API_URL = 'http://localhost:5097/api/notifications';

    async function fetchNotifications() {
        isLoading.value = true;
        try {
            const response = await axios.get(API_URL, {
                headers: {
                    Authorization: `Bearer ${localStorage.getItem('token')}`
                }
            });
            console.log('[NotificationStore] Fetch response:', response.data);
            notifications.value = response.data;
        } catch (error) {
            console.error('Failed to fetch notifications', error);
        } finally {
            isLoading.value = false;
        }
    }

    async function fetchUnreadCount() {
        try {
            const response = await axios.get(`${API_URL}/unread-count`, {
                headers: {
                    Authorization: `Bearer ${localStorage.getItem('token')}`
                }
            });
            unreadCount.value = response.data.count;
        } catch (error) {
            console.error('Failed to fetch unread count', error);
        }
    }

    async function markAsRead(id: number) {
        try {
            await axios.post(`${API_URL}/${id}/read`, {}, {
                headers: {
                    Authorization: `Bearer ${localStorage.getItem('token')}`
                }
            });

            // Optimistic update
            const notification = notifications.value.find(n => n.id_notificacao === id);
            if (notification && !notification.fl_lida) {
                notification.fl_lida = true;
                unreadCount.value--;
            }
        } catch (error) {
            console.error('Failed to mark as read', error);
        }
    }

    async function markAllAsRead() {
        try {
            await axios.post(`${API_URL}/read-all`, {}, {
                headers: {
                    Authorization: `Bearer ${localStorage.getItem('token')}`
                }
            });

            // Optimistic update
            notifications.value.forEach(n => n.fl_lida = true);
            unreadCount.value = 0;
        } catch (error) {
            console.error('Failed to mark all as read', error);
        }
    }

    return {
        notifications,
        unreadCount,
        isLoading,
        fetchNotifications,
        fetchUnreadCount,
        markAsRead,
        markAllAsRead
    };
});
