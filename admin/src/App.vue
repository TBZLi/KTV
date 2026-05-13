<script setup lang="ts">
import SideNavBar from '@/components/layout/SideNavBar.vue'
import TopNavBar from '@/components/layout/TopNavBar.vue'
</script>

<template>
  <div class="flex min-h-screen w-full bg-surface">
    <SideNavBar />
    <div class="flex-1 ml-64 flex flex-col min-h-screen">
      <TopNavBar />
      <main class="flex-1">
        <router-view />
      </main>
    </div>
  </div>
</template>
