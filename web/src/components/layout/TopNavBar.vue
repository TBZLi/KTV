<script setup lang="ts">
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { useAuthStore } from '@/stores/auth'
import { roomApi, authApi } from '@/api'

const router = useRouter()
const auth = useAuthStore()
const showMenu = ref(false)

async function handleLogout() {
  showMenu.value = false
  if (auth.currentRoomId > 0) {
    try { await roomApi.leaveRoom(auth.currentRoomId) } catch { /* ignore */ }
  }
  try { await authApi.logout() } catch { /* ignore */ }
  auth.logout()
  router.push('/login')
}
</script>

<template>
  <header class="fixed top-0 left-0 right-0 z-50 bg-surface flex justify-between items-center w-full px-8 py-4 font-headline text-sm tracking-wide">
    <div class="flex items-center gap-8">
      <span class="text-xl font-bold tracking-tighter text-on-surface">声域友</span>
      <div class="relative w-96">
        <input class="w-full bg-surface-container border-none rounded-full px-6 py-2 focus:ring-2 focus:ring-primary outline-none" placeholder="搜索歌曲、歌手..." type="text"/>
        <span class="material-symbols-outlined absolute right-4 top-2 text-slate-400">search</span>
      </div>
    </div>
    <div class="flex items-center gap-6">
      <button class="relative hover:bg-surface-container p-2 rounded-full transition-colors press-scale">
        <span class="material-symbols-outlined text-primary">notifications</span>
        <span class="absolute top-2 right-2 w-2 h-2 bg-error rounded-full"></span>
      </button>
      <!-- Avatar dropdown -->
      <div class="relative">
        <button
          @click="showMenu = !showMenu"
          class="w-10 h-10 bg-primary-container text-on-primary-container rounded-full flex items-center justify-center text-sm font-bold border-2 border-white shadow-sm hover:shadow-md transition-shadow cursor-pointer"
        >
          {{ auth.user?.displayName?.charAt(0)?.toUpperCase() || '?' }}
        </button>
        <!-- Dropdown -->
        <div
          v-if="showMenu"
          class="absolute right-0 mt-2 w-40 bg-surface-container-lowest rounded-xl shadow-xl ring-1 ring-outline-variant/15 py-2 z-50"
        >
          <div class="px-4 py-2 text-xs text-on-surface-variant border-b border-outline-variant/10">
            {{ auth.user?.displayName }}
          </div>
          <button
            @click="handleLogout"
            class="w-full text-left px-4 py-2.5 text-sm text-error hover:bg-error/5 flex items-center gap-2 transition-colors"
          >
            <span class="material-symbols-outlined text-lg">logout</span>
            退出登录
          </button>
        </div>
      </div>
    </div>
  </header>
  <!-- Click outside to close -->
  <div v-if="showMenu" class="fixed inset-0 z-40" @click="showMenu = false"></div>
</template>
