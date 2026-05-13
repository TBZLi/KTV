<script setup lang="ts">
import { useRoute } from 'vue-router'

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
  <nav class="h-screen w-64 fixed left-0 top-0 bg-surface flex flex-col p-6 gap-y-8 pt-24 z-40">
    <div class="space-y-2">
      <router-link
        v-for="item in navItems"
        :key="item.path"
        :to="item.path"
        class="flex items-center gap-4 px-4 py-3 rounded-2xl transition-all duration-200 press-scale"
        :class="isActive(item.path)
          ? 'bg-surface-container-lowest text-primary shadow-sm'
          : 'text-slate-500 hover:text-primary hover:bg-surface-container-low'"
      >
        <span class="material-symbols-outlined">{{ item.icon }}</span>
        <span class="font-medium">{{ item.label }}</span>
      </router-link>
    </div>
    <div class="mt-auto space-y-4">
      <button class="w-full bg-primary-fixed text-on-primary-fixed py-4 rounded-full font-bold flex items-center justify-center gap-2 hover:opacity-90 transition-opacity press-scale">
        <span class="material-symbols-outlined">queue_music</span>
        快速点歌
      </button>
    </div>
  </nav>
</template>
