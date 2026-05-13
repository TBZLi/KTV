<template>
  <div class="p-8 flex flex-col gap-8">
    <!-- Page Header -->
    <div class="flex flex-col gap-6">
      <h2 class="text-3xl font-display font-bold text-on-surface">房间管理</h2>
      <div class="flex items-center justify-between bg-surface-container-lowest p-2 rounded-xl ring-1 ring-outline-variant/15">
        <div class="flex gap-2">
          <button
            v-for="tab in statusTabs"
            :key="tab.value"
            class="px-6 py-2.5 rounded-lg font-medium text-sm transition-all"
            :class="activeStatus === tab.value
              ? 'bg-primary text-on-primary shadow-[0_4px_12px_rgba(0,99,153,0.15)]'
              : 'text-on-surface-variant hover:bg-surface-container-low'"
            @click="activeStatus = tab.value"
          >
            {{ tab.label }}
          </button>
        </div>
        <div class="relative">
          <span class="material-symbols-outlined absolute left-3 top-1/2 -translate-y-1/2 text-on-surface-variant/50 text-sm">search</span>
          <input
            v-model="searchQuery"
            class="pl-9 pr-4 py-2.5 bg-surface-container-high border-none rounded-lg text-sm text-on-surface placeholder:text-on-surface-variant/50 focus:ring-2 focus:ring-primary w-64"
            placeholder="搜索包厢号..."
            type="text"
          />
        </div>
      </div>
    </div>

    <!-- Data Table -->
    <div class="bg-surface-container-lowest rounded-xl p-6 ring-1 ring-outline-variant/15 flex-1 shadow-[0_8px_32px_rgba(25,28,30,0.03)]">
      <div class="w-full">
        <!-- Header -->
        <div class="grid grid-cols-5 gap-4 py-4 text-sm font-semibold text-on-surface-variant uppercase tracking-wider px-4">
          <div>包厢号</div>
          <div>类型</div>
          <div>状态</div>
          <div>当前订单</div>
          <div class="text-right">操作</div>
        </div>
        <!-- Rows -->
        <div
          v-for="room in rooms"
          :key="room.id"
          class="grid grid-cols-5 gap-4 py-5 items-center px-4 rounded-lg hover:bg-surface-container-low transition-colors group"
        >
          <div class="font-display font-bold text-base text-on-surface">{{ room.roomNumber }}</div>
          <div class="text-sm text-on-surface-variant font-medium">{{ formatRoomType(room.roomType) }}</div>
          <div>
            <span
              class="inline-flex items-center px-3 py-1 rounded-lg text-xs font-semibold"
              :class="roomStatusClass(room.status)"
            >
              {{ formatRoomStatus(room.status) }}
            </span>
          </div>
          <div class="text-sm font-mono" :class="room.currentOrderId ? 'text-on-surface-variant' : 'text-on-surface-variant/50'">
            {{ room.currentOrderId || '--' }}
          </div>
          <div class="flex justify-end gap-2">
            <button @click="openDetail(room)" class="px-3 py-1.5 rounded-lg bg-surface-container-high text-on-surface-variant text-xs font-semibold hover:bg-surface-container-highest transition-colors">
              详情
            </button>
          </div>
        </div>
      </div>

      <!-- Pagination -->
      <div class="flex justify-between items-center mt-8 pt-4 border-t-0 text-sm text-on-surface-variant">
        <p>显示 {{ rooms.length }} 条，共 {{ total }} 条</p>
        <div class="flex gap-1">
          <button
            class="w-8 h-8 rounded-lg flex items-center justify-center hover:bg-surface-container-low text-on-surface-variant disabled:opacity-50"
            :disabled="currentPage <= 1"
            @click="currentPage--"
          >
            <span class="material-symbols-outlined text-sm">chevron_left</span>
          </button>
          <button
            v-for="page in totalPages"
            :key="page"
            class="w-8 h-8 rounded-lg flex items-center justify-center font-medium transition-colors"
            :class="page === currentPage
              ? 'bg-primary text-on-primary'
              : 'hover:bg-surface-container-low text-on-surface-variant'"
            @click="currentPage = page"
          >{{ page }}</button>
          <button
            class="w-8 h-8 rounded-lg flex items-center justify-center hover:bg-surface-container-low text-on-surface-variant disabled:opacity-50"
            :disabled="currentPage >= totalPages"
            @click="currentPage++"
          >
            <span class="material-symbols-outlined text-sm">chevron_right</span>
          </button>
        </div>
      </div>
    </div>

    <!-- Detail Dialog -->
    <div v-if="showDetailDialog && detailRoom" class="fixed inset-0 z-50 flex items-center justify-center bg-black/40" @click.self="showDetailDialog = false">
      <div class="bg-surface-container-lowest rounded-2xl shadow-xl w-full max-w-lg p-8 space-y-4">
        <h3 class="text-xl font-display font-bold text-on-surface">房间详情</h3>
        <div class="space-y-3 text-sm">
          <div class="flex justify-between py-2 border-b border-surface-container-highest">
            <span class="text-on-surface-variant">包厢号</span>
            <span class="font-medium text-on-surface">{{ detailRoom.roomNumber }}</span>
          </div>
          <div class="flex justify-between py-2 border-b border-surface-container-highest">
            <span class="text-on-surface-variant">类型</span>
            <span class="font-medium text-on-surface">{{ formatRoomType(detailRoom.roomType) }}</span>
          </div>
          <div class="flex justify-between py-2 border-b border-surface-container-highest">
            <span class="text-on-surface-variant">状态</span>
            <span
              class="inline-flex items-center px-3 py-1 rounded-lg text-xs font-semibold"
              :class="roomStatusClass(detailRoom.status)"
            >{{ formatRoomStatus(detailRoom.status) }}</span>
          </div>
          <div class="flex justify-between py-2">
            <span class="text-on-surface-variant">当前订单</span>
            <span class="font-mono font-medium text-on-surface">{{ detailRoom.currentOrderId || '--' }}</span>
          </div>
        </div>

        <!-- Status Switching -->
        <div class="pt-2 border-t border-surface-container-highest">
          <p class="text-sm font-semibold text-on-surface-variant mb-3">状态切换</p>
          <div class="flex gap-2 flex-wrap">
            <button
              v-if="detailRoom.status === 'in_use'"
              @click="handleStatusSwitch('idle')"
              class="px-4 py-2 rounded-lg bg-error/10 text-error text-xs font-semibold hover:bg-error/20 transition-colors"
            >结束会话 → 打扫中</button>
            <button
              v-if="detailRoom.status === 'cleaning'"
              @click="handleStatusSwitch('idle')"
              class="px-4 py-2 rounded-lg bg-primary/10 text-primary text-xs font-semibold hover:bg-primary/20 transition-colors"
            >打扫中 → 空闲</button>
            <button
              v-if="detailRoom.status === 'idle'"
              @click="handleStatusSwitch('cleaning')"
              class="px-4 py-2 rounded-lg bg-tertiary-container/20 text-tertiary text-xs font-semibold hover:bg-tertiary-container/30 transition-colors"
            >空闲 → 打扫中</button>
            <p v-if="detailRoom.status === 'in_use'" class="text-xs text-on-surface-variant w-full mt-1">结束会话将同时取消关联订单并退款</p>
          </div>
        </div>

        <div class="flex justify-end pt-2">
          <button @click="showDetailDialog = false" class="px-6 py-3 rounded-lg font-medium text-on-surface-variant hover:bg-surface-container transition-colors">关闭</button>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, watch } from 'vue'
