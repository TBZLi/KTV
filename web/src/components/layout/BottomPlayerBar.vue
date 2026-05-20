<script setup lang="ts">
import { ref, computed, watch, onMounted, onUnmounted, nextTick } from 'vue'
import { usePlayerStore } from '@/stores/player'
import { favoritesApi } from '@/api'
import { parseLrc, findCurrentLine, type LyricLine } from '@/utils/lrcParser'

const player = usePlayerStore()
const API_BASE = import.meta.env.VITE_API_BASE_URL?.replace(/\/api$/, '') || 'https://localhost:5001'

const showLyrics = ref(false)
const isDraggingProgress = ref(false)

// --- Lyrics ---
const lyrics = ref<LyricLine[]>([])
const lyricsContainer = ref<HTMLElement | null>(null)
const currentLine = computed(() => findCurrentLine(lyrics.value, player.currentTime))

async function fetchLyrics(lrcUrl: string) {
  try {
    const res = await fetch(API_BASE + lrcUrl)
    const text = await res.text()
    lyrics.value = parseLrc(text)
  } catch {
    lyrics.value = []
  }
}

watch(() => player.currentTrack?.lrcUrl, (url) => {
  if (url) fetchLyrics(url)
  else lyrics.value = []
}, { immediate: true })

watch(currentLine, async (idx) => {
  if (idx < 0 || !lyricsContainer.value) return
  await nextTick()
  const el = lyricsContainer.value.querySelector(`[data-line="${idx}"]`) as HTMLElement
  if (el) el.scrollIntoView({ behavior: 'smooth', block: 'center' })
})

function seekToLine(time: number) { player.seek(time) }

// --- Funnel style ---
function lineStyle(i: number) {
  const dist = Math.abs(i - currentLine.value)
  if (currentLine.value < 0) return { transform: 'scale(0.85)', opacity: 0.4, color: 'rgba(255,255,255,0.4)' }
  if (dist === 0) return { transform: 'scale(1.15)', opacity: 1, color: '#fff' }
  if (dist === 1) return { transform: 'scale(0.95)', opacity: 0.75, color: 'rgba(255,255,255,0.75)' }
  if (dist === 2) return { transform: 'scale(0.88)', opacity: 0.55, color: 'rgba(255,255,255,0.55)' }
  if (dist === 3) return { transform: 'scale(0.82)', opacity: 0.4, color: 'rgba(255,255,255,0.4)' }
  return { transform: 'scale(0.78)', opacity: 0.3, color: 'rgba(255,255,255,0.3)' }
}

// --- Play mode ---
const playModeIcon = computed(() => {
  switch (player.playMode) {
    case 'repeat-all': return 'repeat'
    case 'repeat-one': return 'repeat_one'
    case 'shuffle': return 'shuffle'
    default: return 'trending_flat'
  }
})
const playModeLabel = computed(() => {
  switch (player.playMode) {
    case 'repeat-all': return '列表循环'
    case 'repeat-one': return '单曲循环'
    case 'shuffle': return '随机播放'
    default: return '顺序播放'
  }
})

// --- Volume ---
function onVolumeClick(e: MouseEvent) {
  const bar = e.currentTarget as HTMLElement
  const rect = bar.getBoundingClientRect()
  player.setVolume(Math.round(Math.max(0, Math.min(1, (e.clientX - rect.left) / rect.width)) * 100))
}
function onVolumeMousedown(e: MouseEvent) {
  const bar = e.currentTarget as HTMLElement
  const onMove = (ev: MouseEvent) => {
    const rect = bar.getBoundingClientRect()
    player.setVolume(Math.round(Math.max(0, Math.min(1, (ev.clientX - rect.left) / rect.width)) * 100))
  }
  const onUp = () => { window.removeEventListener('mousemove', onMove); window.removeEventListener('mouseup', onUp) }
  window.addEventListener('mousemove', onMove)
  window.addEventListener('mouseup', onUp)
}
const volumeIcon = computed(() => {
  if (player.volume === 0) return 'volume_off'
  if (player.volume < 30) return 'volume_mute'
  if (player.volume < 70) return 'volume_down'
  return 'volume_up'
})

