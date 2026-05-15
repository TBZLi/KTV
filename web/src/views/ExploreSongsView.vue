<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { songsApi, feedbacksApi, favoritesApi } from '@/api'
import { useSongOrder } from '@/composables/useSongOrder'
import type { Song } from '@/types'
import { formatDuration } from '@/utils/format'

const { orderSong } = useSongOrder()
const API_BASE = import.meta.env.VITE_API_BASE_URL?.replace(/\/api$/, '') || 'https://localhost:5001'
const genres = ref<string[]>([])
const selectedGenre = ref<string>('')
const songs = ref<Song[]>([])
const favoritedSongIds = ref<Set<number>>(new Set())

// Feedback dialog state
const showFeedbackDialog = ref(false)
const feedbackType = ref('request_song')
const feedbackSongName = ref('')
const feedbackArtist = ref('')
const feedbackDescription = ref('')
const feedbackSuccess = ref(false)
const feedbackLoading = ref(false)

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

async function loadFavorites() {
  try {
    const { data } = await favoritesApi.getList()
    favoritedSongIds.value = new Set(data.map(f => f.songId))
  } catch { /* ignore */ }
}

async function toggleFavorite(songId: number) {
  try {
    if (favoritedSongIds.value.has(songId)) {
      await favoritesApi.remove(songId)
      favoritedSongIds.value.delete(songId)
    } else {
      await favoritesApi.add(songId)
      favoritedSongIds.value.add(songId)
    }
    favoritedSongIds.value = new Set(favoritedSongIds.value)
  } catch (err: any) {
    alert(err.response?.data?.message || '操作失败')
  }
}

function isFavorited(songId: number) {
  return favoritedSongIds.value.has(songId)
}

function openFeedbackDialog() {
  feedbackType.value = 'request_song'
  feedbackSongName.value = ''
  feedbackArtist.value = ''
  feedbackDescription.value = ''
  feedbackSuccess.value = false
  showFeedbackDialog.value = true
}

async function submitFeedback() {
  feedbackLoading.value = true
  try {
    await feedbacksApi.create({
      feedbackType: feedbackType.value,
      songName: feedbackSongName.value || undefined,
      artist: feedbackArtist.value || undefined,
      description: feedbackDescription.value || undefined,
    })
    feedbackSuccess.value = true
    setTimeout(() => { showFeedbackDialog.value = false }, 1500)
  } catch (err: any) {
    alert(err.response?.data?.message || '提交失败')
  } finally {
    feedbackLoading.value = false
  }
}

onMounted(() => {
  loadGenres()
  loadSongs()
  loadFavorites()
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
        <!-- Album cover -->
        <img
          v-if="song.coverUrl"
          :src="API_BASE + song.coverUrl"
          class="w-20 h-20 rounded-xl flex-shrink-0 object-cover cursor-pointer transition-transform duration-200 hover:scale-[2.5] hover:shadow-lg hover:z-10 relative"
          :alt="song.title"
          @error="($event.target as HTMLImageElement).src = API_BASE + '/uploads/covers/default.jpg'"
        />
        <div v-else class="w-20 h-20 bg-slate-200 rounded-xl flex-shrink-0 shadow-inner flex items-center justify-center">
          <span class="material-symbols-outlined text-slate-400">image</span>
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
            <button
              @click="orderSong(song)"
              class="px-8 py-3 bg-primary text-on-primary rounded-full font-bold font-body active:scale-90 transition-transform shadow-lg shadow-primary/10"
            >
              点歌
            </button>
          </div>
        </div>
      </div>
    </div>

    <!-- Feedback FAB -->
    <button
      @click="openFeedbackDialog"
      class="fixed bottom-28 right-8 w-14 h-14 bg-primary text-on-primary rounded-full shadow-lg shadow-primary/30 flex items-center justify-center hover:scale-110 active:scale-95 transition-transform z-40"
      title="反馈"
    >
      <span class="material-symbols-outlined">feedback</span>
    </button>

    <!-- Feedback Dialog -->
    <div v-if="showFeedbackDialog" class="fixed inset-0 z-50 flex items-center justify-center bg-black/40" @click.self="showFeedbackDialog = false">
      <div class="bg-surface-container-lowest rounded-2xl shadow-xl w-full max-w-md p-8 space-y-6">
        <template v-if="!feedbackSuccess">
          <h3 class="text-xl font-display font-bold text-on-surface">用户反馈</h3>
          <div class="space-y-4">
            <div>
              <label class="block text-sm font-medium text-on-surface-variant mb-1">反馈类型</label>
              <select v-model="feedbackType" class="w-full bg-surface-container-high border-none rounded-lg py-3 px-4 text-on-surface focus:ring-2 focus:ring-primary/30 outline-none">
                <option value="request_song">请求添加歌曲</option>
                <option value="report_error">歌曲信息纠错</option>
                <option value="other">其他建议</option>
              </select>
            </div>
            <div v-if="feedbackType === 'request_song' || feedbackType === 'report_error'">
              <label class="block text-sm font-medium text-on-surface-variant mb-1">歌曲名称 {{ feedbackType === 'request_song' ? '(必填)' : '' }}</label>
              <input v-model="feedbackSongName" :required="feedbackType === 'request_song'" class="w-full bg-surface-container-high border-none rounded-lg py-3 px-4 text-on-surface focus:ring-2 focus:ring-primary/30 outline-none" />
            </div>
            <div v-if="feedbackType === 'request_song' || feedbackType === 'report_error'">
              <label class="block text-sm font-medium text-on-surface-variant mb-1">歌手（可选）</label>
              <input v-model="feedbackArtist" class="w-full bg-surface-container-high border-none rounded-lg py-3 px-4 text-on-surface focus:ring-2 focus:ring-primary/30 outline-none" />
            </div>
            <div>
              <label class="block text-sm font-medium text-on-surface-variant mb-1">补充说明（可选）</label>
              <textarea v-model="feedbackDescription" rows="3" class="w-full bg-surface-container-high border-none rounded-lg py-3 px-4 text-on-surface focus:ring-2 focus:ring-primary/30 outline-none resize-none"></textarea>
            </div>
          </div>
          <div class="flex justify-end gap-3 pt-2">
            <button @click="showFeedbackDialog = false" class="px-6 py-3 rounded-lg font-medium text-on-surface-variant hover:bg-surface-container transition-colors">取消</button>
            <button @click="submitFeedback" :disabled="feedbackLoading" class="px-6 py-3 bg-primary text-on-primary rounded-lg font-semibold hover:opacity-90 transition-opacity disabled:opacity-60">
              {{ feedbackLoading ? '提交中...' : '提交' }}
            </button>
          </div>
        </template>
        <template v-else>
          <div class="text-center py-8">
            <span class="material-symbols-outlined text-5xl text-primary mb-4">check_circle</span>
            <p class="text-lg font-semibold text-on-surface">反馈已提交，感谢您的建议</p>
          </div>
        </template>
      </div>
    </div>
  </div>
</template>
