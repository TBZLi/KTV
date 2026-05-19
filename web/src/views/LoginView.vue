<script setup lang="ts">
import { ref } from 'vue'
import { useRouter } from 'vue-router'
import { authApi, roomApi, roomRequestsApi } from '@/api'
import { useAuthStore } from '@/stores/auth'

const router = useRouter()
const authStore = useAuthStore()

const username = ref('')
const password = ref('')
const loading = ref(false)
const errorMsg = ref('')

// Room selection dialog
const showRoomDialog = ref(false)
const roomCode = ref('')
const joinError = ref('')
const requestStatus = ref<'idle' | 'pending' | 'done'>('idle')

async function handleLogin() {
  if (!username.value || !password.value) return
  errorMsg.value = ''
  loading.value = true
  try {
    const { data } = await authApi.login(username.value, password.value)
    authStore.setAuth(data.token, data.user)
    showRoomDialog.value = true
  } catch (err: any) {
    errorMsg.value = err.response?.data?.message || '用户名或密码错误'
  } finally {
    loading.value = false
  }
}

async function handleJoinRoom() {
  joinError.value = ''
  if (!roomCode.value || roomCode.value.length < 4) {
    joinError.value = '请输入有效的房间码'
    return
  }
  try {
    const { data } = await roomApi.joinByCode(roomCode.value.toUpperCase())
    authStore.setCurrentRoomId(data.roomId)
    showRoomDialog.value = false
    router.push('/room')
  } catch (err: any) {
    joinError.value = err.response?.data?.message || '房间不存在或已关闭'
  }
}

async function handleRequestRoom() {
  try {
    await roomRequestsApi.create()
    requestStatus.value = 'pending'
  } catch (err: any) {
    joinError.value = err.response?.data?.message || '申请失败'
  }
}

function skipRoomSelection() {
  showRoomDialog.value = false
  router.push('/explore')
}
</script>

<template>
  <div class="min-h-screen flex items-center justify-center relative overflow-hidden">
    <!-- Login card -->
    <div class="relative z-10 w-full max-w-[480px] glass rounded-2xl p-12 shadow-ambient">
      <!-- Logo section -->
      <div class="text-center mb-10">
        <div class="inline-flex items-center justify-center w-16 h-16 rounded-full bg-primary/10 mb-4">
          <span class="material-symbols-outlined text-4xl text-primary">waves</span>
        </div>
        <h1 class="text-3xl font-headline font-bold text-primary">声域友</h1>
        <p class="text-sm text-on-surface-variant uppercase tracking-widest mt-2 font-label">Hydro-Sonic Canvas</p>
      </div>

      <!-- Form -->
      <form @submit.prevent="handleLogin" class="space-y-5">
        <!-- Username -->
        <div class="relative">
          <span class="material-symbols-outlined absolute left-4 top-1/2 -translate-y-1/2 text-on-surface-variant">person</span>
          <input
            v-model="username"
            type="text"
            placeholder="用户名 / 手机号 / 邮箱"
            class="w-full bg-surface-container-high border-none rounded-lg py-4 pl-12 pr-4 text-base text-on-surface placeholder:text-on-surface-variant font-body focus:ring-2 focus:ring-primary/30 focus:outline-none"
          />
        </div>

        <!-- Password -->
        <div class="relative">
          <span class="material-symbols-outlined absolute left-4 top-1/2 -translate-y-1/2 text-on-surface-variant">lock</span>
          <input
            v-model="password"
            type="password"
            placeholder="密码"
            class="w-full bg-surface-container-high border-none rounded-lg py-4 pl-12 pr-4 text-base text-on-surface placeholder:text-on-surface-variant font-body focus:ring-2 focus:ring-primary/30 focus:outline-none"
          />
        </div>

        <!-- Error message -->
        <p v-if="errorMsg" class="text-error text-sm font-medium">{{ errorMsg }}</p>

        <!-- Login button -->
        <button
          type="submit"
          :disabled="loading"
          class="w-full bg-gradient-to-r from-primary to-secondary text-on-primary rounded-full py-4 font-bold font-label text-base flex items-center justify-center gap-2 active:scale-[0.98] transition-all duration-150 hover:shadow-ambient-primary disabled:opacity-60"
        >
          <template v-if="loading">
            <span class="material-symbols-outlined animate-spin">progress_activity</span>
          </template>
          <template v-else>
            登录
            <span class="material-symbols-outlined">arrow_forward</span>
          </template>
        </button>
      </form>

      <!-- Register link -->
      <p class="text-center text-sm text-on-surface-variant mt-6">
        还没有账号？
        <router-link to="/register" class="text-primary font-medium hover:underline">注册</router-link>
      </p>
    </div>

    <!-- Room Selection Dialog (after login) -->
    <div v-if="showRoomDialog" class="fixed inset-0 z-50 flex items-center justify-center bg-black/50" @click.self="skipRoomSelection">
      <div class="glass rounded-2xl shadow-xl w-full max-w-md p-8 space-y-6">
        <h2 class="text-2xl font-headline font-bold text-on-surface text-center">加入房间</h2>
        <p class="text-sm text-on-surface-variant text-center">输入房间码加入已有房间，或申请创建新房间</p>

        <!-- Join by code -->
        <div class="space-y-3">
          <label class="block text-sm font-medium text-on-surface-variant">输入房间码</label>
          <div class="flex gap-2">
            <input
              v-model="roomCode"
              type="text"
              placeholder="6位房间码"
              maxlength="6"
              class="flex-1 bg-surface-container-high border-none rounded-lg py-3 px-4 text-lg text-on-surface font-mono tracking-widest text-center placeholder:text-on-surface-variant/50 focus:ring-2 focus:ring-primary/30 focus:outline-none uppercase"
            />
            <button
              @click="handleJoinRoom"
              class="px-6 py-3 bg-primary text-on-primary rounded-lg font-semibold hover:opacity-90 transition-opacity"
            >
              加入
            </button>
          </div>
          <p v-if="joinError" class="text-error text-sm">{{ joinError }}</p>
        </div>

        <div class="relative flex items-center">
          <div class="flex-1 border-t border-surface-container-highest"></div>
          <span class="px-4 text-xs text-on-surface-variant">或</span>
          <div class="flex-1 border-t border-surface-container-highest"></div>
        </div>

        <!-- Request room -->
        <div class="text-center">
          <button
            v-if="requestStatus === 'idle'"
            @click="handleRequestRoom"
            class="px-6 py-3 bg-surface-container-high text-on-surface rounded-lg font-medium hover:bg-surface-container transition-colors"
          >
            申请开房间
          </button>
          <div v-else-if="requestStatus === 'pending'" class="text-primary font-medium flex items-center justify-center gap-2">
            <span class="material-symbols-outlined animate-spin text-sm">progress_activity</span>
            申请已提交，等待管理员审批
          </div>
        </div>

        <!-- Skip -->
        <div class="text-center pt-2">
          <button @click="skipRoomSelection" class="text-sm text-on-surface-variant hover:text-on-surface transition-colors">
            稍后再进入
          </button>
        </div>
      </div>
    </div>
  </div>
</template>
