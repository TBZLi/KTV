<template>
  <div class="p-8 max-w-5xl mx-auto w-full">
    <!-- Page Header -->
    <div class="flex items-center justify-between mb-8">
      <div>
        <h1 class="text-3xl font-extrabold font-headline text-on-surface tracking-tight">系统设置</h1>
        <p class="text-outline mt-2 text-sm">配置基础门店信息、价格规则与安全偏好。</p>
      </div>
      <button
        class="bg-primary text-on-primary px-6 py-3 rounded-full flex items-center gap-2 hover:bg-secondary transition-colors press-scale shadow-lg shadow-primary/20"
        @click="handleSave"
      >
        <span class="material-symbols-outlined">save</span>
        <span class="font-semibold">保存设置</span>
      </button>
    </div>

    <div class="space-y-8 pb-20">
      <!-- Section 0: Admin Account -->
      <section class="bg-surface-container-lowest rounded-xl p-8 shadow-sm">
        <div class="flex items-center gap-3 mb-6 pb-4 border-b border-surface-variant">
          <span class="material-symbols-outlined text-primary">admin_panel_settings</span>
          <h2 class="text-xl font-bold font-headline text-on-surface">管理员账号</h2>
        </div>
        <div class="space-y-6">
          <div class="flex items-center justify-between p-4 bg-surface rounded-lg">
            <div>
              <h3 class="font-semibold text-on-surface">管理员用户名</h3>
              <p class="text-sm text-outline mt-1">{{ adminUsername || '加载中...' }}</p>
            </div>
            <button @click="openChangeUsernameDialog" class="px-4 py-2 bg-surface-container-high rounded-lg text-sm font-medium text-primary hover:bg-primary/10 transition-colors">
              修改用户名
            </button>
          </div>
          <div class="flex items-center justify-between p-4 bg-surface rounded-lg">
            <div>
              <h3 class="font-semibold text-on-surface">管理员密码</h3>
              <p class="text-sm text-outline mt-1">定期更新密码以保证系统安全</p>
            </div>
            <button @click="openChangePasswordDialog" class="px-4 py-2 bg-surface-container-high rounded-lg text-sm font-medium text-primary hover:bg-primary/10 transition-colors">
              修改密码
            </button>
          </div>
        </div>
      </section>

      <!-- Section 1: Basic Info -->
      <section class="bg-surface-container-lowest rounded-xl p-8 shadow-sm">
        <div class="flex items-center gap-3 mb-6 pb-4 border-b border-surface-variant">
          <span class="material-symbols-outlined text-primary">storefront</span>
          <h2 class="text-xl font-bold font-headline text-on-surface">基本信息</h2>
        </div>
        <div class="grid grid-cols-1 md:grid-cols-2 gap-8">
          <div class="space-y-2">
            <label class="block text-sm font-semibold text-on-surface-variant">门店名称</label>
            <input
              v-model="settings.storeName"
              class="w-full bg-surface-container-high text-on-surface placeholder-outline border-none rounded-lg focus:ring-2 focus:ring-primary h-12 px-4 transition-all"
              placeholder="声域友 KTV (旗舰店)"
              type="text"
            />
          </div>
          <div class="space-y-2">
            <label class="block text-sm font-semibold text-on-surface-variant">联系电话</label>
            <input
              v-model="settings.storePhone"
              class="w-full bg-surface-container-high text-on-surface placeholder-outline border-none rounded-lg focus:ring-2 focus:ring-primary h-12 px-4 transition-all"
              placeholder="联系电话..."
              type="text"
            />
          </div>
          <div class="space-y-2 md:col-span-2">
            <label class="block text-sm font-semibold text-on-surface-variant">门店地址</label>
            <textarea
              v-model="settings.storeAddress"
              class="w-full bg-surface-container-high text-on-surface placeholder-outline border-none rounded-lg focus:ring-2 focus:ring-primary p-4 min-h-[100px] resize-none transition-all"
              placeholder="输入门店详细地址..."
            ></textarea>
          </div>
          <div class="space-y-2">
            <label class="block text-sm font-semibold text-on-surface-variant">营业时间</label>
            <div class="relative">
              <input
                v-model="settings.businessHours"
                class="w-full bg-surface-container-high text-on-surface placeholder-outline border-none rounded-lg focus:ring-2 focus:ring-primary h-12 pl-10 px-4 transition-all"
                placeholder="14:00 - 02:00"
                type="text"
              />
              <span class="material-symbols-outlined absolute left-3 top-1/2 -translate-y-1/2 text-outline text-lg">schedule</span>
            </div>
          </div>
        </div>
      </section>

      <!-- Section 2: Pricing -->
      <section class="bg-surface-container-lowest rounded-xl p-8 shadow-sm">
        <div class="flex items-center gap-3 mb-6 pb-4 border-b border-surface-variant">
          <span class="material-symbols-outlined text-primary">payments</span>
          <h2 class="text-xl font-bold font-headline text-on-surface">定价设置</h2>
        </div>
        <div class="space-y-8">
          <div class="space-y-2">
            <label class="block text-sm font-semibold text-on-surface-variant">默认包厢单价基数 (¥/小时)</label>
            <div class="relative max-w-md">
              <input
                v-model.number="settings.baseHourlyRate"
                class="w-full bg-surface-container-high text-on-surface placeholder-outline border-none rounded-lg focus:ring-2 focus:ring-primary h-12 pl-12 pr-4 transition-all"
                placeholder="120"
                type="number"
              />
              <span class="absolute left-4 top-1/2 -translate-y-1/2 text-outline font-semibold">¥</span>
            </div>
          </div>

          <div class="space-y-2">
            <label class="block text-sm font-semibold text-on-surface-variant">包厢类型倍率</label>
            <p class="text-xs text-outline mb-3">实际单价 = 单价基数 × 包厢类型倍率（如 VIP 1.5x = 180元/小时）</p>
            <div class="grid grid-cols-3 gap-4 max-w-2xl">
              <div>
                <label class="block text-xs text-on-surface-variant mb-1">VIP 包厢</label>
                <div class="relative">
                  <input
                    v-model.number="settings.roomTypeMultiplierVip"
                    class="w-full bg-surface-container-high text-on-surface placeholder-outline border-none rounded-lg focus:ring-2 focus:ring-primary h-12 pr-10 text-center transition-all"
                    placeholder="1.5"
                    type="number"
                    step="0.1"
                    min="0.1"
                  />
                  <span class="absolute right-3 top-1/2 -translate-y-1/2 text-outline text-sm">x</span>
                </div>
              </div>
              <div>
                <label class="block text-xs text-on-surface-variant mb-1">中包厢</label>
                <div class="relative">
                  <input
                    v-model.number="settings.roomTypeMultiplierMedium"
                    class="w-full bg-surface-container-high text-on-surface placeholder-outline border-none rounded-lg focus:ring-2 focus:ring-primary h-12 pr-10 text-center transition-all"
                    placeholder="1.3"
                    type="number"
                    step="0.1"
                    min="0.1"
                  />
                  <span class="absolute right-3 top-1/2 -translate-y-1/2 text-outline text-sm">x</span>
                </div>
              </div>
              <div>
                <label class="block text-xs text-on-surface-variant mb-1">小包厢</label>
                <div class="relative">
                  <input
                    v-model.number="settings.roomTypeMultiplierSmall"
                    class="w-full bg-surface-container-high text-on-surface placeholder-outline border-none rounded-lg focus:ring-2 focus:ring-primary h-12 pr-10 text-center transition-all"
                    placeholder="1.0"
                    type="number"
                    step="0.1"
                    min="0.1"
                  />
                  <span class="absolute right-3 top-1/2 -translate-y-1/2 text-outline text-sm">x</span>
                </div>
              </div>
            </div>
          </div>

          <div class="flex items-center justify-between p-4 bg-surface rounded-lg">
            <div>
              <h3 class="font-semibold text-on-surface">开启节假日动态定价</h3>
              <p class="text-sm text-outline mt-1">在节假日自动应用溢价规则，按包厢类型设置不同倍率</p>
            </div>
            <label class="relative inline-flex items-center cursor-pointer">
              <input v-model="settings.holidayPricingEnabled" class="sr-only peer" type="checkbox" />
              <div class="w-14 h-7 bg-surface-container-highest peer-focus:outline-none rounded-full peer peer-checked:after:translate-x-full peer-checked:after:border-white after:content-[''] after:absolute after:top-[2px] after:left-[2px] after:bg-white after:border-gray-300 after:border after:rounded-full after:h-6 after:w-6 after:transition-all peer-checked:bg-primary"></div>
            </label>
          </div>

          <!-- Holiday Date Management -->
          <div v-if="settings.holidayPricingEnabled" class="space-y-4">
            <div class="flex items-center justify-between">
              <h3 class="font-semibold text-on-surface">节假日日期管理</h3>
              <button @click="showAddHolidayDialog = true" class="px-4 py-2 bg-primary text-on-primary rounded-lg text-sm font-medium hover:bg-secondary transition-colors press-scale flex items-center gap-1">
                <span class="material-symbols-outlined text-base">add</span>
                新增节假日
              </button>
            </div>

            <!-- Holiday list -->
            <div v-if="holidays.length === 0" class="text-center py-8 text-outline text-sm">
              暂无节假日配置，点击上方按钮添加
            </div>
            <div v-else class="space-y-3">
              <div v-for="h in holidays" :key="h.id" class="flex items-center justify-between p-4 bg-surface rounded-lg">
                <div class="flex items-center gap-4">
                  <span class="material-symbols-outlined text-primary/60">event</span>
                  <div>
                    <div class="font-medium text-on-surface">{{ formatDate(h.startDate) }} ~ {{ formatDate(h.endDate) }}</div>
                    <div class="text-xs text-outline mt-1">
                      VIP <span class="text-primary font-semibold">{{ h.vipMultiplier }}x</span>
                      &nbsp;·&nbsp; 中包 <span class="text-primary font-semibold">{{ h.mediumMultiplier }}x</span>
                      &nbsp;·&nbsp; 小包 <span class="text-primary font-semibold">{{ h.smallMultiplier }}x</span>
                    </div>
                  </div>
                </div>
                <button @click="handleDeleteHoliday(h.id)" class="p-2 text-error/60 hover:text-error hover:bg-error/10 rounded-lg transition-colors">
                  <span class="material-symbols-outlined text-lg">delete</span>
                </button>
              </div>
            </div>
          </div>
        </div>
      </section>

      <!-- Section 3: Security -->
      <section class="bg-surface-container-lowest rounded-xl p-8 shadow-sm">
        <div class="flex items-center gap-3 mb-6 pb-4 border-b border-surface-variant">
          <span class="material-symbols-outlined text-primary">security</span>
          <h2 class="text-xl font-bold font-headline text-on-surface">安全设置</h2>
        </div>
        <div class="space-y-8">
          <div class="space-y-2 max-w-md">
            <label class="block text-sm font-semibold text-on-surface-variant">操作日志保留期限</label>
            <div class="relative">
              <select
                v-model="settings.logRetentionDays"
                class="w-full bg-surface-container-high text-on-surface border-none rounded-lg focus:ring-2 focus:ring-primary h-12 px-4 appearance-none cursor-pointer"
              >
                <option :value="30">30 天</option>
                <option :value="90">90 天</option>
                <option :value="180">180 天</option>
                <option :value="0">永久保留</option>
              </select>
              <span class="material-symbols-outlined absolute right-3 top-1/2 -translate-y-1/2 text-outline pointer-events-none">expand_more</span>
            </div>
          </div>
          <div class="space-y-3">
            <h3 class="font-semibold text-on-surface">敏感操作二次验证</h3>
            <p class="text-sm text-outline">勾选后对应操作需输入管理员密码二次确认</p>
            <div class="grid grid-cols-1 md:grid-cols-2 gap-3 mt-2">
              <label v-for="item in verificationItems" :key="item.key" class="flex items-center gap-3 p-3 bg-surface rounded-lg cursor-pointer hover:bg-surface-container transition-colors">
                <input type="checkbox" v-model="(settings as any)[item.key]" class="w-4 h-4 rounded text-primary focus:ring-primary/30" />
                <span class="text-sm font-medium text-on-surface">{{ item.label }}</span>
              </label>
            </div>
          </div>
        </div>
      </section>

      <!-- Section 4: Operation Logs -->
      <section class="bg-surface-container-lowest rounded-xl p-8 shadow-sm">
        <div class="flex items-center gap-3 mb-6 pb-4 border-b border-surface-variant">
          <span class="material-symbols-outlined text-primary">history</span>
          <h2 class="text-xl font-bold font-headline text-on-surface">操作日志</h2>
        </div>
        <div class="space-y-4">
          <!-- Filters -->
          <div class="flex flex-wrap gap-3 items-center">
            <div class="relative min-w-[160px]">
              <select
                v-model="logFilters.operationType"
                @change="loadLogs(1)"
                class="w-full bg-surface-container-high text-on-surface border-none rounded-lg focus:ring-2 focus:ring-primary h-10 px-4 pr-8 appearance-none cursor-pointer text-sm"
              >
                <option value="">全部类型</option>
                <option value="create">创建</option>
                <option value="update">修改</option>
                <option value="delete">删除</option>
                <option value="balance_adjust">余额调整</option>
                <option value="disable">禁用用户</option>
                <option value="vip_change">切换 VIP</option>
                <option value="refund">退款</option>
                <option value="cancel">取消</option>
                <option value="complete">完成</option>
                <option value="restore">恢复</option>
                <option value="login">登录</option>
              </select>
              <span class="material-symbols-outlined absolute right-2 top-1/2 -translate-y-1/2 text-outline pointer-events-none text-base">expand_more</span>
            </div>
            <input
              v-model="logFilters.username"
              @input="debounceLogSearch"
              placeholder="搜索操作人..."
              class="bg-surface-container-high text-on-surface placeholder-outline border-none rounded-lg focus:ring-2 focus:ring-primary h-10 px-4 text-sm w-40"
            />
            <input
              v-model="logFilters.fromDate"
              @change="loadLogs(1)"
              type="date"
              class="bg-surface-container-high text-on-surface border-none rounded-lg focus:ring-2 focus:ring-primary h-10 px-3 text-sm"
            />
            <span class="text-outline text-sm">至</span>
            <input
              v-model="logFilters.toDate"
              @change="loadLogs(1)"
              type="date"
              class="bg-surface-container-high text-on-surface border-none rounded-lg focus:ring-2 focus:ring-primary h-10 px-3 text-sm"
            />
          </div>

          <!-- Log table -->
          <div class="overflow-x-auto">
            <table class="w-full text-sm">
              <thead>
                <tr class="text-left text-on-surface-variant border-b border-surface-variant">
                  <th class="py-3 px-2 font-medium">时间</th>
                  <th class="py-3 px-2 font-medium">操作人</th>
                  <th class="py-3 px-2 font-medium">操作类型</th>
                  <th class="py-3 px-2 font-medium">操作对象</th>
                  <th class="py-3 px-2 font-medium">详情</th>
                </tr>
              </thead>
              <tbody>
                <tr v-if="logsLoading">
                  <td colspan="5" class="py-8 text-center text-outline">加载中...</td>
                </tr>
                <tr v-else-if="logs.length === 0">
                  <td colspan="5" class="py-8 text-center text-outline">暂无操作记录</td>
                </tr>
                <tr v-for="log in logs" :key="log.id" class="border-b border-surface-variant/50 hover:bg-surface-container-lowest/50 transition-colors">
                  <td class="py-3 px-2 whitespace-nowrap text-on-surface-variant">{{ log.createdAt }}</td>
                  <td class="py-3 px-2 font-medium text-on-surface">{{ log.username }}</td>
                  <td class="py-3 px-2">
                    <span class="px-2 py-1 rounded-full text-xs font-medium" :class="getLogTypeClass(log.operationType)">
                      {{ getLogTypeLabel(log.operationType) }}
                    </span>
                  </td>
                  <td class="py-3 px-2 text-on-surface-variant">{{ log.objectId || '-' }}</td>
                  <td class="py-3 px-2 text-on-surface-variant max-w-[200px] truncate" :title="log.details || ''">{{ log.details || '-' }}</td>
                </tr>
              </tbody>
            </table>
          </div>

          <!-- Pagination -->
          <div v-if="logTotal > logPageSize" class="flex items-center justify-between pt-2">
            <span class="text-xs text-outline">共 {{ logTotal }} 条记录</span>
            <div class="flex items-center gap-1">
              <button
                @click="loadLogs(logPage - 1)"
                :disabled="logPage <= 1"
                class="px-3 py-1.5 rounded-lg text-sm bg-surface-container-high text-on-surface disabled:opacity-30 hover:bg-surface-container-highest transition-colors"
              >
                上一页
              </button>
              <span class="text-sm text-on-surface-variant px-2">{{ logPage }} / {{ Math.ceil(logTotal / logPageSize) }}</span>
              <button
                @click="loadLogs(logPage + 1)"
                :disabled="logPage >= Math.ceil(logTotal / logPageSize)"
                class="px-3 py-1.5 rounded-lg text-sm bg-surface-container-high text-on-surface disabled:opacity-30 hover:bg-surface-container-highest transition-colors"
              >
                下一页
              </button>
            </div>
          </div>
        </div>
      </section>
    </div>

    <!-- Change Admin Username Dialog -->
    <div v-if="showUsernameDialog" class="fixed inset-0 z-50 flex items-center justify-center bg-black/40" @click.self="showUsernameDialog = false">
      <div class="bg-surface-container-lowest rounded-2xl shadow-xl w-full max-w-lg p-8 space-y-6">
        <h3 class="text-xl font-display font-bold text-on-surface">修改管理员用户名</h3>
        <form @submit.prevent="handleChangeUsername" class="space-y-4">
          <div>
            <label class="block text-sm font-medium text-on-surface-variant mb-1">新用户名</label>
            <input v-model="adminForm.newUsername" required class="w-full px-4 py-3 bg-surface-container-high rounded-lg border-none text-on-surface focus:ring-2 focus:ring-primary/30 outline-none" />
          </div>
          <div>
            <label class="block text-sm font-medium text-on-surface-variant mb-1">当前密码（验证身份）</label>
            <input v-model="adminForm.password" type="password" required class="w-full px-4 py-3 bg-surface-container-high rounded-lg border-none text-on-surface focus:ring-2 focus:ring-primary/30 outline-none" />
          </div>
          <p v-if="adminFormError" class="text-xs text-error font-semibold">{{ adminFormError }}</p>
          <div class="flex justify-end gap-3 pt-2">
            <button type="button" @click="showUsernameDialog = false" class="px-6 py-3 rounded-lg font-medium text-on-surface-variant hover:bg-surface-container transition-colors">取消</button>
            <button type="submit" :disabled="adminSaving" class="px-6 py-3 bg-primary text-on-primary rounded-lg font-semibold hover:bg-primary/90 transition-colors disabled:opacity-60">
              {{ adminSaving ? '修改中...' : '确认修改' }}
            </button>
          </div>
        </form>
      </div>
    </div>

    <!-- Change Admin Password Dialog -->
    <div v-if="showPasswordDialog" class="fixed inset-0 z-50 flex items-center justify-center bg-black/40" @click.self="showPasswordDialog = false">
      <div class="bg-surface-container-lowest rounded-2xl shadow-xl w-full max-w-lg p-8 space-y-6">
        <h3 class="text-xl font-display font-bold text-on-surface">修改管理员密码</h3>
        <form @submit.prevent="handleChangePassword" class="space-y-4">
          <div>
            <label class="block text-sm font-medium text-on-surface-variant mb-1">当前密码</label>
            <input v-model="passwordForm.currentPassword" type="password" required class="w-full px-4 py-3 bg-surface-container-high rounded-lg border-none text-on-surface focus:ring-2 focus:ring-primary/30 outline-none" />
          </div>
          <div>
            <label class="block text-sm font-medium text-on-surface-variant mb-1">新密码</label>
            <input v-model="passwordForm.newPassword" type="password" required class="w-full px-4 py-3 bg-surface-container-high rounded-lg border-none text-on-surface focus:ring-2 focus:ring-primary/30 outline-none" />
          </div>
          <div>
            <label class="block text-sm font-medium text-on-surface-variant mb-1">确认新密码</label>
            <input v-model="passwordForm.confirmPassword" type="password" required class="w-full px-4 py-3 bg-surface-container-high rounded-lg border-none text-on-surface focus:ring-2 focus:ring-primary/30 outline-none" />
          </div>
          <p v-if="adminFormError" class="text-xs text-error font-semibold">{{ adminFormError }}</p>
          <div class="flex justify-end gap-3 pt-2">
            <button type="button" @click="showPasswordDialog = false" class="px-6 py-3 rounded-lg font-medium text-on-surface-variant hover:bg-surface-container transition-colors">取消</button>
            <button type="submit" :disabled="adminSaving" class="px-6 py-3 bg-primary text-on-primary rounded-lg font-semibold hover:bg-primary/90 transition-colors disabled:opacity-60">
              {{ adminSaving ? '修改中...' : '确认修改' }}
            </button>
          </div>
        </form>
      </div>
    </div>

    <!-- Add Holiday Dialog -->
    <div v-if="showAddHolidayDialog" class="fixed inset-0 z-50 flex items-center justify-center bg-black/40" @click.self="showAddHolidayDialog = false">
      <div class="bg-surface-container-lowest rounded-2xl shadow-xl w-full max-w-lg p-8 space-y-6">
        <h3 class="text-xl font-display font-bold text-on-surface">新增节假日</h3>
        <form @submit.prevent="handleAddHoliday" class="space-y-4">
          <div class="grid grid-cols-2 gap-4">
            <div>
              <label class="block text-sm font-medium text-on-surface-variant mb-1">开始日期</label>
              <input v-model="holidayForm.startDate" type="date" required class="w-full px-4 py-3 bg-surface-container-high rounded-lg border-none text-on-surface focus:ring-2 focus:ring-primary/30 outline-none" />
            </div>
            <div>
              <label class="block text-sm font-medium text-on-surface-variant mb-1">结束日期</label>
              <input v-model="holidayForm.endDate" type="date" required class="w-full px-4 py-3 bg-surface-container-high rounded-lg border-none text-on-surface focus:ring-2 focus:ring-primary/30 outline-none" />
            </div>
          </div>
          <div class="grid grid-cols-3 gap-4">
            <div>
              <label class="block text-sm font-medium text-on-surface-variant mb-1">VIP 倍率</label>
              <input v-model.number="holidayForm.vipMultiplier" type="number" step="0.1" min="0.1" required class="w-full px-4 py-3 bg-surface-container-high rounded-lg border-none text-on-surface focus:ring-2 focus:ring-primary/30 outline-none" />
            </div>
            <div>
              <label class="block text-sm font-medium text-on-surface-variant mb-1">中包倍率</label>
              <input v-model.number="holidayForm.mediumMultiplier" type="number" step="0.1" min="0.1" required class="w-full px-4 py-3 bg-surface-container-high rounded-lg border-none text-on-surface focus:ring-2 focus:ring-primary/30 outline-none" />
            </div>
            <div>
              <label class="block text-sm font-medium text-on-surface-variant mb-1">小包倍率</label>
              <input v-model.number="holidayForm.smallMultiplier" type="number" step="0.1" min="0.1" required class="w-full px-4 py-3 bg-surface-container-high rounded-lg border-none text-on-surface focus:ring-2 focus:ring-primary/30 outline-none" />
            </div>
          </div>
          <p v-if="holidayFormError" class="text-xs text-error font-semibold">{{ holidayFormError }}</p>
          <div class="flex justify-end gap-3 pt-2">
            <button type="button" @click="showAddHolidayDialog = false" class="px-6 py-3 rounded-lg font-medium text-on-surface-variant hover:bg-surface-container transition-colors">取消</button>
            <button type="submit" :disabled="holidaySaving" class="px-6 py-3 bg-primary text-on-primary rounded-lg font-semibold hover:bg-primary/90 transition-colors disabled:opacity-60">
              {{ holidaySaving ? '添加中...' : '确认添加' }}
            </button>
          </div>
        </form>
      </div>
    </div>

    <!-- Verification Dialog -->
    <div v-if="showVerifyDialog" class="fixed inset-0 z-[60] flex items-center justify-center bg-black/40" @click.self="showVerifyDialog = false">
      <div class="bg-surface-container-lowest rounded-2xl shadow-xl w-full max-w-sm p-8 space-y-4">
        <h3 class="text-lg font-display font-bold text-on-surface">二次验证</h3>
        <p class="text-sm text-on-surface-variant">请输入管理员密码确认操作</p>
        <form @submit.prevent="submitVerify" class="space-y-4">
          <input v-model="verifyInput" type="password" required autofocus placeholder="输入密码" class="w-full px-4 py-3 bg-surface-container-high rounded-lg border-none text-on-surface focus:ring-2 focus:ring-primary/30 outline-none" />
          <p v-if="verifyError" class="text-xs text-error font-semibold">{{ verifyError }}</p>
          <div class="flex justify-end gap-3">
            <button type="button" @click="showVerifyDialog = false" class="px-6 py-3 rounded-lg font-medium text-on-surface-variant hover:bg-surface-container transition-colors">取消</button>
            <button type="submit" class="px-6 py-3 bg-primary text-on-primary rounded-lg font-semibold hover:bg-primary/90 transition-colors">确认</button>
          </div>
        </form>
      </div>
    </div>
  </div>
