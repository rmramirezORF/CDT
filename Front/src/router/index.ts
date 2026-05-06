import { createRouter, createWebHistory, type RouteRecordRaw } from 'vue-router'
import { useAuthStore } from '@/modules/auth/stores/auth'

const routes: RouteRecordRaw[] = [
  // ----- Auth (público / guestOnly) -----
  {
    path: '/login',
    name: 'login',
    component: () => import('@/modules/auth/views/LoginView.vue'),
    meta: { guestOnly: true },
  },
  {
    path: '/register',
    name: 'register',
    component: () => import('@/modules/auth/views/RegisterView.vue'),
    meta: { guestOnly: true },
  },
  {
    path: '/confirm-email',
    name: 'confirm-email',
    component: () => import('@/modules/auth/views/ConfirmEmailView.vue'),
    meta: { guestOnly: true },
  },
  {
    path: '/forgot-password',
    name: 'forgot-password',
    component: () => import('@/modules/auth/views/ForgotPasswordView.vue'),
    meta: { guestOnly: true },
  },
  {
    path: '/reset-password',
    name: 'reset-password',
    component: () => import('@/modules/auth/views/ResetPasswordView.vue'),
    meta: { guestOnly: true },
  },

  // ----- Admin (requiere rol Admin) -----
  {
    path: '/admin',
    component: () => import('@/layouts/AdminLayout.vue'),
    meta: { requiresAuth: true, requiresRole: 'Admin' },
    children: [
      { path: '', redirect: '/admin/usuarios' },
      {
        path: 'usuarios',
        name: 'admin-usuarios',
        component: () => import('@/modules/admin/views/UsersView.vue'),
      },
      {
        path: 'catalogos',
        name: 'admin-catalogos',
        component: () => import('@/modules/admin/views/CatalogosView.vue'),
      },
      {
        path: 'equipos',
        name: 'admin-equipos',
        component: () => import('@/modules/admin/views/EquiposView.vue'),
      },
      {
        path: 'proyectos',
        name: 'admin-proyectos',
        component: () => import('@/modules/admin/views/ProyectosView.vue'),
      },
      {
        path: 'listas',
        name: 'admin-listas',
        component: () => import('@/modules/admin/views/ListasView.vue'),
      },
      {
        path: 'tablero',
        name: 'admin-tablero',
        component: () => import('@/modules/admin/views/KanbanView.vue'),
      },
    ],
  },

  // ----- Home (cualquier autenticado) -----
  {
    path: '/',
    name: 'home',
    component: () => import('@/views/HomeView.vue'),
    meta: { requiresAuth: true },
  },
]

const router = createRouter({
  history: createWebHistory(import.meta.env.BASE_URL),
  routes,
})

router.beforeEach((to, _from, next) => {
  const auth = useAuthStore()

  if (to.meta.requiresAuth && !auth.isAuthenticated) {
    return next({ name: 'login', query: { redirect: to.fullPath } })
  }
  if (to.meta.guestOnly && auth.isAuthenticated) {
    return next(auth.isAdmin ? { name: 'admin-usuarios' } : { name: 'home' })
  }
  if (to.meta.requiresRole && to.meta.requiresRole !== auth.rolGlobal) {
    return next({ name: 'home' })
  }
  next()
})

export default router
