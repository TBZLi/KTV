export function formatCurrency(value: number): string {
  return `¥${value.toFixed(2).replace(/\B(?=(\d{3})+(?!\d))/g, ',')}`
}

export function formatDuration(seconds: number): string {
  const m = Math.floor(seconds / 60)
  const s = seconds % 60
  return `${m}:${s.toString().padStart(2, '0')}`
}

export function formatPlayCount(count: number): string {
  if (count >= 10000) return `${(count / 10000).toFixed(1)}万`
  if (count >= 1000) return `${(count / 1000).toFixed(0)}K`
  return count.toString()
}

export function formatRoomType(type: string): string {
  const map: Record<string, string> = { VIP: '豪华包厢', Medium: '中包厢', Small: '小包厢' }
  return map[type] || type
}

export function formatRoomStatus(status: string): string {
  const map: Record<string, string> = { in_use: '使用中', idle: '空闲', cleaning: '打扫中' }
  return map[status] || status
}

export function formatOrderStatus(status: string): string {
  const map: Record<string, string> = { in_progress: '进行中', completed: '已完成', cancelled: '已取消', refunded: '已退款' }
  return map[status] || status
}

export function formatUserStatus(status: string): string {
  const map: Record<string, string> = { active: '启用', disabled: '禁用' }
  return map[status] || status
}

export function formatFileSize(bytes: number | null): string {
  if (!bytes) return '-'
  if (bytes >= 1024 * 1024) return `${(bytes / (1024 * 1024)).toFixed(1)} MB`
  if (bytes >= 1024) return `${(bytes / 1024).toFixed(0)} KB`
  return `${bytes} B`
}
