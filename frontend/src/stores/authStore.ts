import { defineStore } from 'pinia'
import { ref } from 'vue'
import { http } from '../services/http'

interface AuthResponse { accessToken: string; refreshToken: string; email: string; role: string }

export const useAuthStore = defineStore('auth', () => {
  const tokens = ref<AuthResponse | null>(null)
  const user = ref<{ email: string; role: string } | null>(null)
  const loading = ref(false)

  async function login(email: string, password: string) {
    loading.value = true
    const { data } = await http.post<AuthResponse>('/auth/login', { email, password })
    tokens.value = data
    user.value = { email: data.email, role: data.role }
    loading.value = false
  }

  async function refresh() {
    if (!tokens.value?.refreshToken) return
    const { data } = await http.post<AuthResponse>('/auth/refresh', tokens.value.refreshToken)
    tokens.value = data
    user.value = { email: data.email, role: data.role }
  }

  return { tokens, user, loading, login, refresh }
})
