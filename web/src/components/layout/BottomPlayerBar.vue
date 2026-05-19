<script setup lang="ts">
import { computed, ref, onMounted, onUnmounted } from 'vue'
import { usePlayerStore } from '@/stores/player'

const player = usePlayerStore()
const API_BASE = import.meta.env.VITE_API_BASE_URL?.replace(/\/api$/, '') || 'https://localhost:5001'

const showLyrics = ref(false)
const isDraggingProgress = ref(false)

function onKeydown(e: KeyboardEvent) {
  if (e.key === 'Escape' && showLyrics.value) {
    showLyrics.value = false
  }
}
onMounted(() => window.addEventListener('keydown', onKeydown))
onUnmounted(() => window.removeEventListener('keydown', onKeydown))

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

function openLyrics() {
  if (!player.hasTrack) return
  showLyrics.value = true
}

function closeLyrics() {
  showLyrics.value = false
}

const coverSrc = computed(() =>
  player.currentTrack?.coverUrl
    ? API_BASE + player.currentTrack.coverUrl
    : API_BASE + '/uploads/covers/default.jpg'
)
</script>

<template>
  <!-- ====== 胶囊播放器 ====== -->
  <footer class="fixed bottom-6 left-1/2 -translate-x-1/2 z-50 flex items-center gap-4 bg-white/80 backdrop-blur-2xl rounded-full pl-2 pr-6 py-2 shadow-[0_8px_40px_rgba(0,0,0,0.12)] transition-all duration-500"
    :class="showLyrics ? 'opacity-0 pointer-events-none translate-y-4' : 'opacity-100'"
    style="max-width: 560px;">

    <!-- Cover (click to open lyrics) -->
    <div class="shrink-0" :class="player.hasTrack ? 'cursor-pointer' : 'cursor-default'" @click="openLyrics">
      <div class="relative w-14 h-14 rounded-full overflow-hidden shadow-lg group">
        <img
          :src="coverSrc"
          class="w-full h-full object-cover group-hover:scale-110 transition-transform duration-300"
          :alt="player.currentTrack?.title ?? '封面'"
          @error="($event.target as HTMLImageElement).src = API_BASE + '/uploads/covers/default.jpg'"
        />
        <div class="absolute inset-0 bg-black/30 flex items-center justify-center opacity-0 group-hover:opacity-100 transition-opacity pointer-events-none">
          <span class="material-symbols-outlined text-white text-xl">expand_less</span>
        </div>
      </div>
    </div>

    <!-- Info + progress -->
    <div class="flex-1 min-w-0">
      <p v-if="player.hasTrack" class="text-sm font-bold truncate">
        {{ player.currentTrack.title }} <span class="font-normal text-slate-500">- {{ player.currentTrack.artist }}</span>
      </p>
      <p v-else class="text-sm text-slate-400 truncate">未在播放</p>
      <!-- Inline progress bar -->
      <div
        class="mt-1.5 h-1 w-full bg-slate-200/60 rounded-full cursor-pointer group"
        @click.stop="onProgressClick"
        @mousedown.stop="onProgressMousedown"
      >
        <div class="h-full bg-gradient-to-r from-primary to-secondary rounded-full relative transition-all" :style="{ width: progressPercent + '%' }">
          <div class="absolute right-0 top-1/2 -translate-y-1/2 w-2.5 h-2.5 bg-primary rounded-full shadow opacity-0 group-hover:opacity-100 transition-opacity"></div>
        </div>
      </div>
    </div>

    <!-- Controls -->
    <div class="flex items-center gap-1 shrink-0">
      <button
        class="w-8 h-8 flex items-center justify-center text-slate-400 hover:text-on-surface transition-colors rounded-full hover:bg-slate-100 press-scale"
        :class="{ 'opacity-30 pointer-events-none': !player.hasTrack || player.currentIndex <= 0 }"
        @click="player.playPrev()"
      >
        <span class="material-symbols-outlined text-xl">skip_previous</span>
      </button>
      <button
        class="w-10 h-10 flex items-center justify-center text-primary hover:scale-110 transition-transform press-scale"
        :class="{ 'opacity-30 pointer-events-none': !player.hasTrack }"
        @click="player.togglePlay()"
      >
        <span class="material-symbols-outlined text-3xl" :style="{ fontVariationSettings: `'FILL' ${player.isPlaying ? 1 : 0}` }">
          {{ player.isPlaying ? 'pause_circle' : 'play_circle' }}
        </span>
      </button>
      <button
        class="w-8 h-8 flex items-center justify-center text-slate-400 hover:text-on-surface transition-colors rounded-full hover:bg-slate-100 press-scale"
        :class="{ 'opacity-30 pointer-events-none': !player.hasTrack || player.currentIndex >= player.queue.length - 1 }"
        @click="player.playNext()"
      >
        <span class="material-symbols-outlined text-xl">skip_next</span>
      </button>
    </div>

    <!-- Time -->
    <span v-if="player.hasTrack" class="text-[10px] text-slate-400 font-mono tabular-nums shrink-0">{{ formatTime(player.currentTime) }}</span>
  </footer>

  <!-- ====== 歌词页全屏覆盖 ====== -->
  <Transition name="lyrics-slide">
    <div v-if="showLyrics" class="fixed inset-0 z-[60] flex flex-col">
      <!-- 封面背景 + 毛玻璃 -->
      <img
        :src="coverSrc"
        class="absolute inset-0 w-full h-full object-cover scale-110 blur-3xl brightness-75"
        :alt="player.currentTrack?.title ?? ''"
      />
      <div class="absolute inset-0 bg-black/40 backdrop-blur-xl"></div>

      <!-- 内容层 -->
      <div class="relative z-10 flex flex-col h-full">
        <!-- 顶部：收起按钮 -->
        <div class="flex items-center justify-between px-8 py-5">
          <button @click="closeLyrics" class="w-10 h-10 flex items-center justify-center text-white/80 hover:text-white transition-colors press-scale">
            <span class="material-symbols-outlined">expand_more</span>
          </button>
          <div class="text-center">
            <p class="text-white/60 text-xs font-medium uppercase tracking-widest">正在播放</p>
            <p class="text-white text-sm font-bold">{{ player.currentTrack?.artist }}</p>
          </div>
          <div class="w-10"></div>
        </div>

        <!-- 中间：大封面 + 歌名 -->
        <div class="flex-1 flex flex-col items-center justify-center px-8 gap-8">
          <!-- 大封面 -->
          <img
            :src="coverSrc"
            class="w-64 h-64 rounded-2xl object-cover shadow-2xl shadow-black/40"
            :alt="player.currentTrack?.title ?? ''"
            @error="($event.target as HTMLImageElement).src = API_BASE + '/uploads/covers/default.jpg'"
          />
          <!-- 歌曲信息 -->
          <div class="text-center">
            <h2 class="text-2xl font-bold text-white mb-1">{{ player.currentTrack?.title ?? '未在播放' }}</h2>
            <p class="text-white/60">{{ player.currentTrack?.artist }}</p>
          </div>
          <!-- 歌词占位 -->
          <div class="text-center py-8">
            <p class="text-white/30 text-sm">歌词功能即将上线</p>
          </div>
        </div>

        <!-- 底部：B 风格大控件 -->
        <div class="px-8 pb-10">
          <!-- 进度条（粗，带时间标签） -->
          <div class="relative mb-6">
            <div class="flex justify-between text-[11px] text-white/50 font-mono tabular-nums mb-2">
              <span>{{ formatTime(player.currentTime) }}</span>
              <span>{{ formatTime(player.duration) }}</span>
            </div>
            <div
              class="h-1.5 w-full bg-white/20 rounded-full cursor-pointer group"
              @click="onProgressClick"
              @mousedown="onProgressMousedown"
            >
              <div class="h-full bg-white rounded-r-full relative transition-all" :style="{ width: progressPercent + '%' }">
                <div class="absolute right-0 top-1/2 -translate-y-1/2 w-4 h-4 bg-white border-2 border-white rounded-full shadow-md opacity-0 group-hover:opacity-100 transition-opacity -mr-1"></div>
              </div>
            </div>
          </div>

          <!-- 控制按钮 -->
          <div class="flex items-center justify-center gap-6">
            <button
              class="text-white/60 hover:text-white transition-colors press-scale"
              :class="{ 'opacity-30 pointer-events-none': player.currentIndex <= 0 }"
              @click="player.playPrev()"
            >
              <span class="material-symbols-outlined text-2xl">skip_previous</span>
            </button>
            <button
              class="w-14 h-14 rounded-full bg-white text-slate-900 flex items-center justify-center hover:scale-105 transition-transform shadow-lg press-scale"
              @click="player.togglePlay()"
            >
              <span class="material-symbols-outlined text-3xl">{{ player.isPlaying ? 'pause' : 'play_arrow' }}</span>
            </button>
            <button
              class="text-white/60 hover:text-white transition-colors press-scale"
              :class="{ 'opacity-30 pointer-events-none': player.currentIndex >= player.queue.length - 1 }"
              @click="player.playNext()"
            >
              <span class="material-symbols-outlined text-2xl">skip_next</span>
            </button>
          </div>
        </div>
      </div>
    </div>
  </Transition>
</template>

<style scoped>
.lyrics-slide-enter-active,
.lyrics-slide-leave-active {
  transition: transform 0.4s cubic-bezier(0.16, 1, 0.3, 1), opacity 0.3s ease;
}
.lyrics-slide-enter-from,
.lyrics-slide-leave-to {
  transform: translateY(100%);
  opacity: 0;
}
</style>
