<template>
  <div :class="['app-shell', theme]">
    <header class="topbar">
      <h1>FeatureFlow</h1>
      <button class="ghost" @click="toggleTheme">{{ theme === 'dark' ? '☀️ Light' : '🌙 Dark' }}</button>
    </header>
    <main>
      <RouterView />
    </main>
    <ToastStack />
  </div>
</template>

<script setup lang="ts">
import { computed } from 'vue'
import { RouterView } from 'vue-router'
import { useUiStore } from '../stores/uiStore'
import ToastStack from '../components/ToastStack.vue'

const ui = useUiStore()
const theme = computed(() => ui.theme)
const toggleTheme = () => ui.toggleTheme()
</script>
