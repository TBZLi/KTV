<script setup lang="ts">
import { ref, onMounted, onUnmounted, computed, nextTick, watch } from 'vue'
import { useRouter } from 'vue-router'
import { roomApi, chatApi, favoritesApi } from '@/api'
import { usePlayerStore } from '@/stores/player'
import { useAuthStore } from '@/stores/auth'
import type { RoomInfo } from '@/types'
import { useToast } from '@/composables/useToast'

// Toast
const { toastMsg, showToast } = useToast()

const player = usePlayerStore()
const auth = useAuthStore()
const router = useRouter()

const API_BASE = import.meta.env.VITE_API_BASE_URL?.replace(/\/api$/, '') || 'https://localhost:5001'

const roomInfo = ref<RoomInfo | null>(null)

// Metadata maps (keyed by songId)
const orderedByMap = ref<Record<number, string>>({})
const queueIdMap = ref<Record<number, number>>({})

// Display queue derived from player.queue + metadata
const displayQueue = computed(() =>
  player.queue.map((t, i) => ({
    ...t,
    id: queueIdMap.value[t.songId] ?? 0,
    songTitle: t.title,
    orderedBy: orderedByMap.value[t.songId] ?? '',
    index: i,
  }))
)

// Join room state
const joinRoomCode = ref('')
const joinError = ref('')

async function handleJoinRoom() {
  joinError.value = ''
  if (!joinRoomCode.value || joinRoomCode.value.length < 4) {
    joinError.value = '请输入有效的房间码'
    return
  }
  try {
    const { data } = await roomApi.joinByCode(joinRoomCode.value.toUpperCase())
    auth.setCurrentRoomId(data.roomId)
    await loadRoom()
    await loadQueue()
    await loadChatMessages()
  } catch (err: any) {
    joinError.value = err.response?.data?.message || '房间不存在或已关闭'
  }
}

// Chat state
const chatMessages = ref<{ nickname: string; message: string; timestamp: string }[]>([])
const chatInput = ref('')
const chatContainer = ref<HTMLElement | null>(null)
let chatPollTimer: ReturnType<typeof setInterval> | null = null

const progressPercent = computed(() => {
  if (player.duration <= 0) return 0
  return (player.currentTime / player.duration) * 100
})

async function loadRoom() {
  try {
    const { data } = await roomApi.getCurrent(auth.currentRoomId)
    // Only update if data actually changed (prevents re-render flash on poll)
    const prev = roomInfo.value
    if (prev && prev.roomId === data.roomId && prev.roomCode === data.roomCode && prev.songsQueued === data.songsQueued && prev.onlineUsers === data.onlineUsers) return
    roomInfo.value = data
  } catch (err: any) {
    console.error('loadRoom: error =', err.response?.status, err.response?.data)
    roomInfo.value = { roomId: 0, roomCode: 'N/A', songsQueued: 0, onlineUsers: 0 }
  }
}

async function loadQueue() {
  const { data } = await roomApi.getQueue()

  // Update metadata maps
  const oMap: Record<number, string> = {}
  const qMap: Record<number, number> = {}
  for (const item of data) {
    oMap[item.songId] = item.orderedBy
    qMap[item.songId] = item.id
  }
  orderedByMap.value = oMap
  queueIdMap.value = qMap

  player.loadQueue(
    data.map(item => ({
      songId: item.songId,
      title: item.songTitle,
      artist: item.artist,
      coverUrl: item.coverUrl,
      mediaUrl: item.mediaUrl,
      lrcUrl: item.lrcUrl || '',
    }))
  )
}

async function removeFromQueue(queueId: number) {
  // Find the songId for this queue entry
  const songId = Object.entries(queueIdMap.value).find(([, id]) => id === queueId)?.[0]
  await roomApi.removeFromQueue(queueId)

  if (songId) {
    const sid = Number(songId)
    delete orderedByMap.value[sid]
    delete queueIdMap.value[sid]
    if (player.currentTrack?.songId === sid) {
      player.loadQueue(player.queue.filter(t => t.songId !== sid))
    }
  }
}

