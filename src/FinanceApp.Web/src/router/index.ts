import { createRouter, createWebHistory } from 'vue-router';
import { useAuthStore } from '../stores/auth';
import ReportsView from '../views/ReportsView.vue';

const router = createRouter({
    history: createWebHistory(),
    routes: [
        {
            path: '/login',
            name: 'Login',
            component: () => import('../views/Login.vue')
        },
        {
            path: '/register',
            name: 'Register',
            component: () => import('../views/Register.vue')
        },
        {
            path: '/forgot-password',
            name: 'ForgotPassword',
            component: () => import('../views/auth/ForgotPassword.vue')
        },
        {
            path: '/reset-password',
            name: 'ResetPassword',
            component: () => import('../views/auth/ResetPassword.vue')
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
                    path: '/planning/budgets',
                    name: 'Budgets',
                    component: () => import('../views/BudgetsView.vue')
                },
                {
                    path: '/reports',
                    name: 'Reports',
                    component: () => import('../views/ReportsView.vue')
                },
                {
                    path: '/goals',
                    name: 'Goals',
                    component: () => import('../views/GoalsView.vue')
                },
                {
                    path: '/planning/recurring',
                    name: 'Recurring',
                    component: () => import('../views/RecurringView.vue')
                },
                {
                    path: '/admin/users',
                    name: 'AdminUsers',
                    component: () => import('../views/admin/AdminUsers.vue'),
                    meta: { requiresAdmin: true }
                },
                {
                    path: '/admin/plans',
                    name: 'AdminPlans',
                    component: () => import('../views/admin/AdminPlans.vue'),
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

    // Redirect logic for logged in users trying to access login or register
    if ((to.name === 'Login' || to.name === 'Register') && authStore.isAuthenticated) {
        next('/');
        return;
    }

    next();
});

export default router;
