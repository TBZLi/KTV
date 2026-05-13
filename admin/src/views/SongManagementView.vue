<template>
  <div class="p-8 flex flex-col gap-8 max-w-7xl mx-auto w-full">
    <!-- Page Header -->
    <div class="flex items-center justify-between">
      <h2 class="text-3xl font-display font-bold text-on-surface">歌曲管理</h2>
      <button @click="openAddDialog" class="px-6 py-3 bg-primary text-on-primary rounded-lg font-headline font-semibold flex items-center gap-2 hover:bg-primary/90 transition-colors shadow-sm press-scale">
        <span class="material-symbols-outlined text-sm">add</span>
        新增歌曲
      </button>
    </div>

    <!-- Stats Cards -->
    <section class="grid grid-cols-1 md:grid-cols-3 gap-6">
      <div class="bg-surface-container-lowest rounded-lg p-6 flex flex-col gap-2 relative shadow-sm">
        <div class="flex items-center gap-3 text-on-surface-variant">
          <span class="material-symbols-outlined text-primary">album</span>
          <span class="font-label text-sm font-medium">总曲目</span>
        </div>
        <div class="font-display font-bold text-3xl text-on-surface">{{ songStats.totalSongs }}</div>
      </div>
      <div class="bg-surface-container-lowest rounded-lg p-6 flex flex-col gap-2 relative shadow-sm">
        <div class="flex items-center gap-3 text-on-surface-variant">
          <span class="material-symbols-outlined text-tertiary-container">library_add</span>
          <span class="font-label text-sm font-medium">本周新增</span>
        </div>
        <div class="font-display font-bold text-3xl text-on-surface">{{ songStats.weeklyNew }}</div>
      </div>
      <div class="bg-surface-container-lowest rounded-lg p-6 flex flex-col gap-2 relative shadow-sm">
        <div class="flex items-center gap-3 text-on-surface-variant">
          <span class="material-symbols-outlined text-secondary">play_circle</span>
          <span class="font-label text-sm font-medium">今日播放量</span>
        </div>
        <div class="font-display font-bold text-3xl text-on-surface">{{ formatPlayCount(songStats.todayPlays) }}</div>
      </div>
    </section>

    <!-- Search Row -->
    <section class="flex flex-col md:flex-row justify-between items-center gap-4">
      <div class="relative w-full md:w-96">
        <span class="material-symbols-outlined absolute left-4 top-1/2 -translate-y-1/2 text-on-surface-variant">search</span>
        <input
          v-model="searchQuery"
          @input="onSearch"
          class="w-full pl-12 pr-4 py-3 bg-surface-container-lowest border-none rounded-lg text-on-surface placeholder:text-outline-variant shadow-sm focus:ring-2 focus:ring-primary/20 transition-shadow"
          placeholder="搜索歌曲、歌手..."
          type="text"
        />
      </div>
    </section>

    <!-- Data Table -->
    <section class="bg-surface-container-lowest rounded-lg shadow-sm overflow-hidden flex flex-col">
      <div class="overflow-x-auto">
        <table class="w-full text-left border-collapse">
          <thead>
            <tr class="bg-surface-container-low text-on-surface-variant font-label text-sm border-b border-surface-container-highest">
              <th class="p-4 font-medium w-24">封面</th>
              <th class="p-4 font-medium">歌曲名称</th>
              <th class="p-4 font-medium">歌手</th>
              <th class="p-4 font-medium">风格</th>
              <th class="p-4 font-medium">状态</th>
              <th class="p-4 font-medium text-right w-32">操作</th>
            </tr>
          </thead>
          <tbody class="text-sm font-body text-on-surface">
            <tr
              v-for="song in songs"
              :key="song.id"
              class="border-b border-surface-container-highest last:border-0 hover:bg-surface transition-colors group"
            >
              <td class="p-4">
                <div class="w-12 h-12 bg-slate-200 rounded flex items-center justify-center text-slate-400 text-xs text-center">
                  <span class="material-symbols-outlined text-lg">image</span>
                </div>
              </td>
              <td class="p-4 font-medium">{{ song.title }}</td>
              <td class="p-4 text-on-surface-variant">{{ song.artist }}</td>
              <td class="p-4 text-on-surface-variant">{{ song.genre }}</td>
              <td class="p-4">
                <span
                  class="inline-flex items-center px-2 py-1 rounded text-xs font-medium"
                  :class="song.status === 'active'
                    ? 'bg-primary-fixed text-on-primary-fixed'
                    : 'bg-surface-variant text-on-surface-variant'"
                >
                  {{ song.status === 'active' ? '已上架' : '已下架' }}
                </span>
              </td>
              <td class="p-4 text-right space-x-2">
                <button @click="openEditDialog(song)" class="text-primary hover:text-secondary transition-colors font-medium">编辑</button>
                <button @click="handleDelete(song)" class="text-error hover:text-error/80 transition-colors font-medium">删除</button>
              </td>
            </tr>
          </tbody>
        </table>
      </div>

      <!-- Empty State -->
      <div v-if="songs.length === 0" class="py-16 text-center text-on-surface-variant">
        <span class="material-symbols-outlined text-4xl text-outline-variant">library_music</span>
        <p class="mt-2">暂无歌曲数据</p>
      </div>

      <!-- Pagination -->
      <div v-if="totalPages > 1" class="p-4 border-t border-surface-container-highest flex justify-end items-center gap-2 text-sm text-on-surface-variant">
        <button
          class="p-1 text-outline hover:text-on-surface transition-colors disabled:opacity-50"
          :disabled="currentPage <= 1"
          @click="currentPage--; fetchSongs()"
        >
          <span class="material-symbols-outlined">chevron_left</span>
        </button>
        <button
          v-for="page in displayedPages"
          :key="page"
          class="px-3 py-1 rounded font-medium transition-colors cursor-pointer"
          :class="page === currentPage
            ? 'bg-primary text-on-primary'
            : page === '...'
              ? 'cursor-default'
              : 'hover:bg-surface-container-low'"
          @click="page !== '...' && (currentPage = page as number, fetchSongs())"
        >{{ page }}</button>
        <button
          class="p-1 text-outline hover:text-on-surface transition-colors disabled:opacity-50"
          :disabled="currentPage >= totalPages"
          @click="currentPage++; fetchSongs()"
        >
          <span class="material-symbols-outlined">chevron_right</span>
        </button>
      </div>
    </section>

    <!-- Add/Edit Dialog -->
    <div v-if="showDialog" class="fixed inset-0 z-50 flex items-center justify-center bg-black/40" @click.self="showDialog = false">
      <div class="bg-surface-container-lowest rounded-2xl shadow-xl w-full max-w-lg p-8 space-y-6">
        <h3 class="text-xl font-display font-bold text-on-surface">{{ isEditing ? '编辑歌曲' : '新增歌曲' }}</h3>
        <form @submit.prevent="handleSave" class="space-y-4">
          <div>
            <label class="block text-sm font-medium text-on-surface-variant mb-1">歌曲名称</label>
            <input v-model="form.title" required class="w-full px-4 py-3 bg-surface-container-high rounded-lg border-none text-on-surface focus:ring-2 focus:ring-primary/30 outline-none" />
          </div>
          <div>
            <label class="block text-sm font-medium text-on-surface-variant mb-1">歌手</label>
            <input v-model="form.artist" required class="w-full px-4 py-3 bg-surface-container-high rounded-lg border-none text-on-surface focus:ring-2 focus:ring-primary/30 outline-none" />
          </div>
          <div class="grid grid-cols-2 gap-4">
            <div>
              <label class="block text-sm font-medium text-on-surface-variant mb-1">风格</label>
              <select v-model="form.genre" class="w-full px-4 py-3 bg-surface-container-high rounded-lg border-none text-on-surface focus:ring-2 focus:ring-primary/30 outline-none">
                <option v-for="g in genres" :key="g" :value="g">{{ g }}</option>
              </select>
            </div>
            <div>
              <label class="block text-sm font-medium text-on-surface-variant mb-1">时长（秒）</label>
              <input v-model.number="form.duration" type="number" min="1" required class="w-full px-4 py-3 bg-surface-container-high rounded-lg border-none text-on-surface focus:ring-2 focus:ring-primary/30 outline-none" />
            </div>
          </div>
          <div>
            <label class="block text-sm font-medium text-on-surface-variant mb-1">状态</label>
            <select v-model="form.status" class="w-full px-4 py-3 bg-surface-container-high rounded-lg border-none text-on-surface focus:ring-2 focus:ring-primary/30 outline-none">
              <option value="active">已上架</option>
              <option value="inactive">已下架</option>
            </select>
          </div>
          <div class="flex justify-end gap-3 pt-2">
            <button type="button" @click="showDialog = false" class="px-6 py-3 rounded-lg font-medium text-on-surface-variant hover:bg-surface-container transition-colors">取消</button>
            <button type="submit" :disabled="saving" class="px-6 py-3 bg-primary text-on-primary rounded-lg font-semibold hover:bg-primary/90 transition-colors disabled:opacity-60">
              {{ saving ? '保存中...' : '保存' }}
            </button>
          </div>
        </form>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { songsApi } from '@/api'
