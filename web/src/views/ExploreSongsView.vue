<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { songsApi, feedbacksApi, favoritesApi } from '@/api'
import { useSongOrder } from '@/composables/useSongOrder'
import type { Song } from '@/types'
import { formatDuration, formatPlayCount } from '@/utils/format'
import { useToast } from '@/composables/useToast'

// Toast
const { toastMsg, showToast } = useToast()

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
    status: 'active',
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
    showToast(err.response?.data?.message || '操作失败')
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
    showToast(err.response?.data?.message || '提交失败')
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

<style scoped>
.toast-enter-active,
.toast-leave-active {
  transition: all 0.3s ease;
}
.toast-enter-from,
.toast-leave-to {
  opacity: 0;
  transform: translate(-50%, -20px);
}
</style>

<template>
  <div class="max-w-4xl mx-auto px-8">
    <!-- Page Title -->
    <div class="mb-10">
      <h1 class="text-5xl font-extrabold text-on-surface dark:text-[var(--d-on-surface)] font-display tracking-tight mb-2">探索歌曲</h1>
      <p class="text-on-surface-variant dark:text-[var(--d-on-surface-variant)]">探索属于你的音乐世界，发现最新潮流单曲</p>
    </div>

    <!-- Genre filter pills -->
    <div class="flex items-center gap-3 mb-10 overflow-x-auto pb-2">
      <button
        @click="selectGenre('')"
        :class="[
          'px-6 py-2 rounded-full font-bold text-sm transition-all',
          selectedGenre === ''
            ? 'bg-primary text-on-primary dark:bg-[var(--d-primary)] dark:text-[var(--d-on-primary)]'
            : 'glass text-on-surface dark:text-[var(--d-on-surface)] hover:bg-primary-fixed dark:hover:bg-[var(--d-hover-bg)] shadow-sm',
        ]"
      >
        全部
      </button>
      <button
        v-for="genre in genres"
        :key="genre"
        @click="selectGenre(genre)"
        :class="[
          'px-6 py-2 rounded-full font-bold text-sm transition-all',
          selectedGenre === genre
            ? 'bg-primary text-on-primary dark:bg-[var(--d-primary)] dark:text-[var(--d-on-primary)]'
            : 'glass text-on-surface dark:text-[var(--d-on-surface)] hover:bg-primary-fixed dark:hover:bg-[var(--d-hover-bg)] shadow-sm',
        ]"
      >
        {{ genre }}
      </button>
    </div>

    <!-- Song list -->
    <div class="flex flex-col gap-4">
      <div
        v-for="song in songs"
        :key="song.id"
        class="flex items-center gap-6 p-4 -mx-4 rounded-lg hover:glass hover:shadow-lg dark:hover:shadow-[0_4px_20px_var(--d-shadow-color)] hover:scale-[1.02] transition-all duration-300 cursor-pointer group"
      >
        <!-- Album cover -->
        <img
          v-if="song.coverUrl"
          :src="API_BASE + song.coverUrl"
          class="w-16 h-16 rounded shrink-0 object-cover"
          :alt="song.title"
          @error="($event.target as HTMLImageElement).src = API_BASE + '/uploads/covers/default.jpg'"
        />
        <div v-else class="w-16 h-16 rounded bg-surface-container dark:bg-[var(--d-surface-container-high)] shrink-0 shadow-sm flex items-center justify-center">
          <span class="material-symbols-outlined text-slate-400 dark:text-[var(--d-outline)]">image</span>
        </div>

        <!-- Song info -->
        <div class="flex-1 min-w-0">
          <h3 class="text-xl font-bold text-on-surface dark:text-[var(--d-on-surface)] truncate font-headline mb-1">{{ song.title }}</h3>
          <p class="text-sm text-on-surface-variant dark:text-[var(--d-on-surface-variant)] truncate font-label">{{ song.artist }}</p>
        </div>

        <!-- Play count -->
        <div class="text-right mr-4 hidden sm:block">
          <span class="text-sm font-medium text-on-surface-variant dark:text-[var(--d-on-surface-variant)] font-label">{{ formatPlayCount(song.playCount) }} 次播放</span>
        </div>

        <!-- Actions -->
        <div class="flex items-center gap-3 opacity-0 group-hover:opacity-100 transition-opacity">
          <!-- Favorite toggle -->
          <button
            @click="toggleFavorite(song.id)"
            class="w-10 h-10 flex items-center justify-center rounded-full hover:bg-surface-container-high dark:hover:bg-[var(--d-hover-bg)] transition-colors"
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
            class="px-6 py-3 bg-primary text-on-primary rounded-full font-bold font-body text-sm active:scale-90 transition-transform shadow-lg shadow-primary/10"
          >
            点歌
          </button>
        </div>
      </div>
    </div>

    <!-- Feedback FAB -->
    <button
      @click="openFeedbackDialog"
      class="fixed bottom-28 right-8 w-14 h-14 bg-primary text-on-primary dark:bg-[var(--d-primary)] dark:text-[var(--d-on-primary)] rounded-full shadow-lg shadow-primary/30 flex items-center justify-center hover:scale-110 active:scale-95 transition-transform z-40"
      title="反馈"
    >
      <span class="material-symbols-outlined">feedback</span>
    </button>

    <!-- Feedback Dialog -->
    <div v-if="showFeedbackDialog" class="fixed inset-0 z-50 flex items-center justify-center bg-black/40 dark:bg-black/60" @click.self="showFeedbackDialog = false">
      <div class="glass rounded-2xl shadow-xl w-full max-w-md p-8 space-y-6">
        <template v-if="!feedbackSuccess">
          <h3 class="text-xl font-display font-bold text-on-surface dark:text-[var(--d-on-surface)]">用户反馈</h3>
          <div class="space-y-4">
            <div>
              <label class="block text-sm font-medium text-on-surface-variant dark:text-[var(--d-on-surface-variant)] mb-1">反馈类型</label>
              <select v-model="feedbackType" class="w-full bg-surface-container-high dark:bg-[var(--d-input-bg)] border-none rounded-lg py-3 px-4 text-on-surface dark:text-[var(--d-on-surface)] focus:ring-2 focus:ring-primary/30 outline-none dark:ring-1 dark:ring-[var(--d-outline-variant)]">
                <option value="request_song">请求添加歌曲</option>
                <option value="report_error">歌曲信息纠错</option>
                <option value="other">其他建议</option>
              </select>
            </div>
            <div v-if="feedbackType === 'request_song' || feedbackType === 'report_error'">
              <label class="block text-sm font-medium text-on-surface-variant dark:text-[var(--d-on-surface-variant)] mb-1">歌曲名称 {{ feedbackType === 'request_song' ? '(必填)' : '' }}</label>
              <input v-model="feedbackSongName" :required="feedbackType === 'request_song'" class="w-full bg-surface-container-high dark:bg-[var(--d-input-bg)] border-none rounded-lg py-3 px-4 text-on-surface dark:text-[var(--d-on-surface)] focus:ring-2 focus:ring-primary/30 outline-none dark:ring-1 dark:ring-[var(--d-outline-variant)]" />
            </div>
            <div v-if="feedbackType === 'request_song' || feedbackType === 'report_error'">
              <label class="block text-sm font-medium text-on-surface-variant dark:text-[var(--d-on-surface-variant)] mb-1">歌手（可选）</label>
              <input v-model="feedbackArtist" class="w-full bg-surface-container-high dark:bg-[var(--d-input-bg)] border-none rounded-lg py-3 px-4 text-on-surface dark:text-[var(--d-on-surface)] focus:ring-2 focus:ring-primary/30 outline-none dark:ring-1 dark:ring-[var(--d-outline-variant)]" />
            </div>
            <div>
              <label class="block text-sm font-medium text-on-surface-variant dark:text-[var(--d-on-surface-variant)] mb-1">补充说明（可选）</label>
              <textarea v-model="feedbackDescription" rows="3" class="w-full bg-surface-container-high dark:bg-[var(--d-input-bg)] border-none rounded-lg py-3 px-4 text-on-surface dark:text-[var(--d-on-surface)] focus:ring-2 focus:ring-primary/30 outline-none resize-none dark:ring-1 dark:ring-[var(--d-outline-variant)]"></textarea>
            </div>
          </div>
          <div class="flex justify-end gap-3 pt-2">
            <button @click="showFeedbackDialog = false" class="px-6 py-3 rounded-lg font-medium text-on-surface-variant dark:text-[var(--d-on-surface-variant)] hover:bg-surface-container dark:hover:bg-[var(--d-hover-bg)] transition-colors">取消</button>
            <button @click="submitFeedback" :disabled="feedbackLoading" class="px-6 py-3 bg-primary text-on-primary dark:bg-[var(--d-primary)] dark:text-[var(--d-on-primary)] rounded-lg font-semibold hover:opacity-90 transition-opacity disabled:opacity-60">
              {{ feedbackLoading ? '提交中...' : '提交' }}
            </button>
          </div>
        </template>
        <template v-else>
          <div class="text-center py-8">
            <span class="material-symbols-outlined text-5xl text-primary dark:text-[var(--d-primary)] mb-4">check_circle</span>
            <p class="text-lg font-semibold text-on-surface dark:text-[var(--d-on-surface)]">反馈已提交，感谢您的建议</p>
          </div>
        </template>
      </div>
    </div>

    <!-- Toast -->
    <Transition name="toast">
      <div v-if="toastMsg" class="fixed top-8 left-1/2 -translate-x-1/2 z-[100] bg-error text-on-error px-6 py-3 rounded-xl shadow-lg text-sm font-medium">
        {{ toastMsg }}
      </div>
    </Transition>
  </div>
</template>
