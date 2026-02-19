import axios from 'axios'
import { useAuthStore } from '../stores/authStore'
import { useUiStore } from '../stores/uiStore'

export const http = axios.create({
  baseURL: import.meta.env.VITE_API_URL ?? 'http://localhost:8080/api/v1'
})

http.interceptors.request.use((config) => {
  const auth = useAuthStore()
  if (auth.tokens?.accessToken) config.headers.Authorization = `Bearer ${auth.tokens.accessToken}`
  return config
})

http.interceptors.response.use(
  (response) => response,
  async (error) => {
    const auth = useAuthStore()
    const ui = useUiStore()
    if (error.response?.status === 401 && auth.tokens?.refreshToken) {
      await auth.refresh()
      return http.request(error.config)
    }
    ui.toastError(error.response?.data?.message ?? 'Unexpected error')
    return Promise.reject(error)
  }
)
