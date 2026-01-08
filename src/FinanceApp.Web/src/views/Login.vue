<script setup lang="ts">
import { ref } from 'vue';
import { useAuthStore } from '../stores/auth';

const auth = useAuthStore();
const email = ref('');
const password = ref('');
const name = ref('');
const isRegister = ref(false);

const handleSubmit = async () => {
  try {
    if (isRegister.value) {
      await auth.register({ nomeUsuario: name.value, email: email.value, senha: password.value });
    } else {
      await auth.login({ email: email.value, senha: password.value });
    }
  } catch (error) {
    alert('Erro na autenticação. Verifique os dados.');
  }
};
</script>

<template>
  <div class="flex items-center justify-center min-h-screen bg-gray-100">
    <div class="px-8 py-6 mt-4 text-left bg-white shadow-lg rounded-lg w-full max-w-md">
      <h3 class="text-2xl font-bold text-center text-blue-600 mb-4">{{ isRegister ? 'Criar Conta' : 'Login' }}</h3>
      <form @submit.prevent="handleSubmit">
        <div v-if="isRegister" class="mt-4">
          <label class="block">Nome</label>
          <input type="text" v-model="name" required class="w-full px-4 py-2 mt-2 border rounded-md focus:outline-none focus:ring-1 focus:ring-blue-600">
        </div>
        <div class="mt-4">
          <label class="block">Email</label>
          <input type="email" v-model="email" required class="w-full px-4 py-2 mt-2 border rounded-md focus:outline-none focus:ring-1 focus:ring-blue-600">
        </div>
        <div class="mt-4">
          <label class="block">Senha</label>
          <input type="password" v-model="password" required class="w-full px-4 py-2 mt-2 border rounded-md focus:outline-none focus:ring-1 focus:ring-blue-600">
        </div>
        <div class="flex items-baseline justify-between">
          <button class="px-6 py-2 mt-4 text-white bg-blue-600 rounded-lg hover:bg-blue-900 w-full">{{ isRegister ? 'Registrar' : 'Entrar' }}</button>
        </div>
        <div class="mt-4 text-center">
          <a href="#" @click.prevent="isRegister = !isRegister" class="text-sm text-blue-600 hover:underline">
            {{ isRegister ? 'Já tem conta? Entrar' : 'Não tem conta? Registrar' }}
          </a>
        </div>
      </form>
    </div>
  </div>
</template>
