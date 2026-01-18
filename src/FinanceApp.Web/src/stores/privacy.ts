import { defineStore } from 'pinia';
import { ref, watch } from 'vue';

export const usePrivacyStore = defineStore('privacy', () => {
    // Initialize from localStorage or default to false
    const isHidden = ref(localStorage.getItem('privacy_mode') === 'true');

    // Toggle function
    function toggle() {
        isHidden.value = !isHidden.value;
    }

    // Persist state
    watch(isHidden, (newValue) => {
        localStorage.setItem('privacy_mode', String(newValue));
    });

    return {
        isHidden,
        toggle
    };
});
