<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { chartsApi } from '@/api'
import type { Song } from '@/types'
import { formatPlayCount } from '@/utils/format'

type ChartPeriod = 'daily' | 'weekly'

const period = ref<ChartPeriod>('daily')
const charts = ref<Song[]>([])

async function loadCharts() {
  const api = period.value === 'daily' ? chartsApi.getDaily : chartsApi.getWeekly
  const { data } = await api()
  charts.value = data.slice(0, 10)
}

function setPeriod(p: ChartPeriod) {
  period.value = p
  loadCharts()
}

onMounted(() => {
  loadCharts()
})
</script>

<template>
  <div class="max-w-4xl mx-auto px-8">
    <!-- Header Section -->
    <div class="mb-10 flex flex-col gap-6">
      <h1 class="text-5xl font-extrabold text-on-surface font-display tracking-tight">热门榜单</h1>
      <!-- Period toggle -->
      <div class="flex bg-surface-container-low p-1 rounded-full w-max">
        <button
          @click="setPeriod('daily')"
          :class="[
            'px-6 py-2 rounded-full font-label font-bold transition-all',
            period === 'daily'
              ? 'bg-surface-container-lowest text-primary shadow-sm'
              : 'text-on-surface-variant hover:text-primary',
          ]"
        >
          日榜
        </button>
        <button
          @click="setPeriod('weekly')"
          :class="[
            'px-6 py-2 rounded-full font-label font-bold transition-all',
            period === 'weekly'
              ? 'bg-surface-container-lowest text-primary shadow-sm'
              : 'text-on-surface-variant hover:text-primary',
          ]"
        >
          周榜
        </button>
      </div>
    </div>

    <!-- Ranked song list -->
    <div class="flex flex-col gap-10">
      <div
        v-for="(song, index) in charts"
        :key="song.id"
        class="flex items-center gap-6 group hover:bg-surface-container-lowest p-4 -mx-4 rounded-lg transition-colors cursor-pointer"
      >
        <!-- Rank number -->
        <span
          class="text-3xl font-display w-12 text-center"
          :class="index === 0 ? 'font-black text-tertiary-container' : 'font-bold text-on-surface-variant'"
        >
          {{ index + 1 }}
        </span>

        <!-- Album cover -->
        <img
          v-if="song.coverUrl"
          :src="'http://localhost:5276' + song.coverUrl"
          class="w-16 h-16 rounded shrink-0 shadow-sm object-cover"
          :alt="song.title"
        />
        <div v-else class="w-16 h-16 rounded bg-surface-container shrink-0 shadow-sm flex items-center justify-center">
          <span class="material-symbols-outlined text-slate-400">image</span>
        </div>

        <!-- Song info -->
        <div class="flex-1 min-w-0">
          <h3 class="text-xl font-bold text-on-surface truncate font-headline mb-1">{{ song.title }}</h3>
          <p class="text-sm text-on-surface-variant truncate font-label">{{ song.artist }}</p>
        </div>

        <!-- Play count -->
        <div class="text-right mr-4 hidden sm:block">
          <span class="text-sm font-medium text-on-surface-variant font-label">{{ formatPlayCount(song.playCount) }} 次播放</span>
        </div>

        <!-- Order button -->
        <button
          :class="[
            'px-6 py-3 rounded-full font-bold font-label shrink-0 transition-colors',
            index === 0
              ? 'bg-primary text-on-primary shadow-sm hover:bg-secondary'
              : 'bg-primary-fixed text-on-primary-fixed hover:bg-primary-fixed-dim',
          ]"
        >
          点歌
        </button>
      </div>
    </div>
  </div>
</template>
