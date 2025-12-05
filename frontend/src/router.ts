import { createRouter, createWebHistory } from 'vue-router'
import { useAuthStore } from '@/stores/auth'

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes: [
    {
      path: '/',
      redirect: '/login'
    },
    {
      path: '/login',
      name: 'login',
      component: () => import('@/views/LoginView.vue')
    },
    {
      path: '/user',
      name: 'user',
      component: () => import('@/layouts/UserLayout.vue'),
      meta: { requiresAuth: true, role: 'user' },
      children: [
        {
          path: 'dashboard',
          name: 'user-dashboard',
          component: () => import('@/views/user/DashboardView.vue')
        },
        {
          path: 'chat',
          name: 'user-chat',
          component: () => import('@/views/user/ChatView.vue')
        },
        {
          path: 'assessment',
          name: 'user-assessment',
          component: () => import('@/views/user/AssessmentView.vue')
        },
        {
          path: 'settings',
          name: 'user-settings',
          component: () => import('@/views/user/SettingsView.vue')
        }
      ]
    },
    {
      path: '/admin',
      name: 'admin',
      component: () => import('@/layouts/AdminLayout.vue'),
      meta: { requiresAuth: true, role: 'admin' },
      children: [
        {
          path: 'dashboard',
          name: 'admin-dashboard',
          component: () => import('@/views/admin/DashboardView.vue')
        },
        {
          path: 'users',
          name: 'admin-users',
          component: () => import('@/views/admin/UsersView.vue')
        },
        {
          path: 'tasks',
          name: 'admin-tasks',
          component: () => import('@/views/admin/TasksView.vue')
        },
        {
          path: 'scales',
          name: 'admin-scales',
          component: () => import('@/views/admin/ScalesView.vue')
        },
        {
          path: 'analysis',
          name: 'admin-analysis',
          component: () => import('@/views/admin/AnalysisView.vue')
        },
        {
          path: 'settings',
          name: 'admin-settings',
          component: () => import('@/views/admin/SettingsView.vue')
        }
      ]
    },
    {
      path: '/:pathMatch(.*)*',
      name: 'not-found',
      component: () => import('@/views/NotFoundView.vue')
    }
  ]
})

router.beforeEach((to, _from, next) => {
  const authStore = useAuthStore()
  const requiresAuth = to.matched.some(record => record.meta.requiresAuth)
  const requiredRole = to.meta.role

  if (requiresAuth && !authStore.token) {
    next('/login')
  } else if (requiresAuth && requiredRole && authStore.role !== requiredRole) {
    // Redirect to appropriate dashboard if role doesn't match
    if (authStore.role === 'admin') {
      next('/admin/dashboard')
    } else {
      next('/user/dashboard')
    }
  } else {
    next()
  }
})

export default router
