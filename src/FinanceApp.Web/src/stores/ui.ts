import { defineStore } from 'pinia';

export const useUIStore = defineStore('ui', {
    state: () => ({
        loadingCount: 0,
        isLoading: false
    }),
    actions: {
        startLoading() {
            this.loadingCount++;
            this.isLoading = true;
        },
        stopLoading() {
            if (this.loadingCount > 0) {
                this.loadingCount--;
            }
            // Add a small buffer to prevent flickering
            if (this.loadingCount === 0) {
                setTimeout(() => {
                    if (this.loadingCount === 0) {
                        this.isLoading = false;
                    }
                }, 300);
            }
        }
    }
});
