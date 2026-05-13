<template>
  <div class="p-8 flex flex-col gap-8">
    <!-- Page Header & Filters -->
    <div class="flex flex-col md:flex-row justify-between items-start md:items-center gap-4">
      <h3 class="font-headline text-3xl font-extrabold tracking-tight text-on-surface">订单管理</h3>
      <div class="flex items-center gap-3">
        <!-- Search Bar -->
        <div class="flex items-center bg-surface-container-lowest rounded-full shadow-[0_4px_24px_rgba(0,0,0,0.02)] overflow-hidden">
          <div class="relative">
            <select
              v-model="searchField"
              class="appearance-none bg-transparent text-sm font-medium text-on-surface-variant pl-4 pr-8 py-3 outline-none cursor-pointer"
            >
              <option value="orderId">订单号</option>
              <option value="username">用户名</option>
              <option value="roomNumber">包厢号</option>
            </select>
            <span class="material-symbols-outlined absolute right-2 top-1/2 -translate-y-1/2 text-outline text-lg pointer-events-none">arrow_drop_down</span>
          </div>
          <div class="w-px h-6 bg-outline-variant/30"></div>
          <div class="relative">
            <span class="material-symbols-outlined absolute left-3 top-1/2 -translate-y-1/2 text-outline text-lg">search</span>
            <input
              v-model="searchKeyword"
              @input="onSearch"
              @keydown.enter="doSearch"
              class="bg-transparent text-sm py-3 pl-10 pr-4 w-48 text-on-surface placeholder:text-outline outline-none"
              :placeholder="searchPlaceholder"
            />
          </div>
        </div>
        <div
          class="bg-surface-container-lowest flex items-center gap-2 px-5 py-3 rounded-full shadow-[0_4px_24px_rgba(0,0,0,0.02)] cursor-pointer hover:bg-surface-container-low transition-colors"
          @click="showStatusFilter = !showStatusFilter"
        >
          <span class="material-symbols-outlined text-on-surface-variant text-[20px]">filter_list</span>
          <span class="text-sm font-medium text-on-surface-variant">状态筛选</span>
          <span class="material-symbols-outlined text-on-surface-variant text-[20px] ml-1">arrow_drop_down</span>
        </div>
        <button @click="openCreateDialog" :disabled="activeUsers.length === 0 || allRooms.length === 0" class="bg-primary text-on-primary px-6 py-3 rounded-full text-sm font-bold shadow-[0_8px_16px_-4px_rgba(0,99,153,0.2)] hover:bg-secondary hover:scale-[0.98] transition-all flex items-center gap-2 ml-2 press-scale disabled:opacity-50">
          <span class="material-symbols-outlined text-[20px]">add</span>
          {{ activeUsers.length === 0 || allRooms.length === 0 ? '加载中...' : '新建订单' }}
        </button>
      </div>
    </div>

    <!-- Status Filter Dropdown -->
    <div v-if="showStatusFilter" class="flex gap-2">
      <button
        v-for="tab in statusTabs"
        :key="tab.value"
        class="px-4 py-2 text-sm font-medium rounded-lg transition-colors"
        :class="selectedStatus === tab.value
          ? 'bg-primary text-on-primary'
          : 'bg-surface-container-high text-on-surface-variant hover:bg-surface-container'"
        @click="selectedStatus = tab.value; showStatusFilter = false"
      >
        {{ tab.label }}
      </button>
    </div>

    <!-- Data Grid -->
    <div class="bg-surface-container-lowest rounded-xl p-2 shadow-[0_12px_40px_-12px_rgba(0,0,0,0.05)]">
      <!-- Grid Header -->
      <div class="grid grid-cols-7 gap-4 px-6 py-4 rounded-lg bg-surface-container text-xs font-bold text-on-surface-variant tracking-wider uppercase mb-2">
        <div>订单号</div>
        <div>包厢号</div>
        <div>用户</div>
        <div>金额</div>
        <div>状态</div>
        <div>创建时间</div>
        <div class="text-right">操作</div>
      </div>

      <!-- Grid Rows -->
      <div class="flex flex-col gap-1">
        <div
          v-for="order in orders"
          :key="order.id"
          class="grid grid-cols-7 gap-4 px-6 py-5 rounded-lg items-center text-sm font-medium text-on-surface hover:bg-surface transition-colors cursor-pointer group"
        >
          <div class="font-headline font-bold text-primary">{{ order.id }}</div>
          <div class="flex items-center gap-3">
            <div class="w-8 h-8 rounded-lg bg-surface-container-high flex items-center justify-center text-[10px] text-on-surface-variant font-bold">
              {{ order.roomType === 'VIP' ? 'VIP' : order.roomType === 'Medium' ? 'MED' : 'SML' }}
            </div>
            <span>{{ order.roomNumber }}</span>
          </div>
          <div class="text-on-surface-variant">{{ order.username || `#${order.userId}` }}</div>
          <div
            class="font-headline font-semibold"
            :class="order.status === 'cancelled' ? 'text-on-surface-variant line-through' : ''"
          >{{ formatCurrency(order.amount) }}</div>
          <div>
            <span
              class="inline-flex items-center px-3 py-1 rounded-full text-xs font-bold"
              :class="orderStatusClass(order.status)"
            >
              {{ formatOrderStatus(order.status) }}
            </span>
          </div>
          <div class="text-on-surface-variant">{{ order.createdAt }}</div>
          <div class="text-right flex justify-end gap-3 opacity-0 group-hover:opacity-100 transition-opacity">
            <button @click="openDetail(order)" class="text-primary hover:text-secondary font-bold text-xs">查看详情</button>
            <button
              v-if="order.status === 'in_progress'"
              @click="handleComplete(order)"
              class="text-on-surface-variant hover:text-primary font-bold text-xs"
            >完成</button>
            <button
              v-if="order.status === 'in_progress'"
              @click="handleCancel(order)"
              class="text-on-surface-variant hover:text-error font-bold text-xs"
            >取消</button>
            <button
              v-if="order.status === 'in_progress'"
              @click="handleRefund(order)"
              class="text-on-surface-variant hover:text-error font-bold text-xs"
            >退款</button>
            <button
              v-if="order.status === 'cancelled'"
              @click="handleRestore(order)"
              class="text-secondary-container hover:text-primary font-bold text-xs"
            >恢复</button>
          </div>
        </div>
      </div>

      <!-- Empty State -->
      <div v-if="orders.length === 0" class="py-16 text-center text-on-surface-variant">
        <span class="material-symbols-outlined text-4xl text-outline-variant">receipt_long</span>
        <p class="mt-2">暂无订单数据</p>
      </div>

      <!-- Pagination -->
      <div class="flex justify-between items-center px-6 py-4 mt-2 bg-surface-container-lowest rounded-b-xl border-t-0">
        <span class="text-xs text-on-surface-variant font-medium">
          显示第 {{ (currentPage - 1) * pageSize + 1 }} 到 {{ Math.min(currentPage * pageSize, total) }} 条，共 {{ total }} 条记录
        </span>
        <div class="flex gap-1">
          <button
            class="w-8 h-8 rounded-full flex items-center justify-center text-on-surface-variant hover:bg-surface-container-high transition-colors disabled:opacity-50"
            :disabled="currentPage <= 1"
            @click="currentPage--"
          >
            <span class="material-symbols-outlined text-sm">chevron_left</span>
          </button>
          <button
            v-for="page in totalPages"
            :key="page"
            class="w-8 h-8 rounded-full flex items-center justify-center font-medium text-xs transition-colors"
            :class="page === currentPage
              ? 'bg-primary text-on-primary shadow-sm'
              : 'text-on-surface hover:bg-surface-container-high'"
            @click="currentPage = page"
          >{{ page }}</button>
          <button
            class="w-8 h-8 rounded-full flex items-center justify-center text-on-surface-variant hover:bg-surface-container-high transition-colors disabled:opacity-50"
            :disabled="currentPage >= totalPages"
            @click="currentPage++"
          >
            <span class="material-symbols-outlined text-sm">chevron_right</span>
          </button>
        </div>
      </div>
    </div>

    <!-- Create Order Dialog -->
    <div v-if="showCreateDialog" class="fixed inset-0 z-50 flex items-center justify-center bg-black/40" @click.self="showCreateDialog = false">
      <div class="bg-surface-container-lowest rounded-2xl shadow-xl w-full max-w-lg p-8 space-y-6">
        <h3 class="text-xl font-display font-bold text-on-surface">新建订单</h3>

        <!-- Error message -->
        <div v-if="errorMsg" class="bg-error/10 text-error px-4 py-3 rounded-lg text-sm font-medium flex items-center gap-2">
          <span class="material-symbols-outlined text-lg">error</span>
          {{ errorMsg }}
        </div>

        <form @submit.prevent="handleCreate" class="space-y-4">
          <div>
            <label class="block text-sm font-medium text-on-surface-variant mb-1">用户</label>
            <select v-model.number="createForm.userId" @change="fetchUserBalance" required class="w-full px-4 py-3 bg-surface-container-high rounded-lg border-none text-on-surface focus:ring-2 focus:ring-primary/30 outline-none">
              <option v-for="u in activeUsers" :key="u.id" :value="u.id">{{ u.username }} (#{{ u.id }})</option>
            </select>
            <p v-if="userBalance !== null" class="mt-1.5 text-xs" :class="finalAmount > userBalance ? 'text-error' : 'text-on-surface-variant'">
              账户余额: {{ formatCurrency(userBalance) }}
              <span v-if="finalAmount > userBalance" class="font-semibold">— 余额不足!</span>
            </p>
          </div>
          <div>
            <label class="block text-sm font-medium text-on-surface-variant mb-1">包厢</label>
            <select v-model.number="createForm.roomId" required class="w-full px-4 py-3 bg-surface-container-high rounded-lg border-none text-on-surface focus:ring-2 focus:ring-primary/30 outline-none">
              <option v-for="r in allRooms" :key="r.id" :value="r.id" :disabled="r.status === 'in_use'">
                {{ r.roomNumber }} ({{ formatRoomType(r.roomType) }}) — {{ formatRoomStatus(r.status) }}
              </option>
            </select>
          </div>
          <div>
            <label class="block text-sm font-medium text-on-surface-variant mb-1">使用时长（小时）</label>
            <input v-model.number="createForm.hours" type="number" min="0.5" step="0.5" required class="w-full px-4 py-3 bg-surface-container-high rounded-lg border-none text-on-surface focus:ring-2 focus:ring-primary/30 outline-none" :disabled="createForm.useCustomAmount" />
            <p class="mt-1.5 text-xs text-on-surface-variant">
              单价: {{ formatCurrency(baseHourlyRate) }}/小时
              <span v-if="!createForm.useCustomAmount"> · 预估金额: <span class="font-semibold text-on-surface">{{ formatCurrency(calculatedAmount) }}</span></span>
            </p>
          </div>
          <div>
            <label class="flex items-center gap-2 text-sm font-medium text-on-surface-variant mb-1 cursor-pointer">
              <input type="checkbox" v-model="createForm.useCustomAmount" class="rounded" />
              自定义金额（特殊情况）
            </label>
            <input v-if="createForm.useCustomAmount" v-model.number="createForm.amountOverride" type="number" min="0.01" step="0.01" required placeholder="输入自定义金额" class="w-full px-4 py-3 bg-surface-container-high rounded-lg border-none text-on-surface focus:ring-2 focus:ring-primary/30 outline-none" :class="userBalance !== null && finalAmount > userBalance ? 'ring-2 ring-error/50' : ''" />
          </div>
          <div class="flex justify-end gap-3 pt-2">
            <button type="button" @click="showCreateDialog = false" class="px-6 py-3 rounded-lg font-medium text-on-surface-variant hover:bg-surface-container transition-colors">取消</button>
            <button type="submit" :disabled="saving || !canCreate" class="px-6 py-3 bg-primary text-on-primary rounded-lg font-semibold hover:bg-primary/90 transition-colors disabled:opacity-60">
              {{ saving ? '创建中...' : '创建并扣款' }}
            </button>
          </div>
        </form>
      </div>
    </div>

    <!-- Detail Dialog -->
    <div v-if="showDetailDialog && detailOrder" class="fixed inset-0 z-50 flex items-center justify-center bg-black/40" @click.self="showDetailDialog = false">
      <div class="bg-surface-container-lowest rounded-2xl shadow-xl w-full max-w-lg p-8 space-y-4">
        <h3 class="text-xl font-display font-bold text-on-surface">订单详情</h3>
        <div class="space-y-3 text-sm">
          <div class="flex justify-between py-2 border-b border-surface-container-highest">
            <span class="text-on-surface-variant">订单号</span>
            <span class="font-medium text-primary">{{ detailOrder.id }}</span>
          </div>
          <div class="flex justify-between py-2 border-b border-surface-container-highest">
            <span class="text-on-surface-variant">包厢</span>
            <span class="font-medium">{{ detailOrder.roomNumber }} ({{ detailOrder.roomType }})</span>
          </div>
          <div class="flex justify-between py-2 border-b border-surface-container-highest">
            <span class="text-on-surface-variant">金额</span>
            <span class="font-medium">{{ formatCurrency(detailOrder.amount) }}</span>
          </div>
          <div class="flex justify-between py-2 border-b border-surface-container-highest">
            <span class="text-on-surface-variant">状态</span>
            <span
              class="inline-flex items-center px-3 py-1 rounded-full text-xs font-bold"
              :class="orderStatusClass(detailOrder.status)"
            >{{ formatOrderStatus(detailOrder.status) }}</span>
          </div>
          <div class="flex justify-between py-2 border-b border-surface-container-highest">
            <span class="text-on-surface-variant">用户</span>
            <span class="font-medium">{{ detailOrder.username || `#${detailOrder.userId}` }}</span>
          </div>
          <div class="flex justify-between py-2">
            <span class="text-on-surface-variant">创建时间</span>
            <span class="font-medium">{{ detailOrder.createdAt }}</span>
          </div>
        </div>
        <div class="flex justify-between pt-2">
          <button
            v-if="detailOrder.status !== 'in_progress'"
            @click="handleDelete(detailOrder)"
            class="px-6 py-3 rounded-lg font-medium text-error hover:bg-error/10 transition-colors"
          >删除订单</button>
          <button @click="showDetailDialog = false" class="px-6 py-3 rounded-lg font-medium text-on-surface-variant hover:bg-surface-container transition-colors">关闭</button>
        </div>
      </div>
    </div>

    <!-- Password Verification Dialog -->
    <div v-if="showPasswordDialog" class="fixed inset-0 z-[60] flex items-center justify-center bg-black/40" @click.self="showPasswordDialog = false">
      <div class="bg-surface-container-lowest rounded-2xl shadow-xl w-full max-w-sm p-8 space-y-4">
        <h3 class="text-lg font-display font-bold text-on-surface">二级密码验证</h3>
        <p class="text-sm text-on-surface-variant">请输入管理员密码确认操作</p>
        <form @submit.prevent="submitPassword" class="space-y-4">
          <input v-model="passwordInput" type="password" required autofocus placeholder="输入密码" class="w-full px-4 py-3 bg-surface-container-high rounded-lg border-none text-on-surface focus:ring-2 focus:ring-primary/30 outline-none" />
          <p v-if="passwordError" class="text-xs text-error font-semibold">{{ passwordError }}</p>
          <div class="flex justify-end gap-3">
            <button type="button" @click="showPasswordDialog = false" class="px-6 py-3 rounded-lg font-medium text-on-surface-variant hover:bg-surface-container transition-colors">取消</button>
            <button type="submit" class="px-6 py-3 bg-primary text-on-primary rounded-lg font-semibold hover:bg-primary/90 transition-colors">确认</button>
          </div>
        </form>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, computed, onMounted, watch } from 'vue'