</template>

<script setup lang="ts">
import { ref, onMounted } from 'vue'
import { ElMessage, ElMessageBox } from 'element-plus'
import { settingsApi, holidaysApi, operationLogsApi, authApi } from '@/api'
import type { SystemSettings, Holiday, OperationLog } from '@/types'

const settings = ref<SystemSettings>({
  storeName: '',
  storePhone: '',
  storeAddress: '',
  businessHours: '',
  holidayPricingEnabled: false,
  baseHourlyRate: 0,
  roomTypeMultiplierVip: 1.5,
  roomTypeMultiplierMedium: 1.3,
  roomTypeMultiplierSmall: 1.0,
  logRetentionDays: 30,
  sensitiveOpVerification: false,
  verifyDeleteOrder: true,
  verifyBalanceAdjust: true,
  verifyDisableUser: true,
  verifyBatchSongStatus: true,
  verifyModifySettings: true,
  verifyModifyAdmin: true,
})

// Admin account
const adminUsername = ref('')
const showUsernameDialog = ref(false)
const showPasswordDialog = ref(false)
const adminSaving = ref(false)
const adminFormError = ref('')
const adminForm = ref({ newUsername: '', password: '' })
const passwordForm = ref({ currentPassword: '', newPassword: '', confirmPassword: '' })

