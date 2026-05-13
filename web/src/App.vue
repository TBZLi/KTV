<script setup lang="ts">
import { computed } from 'vue'
import { useRoute } from 'vue-router'
import SideNavBar from '@/components/layout/SideNavBar.vue'
import TopNavBar from '@/components/layout/TopNavBar.vue'
import BottomPlayerBar from '@/components/layout/BottomPlayerBar.vue'

const route = useRoute()
const isLoginPage = computed(() => route.name === 'Login')
</script>

<template>
  <template v-if="isLoginPage">
    <router-view />
  </template>
  <template v-else>
    <TopNavBar />
    <SideNavBar />
    <main class="ml-64 pt-24 px-10 pb-32">
      <div class="max-w-6xl mx-auto">
        <router-view />
      </div>
    </main>
    <BottomPlayerBar />
  </template>
</template>