import { ordersApi, accountsApi, roomsApi, settingsApi, authApi } from '@/api'
import type { Order, Room, User } from '@/types'
import { formatCurrency, formatOrderStatus, formatRoomType, formatRoomStatus } from '@/utils/format'

const orders = ref<Order[]>([])
const total = ref(0)
const currentPage = ref(1)
const pageSize = ref(10)
const selectedStatus = ref('')
const showStatusFilter = ref(false)
const saving = ref(false)
const errorMsg = ref('')

// Create dialog
const showCreateDialog = ref(false)
const createForm = ref({ userId: 2, roomId: 1, hours: 2, useCustomAmount: false, amountOverride: 0 })
const userBalance = ref<number | null>(null)
const baseHourlyRate = ref(120)
const verifyDeleteOrder = ref(true)
const allUsers = ref<User[]>([])
const allRooms = ref<Room[]>([])

// Password verification dialog
const showPasswordDialog = ref(false)
const passwordInput = ref('')
const passwordError = ref('')
const passwordCallback = ref<(() => Promise<void>) | null>(null)

const calculatedAmount = computed(() => createForm.value.hours * baseHourlyRate.value)
const finalAmount = computed(() => createForm.value.useCustomAmount ? createForm.value.amountOverride : calculatedAmount.value)