// Holidays
const holidays = ref<Holiday[]>([])
const showAddHolidayDialog = ref(false)
const holidaySaving = ref(false)
const holidayFormError = ref('')
const holidayForm = ref({ startDate: '', endDate: '', vipMultiplier: 1.5, mediumMultiplier: 1.3, smallMultiplier: 1.2 })

// Verification items config
const verificationItems = [
  { key: 'verifyDeleteOrder', label: '删除订单' },
  { key: 'verifyBalanceAdjust', label: '余额调整' },
  { key: 'verifyDisableUser', label: '禁用/启用用户' },
  { key: 'verifyToggleVip', label: '切换用户 VIP 状态' },
  { key: 'verifyBatchSongStatus', label: '批量上下架歌曲' },
  { key: 'verifyModifySettings', label: '修改系统设置' },
  { key: 'verifyModifyAdmin', label: '修改管理员账号' },
]

// Verification settings (loaded from API, not from local settings)
const loadedVerifyModifySettings = ref(true)
const loadedVerifyModifyAdmin = ref(true)

// Operation Logs
const logs = ref<OperationLog[]>([])
const logsLoading = ref(false)
const logPage = ref(1)
const logTotal = ref(0)
const logPageSize = 20
const logFilters = ref({ operationType: '', username: '', fromDate: '', toDate: '' })
let logSearchTimer: ReturnType<typeof setTimeout> | null = null