// --- Favorites ---
const favoriteIds = ref<Set<number>>(new Set())
async function loadFavorites() {
  try { const res = await favoritesApi.getList(); favoriteIds.value = new Set(res.data.map((f: any) => f.songId)) } catch {}
}
async function toggleFavorite(songId: number) {
  if (favoriteIds.value.has(songId)) { await favoritesApi.remove(songId); favoriteIds.value.delete(songId) }
  else { await favoritesApi.add(songId); favoriteIds.value.add(songId) }
  favoriteIds.value = new Set(favoriteIds.value)
}
onMounted(loadFavorites)

// --- Drag reorder ---
const dragIndex = ref<number | null>(null)
const dragOverIndex = ref<number | null>(null)
function onDragStart(i: number, e: DragEvent) {
  if (i === player.currentIndex) { return }
  dragIndex.value = i; if (e.dataTransfer) e.dataTransfer.effectAllowed = 'move'
}
function onDragOver(i: number, e: DragEvent) {
  e.preventDefault()
  if (e.dataTransfer) e.dataTransfer.dropEffect = i === player.currentIndex ? 'none' : 'move'
  dragOverIndex.value = i
}
function onDragLeave() { dragOverIndex.value = null }
function onDrop(i: number, e: DragEvent) {
  e.preventDefault()
  if (i === player.currentIndex) { dragIndex.value = null; dragOverIndex.value = null; return }
  if (dragIndex.value === null || dragIndex.value === i) { dragIndex.value = null; dragOverIndex.value = null; return }
  const from = dragIndex.value
  const [item] = player.queue.splice(from, 1)
  player.queue.splice(i, 0, item)
  player.currentIndex = 0
  player.skipNextQueueUpdate = true
  dragIndex.value = null; dragOverIndex.value = null
}
function onDragEnd() { dragIndex.value = null; dragOverIndex.value = null }

// --- Auto-scroll playlist to current ---
const playlistContainer = ref<HTMLElement | null>(null)
watch(() => player.currentIndex, async (idx) => {
  if (idx < 0 || !playlistContainer.value) return
  await nextTick()
  const el = playlistContainer.value.querySelector(`[data-playlist="${idx}"]`) as HTMLElement
  if (el) el.scrollIntoView({ behavior: 'smooth', block: 'nearest' })
})

// --- Keydown ---
function onKeydown(e: KeyboardEvent) {
  if (e.key === 'Escape' && showLyrics.value) showLyrics.value = false
}
onMounted(() => window.addEventListener('keydown', onKeydown))
onUnmounted(() => window.removeEventListener('keydown', onKeydown))

// --- Progress ---
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
  player.seek(((e.clientX - rect.left) / rect.width) * player.duration)
}

function onProgressMousedown(e: MouseEvent) {
  if (!player.hasTrack) return
  isDraggingProgress.value = true
  const bar = e.currentTarget as HTMLElement
  const onMove = (ev: MouseEvent) => {
    const rect = bar.getBoundingClientRect()
    player.seek(Math.max(0, Math.min(1, (ev.clientX - rect.left) / rect.width)) * player.duration)
  }
  const onUp = () => { isDraggingProgress.value = false; window.removeEventListener('mousemove', onMove); window.removeEventListener('mouseup', onUp) }
  window.addEventListener('mousemove', onMove)
  window.addEventListener('mouseup', onUp)
}