const activeUsers = computed(() => allUsers.value.filter(u => u.status === 'active'))

const canCreate = computed(() => {
  if (finalAmount.value <= 0) return false
  if (userBalance.value !== null && finalAmount.value > userBalance.value) return false
  const room = allRooms.value.find(r => r.id === createForm.value.roomId)
  if (room?.status === 'in_use') return false
  return true
})

// Detail dialog
const showDetailDialog = ref(false)
const detailOrder = ref<Order | null>(null)

const statusTabs = [
  { label: '全部', value: '' },
  { label: '进行中', value: 'in_progress' },
  { label: '已完成', value: 'completed' },
  { label: '已取消', value: 'cancelled' },
  { label: '已退款', value: 'refunded' },
]

// Search
const searchField = ref('orderId')
const searchKeyword = ref('')
let searchTimer: ReturnType<typeof setTimeout>

const searchPlaceholder = computed(() => {
  switch (searchField.value) {
    case 'orderId': return '搜索订单号...'
    case 'username': return '搜索用户名...'
    case 'roomNumber': return '搜索包厢号...'
    default: return '搜索...'
  }
})

const totalPages = computed(() => Math.max(1, Math.ceil(total.value / pageSize.value)))

function orderStatusClass(status: string) {
  switch (status) {
    case 'in_progress': return 'bg-primary-container text-on-primary-container'
    case 'completed': return 'bg-secondary-container text-on-secondary-container'
    case 'cancelled': return 'bg-surface-variant text-on-surface-variant'
    case 'refunded': return 'bg-error/10 text-error'
    default: return 'bg-surface-variant text-on-surface-variant'
  }
}

