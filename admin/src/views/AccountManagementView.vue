<template>
  <div class="p-8 flex flex-col gap-8">
    <!-- Page Header -->
    <div class="flex items-end justify-between">
      <div>
        <h1 class="font-display text-4xl font-extrabold text-on-surface tracking-tight">账户管理</h1>
        <p class="text-on-surface-variant mt-2 font-body">管理用户账户、余额及访问状态。</p>
      </div>
      <button @click="openAddDialog" class="bg-primary text-on-primary px-6 py-3 rounded-full font-headline font-semibold flex items-center gap-2 shadow-sm hover:opacity-90 transition-opacity press-scale">
        <span class="material-symbols-outlined text-xl">add</span>
        新增账户
      </button>
    </div>

    <!-- Filter Bar -->
    <div class="bg-surface-container rounded-lg p-4 flex flex-wrap gap-4 items-center shadow-sm ring-1 ring-outline-variant/15">
      <div class="relative flex-1 min-w-[200px] max-w-sm">
        <span class="material-symbols-outlined absolute left-4 top-1/2 -translate-y-1/2 text-outline">search</span>
        <input
          v-model="searchQuery"
          @input="onSearch"
          class="w-full bg-surface-container-lowest border-none rounded-lg py-3 pl-12 pr-4 text-on-surface placeholder:text-outline focus:ring-2 focus:ring-primary/50 transition-shadow font-body text-sm outline-none"
          placeholder="搜索用户名..."
          type="text"
        />
      </div>
      <div class="relative min-w-[160px]">
        <select
          v-model="selectedStatus"
          class="w-full bg-surface-container-lowest border-none rounded-lg py-3 pl-4 pr-10 text-on-surface focus:ring-2 focus:ring-primary/50 transition-shadow font-body text-sm appearance-none outline-none"
        >
          <option value="">全部状态</option>
          <option value="active">启用</option>
          <option value="disabled">禁用</option>
        </select>
        <span class="material-symbols-outlined absolute right-4 top-1/2 -translate-y-1/2 text-outline pointer-events-none">arrow_drop_down</span>
      </div>
    </div>

    <!-- Data Table -->
    <div class="bg-surface-container-lowest rounded-xl shadow-sm ring-1 ring-outline-variant/15">
      <div class="overflow-x-auto">
        <table class="w-full text-left font-body text-sm">
          <thead class="bg-surface-container-low text-on-surface-variant font-headline uppercase tracking-wider text-xs border-b border-surface-container-highest">
            <tr>
              <th class="py-4 px-6 font-semibold">用户名</th>
              <th class="py-4 px-6 font-semibold">余额</th>
              <th class="py-4 px-6 font-semibold">状态</th>
              <th class="py-4 px-6 font-semibold">创建时间</th>
              <th class="py-4 px-6 font-semibold text-right">操作</th>
            </tr>
          </thead>
          <tbody class="divide-y divide-surface-container-highest">
            <tr
              v-for="user in accounts"
              :key="user.id"
              class="hover:bg-surface-container transition-colors group"
              :class="user.status === 'disabled' ? 'opacity-60' : ''"
            >
              <td class="py-4 px-6">
                <div class="flex items-center gap-3">
                  <img
                    v-if="user.avatarUrl"
                    :src="BACKEND_BASE + user.avatarUrl"
                    class="w-8 h-8 rounded-full object-cover shrink-0 cursor-pointer transition-transform duration-200 hover:scale-[2.5] hover:shadow-lg"
                    :alt="user.username"
                  />
                  <div
                    v-else
                    class="w-8 h-8 rounded-full flex items-center justify-center font-bold text-xs uppercase shrink-0"
                    :class="user.isVip
                      ? 'bg-tertiary-container text-on-tertiary-container'
                      : user.status === 'active'
                        ? 'bg-primary-container text-on-primary-container'
                        : 'bg-surface-variant text-on-surface-variant'"
                  >{{ getAvatarInitial(user) }}</div>
                  <div>
                    <span class="font-medium text-on-surface block">{{ user.username }}</span>
                    <span
                      v-if="user.isVip"
                      class="text-[10px] text-tertiary-container font-semibold uppercase tracking-wider"
                    >高级会员</span>
                  </div>
                </div>
              </td>
              <td class="py-4 px-6 font-medium text-on-surface">{{ formatCurrency(user.balance) }}</td>
              <td class="py-4 px-6">
                <span
                  class="inline-flex items-center gap-1.5 px-2.5 py-1 rounded-full text-xs font-semibold"
                  :class="user.status === 'active'
                    ? 'bg-secondary-container/20 text-secondary-container'
                    : 'bg-surface-variant text-on-surface-variant'"
                >
                  <span
                    class="w-1.5 h-1.5 rounded-full"
                    :class="user.status === 'active' ? 'bg-secondary-container' : 'bg-outline'"
                  ></span>
                  {{ formatUserStatus(user.status) }}
                </span>
              </td>
              <td class="py-4 px-6 text-on-surface-variant">{{ user.createdAt }}</td>
              <td class="py-4 px-6 text-right">
                <div class="flex items-center justify-end gap-2 opacity-0 group-hover:opacity-100 transition-opacity">
                  <button @click="openEditDialog(user)" class="p-2 text-primary hover:bg-primary/10 rounded-full transition-colors" title="编辑">
                    <span class="material-symbols-outlined text-xl">edit</span>
                  </button>
                  <button @click="openRechargeDialog(user)" class="p-2 text-primary hover:bg-primary/10 rounded-full transition-colors" title="调整余额">
                    <span class="material-symbols-outlined text-xl">account_balance_wallet</span>
                  </button>
                  <button
                    class="p-2 rounded-full transition-colors"
                    :class="user.status === 'active'
                      ? 'text-error hover:bg-error/10'
                      : 'text-secondary-container hover:bg-secondary-container/10'"
                    :title="user.status === 'active' ? '禁用' : '启用'"
                    @click="handleToggleStatus(user.id)"
                  >
                    <span class="material-symbols-outlined text-xl">
                      {{ user.status === 'active' ? 'block' : 'check_circle' }}
                    </span>
                  </button>
                </div>
              </td>
            </tr>
          </tbody>
        </table>
      </div>

      <!-- Empty State -->
      <div v-if="accounts.length === 0" class="py-16 text-center text-on-surface-variant">
        <span class="material-symbols-outlined text-4xl text-outline-variant">manage_accounts</span>
        <p class="mt-2">暂无账户数据</p>
      </div>

      <!-- Pagination -->
      <div class="bg-surface-container-lowest px-6 py-4 border-t border-surface-container-highest flex items-center justify-between">
        <div class="text-sm text-on-surface-variant font-body">
          显示第 <span class="font-medium text-on-surface">{{ (currentPage - 1) * pageSize + 1 }}</span>
          到 <span class="font-medium text-on-surface">{{ Math.min(currentPage * pageSize, total) }}</span>
          条，共 <span class="font-medium text-on-surface">{{ total }}</span> 条结果
        </div>
        <div class="flex items-center gap-1">
          <button
            class="p-2 rounded-lg text-outline hover:bg-surface-container transition-colors disabled:opacity-50 disabled:cursor-not-allowed"
            :disabled="currentPage <= 1"
            @click="currentPage--"
          >
            <span class="material-symbols-outlined">chevron_left</span>
          </button>
          <button
            v-for="page in Math.min(totalPages, 3)"
            :key="page"
            class="w-8 h-8 rounded-lg flex items-center justify-center text-sm font-medium transition-colors"
            :class="page === currentPage
              ? 'bg-primary text-on-primary'
              : 'text-on-surface hover:bg-surface-container'"
            @click="currentPage = page"
          >{{ page }}</button>
          <span v-if="totalPages > 4" class="px-2 text-outline">...</span>
          <button
            v-if="totalPages > 3"
            class="w-8 h-8 rounded-lg flex items-center justify-center text-sm font-medium text-on-surface hover:bg-surface-container transition-colors"
            @click="currentPage = totalPages"
          >{{ totalPages }}</button>
          <button
            class="p-2 rounded-lg text-on-surface hover:bg-surface-container transition-colors disabled:opacity-50 disabled:cursor-not-allowed"
            :disabled="currentPage >= totalPages"
            @click="currentPage++"
          >
            <span class="material-symbols-outlined">chevron_right</span>
          </button>
        </div>
      </div>
    </div>

    <!-- Add Account Dialog -->
    <div v-if="showAddDialog" class="fixed inset-0 z-50 flex items-center justify-center bg-black/40" @click.self="showAddDialog = false">
      <div class="bg-surface-container-lowest rounded-2xl shadow-xl w-full max-w-lg p-8 space-y-6">
        <h3 class="text-xl font-display font-bold text-on-surface">新增账户</h3>
        <form @submit.prevent="handleAdd" class="space-y-4">
          <div>
            <label class="block text-sm font-medium text-on-surface-variant mb-1">用户名</label>
            <input v-model="addForm.username" required class="w-full px-4 py-3 bg-surface-container-high rounded-lg border-none text-on-surface focus:ring-2 focus:ring-primary/30 outline-none" />
          </div>
          <div>
            <label class="block text-sm font-medium text-on-surface-variant mb-1">密码</label>
            <input v-model="addForm.password" type="password" required class="w-full px-4 py-3 bg-surface-container-high rounded-lg border-none text-on-surface focus:ring-2 focus:ring-primary/30 outline-none" />
          </div>
          <div>
            <label class="block text-sm font-medium text-on-surface-variant mb-1">显示名称</label>
            <input v-model="addForm.displayName" required class="w-full px-4 py-3 bg-surface-container-high rounded-lg border-none text-on-surface focus:ring-2 focus:ring-primary/30 outline-none" />
          </div>
          <div>
            <label class="block text-sm font-medium text-on-surface-variant mb-1">手机号</label>
            <input v-model="addForm.phone" class="w-full px-4 py-3 bg-surface-container-high rounded-lg border-none text-on-surface focus:ring-2 focus:ring-primary/30 outline-none" />
          </div>
          <div class="flex justify-end gap-3 pt-2">
            <button type="button" @click="showAddDialog = false" class="px-6 py-3 rounded-lg font-medium text-on-surface-variant hover:bg-surface-container transition-colors">取消</button>
            <button type="submit" :disabled="saving" class="px-6 py-3 bg-primary text-on-primary rounded-lg font-semibold hover:bg-primary/90 transition-colors disabled:opacity-60">
              {{ saving ? '创建中...' : '创建' }}
            </button>
          </div>
        </form>
      </div>
    </div>

    <!-- Edit Account Dialog -->
    <div v-if="showEditDialog && editingUser" class="fixed inset-0 z-50 flex items-center justify-center bg-black/40" @click.self="showEditDialog = false">
      <div class="bg-surface-container-lowest rounded-2xl shadow-xl w-full max-w-lg p-8 space-y-6">
        <h3 class="text-xl font-display font-bold text-on-surface">编辑账户</h3>
        <form @submit.prevent="handleEdit" class="space-y-4">
          <!-- Avatar -->
          <div class="flex items-center gap-4">
            <img
              v-if="avatarPreview || editingUser.avatarUrl"
              :src="avatarPreview || (BACKEND_BASE + editingUser.avatarUrl)"
              class="w-16 h-16 rounded-full object-cover ring-2 ring-surface-container-highest"
            />
            <div v-else class="w-16 h-16 rounded-full bg-primary-container text-on-primary-container flex items-center justify-center font-bold text-lg">{{ getAvatarInitial(editingUser) }}</div>
            <label class="px-4 py-2 bg-surface-container-high rounded-lg cursor-pointer hover:bg-surface-container-highest transition-colors text-sm text-on-surface-variant flex items-center gap-2">
              <span class="material-symbols-outlined text-sm">swap_horiz</span>
              更换头像
              <input type="file" accept=".jpg,.jpeg,.png,.gif,.webp" class="hidden" @change="onAvatarSelected" />
            </label>
          </div>
          <div>
            <label class="block text-sm font-medium text-on-surface-variant mb-1">用户名</label>
            <div class="w-full px-4 py-3 bg-surface-container-highest rounded-lg text-on-surface-variant">{{ editingUser.username }}</div>
          </div>
          <div>
            <label class="block text-sm font-medium text-on-surface-variant mb-1">显示名称</label>
            <input v-model="editForm.displayName" class="w-full px-4 py-3 bg-surface-container-high rounded-lg border-none text-on-surface focus:ring-2 focus:ring-primary/30 outline-none" />
          </div>
          <div>
            <label class="block text-sm font-medium text-on-surface-variant mb-1">手机号</label>
            <input v-model="editForm.phone" class="w-full px-4 py-3 bg-surface-container-high rounded-lg border-none text-on-surface focus:ring-2 focus:ring-primary/30 outline-none" />
          </div>
          <div>
            <label class="block text-sm font-medium text-on-surface-variant mb-1">VIP</label>
            <select v-model="editForm.isVip" class="w-full px-4 py-3 bg-surface-container-high rounded-lg border-none text-on-surface focus:ring-2 focus:ring-primary/30 outline-none">
              <option :value="false">普通用户</option>
              <option :value="true">VIP 会员</option>
            </select>
          </div>
          <div class="flex justify-end gap-3 pt-2">
            <button type="button" @click="showEditDialog = false" class="px-6 py-3 rounded-lg font-medium text-on-surface-variant hover:bg-surface-container transition-colors">取消</button>
            <button type="submit" :disabled="saving" class="px-6 py-3 bg-primary text-on-primary rounded-lg font-semibold hover:bg-primary/90 transition-colors disabled:opacity-60">
              {{ saving ? '保存中...' : '保存' }}
            </button>
          </div>
        </form>
      </div>
    </div>

    <!-- Recharge Dialog -->
    <div v-if="showRechargeDialog && rechargingUser" class="fixed inset-0 z-50 flex items-center justify-center bg-black/40" @click.self="showRechargeDialog = false">
      <div class="bg-surface-container-lowest rounded-2xl shadow-xl w-full max-w-lg p-8 space-y-6">
        <h3 class="text-xl font-display font-bold text-on-surface">调整余额</h3>
        <p class="text-sm text-on-surface-variant">用户 <span class="font-medium text-on-surface">{{ rechargingUser.username }}</span>，当前余额 {{ formatCurrency(rechargingUser.balance) }}</p>
        <form @submit.prevent="handleRecharge" class="space-y-4">
          <div>
            <label class="block text-sm font-medium text-on-surface-variant mb-1">调整金额</label>
            <input v-model.number="rechargeAmount" type="number" step="0.01" required class="w-full px-4 py-3 bg-surface-container-high rounded-lg border-none text-on-surface focus:ring-2 focus:ring-primary/30 outline-none" />
            <p class="mt-1.5 text-xs text-on-surface-variant">正数 = 充值，负数 = 扣款</p>
          </div>
          <div class="flex gap-2">
            <button type="button" @click="rechargeAmount = 50" class="px-4 py-2 bg-surface-container-high rounded-lg text-sm hover:bg-surface-container transition-colors">+50</button>
            <button type="button" @click="rechargeAmount = 100" class="px-4 py-2 bg-surface-container-high rounded-lg text-sm hover:bg-surface-container transition-colors">+100</button>
            <button type="button" @click="rechargeAmount = -50" class="px-4 py-2 bg-surface-container-high rounded-lg text-sm hover:bg-surface-container transition-colors">-50</button>
            <button type="button" @click="rechargeAmount = -100" class="px-4 py-2 bg-surface-container-high rounded-lg text-sm hover:bg-surface-container transition-colors">-100</button>
          </div>
          <div class="flex justify-end gap-3 pt-2">
            <button type="button" @click="showRechargeDialog = false" class="px-6 py-3 rounded-lg font-medium text-on-surface-variant hover:bg-surface-container transition-colors">取消</button>
            <button type="submit" :disabled="saving" class="px-6 py-3 bg-primary text-on-primary rounded-lg font-semibold hover:bg-primary/90 transition-colors disabled:opacity-60">
              {{ saving ? '处理中...' : '确认调整' }}
            </button>
          </div>
        </form>
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
import { accountsApi, authApi, settingsApi, uploadApi } from '@/api'
import type { User } from '@/types'
import { formatCurrency, formatUserStatus } from '@/utils/format'

