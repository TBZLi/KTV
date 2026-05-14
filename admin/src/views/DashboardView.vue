<template>
  <div class="p-8 space-y-8">
    <!-- Stats Row -->
    <div class="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-6">
      <div
        v-for="card in statCards"
        :key="card.label"
        class="bg-surface-container-lowest p-6 rounded-xl flex items-center justify-between shadow-sm"
      >
        <div>
          <p class="text-on-surface-variant text-sm font-medium font-body mb-1">{{ card.label }}</p>
          <h3 class="text-3xl font-bold font-headline text-on-surface">{{ card.value }}</h3>
        </div>
        <div
          class="w-12 h-12 rounded-full flex items-center justify-center"
          :class="card.iconBg"
        >
          <span class="material-symbols-outlined" :class="card.iconColor">{{ card.icon }}</span>
        </div>
      </div>
    </div>

    <!-- Revenue Period Filter -->
    <div class="bg-surface-container-lowest p-4 rounded-xl flex items-center gap-3 shadow-sm">
      <span class="text-sm font-medium text-on-surface-variant">收入统计范围:</span>
      <button
        v-for="p in revenuePeriods"
        :key="p.value"
        class="px-4 py-2 rounded-lg text-sm font-medium transition-colors"
        :class="selectedPeriod === p.value
          ? 'bg-primary text-on-primary'
          : 'bg-surface-container-high text-on-surface-variant hover:bg-surface-container'"
        @click="selectPeriod(p.value)"
      >{{ p.label }}</button>
      <div v-if="selectedPeriod === 'custom'" class="flex items-center gap-2 ml-2">
        <input v-model="customFrom" type="date" class="px-3 py-2 bg-surface-container-high rounded-lg text-sm text-on-surface border-none outline-none" />
        <span class="text-on-surface-variant">至</span>
        <input v-model="customTo" type="date" class="px-3 py-2 bg-surface-container-high rounded-lg text-sm text-on-surface border-none outline-none" />
        <button @click="fetchStatsWithRange" class="px-4 py-2 bg-primary text-on-primary rounded-lg text-sm font-medium">查询</button>
      </div>
    </div>

    <!-- Chart Area -->
    <div class="bg-surface-container-lowest p-8 rounded-xl h-96 flex flex-col shadow-sm">
      <h3 class="text-lg font-bold font-headline mb-6 text-on-surface">收入趋势</h3>
      <div class="flex-1 bg-surface-container flex items-center justify-center rounded-lg ring-1 ring-outline-variant/15">
        <span class="text-on-surface-variant text-sm font-medium">图表组件：收入趋势 占位符</span>
      </div>
    </div>

    <!-- Two Columns Layout -->
    <div class="grid grid-cols-1 lg:grid-cols-12 gap-8">
      <!-- Left: Latest Orders -->
      <div class="lg:col-span-7 bg-surface-container-lowest p-8 rounded-xl shadow-sm">
        <h3 class="text-lg font-bold font-headline mb-6 text-on-surface">最新订单</h3>
        <div class="overflow-x-auto">
          <table class="w-full text-left border-collapse">
            <thead>
              <tr class="border-b border-outline-variant/15 text-on-surface-variant text-sm font-medium">
                <th class="pb-4 pr-4 font-normal">订单号</th>
                <th class="pb-4 px-4 font-normal">包厢号</th>
                <th class="pb-4 px-4 font-normal">金额</th>
                <th class="pb-4 px-4 font-normal">状态</th>
                <th class="pb-4 pl-4 font-normal">时间</th>
              </tr>
            </thead>
            <tbody class="text-sm text-on-surface font-body">
              <tr
                v-for="order in latestOrders"
                :key="order.id"
                class="border-b border-outline-variant/15 last:border-0 hover:bg-surface-container-low transition-colors"
              >
                <td class="py-4 pr-4 font-mono text-xs">{{ order.id }}</td>
                <td class="py-4 px-4">{{ order.roomNumber }}</td>
                <td class="py-4 px-4 font-semibold">{{ formatCurrency(order.amount) }}</td>
                <td class="py-4 px-4">
                  <span
                    class="inline-flex items-center px-2 py-1 rounded-full text-xs font-medium"
                    :class="orderStatusClass(order.status)"
                  >
                    {{ formatOrderStatus(order.status) }}
                  </span>
                </td>
                <td class="py-4 pl-4 text-on-surface-variant">{{ order.createdAt }}</td>
              </tr>
            </tbody>
          </table>
        </div>
      </div>

      <!-- Right: Top Songs Ranking -->
      <div class="lg:col-span-5 bg-surface-container-lowest p-8 rounded-xl shadow-sm">
        <h3 class="text-lg font-bold font-headline mb-6 text-on-surface">热门歌曲排行</h3>
        <div class="space-y-4">
          <div
            v-for="(song, index) in topSongs"
            :key="song.id"
            class="flex items-center justify-between p-3 rounded-lg hover:bg-surface-container-low transition-colors group cursor-pointer"
          >
            <div class="flex items-center gap-4">
              <span
                class="text-xl font-bold font-headline w-6 text-center"
                :class="rankColor(index)"
              >{{ index + 1 }}</span>
              <img
                v-if="song.coverUrl"
                :src="'http://localhost:5276' + song.coverUrl"
                class="w-12 h-12 rounded-lg overflow-hidden shrink-0 object-cover"
                :alt="song.title"
              />
              <div v-else class="w-12 h-12 bg-slate-200 rounded-lg overflow-hidden shrink-0 flex items-center justify-center">
                <span class="material-symbols-outlined text-slate-400 text-lg">music_note</span>
              </div>
              <div>
                <p class="text-sm font-bold font-headline text-on-surface group-hover:text-primary transition-colors">{{ song.title }}</p>
                <p class="text-xs text-on-surface-variant font-body">{{ song.artist }}</p>
              </div>
            </div>
            <div class="text-right">
              <p class="text-sm font-semibold text-on-surface">{{ formatPlayCount(song.playCount) }}</p>
              <p class="text-xs text-on-surface-variant">次播放</p>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted } from 'vue'
