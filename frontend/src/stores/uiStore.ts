import { defineStore } from 'pinia'
import { ref } from 'vue'

export const useUiStore = defineStore('ui', () => {
  const theme = ref<'light' | 'dark'>('dark')
  const toasts = ref<{ id: number; text: string; type: 'error' | 'success' }[]>([])
  const toastError = (text: string) => toasts.value.push({ id: Date.now(), text, type: 'error' })
  const toastSuccess = (text: string) => toasts.value.push({ id: Date.now(), text, type: 'success' })
  const removeToast = (id: number) => (toasts.value = toasts.value.filter((x) => x.id !== id))
  const toggleTheme = () => (theme.value = theme.value === 'dark' ? 'light' : 'dark')
  return { theme, toasts, toastError, toastSuccess, removeToast, toggleTheme }
})
