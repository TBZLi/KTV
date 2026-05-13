<script setup lang="ts">
import { ref, onMounted, computed } from 'vue'
import { favoritesApi } from '@/api'
import type { Favorite } from '@/types'

const favorites = ref<Favorite[]>([])
const songCount = computed(() => favorites.value.length)

async function loadFavorites() {
  const { data } = await favoritesApi.getList()
  favorites.value = data
}

async function unfavorite(songId: number) {
  await favoritesApi.remove(songId)
  favorites.value = favorites.value.filter((f) => f.songId !== songId)
}

onMounted(() => {
  loadFavorites()
})
</script>

<template>
  <div>
    <!-- Page header -->
    <div class="mb-10 flex items-end gap-6">
      <h2 class="text-5xl font-black text-on-surface tracking-tighter font-display">我的收藏</h2>
      <span class="text-sm font-bold text-primary mb-2 font-label uppercase tracking-widest">{{ songCount }} 首歌曲</span>
    </div>

    <!-- Favorites list -->
    <div class="space-y-10">
      <div
        v-for="favorite in favorites"
        :key="favorite.id"
        class="group flex items-center gap-6 p-4 rounded-lg hover:bg-surface-container-low transition-all duration-200"
      >
        <!-- Album cover -->
        <div class="w-20 h-20 bg-slate-200 rounded-lg flex-shrink-0 shadow-sm overflow-hidden flex items-center justify-center">
          <span class="material-symbols-outlined text-slate-400 text-lg opacity-50">image</span>
        </div>

        <!-- Song info -->
        <div class="flex-1 min-w-0">
          <h3 class="text-xl font-bold text-on-surface truncate font-display">{{ favorite.song.title }}</h3>
          <p class="text-sm text-slate-500 font-medium font-body">{{ favorite.song.artist }}</p>
        </div>

        <!-- Actions (hidden by default, shown on hover) -->
        <div class="flex items-center gap-4 opacity-0 group-hover:opacity-100 transition-opacity">
          <button
            @click="unfavorite(favorite.songId)"
            class="p-3 text-error hover:bg-error/10 rounded-full transition-colors"
          >
            <span class="material-symbols-outlined" style="font-variation-settings: 'FILL' 1">favorite</span>
          </button>
          <button class="px-8 py-3 bg-primary text-white rounded-full font-bold text-sm tracking-wide hover:bg-secondary active:scale-95 transition-all shadow-md shadow-primary/20">
            点歌
          </button>
        </div>
      </div>
    </div>

    <!-- Empty state -->
    <div v-if="favorites.length === 0" class="text-center py-20">
      <span class="material-symbols-outlined text-6xl text-surface-container-highest">favorite_border</span>
      <p class="text-on-surface-variant mt-4">还没有收藏的歌曲</p>
    </div>
  </div>
</template>