const BACKEND_BASE = import.meta.env.VITE_API_BASE_URL?.replace(/\/api$/, '') || 'https://localhost:5001'

const accounts = ref<User[]>([])
const total = ref(0)
const currentPage = ref(1)
const pageSize = ref(10)
const searchQuery = ref('')
const selectedStatus = ref('')
const saving = ref(false)
let searchTimer: ReturnType<typeof setTimeout>

// Add dialog
const showAddDialog = ref(false)
const addForm = ref({ username: '', password: '', displayName: '', phone: '' })

// Edit dialog
const showEditDialog = ref(false)
const editingUser = ref<User | null>(null)
const editForm = ref({ displayName: '', phone: '', isVip: false })
const originalIsVip = ref(false)
const newAvatarFile = ref<File | null>(null)
const avatarPreview = ref<string | null>(null)

// Recharge dialog
const showRechargeDialog = ref(false)
const rechargingUser = ref<User | null>(null)
const rechargeAmount = ref(100)

// Password verification dialog
const showPasswordDialog = ref(false)
const passwordInput = ref('')
const passwordError = ref('')
const passwordCallback = ref<(() => Promise<void>) | null>(null)

// Verification settings
const verifyBalanceAdjust = ref(true)
const verifyDisableUser = ref(true)
const verifyToggleVip = ref(true)

