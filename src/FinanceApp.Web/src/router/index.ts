import { createRouter, createWebHistory } from 'vue-router';
import { useAuthStore } from '../stores/auth';

const router = createRouter({
    history: createWebHistory(),
    routes: [
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
                },
                {
                    path: '/settings/categories',
                    name: 'Categories',
                    component: () => import('../views/settings/CategoriesView.vue')
                },
                {
                    path: '/settings/wallets',
                    name: 'Wallets',
                    component: () => import('../views/settings/WalletsView.vue')
                },
                {
                    path: '/admin/users',
                    name: 'AdminUsers',
                    component: () => import('../views/admin/AdminUsers.vue'),
                    meta: { requiresAdmin: true }
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

    // Check for Admin requirement
    if (to.matched.some(record => record.meta.requiresAdmin)) {
        if (!authStore.user?.isAdmin) {
            next('/'); // Redirect to dashboard if not admin
            return;
        }
    }

    // Redirect logic for logged in users trying to access login
    if (to.name === 'Login' && authStore.isAuthenticated) {
        next('/');
        return;
    }

    next();
});

export default router;