// Password verification dialog (separate from admin password change dialog)
const showVerifyDialog = ref(false)
const verifyInput = ref('')
const verifyError = ref('')
const verifyCallback = ref<(() => Promise<void>) | null>(null)

// --- Admin Account ---
async function fetchAdminAccount() {
  try {
    const res = await settingsApi.getAdminAccount()
    adminUsername.value = res.data.username
  } catch {
    adminUsername.value = '加载失败'
  }
}

function openChangeUsernameDialog() {
  adminForm.value = { newUsername: adminUsername.value, password: '' }
  adminFormError.value = ''
  showUsernameDialog.value = true
}

function openChangePasswordDialog() {
  passwordForm.value = { currentPassword: '', newPassword: '', confirmPassword: '' }
  adminFormError.value = ''
  showPasswordDialog.value = true
}

// --- Holidays ---
async function loadHolidays() {
  try {
    const res = await holidaysApi.getAll()
    holidays.value = res.data
  } catch { /* ignore */ }
}

async function handleAddHoliday() {
  holidayFormError.value = ''
  if (holidayForm.value.startDate > holidayForm.value.endDate) {
    holidayFormError.value = '开始日期不能晚于结束日期'
    return
  }
  holidaySaving.value = true
  try {
    await holidaysApi.create(holidayForm.value)
    showAddHolidayDialog.value = false
    ElMessage.success('节假日已添加')
    holidayForm.value = { startDate: '', endDate: '', vipMultiplier: 1.5, mediumMultiplier: 1.3, smallMultiplier: 1.2 }
    await loadHolidays()
  } catch (err: any) {
    holidayFormError.value = err.response?.data?.message || err.response?.data || '添加失败'
  } finally {
    holidaySaving.value = false
  }
}

