<script setup lang="ts">
</script>

<template>
  <header class="fixed top-0 left-0 right-0 z-50 bg-surface flex justify-between items-center w-full px-8 py-4 font-headline text-sm tracking-wide">
    <div class="flex items-center gap-8">
      <span class="text-xl font-bold tracking-tighter text-on-surface">声域友</span>
      <div class="relative w-96">
        <input class="w-full bg-surface-container border-none rounded-full px-6 py-2 focus:ring-2 focus:ring-primary outline-none" placeholder="搜索歌曲、歌手..." type="text"/>
        <span class="material-symbols-outlined absolute right-4 top-2 text-slate-400">search</span>
      </div>
    </div>
    <div class="flex items-center gap-6">
      <button class="relative hover:bg-surface-container p-2 rounded-full transition-colors press-scale">
        <span class="material-symbols-outlined text-primary">notifications</span>
        <span class="absolute top-2 right-2 w-2 h-2 bg-error rounded-full"></span>
      </button>
      <div class="w-10 h-10 bg-slate-200 rounded-full flex items-center justify-center text-[10px] text-slate-500 font-bold border-2 border-white shadow-sm">用户头像</div>
    </div>
  </header>
</template>
