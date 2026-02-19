<template>
  <section class="card narrow fade-in">
    <h2>Welcome back</h2>
    <p class="muted">Sign in to continue managing your roadmap.</p>
    <form @submit.prevent="submit" class="form-grid">
      <input v-model="email" placeholder="Email" type="email" required />
      <input v-model="password" placeholder="Password" type="password" required />
      <button :disabled="auth.loading">{{ auth.loading ? 'Signing in...' : 'Sign In' }}</button>
    </form>
  </section>
</template>
<script setup lang="ts">
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '../../stores/authStore'

const auth = useAuthStore()
const router = useRouter()
const email = ref('admin@example.com')
const password = ref('Password123!')

const submit = async () => {
  await auth.login(email.value, password.value)
  router.push('/')
}
</script>
