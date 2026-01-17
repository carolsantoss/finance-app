import { defineStore } from 'pinia';
import { ref, watchEffect } from 'vue';

export const useThemeStore = defineStore('theme', () => {
    const isDark = ref(localStorage.getItem('theme') !== 'light');

    const toggleTheme = () => {
        isDark.value = !isDark.value;
    };

    watchEffect(() => {
        const root = document.documentElement;
        if (isDark.value) {
            root.classList.remove('light');
            root.classList.add('dark');
            localStorage.setItem('theme', 'dark');
        } else {
            root.classList.remove('dark');
            root.classList.add('light');
            localStorage.setItem('theme', 'light');
        }
    });

    return {
        isDark,
        toggleTheme
    };
});
