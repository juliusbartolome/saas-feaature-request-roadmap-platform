import { defineStore } from 'pinia'
import { ref } from 'vue'
import { http } from '../../services/http'
import { useUiStore } from '../../stores/uiStore'

export interface FeatureItem { id: string; title: string; description: string; status: string; voteCount: number; comments: { id: string; body: string }[] }

export const useRoadmapStore = defineStore('roadmap', () => {
  const loading = ref(false)
  const features = ref<FeatureItem[]>([])
  const ui = useUiStore()

  async function fetch(search = '', status = '') {
    loading.value = true
    const { data } = await http.get('/features', { params: { search, status } })
    features.value = data.items
    loading.value = false
  }

  async function create(title: string, description: string) {
    const { data } = await http.post<FeatureItem>('/features', { title, description })
    features.value = [data, ...features.value]
    ui.toastSuccess('Feature submitted')
  }

  async function vote(featureId: string) {
    const found = features.value.find((f) => f.id === featureId)
    if (found) found.voteCount += 1
    try {
      await http.post(`/features/${featureId}/votes`)
    } catch {
      if (found) found.voteCount -= 1
    }
  }

  async function comment(featureId: string, body: string) {
    const { data } = await http.post(`/features/${featureId}/comments`, { body })
    const found = features.value.find((f) => f.id === featureId)
    found?.comments.push(data)
  }

  return { loading, features, fetch, create, vote, comment }
})