const totalPages = computed(() => Math.max(1, Math.ceil(total.value / pageSize.value)))

function getAvatarInitial(user: User): string {
  const prefix = user.isVip ? 'V' : user.username.charAt(0).toUpperCase()
  const num = String(user.id).slice(-1)
  return `${prefix}${num}`
}

function onSearch() {
  clearTimeout(searchTimer)
  searchTimer = setTimeout(() => {
    currentPage.value = 1
    fetchAccounts()
  }, 300)
}

async function fetchAccounts() {
  const res = await accountsApi.getList({
    search: searchQuery.value || undefined,
    status: selectedStatus.value || undefined,
    page: currentPage.value,
    pageSize: pageSize.value,
  })
  accounts.value = res.data.items
  total.value = res.data.total
}

async function handleToggleStatus(id: number) {
  const user = accounts.value.find(u => u.id === id)
  if (!user) return

  if (user.status === 'active') {
    // Disabling: check in-progress orders first
    try {
      const preview = await accountsApi.getDisablePreview(id)
      const count = preview.data.inProgressCount
      const msg = count > 0
        ? `该用户有 ${count} 个进行中订单，禁用后将全部取消并退款。确认禁用？`
        : '确认禁用该用户？'
      if (!confirm(msg)) return

      const doDisable = async () => {
        try {
          await accountsApi.disable(id)
          fetchAccounts()
        } catch (err: any) {
          alert(err.response?.data?.message || err.response?.data || '操作失败')
        }
      }
      if (verifyDisableUser.value) {
        openPasswordDialog(doDisable)
      } else {
        await doDisable()
      }
    } catch (err: any) {
      alert(err.response?.data?.message || err.response?.data || '操作失败')
      return
    }
  } else {
    // Enabling: simple toggle
    await accountsApi.toggleStatus(id)
    fetchAccounts()
  }
}