async function handleDeleteHoliday(id: number) {
  try {
    await ElMessageBox.confirm('确认删除该节假日配置？', '删除确认', { type: 'warning', confirmButtonText: '确认删除', cancelButtonText: '取消' })
    await holidaysApi.delete(id)
    ElMessage.success('节假日已删除')
    await loadHolidays()
  } catch { /* cancelled */ }
}

function formatDate(dateStr: string) {
  return dateStr.split('T')[0] || dateStr
}

// --- Operation Logs ---
async function loadLogs(page = 1) {
  logsLoading.value = true
  logPage.value = page
  try {
    const params: any = { page, pageSize: logPageSize }
    if (logFilters.value.operationType) params.operationType = logFilters.value.operationType
    if (logFilters.value.username) params.username = logFilters.value.username
    if (logFilters.value.fromDate) params.fromDate = logFilters.value.fromDate
    if (logFilters.value.toDate) params.toDate = logFilters.value.toDate
    const res = await operationLogsApi.getList(params)
    logs.value = res.data.items
    logTotal.value = res.data.total
  } catch { /* ignore */ }
  logsLoading.value = false
}

function debounceLogSearch() {
  if (logSearchTimer) clearTimeout(logSearchTimer)
  logSearchTimer = setTimeout(() => loadLogs(1), 300)
}

