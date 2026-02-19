import { createRouter, createWebHistory } from 'vue-router'
import LoginPage from '../features/auth/LoginPage.vue'
import RoadmapPage from '../features/roadmap/RoadmapPage.vue'
import AdminPage from '../features/admin/AdminPage.vue'
import { useAuthStore } from '../stores/authStore'

const router = createRouter({
  history: createWebHistory(),
  routes: [
    { path: '/', component: RoadmapPage },
    { path: '/login', component: LoginPage },
    { path: '/admin', component: AdminPage, meta: { adminOnly: true } }
  ]
})

router.beforeEach((to) => {
  const auth = useAuthStore()
  if (to.meta.adminOnly && auth.user?.role !== 'Admin') return '/'
  return true
})

export default router