function openAddDialog() {
  addForm.value = { username: '', password: '', displayName: '', phone: '' }
  showAddDialog.value = true
}

async function handleAdd() {
  saving.value = true
  try {
    await accountsApi.create(addForm.value)
    showAddDialog.value = false
    await fetchAccounts()
  } finally {
    saving.value = false
  }
}

function openEditDialog(user: User) {
  editingUser.value = user
  editForm.value = { displayName: user.displayName || '', phone: '', isVip: user.isVip }
  originalIsVip.value = user.isVip
  newAvatarFile.value = null
  avatarPreview.value = null
  showEditDialog.value = true
}

function onAvatarSelected(e: Event) {
  const file = (e.target as HTMLInputElement).files?.[0]
  if (!file) return
  newAvatarFile.value = file
  avatarPreview.value = URL.createObjectURL(file)
}

async function handleEdit() {
  if (!editingUser.value) return
  const vipChanged = editForm.value.isVip !== originalIsVip.value
  if (vipChanged && verifyToggleVip.value) {
    openPasswordDialog(doSaveEdit)
  } else {
    await doSaveEdit()
  }
}

async function doSaveEdit() {
  if (!editingUser.value) return
  saving.value = true
  try {
    const updateData: Record<string, any> = { ...editForm.value }

    // Upload new avatar if selected
    if (newAvatarFile.value) {
      const res = await uploadApi.avatar(newAvatarFile.value)
      updateData.avatarUrl = res.data.url
    }

    await accountsApi.update(editingUser.value.id, updateData as Partial<User>)
    showEditDialog.value = false
    await fetchAccounts()
  } finally {
    saving.value = false
  }
}