async function fetchOrders() {
  const res = await ordersApi.getList({
    status: selectedStatus.value || undefined,
    searchField: searchKeyword.value ? searchField.value : undefined,
    searchKeyword: searchKeyword.value || undefined,
    page: currentPage.value,
    pageSize: pageSize.value,
  })
  orders.value = res.data.items
  total.value = res.data.total
}

function onSearch() {
  clearTimeout(searchTimer)
  searchTimer = setTimeout(() => {
    doSearch()
  }, 300)
}

function doSearch() {
  clearTimeout(searchTimer)
  currentPage.value = 1
  fetchOrders()
}

async function fetchUserBalance() {
  try {
    const res = await accountsApi.getById(createForm.value.userId)
    userBalance.value = res.data.balance
  } catch {
    userBalance.value = null
  }
}

async function openCreateDialog() {
  // Refresh users and rooms before opening
  const [usersRes, roomsRes] = await Promise.all([
    accountsApi.getList({ pageSize: 1000 }),
    roomsApi.getList({ pageSize: 1000 }),
  ])
  allUsers.value = usersRes.data.items
  allRooms.value = roomsRes.data.items

  if (activeUsers.value.length === 0 || allRooms.value.length === 0) return
  createForm.value = { userId: activeUsers.value[0].id, roomId: allRooms.value[0].id, hours: 2, useCustomAmount: false, amountOverride: 0 }
  errorMsg.value = ''
  userBalance.value = null
  showCreateDialog.value = true
  fetchUserBalance()
}