function getLogTypeLabel(type: string) {
  const map: Record<string, string> = {
    create: '创建', update: '修改', delete: '删除', balance_adjust: '余额调整',
    disable: '禁用', refund: '退款', cancel: '取消', complete: '完成',
    restore: '恢复', login: '登录', toggle_status: '状态切换',
    update_status: '状态更新', end_session: '结束会话',
    change_username: '改用户名', change_password: '改密码',
    vip_change: '切换 VIP',
  }
  return map[type] || type
}

function getLogTypeClass(type: string) {
  if (['delete', 'disable', 'cancel'].includes(type)) return 'bg-error/10 text-error'
  if (['create', 'restore'].includes(type)) return 'bg-primary/10 text-primary'
  if (['balance_adjust', 'change_password', 'change_username'].includes(type)) return 'bg-warning/10 text-warning'
  if (type === 'login') return 'bg-outline/10 text-outline'
  return 'bg-surface-container-high text-on-surface-variant'
}

// --- Save & Init ---
async function handleSave() {
  const doSave = async () => {
    // Convert camelCase keys to snake_case for the API (DB stores snake_case)
    const camelToSnake: Record<string, string> = {
      storeName: 'store_name', storePhone: 'store_phone', storeAddress: 'store_address',
      businessHours: 'business_hours', holidayPricingEnabled: 'holiday_pricing_enabled',
      baseHourlyRate: 'base_hourly_rate',
      roomTypeMultiplierVip: 'room_type_multiplier_vip',
      roomTypeMultiplierMedium: 'room_type_multiplier_medium',
      roomTypeMultiplierSmall: 'room_type_multiplier_small', logRetentionDays: 'log_retention_days',
      sensitiveOpVerification: 'sensitive_op_verification',
      verifyDeleteOrder: 'verify_delete_order', verifyBalanceAdjust: 'verify_balance_adjust',
      verifyDisableUser: 'verify_disable_user', verifyBatchSongStatus: 'verify_batch_song_status',
      verifyModifySettings: 'verify_modify_settings', verifyModifyAdmin: 'verify_modify_admin',
    }
    const payload: Record<string, any> = {}
    for (const [key, value] of Object.entries(settings.value)) {
      const dbKey = camelToSnake[key] || key
      payload[dbKey] = typeof value === 'boolean' || typeof value === 'number' ? String(value) : value
    }
    console.log('[Settings] Saving payload:', payload)
    try {
      const res = await settingsApi.update(payload)
      console.log('[Settings] Save response:', res.status, res.data)
      ElMessage.success('设置已保存')
    } catch (err: any) {
      const msg = err?.response?.data?.message || '保存失败，请重试'
      console.error('[Settings] Save failed:', err?.response?.status, msg)
      ElMessage.error(msg)
      throw err
    }
  }
  if (loadedVerifyModifySettings.value) {
    openVerifyDialog(doSave)
  } else {
    await doSave()
  }
}