import type { Song, SongStats } from '@/types'
import { formatPlayCount } from '@/utils/format'

const songs = ref<Song[]>([])
const total = ref(0)
const currentPage = ref(1)
const pageSize = ref(10)
const searchQuery = ref('')
let searchTimer: ReturnType<typeof setTimeout>

const songStats = ref<SongStats>({ totalSongs: 0, weeklyNew: 0, todayPlays: 0 })
const genres = ref<string[]>([])

// Dialog state
const showDialog = ref(false)
const isEditing = ref(false)
const saving = ref(false)
const editingId = ref<number | null>(null)
const form = ref({
  title: '',
  artist: '',
  genre: '流行',
  duration: 240,
  status: 'active' as 'active' | 'inactive',
})

const totalPages = computed(() => Math.max(1, Math.ceil(total.value / pageSize.value)))

const displayedPages = computed(() => {
  const pages: (number | string)[] = []
  const tp = totalPages.value
  const cp = currentPage.value
  if (tp <= 5) {
    for (let i = 1; i <= tp; i++) pages.push(i)
  } else {
    pages.push(1)
    if (cp > 3) pages.push('...')
    for (let i = Math.max(2, cp - 1); i <= Math.min(tp - 1, cp + 1); i++) pages.push(i)
    if (cp < tp - 2) pages.push('...')
    pages.push(tp)
  }
  return pages
})

