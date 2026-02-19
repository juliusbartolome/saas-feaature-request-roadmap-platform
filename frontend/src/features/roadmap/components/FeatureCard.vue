<template>
  <article class="card feature-card slide-up">
    <div class="between">
      <h3>{{ feature.title }}</h3>
      <span :class="['badge', feature.status.toLowerCase()]">{{ feature.status }}</span>
    </div>
    <p class="muted">{{ feature.description }}</p>
    <div class="between">
      <button class="ghost" @click="$emit('vote')">▲ {{ feature.voteCount }}</button>
      <input v-model="comment" placeholder="Add comment" @keyup.enter="sendComment" />
    </div>
    <ul>
      <li v-for="c in feature.comments" :key="c.id" class="muted small">{{ c.body }}</li>
    </ul>
  </article>
</template>
<script setup lang="ts">
import { ref } from 'vue'
import type { FeatureItem } from '../roadmapStore'

const props = defineProps<{ feature: FeatureItem }>()
const emit = defineEmits<{ vote: []; comment: [body: string] }>()
const comment = ref('')
const sendComment = () => {
  if (!comment.value.trim()) return
  emit('comment', comment.value)
  comment.value = ''
}
</script>
