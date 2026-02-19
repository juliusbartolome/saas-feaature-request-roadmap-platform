<template>
  <section>
    <div class="toolbar">
      <input v-model="search" placeholder="Search features..." />
      <select v-model="statusFilter">
        <option value="">All</option>
        <option value="Planned">Planned</option>
        <option value="InProgress">In Progress</option>
        <option value="Released">Released</option>
      </select>
      <button @click="openCreate = !openCreate">+ New request</button>
    </div>

    <div v-if="loading" class="grid">
      <SkeletonCard v-for="n in 3" :key="n" />
    </div>

    <div v-else-if="!features.length" class="empty-state">No feature requests found.</div>

    <div v-else class="grid">
      <FeatureCard v-for="feature in features" :key="feature.id" :feature="feature" @vote="vote(feature.id)" @comment="comment(feature.id, $event)" />
    </div>

    <div v-if="openCreate" class="card modal">
      <h3>Create feature request</h3>
      <input v-model="title" placeholder="Title" />
      <textarea v-model="description" placeholder="Describe customer value" rows="4" />
      <button @click="create">Submit</button>
    </div>
  </section>
</template>
<script setup lang="ts">
import { computed, ref, watchEffect } from 'vue'
import { useRoadmapStore } from './roadmapStore'
import FeatureCard from './components/FeatureCard.vue'
import SkeletonCard from '../../components/SkeletonCard.vue'

const roadmap = useRoadmapStore()
const search = ref('')
const statusFilter = ref('')
const openCreate = ref(false)
const title = ref('')
const description = ref('')

watchEffect(() => roadmap.fetch(search.value, statusFilter.value))

const features = computed(() => roadmap.features)
const loading = computed(() => roadmap.loading)

const vote = (id: string) => roadmap.vote(id)
const comment = (id: string, text: string) => roadmap.comment(id, text)
const create = async () => {
  await roadmap.create(title.value, description.value)
  title.value = ''
  description.value = ''
  openCreate.value = false
}
</script>