async function fetchSongs() {
  const res = await songsApi.getList({
    search: searchQuery.value || undefined,
    page: currentPage.value,
    pageSize: pageSize.value,
  })
  songs.value = res.data.items
  total.value = res.data.total
}

async function fetchStats() {
  const res = await songsApi.getStats()
  songStats.value = res.data
}

async function fetchGenres() {
  const res = await songsApi.getGenres()
  genres.value = res.data
}

function onSearch() {
  clearTimeout(searchTimer)
  searchTimer = setTimeout(() => {
    currentPage.value = 1
    fetchSongs()
  }, 300)
}

function openAddDialog() {
  isEditing.value = false
  editingId.value = null
  form.value = { title: '', artist: '', genre: '流行', duration: 240, status: 'active' }
  showDialog.value = true
}

function openEditDialog(song: Song) {
  isEditing.value = true
  editingId.value = song.id
  form.value = {
    title: song.title,
    artist: song.artist,
    genre: song.genre,
    duration: song.duration,
    status: song.status,
  }
  showDialog.value = true
}

async function handleSave() {
  saving.value = true
  try {
    if (isEditing.value && editingId.value) {
      await songsApi.update(editingId.value, form.value as Partial<Song>)
    } else {
      await songsApi.create(form.value as Partial<Song>)
    }
    showDialog.value = false
    await fetchSongs()
    await fetchStats()
  } finally {
    saving.value = false
  }
}

async function handleDelete(song: Song) {
  if (!confirm(`确定删除歌曲「${song.title}」吗？`)) return
  await songsApi.delete(song.id)
  await fetchSongs()
  await fetchStats()
}

onMounted(() => {
  fetchStats()
  fetchSongs()
  fetchGenres()
})
</script>
