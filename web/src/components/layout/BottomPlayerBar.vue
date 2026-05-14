<script setup lang="ts">
import { computed, ref } from 'vue'
import { usePlayerStore } from '@/stores/player'

const player = usePlayerStore()

const API_BASE = 'http://localhost:5276'

const isDraggingProgress = ref(false)

const progressPercent = computed(() => {
  if (player.duration <= 0) return 0
  return (player.currentTime / player.duration) * 100
})

function formatTime(seconds: number): string {
  const m = Math.floor(seconds / 60)
  const s = Math.floor(seconds % 60)
  return `${m.toString().padStart(2, '0')}:${s.toString().padStart(2, '0')}`
}

function onProgressClick(e: MouseEvent) {
  if (!player.hasTrack) return
  const bar = e.currentTarget as HTMLElement
  const rect = bar.getBoundingClientRect()
  const percent = (e.clientX - rect.left) / rect.width
  player.seek(percent * player.duration)
}

function onProgressMousedown(e: MouseEvent) {
  if (!player.hasTrack) return
  isDraggingProgress.value = true
  const bar = e.currentTarget as HTMLElement

  const onMove = (ev: MouseEvent) => {
    const rect = bar.getBoundingClientRect()
    const percent = Math.max(0, Math.min(1, (ev.clientX - rect.left) / rect.width))
    player.seek(percent * player.duration)
  }

  const onUp = () => {
    isDraggingProgress.value = false
    window.removeEventListener('mousemove', onMove)
    window.removeEventListener('mouseup', onUp)
  }

  window.addEventListener('mousemove', onMove)
  window.addEventListener('mouseup', onUp)
}

function onVolumeClick(e: MouseEvent) {
  const bar = e.currentTarget as HTMLElement
  const rect = bar.getBoundingClientRect()
  const percent = Math.max(0, Math.min(1, (e.clientX - rect.left) / rect.width))
  player.setVolume(Math.round(percent * 100))
}
</script>

<template>
  <footer class="fixed bottom-0 left-0 w-full z-50 h-24 bg-white/70 backdrop-blur-xl flex justify-between items-center px-12 rounded-t-[3rem] shadow-[0_-8px_32px_rgba(25,28,30,0.06)]">
    <!-- Progress bar at top of player -->
    <div
      class="absolute top-0 left-0 w-full h-1 bg-surface-container cursor-pointer group"
      @click="onProgressClick"
      @mousedown="onProgressMousedown"
    >
      <div
        class="h-full bg-gradient-to-r from-primary to-secondary transition-all"
        :style="{ width: progressPercent + '%' }"
      ></div>
    </div>

    <!-- Left: track info -->
    <div class="flex items-center gap-4 w-1/4">
      <img
        v-if="player.currentTrack?.coverUrl"
        :src="API_BASE + player.currentTrack.coverUrl"
        class="w-12 h-12 rounded-lg flex-shrink-0 object-cover"
        :alt="player.currentTrack?.title"
        @error="($event.target as HTMLImageElement).src = API_BASE + '/uploads/covers/default.jpg'"
      />
      <div v-else class="w-12 h-12 bg-slate-200 rounded-lg flex items-center justify-center text-[8px] text-slate-500 text-center px-1">
        <span class="material-symbols-outlined text-slate-400">music_note</span>
      </div>
      <div class="overflow-hidden">
        <p v-if="player.hasTrack" class="font-label text-xs font-bold uppercase tracking-widest text-primary">Playing</p>
        <p v-else class="font-label text-xs font-bold uppercase tracking-widest text-slate-400">Stopped</p>
        <p class="text-sm font-bold truncate">
          {{ player.currentTrack?.title ?? '未在播放' }}
          <span v-if="player.currentTrack" class="font-normal text-slate-500"> - {{ player.currentTrack.artist }}</span>
        </p>
      </div>
    </div>

    <!-- Center: controls -->
    <div class="flex items-center gap-8">
      <button
        class="text-slate-400 hover:scale-110 transition-transform press-scale"
        :class="{ 'opacity-30 pointer-events-none': !player.hasTrack || player.currentIndex <= 0 }"
        @click="player.playPrev()"
      >
        <span class="material-symbols-outlined">skip_previous</span>
      </button>
      <button
        class="text-primary hover:scale-110 transition-transform scale-125 press-scale"
        :class="{ 'opacity-30 pointer-events-none': !player.hasTrack }"
        @click="player.togglePlay()"
      >
        <span class="material-symbols-outlined" :style="{ fontVariationSettings: `'FILL' ${player.isPlaying ? 1 : 0}` }">
          {{ player.isPlaying ? 'pause_circle' : 'play_circle' }}
        </span>
      </button>
      <button
        class="text-slate-400 hover:scale-110 transition-transform press-scale"
        :class="{ 'opacity-30 pointer-events-none': !player.hasTrack || player.currentIndex >= player.queue.length - 1 }"
        @click="player.playNext()"
      >
        <span class="material-symbols-outlined">skip_next</span>
      </button>
    </div>

    <!-- Right: time + volume -->
    <div class="flex items-center gap-6 w-1/4 justify-end">
      <!-- Time display -->
      <span v-if="player.hasTrack" class="text-xs text-slate-400 font-mono tabular-nums whitespace-nowrap">
        {{ formatTime(player.currentTime) }} / {{ formatTime(player.duration) }}
      </span>
      <!-- Volume -->
      <div class="flex items-center gap-3 w-32">
        <span class="material-symbols-outlined text-slate-400 cursor-pointer" @click="player.setVolume(player.volume > 0 ? 0 : 80)">
          {{ player.volume === 0 ? 'volume_off' : player.volume < 50 ? 'volume_down' : 'volume_up' }}
        </span>
        <div class="h-1 flex-grow bg-surface-container rounded-full cursor-pointer" @click="onVolumeClick">
          <div class="h-full bg-primary rounded-full relative" :style="{ width: player.volume + '%' }">
            <div class="absolute right-0 top-1/2 -translate-y-1/2 w-3 h-3 bg-primary border-2 border-white rounded-full shadow-sm"></div>
          </div>
        </div>
      </div>
    </div>
  </footer>
</template>
