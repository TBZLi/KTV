<script setup lang="ts">
import { ref, onMounted, computed } from 'vue'
import { favoritesApi } from '@/api'
import { useSongOrder } from '@/composables/useSongOrder'
import type { Favorite } from '@/types'

const { orderSong } = useSongOrder()
const API_BASE = import.meta.env.VITE_API_BASE_URL?.replace(/\/api$/, '') || 'https://localhost:5001'
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
  <div class="max-w-4xl mx-auto px-8">
    <!-- Page header -->
    <div class="mb-10">
      <h1 class="text-5xl font-extrabold text-on-surface font-display tracking-tight mb-2">我的收藏</h1>
      <p class="text-on-surface-variant">{{ songCount }} 首歌曲</p>
    </div>

    <!-- Favorites list -->
    <div class="flex flex-col gap-4">
      <div
        v-for="favorite in favorites"
        :key="favorite.id"
        class="flex items-center gap-6 p-4 -mx-4 rounded-lg hover:glass hover:shadow-lg hover:scale-[1.02] transition-all duration-300 cursor-pointer group"
      >
        <!-- Album cover -->
        <img
          v-if="favorite.song.coverUrl"
          :src="API_BASE + favorite.song.coverUrl"
          class="w-16 h-16 rounded shrink-0 object-cover"
          :alt="favorite.song.title"
          @error="($event.target as HTMLImageElement).src = API_BASE + '/uploads/covers/default.jpg'"
        />
        <div v-else class="w-16 h-16 rounded bg-surface-container shrink-0 shadow-sm flex items-center justify-center">
          <span class="material-symbols-outlined text-slate-400">image</span>
        </div>

        <!-- Song info -->
        <div class="flex-1 min-w-0">
          <h3 class="text-xl font-bold text-on-surface truncate font-headline mb-1">{{ favorite.song.title }}</h3>
          <p class="text-sm text-on-surface-variant truncate font-label">{{ favorite.song.artist }}</p>
        </div>

        <!-- Actions (hidden by default, shown on hover) -->
        <div class="flex items-center gap-3 opacity-0 group-hover:opacity-100 transition-opacity">
          <button
            @click="unfavorite(favorite.songId)"
            class="w-10 h-10 flex items-center justify-center text-error hover:bg-error/10 rounded-full transition-colors"
          >
            <span class="material-symbols-outlined" style="font-variation-settings: 'FILL' 1">favorite</span>
          </button>
          <button
            @click="orderSong(favorite.song)"
            class="px-6 py-3 bg-primary text-on-primary rounded-full font-bold text-sm active:scale-90 transition-transform shadow-lg shadow-primary/10"
          >
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