import { roomsApi } from '@/api'
import type { Room } from '@/types'
import { formatRoomType, formatRoomStatus } from '@/utils/format'

const rooms = ref<Room[]>([])
const total = ref(0)
const currentPage = ref(1)
const pageSize = ref(10)
const activeStatus = ref('')
const searchQuery = ref('')
const showDetailDialog = ref(false)
const detailRoom = ref<Room | null>(null)

const statusTabs = [
  { label: '全部', value: '' },
  { label: '使用中', value: 'in_use' },
  { label: '空闲', value: 'idle' },
  { label: '打扫中', value: 'cleaning' },
]

const totalPages = computed(() => Math.max(1, Math.ceil(total.value / pageSize.value)))

function roomStatusClass(status: string) {
  switch (status) {
    case 'in_use': return 'bg-primary/10 text-primary'
    case 'idle': return 'bg-surface-container-high text-on-surface-variant'
    case 'cleaning': return 'bg-tertiary-container/20 text-tertiary'
    default: return 'bg-surface-variant text-on-surface-variant'
  }
}

function openDetail(room: Room) {
  detailRoom.value = room
  showDetailDialog.value = true
}

async function fetchRooms() {
  const res = await roomsApi.getList({
    status: activeStatus.value || undefined,
    search: searchQuery.value || undefined,
    page: currentPage.value,
    pageSize: pageSize.value,
  })
  rooms.value = res.data.items
  total.value = res.data.total
}

async function handleStatusSwitch(targetStatus: string) {
  if (!detailRoom.value) return
  const room = detailRoom.value

  if (room.status === 'in_use') {
    if (!confirm(`确定结束 ${room.roomNumber} 的使用会话？房间将变为打扫中，关联订单将退款。`)) return
    try {
      await roomsApi.endSession(room.id)
    } catch (err: any) {
      alert(err.response?.data?.message || err.response?.data || '操作失败')
      return
    }
  } else {
    const statusLabel = targetStatus === 'idle' ? '空闲' : '打扫中'
    if (!confirm(`确定将 ${room.roomNumber} 状态切换为"${statusLabel}"？`)) return
    try {
      await roomsApi.updateStatus(room.id, targetStatus)
    } catch (err: any) {
      alert(err.response?.data?.message || err.response?.data || '操作失败')
      return
    }
  }

  showDetailDialog.value = false
  await fetchRooms()
}

onMounted(() => {
  fetchRooms()
})

watch([activeStatus, searchQuery, currentPage], () => {
  fetchRooms()
})
</script>
