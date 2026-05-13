import { apiClient } from './client'
import type { Song, PlayQueueItem, Favorite, RoomInfo, PaginatedResult, User } from '@/types'

export const authApi = {
  login: (username: string, password: string) =>
    apiClient.post<{ token: string; user: User }>('/api/auth/login', { username, password }),
  logout: () => apiClient.post('/api/auth/logout'),
}

export const songsApi = {
  getList: (params: { search?: string; genre?: string; page?: number; pageSize?: number }) =>
    apiClient.get<PaginatedResult<Song>>('/api/songs', { params }),
  getGenres: () => apiClient.get<string[]>('/api/songs/genres'),
}

export const chartsApi = {
  getDaily: () => apiClient.get<Song[]>('/api/charts/daily'),
  getWeekly: () => apiClient.get<Song[]>('/api/charts/weekly'),
}

export const favoritesApi = {
  getList: () => apiClient.get<Favorite[]>('/api/favorites'),
  add: (songId: number) => apiClient.post(`/api/favorites/${songId}`),
  remove: (songId: number) => apiClient.delete(`/api/favorites/${songId}`),
}

export const roomApi = {
  getCurrent: () => apiClient.get<RoomInfo>('/api/room/current'),
  getQueue: () => apiClient.get<PlayQueueItem[]>('/api/room/queue'),
  reorder: (queueId: number, newOrder: number) =>
    apiClient.post('/api/room/queue/reorder', { queueId, newOrder }),
  removeFromQueue: (queueId: number) =>
    apiClient.delete(`/api/room/queue/${queueId}`),
}

export const ordersApi = {
  orderSong: (songId: number, roomId: number) =>
    apiClient.post('/api/orders/song', { songId, roomId }),
}
