import { apiClient } from './client'
import type { Song, PlayQueueItem, Favorite, RoomInfo, PaginatedResult, User } from '@/types'

export const authApi = {
  login: (username: string, password: string) =>
    apiClient.post<{ token: string; user: User }>('/api/auth/login', { username, password }),
  register: (data: { username?: string; phone?: string; email?: string; password: string; displayName: string }) =>
    apiClient.post('/api/auth/register', data),
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
  getCurrent: (roomId?: number) => apiClient.get<RoomInfo>('/api/room/current', { params: roomId ? { roomId } : undefined }),
  getQueue: () => apiClient.get<PlayQueueItem[]>('/api/room/queue'),
  reorder: (queueId: number, newOrder: number) =>
    apiClient.post('/api/room/queue/reorder', { queueId, newOrder }),
  reorderBatch: (queueIds: number[]) =>
    apiClient.post('/api/room/queue/reorder-batch', { queueIds }),
  removeFromQueue: (queueId: number) =>
    apiClient.delete(`/api/room/queue/${queueId}`),
  joinByCode: (roomCode: string) =>
    apiClient.post('/api/rooms/join', { roomCode }),
  leaveRoom: (roomId: number) =>
    apiClient.post(`/api/rooms/${roomId}/leave`),
  orderSong: (songId: number, roomId: number) =>
    apiClient.post('/api/room/queue', { songId, roomId }),
}

export const roomRequestsApi = {
  create: () => apiClient.post('/api/roomrequests'),
}

export const feedbacksApi = {
  create: (data: { feedbackType: string; songName?: string; artist?: string; description?: string }) =>
    apiClient.post('/api/feedbacks', data),
}

export const chatApi = {
  sendMessage: (roomId: number, message: string) =>
    apiClient.post('/api/chat/send', { roomId, message }),
  getMessages: (roomId: number) =>
    apiClient.get<{ id: number; nickname: string; message: string; timestamp: string }[]>('/api/chat/messages', { params: { roomId } }),
}

