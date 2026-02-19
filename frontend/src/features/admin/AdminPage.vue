<template>
  <section class="card fade-in">
    <h2>Admin analytics</h2>
    <div v-if="loading" class="grid"><SkeletonCard v-for="n in 3" :key="n" /></div>
    <div v-else class="kpis">
      <div class="kpi"><span>Total users</span><strong>{{ analytics.users }}</strong></div>
      <div class="kpi"><span>Feature requests</span><strong>{{ analytics.features }}</strong></div>
      <div class="kpi"><span>Approved comments</span><strong>{{ analytics.approvedComments }}</strong></div>
    </div>
  </section>
</template>
<script setup lang="ts">
import { onMounted, ref } from 'vue'
import { http } from '../../services/http'
import SkeletonCard from '../../components/SkeletonCard.vue'

const loading = ref(true)
const analytics = ref({ users: 0, features: 0, approvedComments: 0 })

onMounted(async () => {
  const { data } = await http.get('/admin/analytics')
  analytics.value = data
  loading.value = false
})
</script>