import { dashboardApi } from '@/api'
import type { DashboardStats, Order, Song } from '@/types'
import { formatCurrency, formatOrderStatus, formatPlayCount } from '@/utils/format'

const stats = ref<DashboardStats>({
  totalRooms: 0,
  todayOrders: 0,
  totalRevenue: 0,
  activeUsers: 0,
})
const latestOrders = ref<Order[]>([])
const topSongs = ref<Song[]>([])

const revenuePeriods = [
  { label: '全部', value: 'all' },
  { label: '今日', value: 'today' },
  { label: '本周', value: 'week' },
  { label: '本月', value: 'month' },
  { label: '自定义', value: 'custom' },
]
const selectedPeriod = ref('all')
const customFrom = ref('')
const customTo = ref('')

function getDateRange(period: string): { from?: string; to?: string } {
  const now = new Date()
  const today = new Date(now.getFullYear(), now.getMonth(), now.getDate())
  switch (period) {
    case 'today':
      return { from: today.toISOString(), to: new Date(today.getTime() + 86400000).toISOString() }
    case 'week': {
      const day = today.getDay() || 7
      const monday = new Date(today.getTime() - (day - 1) * 86400000)
      return { from: monday.toISOString(), to: new Date(today.getTime() + 86400000).toISOString() }
    }
    case 'month': {
      const firstDay = new Date(now.getFullYear(), now.getMonth(), 1)
      return { from: firstDay.toISOString(), to: new Date(today.getTime() + 86400000).toISOString() }
    }
    case 'custom':
      return {
        from: customFrom.value ? new Date(customFrom.value).toISOString() : undefined,
        to: customTo.value ? new Date(new Date(customTo.value).getTime() + 86400000).toISOString() : undefined,
      }
    default:
      return {}
  }
}

async function selectPeriod(period: string) {
  selectedPeriod.value = period
  if (period !== 'custom') {
    await fetchStatsWithRange()
  }
}

async function fetchStatsWithRange() {
  const range = getDateRange(selectedPeriod.value)
  const params: any = {}
  if (range.from) params.from = range.from
  if (range.to) params.to = range.to
  const statsRes = await dashboardApi.getStats(params)
  stats.value = statsRes.data
}

const statCards = computed(() => [
  { icon: 'meeting_room', value: stats.value.totalRooms, label: '总包厢数', iconBg: 'bg-primary-fixed', iconColor: 'text-primary' },
  { icon: 'receipt_long', value: stats.value.todayOrders, label: '今日订单', iconBg: 'bg-secondary-fixed', iconColor: 'text-secondary' },
  { icon: 'payments', value: formatCurrency(stats.value.totalRevenue), label: '总收入', iconBg: 'bg-tertiary-fixed', iconColor: 'text-tertiary' },
  { icon: 'group', value: stats.value.activeUsers, label: '活跃用户', iconBg: 'bg-primary-container', iconColor: 'text-on-primary-container' },
])

function orderStatusClass(status: string) {
  switch (status) {
    case 'in_progress': return 'bg-tertiary-fixed text-on-tertiary-fixed'
    case 'completed': return 'bg-primary-fixed text-on-primary-fixed'
    case 'cancelled': return 'bg-error-container text-on-error-container'
    default: return 'bg-surface-variant text-on-surface-variant'
  }
}

function rankColor(index: number) {
  switch (index) {
    case 0: return 'text-tertiary'
    case 1: return 'text-secondary'
    case 2: return 'text-primary'
    default: return 'text-on-surface-variant'
  }
}

onMounted(async () => {
  const [statsRes, ordersRes, songsRes] = await Promise.all([
    dashboardApi.getStats(),
    dashboardApi.getLatestOrders(),
    dashboardApi.getTopSongs(),
  ])
  stats.value = statsRes.data
  latestOrders.value = ordersRes.data
  topSongs.value = songsRes.data
})
</script>