function openLyrics() { if (player.hasTrack) showLyrics.value = true }
function closeLyrics() { showLyrics.value = false }

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
    <div v-if="showLyrics" class="fixed inset-0 z-[60] flex">
      <!-- 封面背景 + 毛玻璃 -->
      <img
        :src="coverSrc"
        class="absolute inset-0 w-full h-full object-cover scale-110 blur-3xl brightness-50"
        :alt="player.currentTrack?.title ?? ''"
      />
      <div class="absolute inset-0 bg-black/50 backdrop-blur-xl"></div>

      <!-- 左侧：封面 + 信息 + 控件 -->
      <div class="relative z-10 w-[38%] flex flex-col items-center justify-center px-10 gap-5">
        <!-- 收起按钮 -->
        <button @click="closeLyrics" class="absolute top-6 left-6 w-10 h-10 flex items-center justify-center text-white/60 hover:text-white transition-colors press-scale">
          <span class="material-symbols-outlined">expand_more</span>
        </button>

        <!-- 大封面 -->
        <img
          :src="coverSrc"
          class="w-56 h-56 object-cover shadow-2xl shadow-black/40 transition-all duration-700"
          :class="player.isPlaying ? 'rounded-[45%]' : 'rounded-full'"
          :alt="player.currentTrack?.title ?? ''"
          @error="($event.target as HTMLImageElement).src = API_BASE + '/uploads/covers/default.jpg'"
        />

        <!-- 歌曲信息 -->
        <div class="text-center">
          <h2 class="text-2xl font-bold text-white">{{ player.currentTrack?.title ?? '未在播放' }}</h2>
          <p class="text-white/50 mt-1">{{ player.currentTrack?.artist }}</p>
        </div>

        <!-- 进度条 -->
        <div class="w-full max-w-xs mt-2">
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

        <!-- 播放控制 + 模式 + 音量 -->
        <div class="flex items-center justify-center gap-5 mt-4">
          <!-- 播放模式 -->
          <button class="w-9 h-9 flex items-center justify-center text-white/40 hover:text-white/80 transition-colors press-scale relative group"
            @click="player.togglePlayMode()">
            <span class="material-symbols-outlined text-xl" :class="player.playMode !== 'off' ? 'text-[#71fcfe]' : ''">{{ playModeIcon }}</span>
            <span class="absolute -top-8 left-1/2 -translate-x-1/2 whitespace-nowrap text-[10px] text-white/60 bg-black/60 rounded px-2 py-0.5 opacity-0 group-hover:opacity-100 transition-opacity pointer-events-none">{{ playModeLabel }}</span>
          </button>

          <button class="text-white/60 hover:text-white press-scale" @click="player.playPrev()">
            <span class="material-symbols-outlined text-3xl">skip_previous</span>
          </button>
          <button class="w-16 h-16 rounded-full bg-white text-slate-900 flex items-center justify-center hover:scale-105 transition-transform shadow-lg press-scale"
            @click="player.togglePlay()">
            <span class="material-symbols-outlined text-4xl">{{ player.isPlaying ? 'pause' : 'play_arrow' }}</span>
          </button>
          <button class="text-white/60 hover:text-white press-scale" @click="player.playNext()">
            <span class="material-symbols-outlined text-3xl">skip_next</span>
          </button>

          <!-- 音量 -->
          <div class="flex items-center gap-1.5 group/vol">
            <button class="w-9 h-9 flex items-center justify-center text-white/40 hover:text-white/80 transition-colors press-scale"
              @click="player.setVolume(player.volume === 0 ? 80 : 0)">
              <span class="material-symbols-outlined text-xl">{{ volumeIcon }}</span>
            </button>
            <div class="w-0 overflow-hidden group-hover/vol:w-24 transition-all duration-300">
              <div
                class="h-1 w-full bg-white/20 rounded-full cursor-pointer mt-1"
                @click="onVolumeClick"
                @mousedown="onVolumeMousedown"
              >
                <div class="h-full bg-white rounded-full" :style="{ width: player.volume + '%' }"></div>
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- 右侧：歌词 + 播放列表 -->
      <div class="relative z-10 flex-1 flex">
        <!-- 歌词漏斗 -->
        <div class="flex-1 flex flex-col">
          <div ref="lyricsContainer" class="flex-1 overflow-y-auto px-10 scroll-smooth scrollbar-hide" style="mask-image: linear-gradient(transparent, black 15%, black 85%, transparent);">
            <div class="h-[35vh]"></div>
            <!-- 无歌词提示 -->
            <div v-if="lyrics.length === 0" class="text-center py-8">
              <p class="text-white/20 text-sm">暂无歌词</p>
            </div>
            <div v-else class="space-y-2">
              <div v-for="(line, i) in lyrics" :key="i"
                :data-line="i"
                @click="seekToLine(line.time)"
                class="cursor-pointer transition-all duration-500 ease-out py-2 origin-left"
                :style="{
                  ...lineStyle(i),
                  fontSize: i === currentLine ? '1.75rem' : '1.25rem',
                  fontWeight: i === currentLine ? 700 : 400,
                  letterSpacing: i === currentLine ? '-0.01em' : '0',
                  textShadow: i === currentLine ? '0 0 30px rgba(113,252,254,0.25)' : 'none',
                }"
              >
                {{ line.text || '···' }}
              </div>
            </div>
            <div class="h-[40vh]"></div>
          </div>
        </div>

        <!-- 播放列表侧栏 -->
        <div class="w-72 border-l border-white/10 flex flex-col">
          <div class="px-5 py-4 flex items-center gap-2">
            <span class="material-symbols-outlined text-white/50 text-lg">queue_music</span>
            <p class="text-white/70 text-sm font-bold">播放列表</p>
            <span class="text-white/30 text-xs ml-1">{{ player.queue.length }} 首</span>
          </div>
          <div ref="playlistContainer" class="flex-1 overflow-y-auto scrollbar-hide px-3 pb-4">
            <!-- 正在播放 (index 0, pinned) -->
            <div v-if="player.queue.length > 0" class="px-3 py-3 rounded-xl bg-white/10 mb-2 border border-[#71fcfe]/20">
              <div class="flex items-center gap-3">
                <span class="material-symbols-outlined text-[#71fcfe] text-lg flex-shrink-0">
                  {{ player.isPlaying ? 'equalizer' : 'play_circle' }}
                </span>
                <img
                  :src="player.queue[0].coverUrl ? (API_BASE + player.queue[0].coverUrl) : API_BASE + '/uploads/covers/default.jpg'"
                  class="w-10 h-10 rounded-lg object-cover flex-shrink-0"
                />
                <div class="min-w-0 flex-1">
                  <p class="text-[10px] text-[#71fcfe]/70 font-semibold mb-0.5">正在播放</p>
                  <p class="text-sm text-white font-bold truncate">{{ player.queue[0].title }}</p>
                  <p class="text-xs text-white/50 truncate">{{ player.queue[0].artist }}</p>
                </div>
              </div>
            </div>

            <!-- 接下来 (index 1+) -->
            <div v-if="player.queue.length > 1" class="px-1 py-2 flex items-center gap-2">
              <span class="text-[10px] text-white/30 font-medium">接下来</span>
            </div>
            <div v-for="(track, i) in player.queue.slice(1)" :key="track.songId"
              :data-playlist="i + 1"
              class="flex items-center gap-3 px-3 py-2.5 rounded-xl cursor-pointer transition-all mb-1 group"
              :class="[
                dragOverIndex === i + 1 ? 'border-t-2 border-[#71fcfe]' : '',
                dragIndex === i + 1 ? 'opacity-40' : ''
              ]"
              draggable="true"
              @dragstart="onDragStart(i + 1, $event)"
              @dragover="onDragOver(i + 1, $event)"
              @dragleave="onDragLeave"
              @drop="onDrop(i + 1, $event)"
              @dragend="onDragEnd"
              @click="player.playTrackBySongId(track.songId)"
            >
              <span class="material-symbols-outlined text-white/20 text-sm cursor-grab active:cursor-grabbing flex-shrink-0">drag_indicator</span>
              <img
                :src="track.coverUrl ? (API_BASE + track.coverUrl) : API_BASE + '/uploads/covers/default.jpg'"
                class="w-10 h-10 rounded-lg object-cover flex-shrink-0"
              />
              <div class="min-w-0 flex-1">
                <p class="text-sm truncate text-white/70">
                  {{ track.title }}
                </p>
                <p class="text-xs truncate text-white/30">
                  {{ track.artist }}
                </p>
              </div>
              <button
                class="flex-shrink-0 press-scale opacity-0 group-hover:opacity-100 transition-opacity"
                @click.stop="toggleFavorite(track.songId)"
              >
                <span class="material-symbols-outlined text-lg transition-colors"
                  :class="favoriteIds.has(track.songId) ? 'text-red-400' : 'text-white/30 hover:text-white/60'"
                  :style="{ fontVariationSettings: `'FILL' ${favoriteIds.has(track.songId) ? 1 : 0}` }"
                >favorite</span>
              </button>
            </div>
            <p v-if="player.queue.length === 0" class="text-center text-white/20 text-sm py-10">播放列表为空</p>
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
.scrollbar-hide::-webkit-scrollbar { display: none; }
.scrollbar-hide { -ms-overflow-style: none; scrollbar-width: none; }
</style>