function playSong(songId: number) {
  player.playTrackBySongId(songId)
}

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
async function onDrop(i: number, e: DragEvent) {
  e.preventDefault()
  if (i === player.currentIndex) { dragIndex.value = null; dragOverIndex.value = null; return }
  if (dragIndex.value === null || dragIndex.value === i) { dragIndex.value = null; dragOverIndex.value = null; return }
  const from = dragIndex.value

  // Reorder player.queue directly
  const [item] = player.queue.splice(from, 1)
  player.queue.splice(i, 0, item)
  player.currentIndex = 0
  player.skipNextQueueUpdate = true
  dragIndex.value = null; dragOverIndex.value = null

  // Persist to server (queue IDs in new order)
  const ids = player.queue.map(t => queueIdMap.value[t.songId]).filter(Boolean)
  try { await roomApi.reorderBatch(ids) } catch {}
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

async function sendChatMessage() {
  if (!chatInput.value.trim()) return
  if (!roomInfo.value || roomInfo.value.roomId <= 0) {
    showToast('未加入房间，无法发送消息')
    return
  }
  const msg = chatInput.value.trim()
  try {
    await chatApi.sendMessage(roomInfo.value.roomId, msg)
    chatInput.value = ''
    await loadChatMessages()
  } catch (err: any) {
    console.error('发送消息失败:', err)
    showToast('发送失败: ' + (err.message || '未知错误'))
  }
}

async function loadChatMessages() {
  if (!roomInfo.value || roomInfo.value.roomId <= 0) return
  try {
    const { data } = await chatApi.getMessages(roomInfo.value.roomId)
    const mapped = data.map(m => ({ nickname: m.nickname, message: m.message, timestamp: m.timestamp }))
    // Only update if messages actually changed
    if (mapped.length === chatMessages.value.length && mapped.every((m, i) => m.message === chatMessages.value[i].message && m.timestamp === chatMessages.value[i].timestamp)) return
    chatMessages.value = mapped
    scrollToChatBottom()
  } catch { /* ignore */ }
}

function scrollToChatBottom() {
  nextTick(() => {
    if (chatContainer.value) {
      chatContainer.value.scrollTop = chatContainer.value.scrollHeight
    }
  })
}

async function leaveCurrentRoom() {
  if (!roomInfo.value || roomInfo.value.roomId <= 0) return
  try {
    await roomApi.leaveRoom(roomInfo.value.roomId)
    router.push('/explore')
  } catch (err: any) {
    console.error('退出房间失败:', err)
  }
}

function formatTime(seconds: number): string {
  const m = Math.floor(seconds / 60)
  const s = Math.floor(seconds % 60)
  return `${m.toString().padStart(2, '0')}:${s.toString().padStart(2, '0')}`
}

onMounted(async () => {
  await loadRoom()
  await loadQueue()
  await loadChatMessages()
  loadFavorites()
  // Poll chat messages every 2 seconds
  chatPollTimer = setInterval(() => {
    loadChatMessages()
    loadQueue()
    loadRoom()
  }, 2000)
})

onUnmounted(() => {
  if (chatPollTimer) {
    clearInterval(chatPollTimer)
    chatPollTimer = null
  }
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
  <!-- Not in a room -->
  <div v-if="!roomInfo || roomInfo.roomId <= 0" class="flex flex-col items-center justify-center min-h-[60vh] gap-6">
    <div class="w-20 h-20 bg-primary/10 rounded-full flex items-center justify-center">
      <span class="material-symbols-outlined text-5xl text-primary">meeting_room</span>
    </div>
    <h2 class="text-2xl font-bold text-on-surface">你还没加入任何房间</h2>
    <p class="text-on-surface-variant">输入房间码加入已有房间</p>
    <div class="flex gap-2 w-80">
      <input
        v-model="joinRoomCode"
        type="text"
        placeholder="6位房间码"
        maxlength="6"
        class="flex-1 bg-surface-container-high border-none rounded-lg py-3 px-4 text-lg text-on-surface font-mono tracking-widest text-center placeholder:text-on-surface-variant/50 focus:ring-2 focus:ring-primary/30 focus:outline-none uppercase"
      />
      <button
        @click="handleJoinRoom"
        class="px-6 py-3 bg-primary text-on-primary rounded-lg font-semibold hover:opacity-90 transition-opacity press-scale"
      >
        加入
      </button>
    </div>
    <p v-if="joinError" class="text-error text-sm">{{ joinError }}</p>
  </div>

  <!-- In a room -->
  <div v-else class="flex gap-8">
    <!-- Left: Room content -->
    <div class="flex-1">
      <!-- Header Section -->
      <div class="mb-10">
        <div class="flex items-center justify-between mb-4">
          <h1 class="text-4xl font-extrabold tracking-tight text-on-surface">当前房间</h1>
          <button
            @click="leaveCurrentRoom"
            class="flex items-center gap-2 px-4 py-2 text-sm font-medium text-error bg-error/10 hover:bg-error/20 rounded-full transition-colors press-scale"
          >
            <span class="material-symbols-outlined text-lg">logout</span>
            退出房间
          </button>
        </div>
        <div class="flex gap-8 text-sm font-medium text-on-surface-variant">
          <span v-if="roomInfo" class="flex items-center gap-2 glass px-4 py-2 rounded-full">
            <span class="material-symbols-outlined text-sm">label</span> 房间码: {{ roomInfo.roomCode }}
          </span>
          <span v-if="roomInfo" class="flex items-center gap-2 glass px-4 py-2 rounded-full">
            <span class="material-symbols-outlined text-sm">group</span> 在线: {{ roomInfo.onlineUsers }} 人
          </span>
          <span class="flex items-center gap-2 glass px-4 py-2 rounded-full">
            <span class="material-symbols-outlined text-sm">format_list_bulleted</span> 已点歌曲: {{ displayQueue.length }}
          </span>
        </div>
      </div>

      <!-- Now Playing hero card -->
      <div v-if="player.currentTrack" class="glass rounded-xl p-8 shadow-sm flex gap-8 items-center mb-10">
        <img
          :src="API_BASE + player.currentTrack.coverUrl"
          class="w-48 h-48 rounded flex-shrink-0 object-cover cursor-pointer transition-transform duration-200 hover:scale-[2.5] hover:shadow-lg hover:z-10 relative"
          :alt="player.currentTrack.title"
          @error="($event.target as HTMLImageElement).src = API_BASE + '/uploads/covers/default.jpg'"
        />
        <div class="flex-grow">
          <div class="flex justify-between items-start mb-6">
            <div>
              <h2 class="text-3xl font-bold mb-1">{{ player.currentTrack.title }}</h2>
              <p class="text-on-surface-variant">{{ player.currentTrack.artist }}</p>
            </div>
            <button
              class="p-3 bg-primary text-white rounded-full hover:opacity-90 transition-opacity flex items-center gap-2 px-6 font-bold"
              @click="player.playNext()"
              :class="{ 'opacity-30 pointer-events-none': player.currentIndex >= player.queue.length - 1 }"
            >
              <span class="material-symbols-outlined">skip_next</span> 切歌
            </button>
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
      <div v-else class="glass rounded-xl p-8 shadow-sm flex gap-8 items-center mb-10">
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

      <!-- Now Playing (index 0, pinned) -->
      <div v-if="displayQueue.length > 0" class="glass rounded-xl p-6 mb-4 flex items-center gap-5 border border-primary/20 shadow-sm">
        <span class="material-symbols-outlined text-primary text-lg flex-shrink-0">
          {{ player.isPlaying ? 'equalizer' : 'play_circle' }}
        </span>
        <img
          :src="API_BASE + displayQueue[0].coverUrl"
          class="w-14 h-14 rounded-lg flex-shrink-0 object-cover shadow"
          :alt="displayQueue[0].songTitle"
          @error="($event.target as HTMLImageElement).src = API_BASE + '/uploads/covers/default.jpg'"
        />
        <div class="flex-1 min-w-0">
          <p class="text-sm text-primary font-semibold mb-0.5">正在播放</p>
          <p class="font-bold truncate">{{ displayQueue[0].songTitle }}</p>
          <p class="text-sm text-on-surface-variant truncate">{{ displayQueue[0].artist }} · {{ displayQueue[0].orderedBy }} 点播</p>
        </div>
        <button
          class="press-scale flex-shrink-0"
          @click.stop="toggleFavorite(displayQueue[0].songId)"
        >
          <span class="material-symbols-outlined text-lg"
            :class="favoriteIds.has(displayQueue[0].songId) ? 'text-red-400' : 'text-slate-300 hover:text-slate-500'"
            :style="{ fontVariationSettings: `'FILL' ${favoriteIds.has(displayQueue[0].songId) ? 1 : 0}` }"
          >favorite</span>
        </button>
        <button
          @click.stop="removeFromQueue(displayQueue[0].id)"
          class="text-error hover:scale-110 transition-transform press-scale flex-shrink-0"
        >
          <span class="material-symbols-outlined">delete</span>
        </button>
      </div>

      <!-- Up Next (index 1+) -->
      <div v-if="displayQueue.length > 1" class="mb-3 px-2 flex items-center gap-2">
        <span class="text-sm font-medium text-on-surface-variant">接下来</span>
        <span class="text-xs text-on-surface-variant/50">{{ displayQueue.length - 1 }} 首</span>
      </div>
      <div ref="playlistContainer" class="rounded-xl overflow-hidden">
        <div
          v-for="(item, index) in displayQueue.slice(1)"
          :key="item.songId"
          :data-playlist="index + 1"
          class="flex items-center px-8 py-5 hover:glass hover:shadow-lg hover:scale-[1.02] transition-all duration-300 group cursor-pointer"
          :class="[
            dragOverIndex === index + 1 ? 'border-t-2 border-primary' : '',
            dragIndex === index + 1 ? 'opacity-40' : '',
          ]"
          draggable="true"
          @dragstart="onDragStart(index + 1, $event)"
          @dragover="onDragOver(index + 1, $event)"
          @dragleave="onDragLeave"
          @drop="onDrop(index + 1, $event)"
          @dragend="onDragEnd"
          @click="playSong(item.songId)"
        >
          <span class="material-symbols-outlined text-slate-300 text-sm cursor-grab active:cursor-grabbing mr-3 opacity-0 group-hover:opacity-100 transition-opacity">drag_indicator</span>
          <span class="w-8 font-bold text-slate-400">{{ String(index + 1).padStart(2, '0') }}</span>
          <img
            :src="API_BASE + item.coverUrl"
            class="w-12 h-12 rounded flex-shrink-0 object-cover mx-6"
            :alt="item.songTitle"
            @error="($event.target as HTMLImageElement).src = API_BASE + '/uploads/covers/default.jpg'"
          />
          <div class="flex-grow grid grid-cols-3 items-center">
            <span class="font-bold">{{ item.songTitle }}</span>
            <span class="text-on-surface-variant">{{ item.artist }}</span>
            <div class="flex items-center justify-between">
              <span class="text-sm text-slate-400">点播者: {{ item.orderedBy }}</span>
              <div class="flex items-center gap-2">
                <button
                  class="press-scale opacity-0 group-hover:opacity-100 transition-opacity"
                  @click.stop="toggleFavorite(item.songId)"
                >
                  <span class="material-symbols-outlined text-lg transition-colors"
                    :class="favoriteIds.has(item.songId) ? 'text-red-400' : 'text-slate-300 hover:text-slate-500'"
                    :style="{ fontVariationSettings: `'FILL' ${favoriteIds.has(item.songId) ? 1 : 0}` }"
                  >favorite</span>
                </button>
                <button
                  @click.stop="removeFromQueue(item.id)"
                  class="text-error hover:scale-110 transition-transform opacity-0 group-hover:opacity-100 transition-opacity"
                >
                  <span class="material-symbols-outlined">delete</span>
                </button>
              </div>
            </div>
          </div>
        </div>
      </div>

      <!-- Empty state -->
      <div v-if="displayQueue.length === 0" class="text-center py-20">
        <span class="material-symbols-outlined text-6xl text-surface-container-highest">queue_music</span>
        <p class="text-on-surface-variant mt-4">播放列表为空</p>
      </div>
    </div>

    <!-- Right: Chat panel -->
    <div class="w-80 flex-shrink-0 glass rounded-xl shadow-sm flex flex-col" style="height: calc(100vh - 200px)">
      <div class="p-4 border-b border-surface-container-highest">
        <h3 class="font-bold text-on-surface flex items-center gap-2">
          <span class="material-symbols-outlined text-primary text-xl">chat</span>
          实时聊天
        </h3>
      </div>
      <!-- Messages -->
      <div ref="chatContainer" class="flex-1 overflow-y-auto p-4 space-y-3">
        <div v-for="(msg, i) in chatMessages" :key="i" class="text-sm">
          <div v-if="msg.nickname === '系统'" class="text-center text-xs text-on-surface-variant/60 py-1">
            [{{ msg.timestamp }}] {{ msg.message }}
          </div>
          <div v-else class="flex flex-col">
            <div class="flex items-center gap-2 mb-0.5">
              <span class="font-semibold text-primary text-xs">{{ msg.nickname }}</span>
              <span class="text-[10px] text-on-surface-variant/50">{{ msg.timestamp }}</span>
            </div>
            <p class="text-on-surface bg-surface-container-high rounded-lg px-3 py-2 inline-block max-w-[90%]">{{ msg.message }}</p>
          </div>
        </div>
        <div v-if="chatMessages.length === 0" class="text-center text-xs text-on-surface-variant/50 py-8">
          还没有消息，来说点什么吧
        </div>
      </div>
      <!-- Input -->
      <div class="p-3 border-t border-surface-container-highest">
        <form @submit.prevent="sendChatMessage" class="flex gap-2">
          <input
            v-model="chatInput"
            type="text"
            placeholder="发消息..."
            maxlength="200"
            class="flex-1 bg-surface-container-high border-none rounded-lg py-2.5 px-3 text-sm text-on-surface placeholder:text-on-surface-variant/50 focus:ring-2 focus:ring-primary/30 focus:outline-none"
          />
          <button
            type="submit"
            class="px-4 py-2.5 bg-primary text-on-primary rounded-lg text-sm font-medium hover:opacity-90 transition-opacity"
          >
            <span class="material-symbols-outlined text-lg">send</span>
          </button>
        </form>
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
