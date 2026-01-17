<script setup lang="ts">
import Toast from './components/ui/Toast.vue';
import GlobalLoader from './components/ui/GlobalLoader.vue';
</script>

<template>
  <div class="min-h-screen bg-gray-100 text-gray-900 font-sans">
    <GlobalLoader />
    <Toast />
    <router-view />
  </div>
</template>
