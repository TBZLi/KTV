<script setup lang="ts">
import { ref, onMounted, computed } from 'vue'
import { roomApi } from '@/api'
import { usePlayerStore } from '@/stores/player'
import type { RoomInfo, PlayQueueItem } from '@/types'

const player = usePlayerStore()
const API_BASE = 'http://localhost:5276'

const roomInfo = ref<RoomInfo | null>(null)
const queue = ref<PlayQueueItem[]>([])

const progressPercent = computed(() => {
  if (player.duration <= 0) return 0
  return (player.currentTime / player.duration) * 100
})

async function loadRoom() {
  const { data } = await roomApi.getCurrent()
  roomInfo.value = data
}

async function loadQueue() {
  const { data } = await roomApi.getQueue()
  queue.value = data

  // Sync queue to player store
  player.loadQueue(
    data.map(item => ({
      songId: item.songId,
      title: item.songTitle,
      artist: item.artist,
      coverUrl: item.coverUrl,
      mediaUrl: item.mediaUrl,
    }))
  )
}

async function removeFromQueue(queueId: number) {
  const item = queue.value.find(q => q.id === queueId)
  await roomApi.removeFromQueue(queueId)
  queue.value = queue.value.filter(q => q.id !== queueId)

  // If removed item was playing, player will need update
  if (item && player.currentTrack?.songId === item.songId) {
    player.loadQueue(
      queue.value.map(q => ({
        songId: q.songId,
        title: q.songTitle,
        artist: q.artist,
        coverUrl: q.coverUrl,
        mediaUrl: q.mediaUrl,
      }))
    )
  }
}

function playSong(songId: number) {
  player.playTrackBySongId(songId)
}

function formatTime(seconds: number): string {
  const m = Math.floor(seconds / 60)
  const s = Math.floor(seconds % 60)
  return `${m.toString().padStart(2, '0')}:${s.toString().padStart(2, '0')}`
}

onMounted(() => {
  loadRoom()
  loadQueue()
})
</script>