// Wrap admin changes with verification
async function handleChangeUsername() {
  const doChange = async () => {
    adminFormError.value = ''
    adminSaving.value = true
    try {
      await settingsApi.updateAdminUsername(adminForm.value)
      showUsernameDialog.value = false
      ElMessage.success('用户名已修改')
      await fetchAdminAccount()
    } catch (err: any) {
      adminFormError.value = err.response?.data?.message || err.response?.data || '修改失败'
    } finally {
      adminSaving.value = false
    }
  }
  if (loadedVerifyModifyAdmin.value) {
    // First validate the form fields
    if (!adminForm.value.newUsername || !adminForm.value.password) {
      adminFormError.value = '请填写所有字段'
      return
    }
    showUsernameDialog.value = false
    openVerifyDialog(doChange)
  } else {
    await doChange()
  }
}

async function handleChangePassword() {
  const doChange = async () => {
    adminFormError.value = ''
    if (passwordForm.value.newPassword !== passwordForm.value.confirmPassword) {
      adminFormError.value = '两次输入的密码不一致'
      return
    }
    adminSaving.value = true
    try {
      await settingsApi.updateAdminPassword(passwordForm.value)
      showPasswordDialog.value = false
      ElMessage.success('密码已修改')
    } catch (err: any) {
      adminFormError.value = err.response?.data?.message || err.response?.data || '修改失败'
    } finally {
      adminSaving.value = false
    }
  }
  if (loadedVerifyModifyAdmin.value) {
    // First validate the form fields
    if (!passwordForm.value.currentPassword || !passwordForm.value.newPassword || !passwordForm.value.confirmPassword) {
      adminFormError.value = '请填写所有字段'
      return
    }
    showPasswordDialog.value = false
    openVerifyDialog(doChange)
  } else {
    await doChange()
  }
}

