<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { roomApi } from '@/api'
import type { RoomInfo, PlayQueueItem } from '@/types'

const roomInfo = ref<RoomInfo | null>(null)
const queue = ref<PlayQueueItem[]>([])
const nowPlaying = ref<{ title: string; artist: string; duration: number; currentTime: number } | null>(null)

async function loadRoom() {
  const { data } = await roomApi.getCurrent()
  roomInfo.value = data
}

async function loadQueue() {
  const { data } = await roomApi.getQueue()
  queue.value = data
  if (data.length > 0) {
    nowPlaying.value = {
      title: data[0].songTitle,
      artist: data[0].artist,
      duration: 270,
      currentTime: 0,
    }
  }
}

async function removeFromQueue(queueId: number) {
  await roomApi.removeFromQueue(queueId)
  queue.value = queue.value.filter((item) => item.id !== queueId)
}

function formatTime(seconds: number): string {
  const m = Math.floor(seconds / 60)
  const s = seconds % 60
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
    <div v-if="nowPlaying" class="bg-surface-container-lowest rounded-xl p-8 shadow-sm flex gap-8 items-center mb-10">
      <!-- Large album cover -->
      <img
        v-if="queue.length > 0 && queue[0].coverUrl"
        :src="'http://localhost:5276' + queue[0].coverUrl"
        class="w-48 h-48 rounded-lg flex-shrink-0 object-cover cursor-pointer"
        :alt="nowPlaying.title"
        @error="($event.target as HTMLImageElement).src = 'http://localhost:5276/uploads/covers/default.jpg'"
      />
      <div v-else class="w-48 h-48 bg-slate-200 rounded-lg flex-shrink-0 flex items-center justify-center text-slate-400 text-center font-bold px-4">
        <span class="material-symbols-outlined text-6xl text-slate-400">music_note</span>
      </div>
      <div class="flex-grow">
        <div class="flex justify-between items-start mb-6">
          <div>
            <h2 class="text-3xl font-bold mb-1">{{ nowPlaying.title }}</h2>
            <p class="text-on-surface-variant">{{ nowPlaying.artist }}</p>
          </div>
          <div class="flex gap-2">
            <button class="p-3 bg-surface-container rounded-full hover:bg-surface-variant transition-colors">
              <span class="material-symbols-outlined">replay</span>
            </button>
            <button class="p-3 bg-primary text-white rounded-full hover:opacity-90 transition-opacity flex items-center gap-2 px-6 font-bold">
              <span class="material-symbols-outlined">skip_next</span> 切歌
            </button>
          </div>
        </div>
        <!-- Progress Bar -->
        <div class="space-y-2">
          <div class="h-1.5 w-full bg-surface-container rounded-full overflow-hidden">
            <div class="h-full w-1/3 bg-primary rounded-full"></div>
          </div>
          <div class="flex justify-between text-xs font-bold text-slate-400 tracking-widest">
            <span>{{ formatTime(nowPlaying.currentTime) }}</span>
            <span>{{ formatTime(nowPlaying.duration) }}</span>
          </div>
        </div>
      </div>
    </div>

    <!-- Queue section -->
    <h3 class="text-xl font-bold px-2 mb-4">播放列表</h3>
    <div class="bg-surface-container-lowest rounded-xl overflow-hidden">
      <div
        v-for="(item, index) in queue"
        :key="item.id"
        class="flex items-center px-8 py-6 hover:bg-surface-container-low transition-colors group"
      >
        <span class="w-8 text-slate-400 font-bold">{{ String(index + 1).padStart(2, '0') }}</span>
        <img
          v-if="item.coverUrl"
          :src="'http://localhost:5276' + item.coverUrl"
          class="w-12 h-12 rounded-lg mx-6 flex-shrink-0 object-cover cursor-pointer transition-transform duration-200 hover:scale-[2.5] hover:shadow-lg hover:z-10 relative"
          :alt="item.songTitle"
          @error="($event.target as HTMLImageElement).src = 'http://localhost:5276/uploads/covers/default.jpg'"
        />
        <div v-else class="w-12 h-12 bg-slate-200 rounded-lg mx-6 flex-shrink-0 flex items-center justify-center">
          <span class="material-symbols-outlined text-slate-400 text-sm">image</span>
        </div>
        <div class="flex-grow grid grid-cols-4 items-center">
          <span class="font-bold">{{ item.songTitle }}</span>
          <span class="text-on-surface-variant">{{ item.artist }}</span>
          <span class="text-sm text-slate-400">点播者: {{ item.orderedBy }}</span>
          <div class="flex justify-end gap-4 opacity-0 group-hover:opacity-100 transition-opacity">
            <button class="text-primary hover:scale-110 transition-transform">
              <span class="material-symbols-outlined">vertical_align_top</span>
            </button>
            <button
              @click="removeFromQueue(item.id)"
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