async function handleCreate() {
  errorMsg.value = ''
  if (userBalance.value !== null && finalAmount.value > userBalance.value) {
    errorMsg.value = `账户余额不足，当前余额 ${formatCurrency(userBalance.value)}，需要 ${formatCurrency(finalAmount.value)}`
    return
  }
  saving.value = true
  try {
    const payload: any = {
      userId: createForm.value.userId,
      roomId: createForm.value.roomId,
    }
    if (createForm.value.useCustomAmount) {
      payload.amount = createForm.value.amountOverride
    } else {
      payload.hours = createForm.value.hours
    }
    await ordersApi.create(payload)
    showCreateDialog.value = false
    await fetchOrders()
  } catch (err: any) {
    errorMsg.value = err.response?.data?.message || err.response?.data || '创建订单失败'
  } finally {
    saving.value = false
  }
}

function openDetail(order: Order) {
  detailOrder.value = order
  showDetailDialog.value = true
}

async function handleRefund(order: Order) {
  if (!confirm(`确定对订单 ${order.id} 执行退款操作？退款金额将返还至用户账户。`)) return
  try {
    await ordersApi.refund(order.id)
    await fetchOrders()
  } catch (err: any) {
    alert(err.response?.data?.message || err.response?.data || '退款失败')
  }
}

