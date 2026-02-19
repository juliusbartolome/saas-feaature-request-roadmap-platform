<template>
  <div class="toast-stack" aria-live="assertive">
    <div v-for="toast in ui.toasts" :key="toast.id" :class="['toast', toast.type]" @animationend="ui.removeToast(toast.id)">{{ toast.text }}</div>
  </div>
</template>
<script setup lang="ts">
import { useUiStore } from '../stores/uiStore'
const ui = useUiStore()
</script>
