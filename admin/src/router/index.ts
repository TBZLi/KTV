import { createRouter, createWebHistory } from 'vue-router'
import { useAuthStore } from '@/stores/auth'
import { authApi } from '@/api'

const router = createRouter({
  history: createWebHistory(),
  routes: [
    { path: '/', redirect: '/dashboard' },
    {
      path: '/dashboard',
      name: 'Dashboard',
      component: () => import('@/views/DashboardView.vue'),
      meta: { title: '仪表盘', icon: 'dashboard', requiresAuth: true },
    },
    {
      path: '/rooms',
      name: 'Rooms',
      component: () => import('@/views/RoomManagementView.vue'),
      meta: { title: '房间管理', icon: 'meeting_room', requiresAuth: true },
    },
    {
      path: '/songs',
      name: 'Songs',
      component: () => import('@/views/SongManagementView.vue'),
      meta: { title: '歌曲管理', icon: 'library_music', requiresAuth: true },
    },
    {
      path: '/songs/:id',
      name: 'SongDetail',
      component: () => import('@/views/SongDetailView.vue'),
      meta: { title: '歌曲详情', requiresAuth: true },
    },
    {
      path: '/orders',
      name: 'Orders',
      component: () => import('@/views/OrderManagementView.vue'),
      meta: { title: '订单管理', icon: 'receipt_long', requiresAuth: true },
    },
    {
      path: '/accounts',
      name: 'Accounts',
      component: () => import('@/views/AccountManagementView.vue'),
      meta: { title: '账户管理', icon: 'manage_accounts', requiresAuth: true },
    },
    {
      path: '/settings',
      name: 'Settings',
      component: () => import('@/views/SystemSettingsView.vue'),
      meta: { title: '系统设置', icon: 'settings', requiresAuth: true },
    },
    {
      path: '/:pathMatch(.*)*',
      redirect: '/dashboard',
    },
  ],
})

router.beforeEach(async (to) => {
  const auth = useAuthStore()

  // Auto-login as admin if no token (admin console has no login page)
  if (!auth.isLoggedIn) {
    try {
      const { data } = await authApi.login('admin', 'demo_hash_admin')
      auth.setAuth(data.token, data.user)
    } catch {
      // Retry once after a short delay (backend might still be starting)
      await new Promise(r => setTimeout(r, 1000))
      try {
        const { data } = await authApi.login('admin', 'demo_hash_admin')
        auth.setAuth(data.token, data.user)
      } catch {
        console.error('Auto-login failed, pages will show empty data')
      }
    }
  }
})

export default router
