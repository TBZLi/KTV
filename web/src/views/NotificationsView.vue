<script setup lang="ts">
import { ref } from 'vue'
import { useRouter } from 'vue-router'

const router = useRouter()
const notifications = ref<any[]>([])
</script>

<template>
  <div class="min-h-screen relative">
    <!-- Top bar -->
    <div class="relative z-10 max-w-2xl mx-auto px-5 flex items-center gap-3 py-4">
      <button
        @click="router.back()"
        class="w-10 h-10 rounded-xl bg-surface-container-lowest/70 backdrop-blur-lg border border-outline-variant/10 shadow-ambient-sm flex items-center justify-center hover:bg-surface-container hover:scale-105 transition-all"
      >
        <span class="material-symbols-outlined text-lg text-on-surface">arrow_back</span>
      </button>
      <span class="text-lg font-bold text-on-surface font-headline">消息通知</span>
    </div>

    <div class="relative z-10 max-w-2xl mx-auto px-5 pb-16">
      <!-- Notification list -->
      <div v-if="notifications.length > 0" class="flex flex-col gap-3">
        <div
          v-for="item in notifications"
          :key="item.id"
          class="bg-surface-container-lowest/60 backdrop-blur-xl rounded-2xl p-4 shadow-ambient-sm border border-outline-variant/10"
        >
          <div class="text-sm text-on-surface">{{ item.message }}</div>
          <div class="text-xs text-outline mt-1">{{ item.time }}</div>
        </div>
      </div>

      <!-- Empty state -->
      <div v-else class="flex flex-col items-center justify-center py-32">
        <div class="w-20 h-20 rounded-full bg-primary/10 flex items-center justify-center mb-5">
          <span class="material-symbols-outlined text-4xl text-primary">notifications_off</span>
        </div>
        <p class="text-lg font-bold text-on-surface font-headline mb-1">暂无通知</p>
        <p class="text-sm text-outline">有新消息时会在这里提醒你</p>
      </div>
    </div>
  </div>
</template>
