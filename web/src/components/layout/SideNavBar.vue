<script setup lang="ts">
import { useRoute } from 'vue-router'

defineProps<{ collapsed: boolean }>()
const emit = defineEmits<{ toggle: [] }>()

const route = useRoute()

const navItems = [
  { path: '/explore', icon: 'explore', label: '探索歌曲' },
  { path: '/charts', icon: 'trending_up', label: '热门榜单' },
  { path: '/favorites', icon: 'favorite', label: '我的收藏' },
  { path: '/room', icon: 'meeting_room', label: '当前房间' },
]

const isActive = (path: string) => route.path === path
</script>

<template>
  <nav
    class="h-screen fixed left-0 top-0 glass flex flex-col pt-24 z-40 overflow-hidden transition-all duration-300"
    :class="collapsed ? 'w-20 p-3' : 'w-64 p-6'"
  >
    <div class="space-y-2">
      <router-link
        v-for="item in navItems"
        :key="item.path"
        :to="item.path"
        class="flex items-center rounded-2xl transition-all duration-200 press-scale"
        :class="[
          collapsed ? 'justify-center p-3' : 'gap-4 px-4 py-3',
          isActive(item.path)
            ? 'bg-surface-container-lowest text-primary shadow-sm'
            : 'text-slate-500 hover:text-primary hover:bg-surface-container-low',
        ]"
      >
        <span class="material-symbols-outlined">{{ item.icon }}</span>
        <span v-if="!collapsed" class="font-medium whitespace-nowrap">{{ item.label }}</span>
      </router-link>
    </div>

    <div class="mt-auto" :class="collapsed ? 'flex flex-col items-center' : ''">
      <button
        class="bg-primary-fixed text-on-primary-fixed rounded-full font-bold flex items-center justify-center gap-2 hover:opacity-90 transition-all press-scale"
        :class="collapsed ? 'p-3' : 'w-full py-4'"
      >
        <span class="material-symbols-outlined">queue_music</span>
        <span v-if="!collapsed" class="whitespace-nowrap">快速点歌</span>
      </button>
    </div>
  </nav>

  <!-- 侧边栏边缘收起/展开按钮 -->
  <button
    @click="emit('toggle')"
    class="fixed top-1/2 -translate-y-1/2 z-50 glass w-6 h-16 rounded-r-full flex items-center justify-center hover:bg-white/80 transition-all duration-300 press-scale"
    :style="{ left: collapsed ? '80px' : '256px' }"
  >
    <span
      class="material-symbols-outlined text-sm text-on-surface-variant transition-transform duration-300"
      :class="collapsed ? 'rotate-180' : ''"
    >
      chevron_left
    </span>
  </button>
</template>
