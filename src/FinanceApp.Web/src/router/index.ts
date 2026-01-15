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

    // Check if any of the matched routes requires auth
    if (to.matched.some(record => record.meta.requiresAuth)) {
        if (!authStore.isAuthenticated) {
            next('/login');
            return;
        }
    }

    // Redirect logic for logged in users trying to access login/register
    if ((to.name === 'Login' || to.name === 'Register') && authStore.isAuthenticated) {
        next('/');
        return;
    }

    next();
});

export default router;
