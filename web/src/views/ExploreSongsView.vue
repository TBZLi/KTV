<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { songsApi } from '@/api'
import type { Song } from '@/types'
import { formatDuration } from '@/utils/format'

const genres = ref<string[]>([])
const selectedGenre = ref<string>('')
const songs = ref<Song[]>([])
const favoritedSongIds = ref<Set<number>>(new Set())

async function loadGenres() {
  const { data } = await songsApi.getGenres()
  genres.value = data
}

async function loadSongs() {
  const { data } = await songsApi.getList({
    genre: selectedGenre.value || undefined,
    page: 1,
    pageSize: 50,
  })
  songs.value = data.items
}

function selectGenre(genre: string) {
  selectedGenre.value = genre
  loadSongs()
}

function toggleFavorite(songId: number) {
  if (favoritedSongIds.value.has(songId)) {
    favoritedSongIds.value.delete(songId)
  } else {
    favoritedSongIds.value.add(songId)
  }
  favoritedSongIds.value = new Set(favoritedSongIds.value)
}

function isFavorited(songId: number) {
  return favoritedSongIds.value.has(songId)
}

onMounted(() => {
  loadGenres()
  loadSongs()
})
</script>

<template>
  <div class="max-w-6xl mx-auto px-8">
    <!-- Page Title -->
    <div class="mb-10">
      <h1 class="text-5xl font-extrabold font-headline tracking-tighter text-on-background mb-2">探索歌曲</h1>
      <p class="text-on-surface-variant font-body">探索属于你的音乐世界，发现最新潮流单曲</p>
    </div>

    <!-- Genre filter pills -->
    <div class="flex items-center gap-3 mb-12 overflow-x-auto pb-2">
      <button
        @click="selectGenre('')"
        :class="[
          'px-8 py-3 rounded-full font-bold font-body transition-all',
          selectedGenre === ''
            ? 'bg-primary text-on-primary'
            : 'bg-surface-container-lowest text-on-surface hover:bg-primary-fixed shadow-sm',
        ]"
      >
        全部
      </button>
      <button
        v-for="genre in genres"
        :key="genre"
        @click="selectGenre(genre)"
        :class="[
          'px-8 py-3 rounded-full font-bold font-body transition-all',
          selectedGenre === genre
            ? 'bg-primary text-on-primary'
            : 'bg-surface-container-lowest text-on-surface hover:bg-primary-fixed shadow-sm',
        ]"
      >
        {{ genre }}
      </button>
    </div>

    <!-- Song list -->
    <div class="grid gap-6">
      <div
        v-for="song in songs"
        :key="song.id"
        class="group flex items-center gap-6 p-4 bg-white hover:bg-surface-container-low rounded-lg transition-all duration-300 active:scale-[0.99]"
      >
        <!-- Album cover placeholder -->
        <div class="w-20 h-20 bg-slate-200 rounded-xl flex-shrink-0 shadow-inner flex items-center justify-center">
          <span class="text-[10px] font-bold text-slate-400 uppercase tracking-widest">
            <span class="material-symbols-outlined text-slate-400">image</span>
          </span>
        </div>

        <!-- Song info grid -->
        <div class="flex-1 grid grid-cols-12 items-center gap-4">
          <div class="col-span-5">
            <h3 class="text-xl font-bold font-headline text-on-surface">{{ song.title }}</h3>
            <p class="text-on-surface-variant font-body text-sm mt-0.5">{{ song.artist }}</p>
          </div>
          <div class="col-span-2 text-on-surface-variant font-body font-medium">{{ formatDuration(song.duration) }}</div>
          <div class="col-span-5 flex justify-end items-center gap-4">
            <!-- Favorite toggle -->
            <button
              @click="toggleFavorite(song.id)"
              class="w-12 h-12 flex items-center justify-center rounded-full hover:bg-surface-container-high transition-colors"
            >
              <span
                class="material-symbols-outlined"
                :class="isFavorited(song.id) ? 'text-error' : 'text-outline'"
                :style="isFavorited(song.id) ? 'font-variation-settings: FILL 1' : ''"
              >
                favorite
              </span>
            </button>
            <!-- Order button -->
            <button class="px-8 py-3 bg-primary text-on-primary rounded-full font-bold font-body active:scale-90 transition-transform shadow-lg shadow-primary/10">
              点歌
            </button>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>
