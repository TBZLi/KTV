<script setup lang="ts">
import { ref } from 'vue'

const isPlaying = ref(true)
const volume = ref(75)
</script>

<template>
  <footer class="fixed bottom-0 left-0 w-full z-50 h-24 bg-white/70 backdrop-blur-xl flex justify-between items-center px-12 rounded-t-[3rem] shadow-[0_-8px_32px_rgba(25,28,30,0.06)]">
    <div class="absolute top-0 left-0 w-full h-1 bg-gradient-to-r from-primary to-secondary"></div>
    <div class="flex items-center gap-4 w-1/4">
      <div class="w-12 h-12 bg-slate-200 rounded-lg flex items-center justify-center text-[8px] text-slate-500 text-center px-1">专辑封面</div>
      <div class="overflow-hidden">
        <p class="font-label text-xs font-bold uppercase tracking-widest text-primary">Playing</p>
        <p class="text-sm font-bold truncate">晴天</p>
      </div>
    </div>
    <div class="flex items-center gap-8">
      <button class="text-slate-400 hover:scale-110 transition-transform press-scale">
        <span class="material-symbols-outlined">skip_previous</span>
      </button>
      <button class="text-primary hover:scale-110 transition-transform scale-125 press-scale">
        <span class="material-symbols-outlined" :style="{ fontVariationSettings: `'FILL' ${isPlaying ? 1 : 0}` }">
          {{ isPlaying ? 'pause_circle' : 'play_circle' }}
        </span>
      </button>
      <button class="text-slate-400 hover:scale-110 transition-transform press-scale">
        <span class="material-symbols-outlined">skip_next</span>
      </button>
    </div>
    <div class="flex items-center gap-6 w-1/4 justify-end">
      <div class="flex items-center gap-3 w-32">
        <span class="material-symbols-outlined text-slate-400">volume_up</span>
        <div class="h-1 flex-grow bg-surface-container rounded-full">
          <div class="h-full bg-primary rounded-full relative" :style="{ width: volume + '%' }">
            <div class="absolute right-0 top-1/2 -translate-y-1/2 w-3 h-3 bg-primary border-2 border-white rounded-full shadow-sm"></div>
          </div>
        </div>
      </div>
      <button class="text-slate-400 hover:scale-110 transition-transform press-scale">
        <span class="material-symbols-outlined">lyrics</span>
      </button>
    </div>
  </footer>
</template>