function openRechargeDialog(user: User) {
  rechargingUser.value = user
  rechargeAmount.value = 100
  showRechargeDialog.value = true
}

async function handleRecharge() {
  if (!rechargingUser.value) return
  const doRecharge = async () => {
    saving.value = true
    try {
      await accountsApi.recharge(rechargingUser.value!.id, rechargeAmount.value)
      showRechargeDialog.value = false
      await fetchAccounts()
    } finally {
      saving.value = false
    }
  }
  if (verifyBalanceAdjust.value) {
    openPasswordDialog(doRecharge)
  } else {
    await doRecharge()
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
  await fetchAccounts()
  try {
    const settingsRes = await settingsApi.get()
    const sData = settingsRes.data as any
    const rawBalance = sData.verify_balance_adjust ?? sData.verifyBalanceAdjust
    if (rawBalance !== undefined) verifyBalanceAdjust.value = rawBalance === 'true' || rawBalance === true
    const rawDisable = sData.verify_disable_user ?? sData.verifyDisableUser
    if (rawDisable !== undefined) verifyDisableUser.value = rawDisable === 'true' || rawDisable === true
    const rawVip = sData.verify_toggle_vip ?? sData.verifyToggleVip
    if (rawVip !== undefined) verifyToggleVip.value = rawVip === 'true' || rawVip === true
  } catch { /* use defaults */ }
})

watch([currentPage, selectedStatus], () => {
  fetchAccounts()
})
</script>