async function handleComplete(order: Order) {
  if (!confirm(`确定将订单 ${order.id} 标记为完成？`)) return
  try {
    await ordersApi.complete(order.id)
    await fetchOrders()
  } catch (err: any) {
    alert(err.response?.data?.message || err.response?.data || '操作失败')
  }
}

async function handleCancel(order: Order) {
  if (!confirm(`确定取消订单 ${order.id}？退款金额将返还至用户账户。`)) return
  try {
    await ordersApi.cancel(order.id)
    await fetchOrders()
  } catch (err: any) {
    alert(err.response?.data?.message || err.response?.data || '取消失败')
  }
}

async function handleRestore(order: Order) {
  if (!confirm(`确定恢复订单 ${order.id}？将从用户账户重新扣款 ¥${order.amount.toFixed(2)}，并重新绑定房间。`)) return
  try {
    await ordersApi.restore(order.id)
    await fetchOrders()
  } catch (err: any) {
    alert(err.response?.data?.message || err.response?.data || '恢复失败')
  }
}

async function handleDelete(order: Order) {
  const doDelete = async () => {
    try {
      await ordersApi.delete(order.id)
      showDetailDialog.value = false
      await fetchOrders()
    } catch (err: any) {
      alert(err.response?.data?.message || err.response?.data || '删除失败')
    }
  }
  if (verifyDeleteOrder.value) {
    openPasswordDialog(doDelete)
  } else {
    await doDelete()
  }
}

function openPasswordDialog(callback: () => Promise<void>) {
  passwordInput.value = ''
  passwordError.value = ''
  passwordCallback.value = callback
  showPasswordDialog.value = true
}

async function submitPassword() {
  passwordError.value = ''
  try {
    await authApi.verifyPassword(passwordInput.value)
    showPasswordDialog.value = false
    if (passwordCallback.value) await passwordCallback.value()
  } catch {
    passwordError.value = '密码错误'
  }
}

onMounted(async () => {
  const [ordersRes, settingsRes, usersRes, roomsRes] = await Promise.all([
    ordersApi.getList({ page: 1, pageSize: pageSize.value }),
    settingsApi.get(),
    accountsApi.getList({ pageSize: 1000 }),
    roomsApi.getList({ pageSize: 1000 }),
  ])
  orders.value = ordersRes.data.items
  total.value = ordersRes.data.total
  const sData = settingsRes.data as any
  const rawRate = sData.base_hourly_rate ?? sData.baseHourlyRate
  if (rawRate) baseHourlyRate.value = Number(rawRate)
  // Support both snake_case (from DB) and camelCase keys
  const rawVerify = sData.verify_delete_order ?? sData.verifyDeleteOrder
  if (rawVerify !== undefined) verifyDeleteOrder.value = rawVerify === 'true' || rawVerify === true
  allUsers.value = usersRes.data.items
  allRooms.value = roomsRes.data.items
})

watch([currentPage, selectedStatus, searchField], () => {
  fetchOrders()
})
</script>
