<script setup lang="ts">
import { ref, onMounted, onUnmounted, computed, nextTick } from 'vue'
import { useRouter } from 'vue-router'
import { roomApi, chatApi } from '@/api'
import { usePlayerStore } from '@/stores/player'
import { useAuthStore } from '@/stores/auth'
import type { RoomInfo, PlayQueueItem } from '@/types'

const player = usePlayerStore()
const auth = useAuthStore()
const router = useRouter()

const roomInfo = ref<RoomInfo | null>(null)
const queue = ref<PlayQueueItem[]>([])

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
    console.log('loadRoom: currentRoomId =', auth.currentRoomId)
    const { data } = await roomApi.getCurrent(auth.currentRoomId)
    console.log('loadRoom: result =', data)
    roomInfo.value = data
  } catch (err: any) {
    console.error('loadRoom: error =', err.response?.status, err.response?.data)
    roomInfo.value = { roomId: 0, roomCode: 'N/A', songsQueued: 0, onlineUsers: 0 }
  }
}

async function loadQueue() {
  const { data } = await roomApi.getQueue()
  queue.value = data

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

async function sendChatMessage() {
  if (!chatInput.value.trim()) return
  if (!roomInfo.value || roomInfo.value.roomId <= 0) {
    alert('未加入房间，无法发送消息')
    return
  }
  const msg = chatInput.value.trim()
  try {
    await chatApi.sendMessage(roomInfo.value.roomId, msg)
    chatInput.value = ''
    await loadChatMessages()
  } catch (err: any) {
    console.error('发送消息失败:', err)
    alert('发送失败: ' + (err.message || '未知错误'))
  }
}

async function loadChatMessages() {
  if (!roomInfo.value || roomInfo.value.roomId <= 0) return
  try {
    const { data } = await chatApi.getMessages(roomInfo.value.roomId)
    chatMessages.value = data.map(m => ({ nickname: m.nickname, message: m.message, timestamp: m.timestamp }))
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
          <span v-if="roomInfo" class="flex items-center gap-2 bg-surface-container-low px-4 py-2 rounded-full">
            <span class="material-symbols-outlined text-sm">label</span> 房间码: {{ roomInfo.roomCode }}
          </span>
          <span v-if="roomInfo" class="flex items-center gap-2 bg-surface-container-low px-4 py-2 rounded-full">
            <span class="material-symbols-outlined text-sm">group</span> 在线: {{ roomInfo.onlineUsers }} 人
          </span>
          <span class="flex items-center gap-2 bg-surface-container-low px-4 py-2 rounded-full">
            <span class="material-symbols-outlined text-sm">format_list_bulleted</span> 已点歌曲: {{ queue.length }}
          </span>
        </div>
      </div>

      <!-- Now Playing hero card -->
      <div v-if="player.currentTrack" class="bg-surface-container-lowest rounded-xl p-8 shadow-sm flex gap-8 items-center mb-10">
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
          <div class="flex-grow grid grid-cols-4 items-center">
            <span class="font-bold" :class="{ 'text-primary': player.currentTrack?.songId === item.songId }">{{ item.songTitle }}</span>
            <span class="text-on-surface-variant">{{ item.artist }}</span>
            <span class="text-sm text-slate-400">点播者: {{ item.orderedBy }}</span>
            <div class="flex justify-end gap-4 opacity-0 group-hover:opacity-100 transition-opacity">
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

    <!-- Right: Chat panel -->
    <div class="w-80 flex-shrink-0 bg-surface-container-lowest rounded-xl shadow-sm flex flex-col" style="height: calc(100vh - 200px)">
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
            {{ msg.message }}
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
  </div>
</template>
