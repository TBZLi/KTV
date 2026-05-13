<script setup lang="ts">
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { authApi } from '@/api'
import { useAuthStore } from '@/stores/auth'

const router = useRouter()
const authStore = useAuthStore()

const username = ref('')
const password = ref('')
const loading = ref(false)

async function handleLogin() {
  if (!username.value || !password.value) return
  loading.value = true
  try {
    const { data } = await authApi.login(username.value, password.value)
    authStore.setAuth(data.token, data.user)
    router.push('/explore')
  } catch {
    // authApi error handled by interceptor
  } finally {
    loading.value = false
  }
}
</script>

<template>
  <div class="min-h-screen bg-surface flex items-center justify-center relative overflow-hidden">
    <!-- Decorative background circles -->
    <div class="absolute top-[-200px] left-[-200px] w-[500px] h-[500px] rounded-full bg-primary-fixed/40 blur-[120px] pointer-events-none" />
    <div class="absolute bottom-[-200px] right-[-200px] w-[600px] h-[600px] rounded-full bg-secondary-fixed/50 blur-[150px] pointer-events-none" />

    <!-- Login card -->
    <div class="relative z-10 w-full max-w-[480px] bg-surface-container-lowest rounded-2xl p-12 shadow-ambient">
      <!-- Logo section -->
      <div class="text-center mb-10">
        <div class="inline-flex items-center justify-center w-16 h-16 rounded-full bg-primary/10 mb-4">
          <span class="material-symbols-outlined text-4xl text-primary">waves</span>
        </div>
        <h1 class="text-3xl font-headline font-bold text-primary">声域友</h1>
        <p class="text-sm text-on-surface-variant uppercase tracking-widest mt-2 font-label">Hydro-Sonic Canvas</p>
      </div>

      <!-- Form -->
      <form @submit.prevent="handleLogin" class="space-y-5">
        <!-- Username -->
        <div class="relative">
          <span class="material-symbols-outlined absolute left-4 top-1/2 -translate-y-1/2 text-on-surface-variant">person</span>
          <input
            v-model="username"
            type="text"
            placeholder="用户名"
            class="w-full bg-surface-container-high border-none rounded-lg py-4 pl-12 pr-4 text-base text-on-surface placeholder:text-on-surface-variant font-body focus:ring-2 focus:ring-primary/30 focus:outline-none"
          />
        </div>

        <!-- Password -->
        <div class="relative">
          <span class="material-symbols-outlined absolute left-4 top-1/2 -translate-y-1/2 text-on-surface-variant">lock</span>
          <input
            v-model="password"
            type="password"
            placeholder="密码"
            class="w-full bg-surface-container-high border-none rounded-lg py-4 pl-12 pr-4 text-base text-on-surface placeholder:text-on-surface-variant font-body focus:ring-2 focus:ring-primary/30 focus:outline-none"
          />
        </div>

        <!-- Login button -->
        <button
          type="submit"
          :disabled="loading"
          class="w-full bg-gradient-to-r from-primary to-secondary text-on-primary rounded-full py-4 font-bold font-label text-base flex items-center justify-center gap-2 active:scale-[0.98] transition-all duration-150 hover:shadow-ambient-primary disabled:opacity-60"
        >
          <template v-if="loading">
            <span class="material-symbols-outlined animate-spin">progress_activity</span>
          </template>
          <template v-else>
            登录
            <span class="material-symbols-outlined">arrow_forward</span>
          </template>
        </button>
      </form>
    </div>
  </div>
</template>