<template>
  <div>
    <!-- Header Section -->
    <div class="mb-10">
      <h1 class="text-4xl font-extrabold tracking-tight mb-4 text-on-surface">当前房间</h1>
      <div class="flex gap-8 text-sm font-medium text-on-surface-variant">
        <span v-if="roomInfo" class="flex items-center gap-2 bg-surface-container-low px-4 py-2 rounded-full">
          <span class="material-symbols-outlined text-sm">label</span> 包厢号: {{ roomInfo.roomNumber }}
        </span>
        <span class="flex items-center gap-2 bg-surface-container-low px-4 py-2 rounded-full">
          <span class="material-symbols-outlined text-sm">format_list_bulleted</span> 已点歌曲: {{ queue.length }}
        </span>
      </div>
    </div>

    <!-- Now Playing hero card -->
    <div v-if="player.currentTrack" class="bg-surface-container-lowest rounded-xl p-8 shadow-sm flex gap-8 items-center mb-10">
      <!-- Large album cover -->
      <img
        v-if="player.currentTrack.coverUrl"
        :src="API_BASE + player.currentTrack.coverUrl"
        class="w-48 h-48 rounded-lg flex-shrink-0 object-cover cursor-pointer"
        :alt="player.currentTrack.title"
        @error="($event.target as HTMLImageElement).src = API_BASE + '/uploads/covers/default.jpg'"
      />
      <div v-else class="w-48 h-48 bg-slate-200 rounded-lg flex-shrink-0 flex items-center justify-center text-slate-400 text-center font-bold px-4">
        <span class="material-symbols-outlined text-6xl text-slate-400">music_note</span>
      </div>
      <div class="flex-grow">
        <div class="flex justify-between items-start mb-6">
          <div>
            <h2 class="text-3xl font-bold mb-1">{{ player.currentTrack.title }}</h2>
            <p class="text-on-surface-variant">{{ player.currentTrack.artist }}</p>
          </div>
          <div class="flex gap-2">
            <button class="p-3 bg-surface-container rounded-full hover:bg-surface-variant transition-colors">
              <span class="material-symbols-outlined">replay</span>
            </button>
            <button
              class="p-3 bg-primary text-white rounded-full hover:opacity-90 transition-opacity flex items-center gap-2 px-6 font-bold"
              @click="player.playNext()"
              :class="{ 'opacity-30 pointer-events-none': player.currentIndex >= player.queue.length - 1 }"
            >
              <span class="material-symbols-outlined">skip_next</span> 切歌
            </button>
          </div>
        </div>
        <!-- Progress Bar -->
        <div class="space-y-2">
          <div class="h-1.5 w-full bg-surface-container rounded-full overflow-hidden">
            <div class="h-full bg-primary rounded-full transition-all" :style="{ width: progressPercent + '%' }"></div>
          </div>
          <div class="flex justify-between text-xs font-bold text-slate-400 tracking-widest">
            <span>{{ formatTime(player.currentTime) }}</span>
            <span>{{ formatTime(player.duration) }}</span>
          </div>
        </div>
      </div>
    </div>

    <!-- Empty now-playing state -->
    <div v-else class="bg-surface-container-lowest rounded-xl p-8 shadow-sm flex gap-8 items-center mb-10">
      <div class="w-48 h-48 bg-slate-100 rounded-lg flex-shrink-0 flex items-center justify-center">
        <span class="material-symbols-outlined text-6xl text-slate-300">music_off</span>
      </div>
      <div>
        <h2 class="text-2xl font-bold text-slate-400 mb-2">未在播放</h2>
        <p class="text-slate-400">在歌曲列表中点歌开始播放</p>
      </div>
    </div>

    <!-- Queue section -->
    <h3 class="text-xl font-bold px-2 mb-4">播放列表</h3>
    <div class="bg-surface-container-lowest rounded-xl overflow-hidden">
      <div
        v-for="(item, index) in queue"
        :key="item.id"
        class="flex items-center px-8 py-6 hover:bg-surface-container-low transition-colors group cursor-pointer"
        :class="{ 'bg-primary/5': player.currentTrack?.songId === item.songId }"
        @click="playSong(item.songId)"
      >
        <span
          class="w-8 font-bold"
          :class="player.currentTrack?.songId === item.songId ? 'text-primary' : 'text-slate-400'"
        >
          <span v-if="player.currentTrack?.songId === item.songId && player.isPlaying" class="material-symbols-outlined text-sm align-middle">equalizer</span>
          <span v-else>{{ String(index + 1).padStart(2, '0') }}</span>
        </span>
        <img
          v-if="item.coverUrl"
          :src="API_BASE + item.coverUrl"
          class="w-12 h-12 rounded-lg mx-6 flex-shrink-0 object-cover cursor-pointer transition-transform duration-200 hover:scale-[2.5] hover:shadow-lg hover:z-10 relative"
          :alt="item.songTitle"
          @error="($event.target as HTMLImageElement).src = API_BASE + '/uploads/covers/default.jpg'"
        />
        <div v-else class="w-12 h-12 bg-slate-200 rounded-lg mx-6 flex-shrink-0 flex items-center justify-center">
          <span class="material-symbols-outlined text-slate-400 text-sm">image</span>
        </div>
        <div class="flex-grow grid grid-cols-4 items-center">
          <span class="font-bold" :class="{ 'text-primary': player.currentTrack?.songId === item.songId }">{{ item.songTitle }}</span>
          <span class="text-on-surface-variant">{{ item.artist }}</span>
          <span class="text-sm text-slate-400">点播者: {{ item.orderedBy }}</span>
          <div class="flex justify-end gap-4 opacity-0 group-hover:opacity-100 transition-opacity">
            <button class="text-primary hover:scale-110 transition-transform">
              <span class="material-symbols-outlined">vertical_align_top</span>
            </button>
            <button
              @click.stop="removeFromQueue(item.id)"
              class="text-error hover:scale-110 transition-transform"
            >
              <span class="material-symbols-outlined">delete</span>
            </button>
          </div>
        </div>
      </div>
    </div>

    <!-- Empty state -->
    <div v-if="queue.length === 0" class="text-center py-20">
      <span class="material-symbols-outlined text-6xl text-surface-container-highest">queue_music</span>
      <p class="text-on-surface-variant mt-4">播放列表为空</p>
    </div>
  </div>
</template>
