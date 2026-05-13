import { apiClient } from './client'
import type { DashboardStats, Room, Song, Order, User, PaginatedResult, SystemSettings, Holiday, OperationLog } from '@/types'
import type { SongStats } from '@/types'

export const dashboardApi = {
  getStats: (params?: { from?: string; to?: string }) => apiClient.get<DashboardStats>('/api/dashboard/stats', { params }),
  getLatestOrders: () => apiClient.get<Order[]>('/api/dashboard/latest-orders'),
  getTopSongs: () => apiClient.get<Song[]>('/api/dashboard/top-songs'),
}

export const roomsApi = {
  getList: (params: { status?: string; search?: string; page?: number; pageSize?: number }) =>
    apiClient.get<PaginatedResult<Room>>('/api/rooms', { params }),
  getById: (id: number) => apiClient.get<Room>(`/api/rooms/${id}`),
  updateStatus: (id: number, status: string) =>
    apiClient.put(`/api/rooms/${id}/status`, { status }),
  endSession: (id: number) =>
    apiClient.post(`/api/rooms/${id}/end-session`),
}

export const songsApi = {
  getList: (params: { search?: string; genre?: string; page?: number; pageSize?: number }) =>
    apiClient.get<PaginatedResult<Song>>('/api/songs', { params }),
  getStats: () => apiClient.get<SongStats>('/api/songs/stats'),
  create: (data: Partial<Song>) => apiClient.post('/api/songs', data),
  update: (id: number, data: Partial<Song>) => apiClient.put(`/api/songs/${id}`, data),
  delete: (id: number) => apiClient.delete(`/api/songs/${id}`),
  getGenres: () => apiClient.get<string[]>('/api/songs/genres'),
}

export const ordersApi = {
  getList: (params: { status?: string; searchField?: string; searchKeyword?: string; page?: number; pageSize?: number }) =>
    apiClient.get<PaginatedResult<Order>>('/api/orders', { params }),
  getById: (id: string) => apiClient.get<Order>(`/api/orders/${id}`),
  create: (data: { userId: number; roomId: number; hours?: number; amount?: number }) =>
    apiClient.post('/api/orders', data),
  refund: (id: string) => apiClient.post(`/api/orders/${id}/refund`),
  complete: (id: string) => apiClient.post(`/api/orders/${id}/complete`),
  cancel: (id: string) => apiClient.post(`/api/orders/${id}/cancel`),
  restore: (id: string) => apiClient.post(`/api/orders/${id}/restore`),
  delete: (id: string) => apiClient.delete(`/api/orders/${id}`),
}

export const accountsApi = {
  getList: (params: { search?: string; status?: string; page?: number; pageSize?: number }) =>
    apiClient.get<PaginatedResult<User>>('/api/accounts', { params }),
  getById: (id: number) => apiClient.get<User>(`/api/accounts/${id}`),
  create: (data: { username: string; password: string; displayName: string; phone?: string }) =>
    apiClient.post('/api/accounts', data),
  update: (id: number, data: { displayName?: string; phone?: string; isVip?: boolean }) =>
    apiClient.put(`/api/accounts/${id}`, data),
  recharge: (id: number, amount: number) =>
    apiClient.post(`/api/accounts/${id}/recharge`, { amount }),
  toggleStatus: (id: number) =>
    apiClient.put(`/api/accounts/${id}/toggle-status`),
  getDisablePreview: (id: number) =>
    apiClient.get<{ inProgressCount: number }>(`/api/accounts/${id}/disable-preview`),
  disable: (id: number) =>
    apiClient.post(`/api/accounts/${id}/disable`),
}

export const settingsApi = {
  get: () => apiClient.get<SystemSettings>('/api/settings'),
  update: (data: Partial<SystemSettings>) => apiClient.put('/api/settings', data),
  getAdminAccount: () => apiClient.get<{ username: string }>('/api/settings/admin-account'),
  updateAdminUsername: (data: { newUsername: string; password: string }) =>
    apiClient.post('/api/settings/admin-account/username', data),
  updateAdminPassword: (data: { currentPassword: string; newPassword: string; confirmPassword: string }) =>
    apiClient.post('/api/settings/admin-account/password', data),
}

export const holidaysApi = {
  getAll: () => apiClient.get<Holiday[]>('/api/holidays'),
  create: (data: { startDate: string; endDate: string; vipMultiplier: number; mediumMultiplier: number; smallMultiplier: number }) =>
    apiClient.post('/api/holidays', data),
  delete: (id: number) => apiClient.delete(`/api/holidays/${id}`),
}

export const operationLogsApi = {
  getList: (params: { operationType?: string; username?: string; fromDate?: string; toDate?: string; page?: number; pageSize?: number }) =>
    apiClient.get<PaginatedResult<OperationLog>>('/api/operationlogs', { params }),
}

export const authApi = {
  login: (username: string, password: string) =>
    apiClient.post<{ token: string; user: User }>('/api/auth/login', { username, password }),
  logout: () => apiClient.post('/api/auth/logout'),
  getMe: () => apiClient.get<User>('/api/auth/me'),
  verifyPassword: (password: string) =>
    apiClient.post('/api/auth/verify-password', { password }),
}
