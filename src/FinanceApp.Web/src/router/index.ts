import { createRouter, createWebHistory } from 'vue-router';
import { useAuthStore } from '../stores/auth';

const router = createRouter({
    history: createWebHistory(),
    routes: [
        {
            path: '/register',
            name: 'Register',
            component: () => import('../views/Register.vue')
        },
        {
            path: '/login',
            name: 'Login',
            component: () => import('../views/Login.vue')
        },
        {
            path: '/lancamento',
            name: 'Lancamento',
            component: () => import('../views/Lancamento.vue'),
            meta: { requiresAuth: true }
        },
        {
            path: '/',
            component: () => import('../layouts/MainLayout.vue'),
            meta: { requiresAuth: true },
            children: [
                {
                    path: '',
                    name: 'Dashboard',
                    component: () => import('../views/Dashboard.vue')
                },
                {
                    path: '/extratos',
                    name: 'Extratos',
                    component: () => import('../views/Extratos.vue')
                },
                {
                    path: '/perfil',
                    name: 'Perfil',
                    component: () => import('../views/Perfil.vue')
                }
            ]
        }
    ]
});

router.beforeEach((to, from, next) => {
    const authStore = useAuthStore();
    if (to.meta.requiresAuth && !authStore.isAuthenticated) {
        next('/login');
    } else {
        next();
    }
});

export default router;