function openVerifyDialog(callback: () => Promise<void>) {
  verifyInput.value = ''
  verifyError.value = ''
  verifyCallback.value = callback
  showVerifyDialog.value = true
}

async function submitVerify() {
  verifyError.value = ''
  try {
    await authApi.verifyPassword(verifyInput.value)
  } catch {
    verifyError.value = '密码错误'
    return
  }
  showVerifyDialog.value = false
  if (verifyCallback.value) await verifyCallback.value()
}

onMounted(async () => {
  try {
    const res = await settingsApi.get()
    // API returns snake_case keys from DB, map to camelCase for the frontend
    const data = res.data as any
    console.log('[Settings] Loaded from API:', data)
    const snakeToCamel: Record<string, string> = {
      store_name: 'storeName', store_phone: 'storePhone', store_address: 'storeAddress',
      business_hours: 'businessHours', holiday_pricing_enabled: 'holidayPricingEnabled',
      base_hourly_rate: 'baseHourlyRate', log_retention_days: 'logRetentionDays',
      room_type_multiplier_vip: 'roomTypeMultiplierVip',
      room_type_multiplier_medium: 'roomTypeMultiplierMedium',
      room_type_multiplier_small: 'roomTypeMultiplierSmall',
      verify_delete_order: 'verifyDeleteOrder', verify_balance_adjust: 'verifyBalanceAdjust',
      verify_disable_user: 'verifyDisableUser', verify_batch_song_status: 'verifyBatchSongStatus',
      verify_modify_settings: 'verifyModifySettings', verify_modify_admin: 'verifyModifyAdmin',
    }
    for (const [snake, camel] of Object.entries(snakeToCamel)) {
      if (data[snake] !== undefined) {
        let val: any = data[snake]
        if (typeof val === 'string' && (val === 'true' || val === 'false')) val = val === 'true'
        if (typeof val === 'string' && !isNaN(Number(val)) && (snake.includes('rate') || snake.includes('days'))) val = Number(val)
        ;(settings.value as any)[camel] = val
      }
    }
    // Also keep any camelCase keys the API might return directly
    for (const key of Object.keys(data)) {
      if (!key.includes('_') && data[key] !== undefined) {
        let val: any = data[key]
        if (typeof val === 'string' && (val === 'true' || val === 'false')) val = val === 'true'
        ;(settings.value as any)[key] = val
      }
    }
    if (data.verify_modify_settings !== undefined) loadedVerifyModifySettings.value = data.verify_modify_settings === 'true' || data.verify_modify_settings === true
    if (data.verify_modify_admin !== undefined) loadedVerifyModifyAdmin.value = data.verify_modify_admin === 'true' || data.verify_modify_admin === true
  } catch (err) {
    console.error('[Settings] Failed to load settings:', err)
    ElMessage.error('加载设置失败，请刷新页面重试')
  }
  await fetchAdminAccount()
  await loadHolidays()
  await loadLogs()
})
</script>
