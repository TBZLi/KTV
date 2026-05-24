import type { User, Room, RoomRequest, Feedback, Song, DashboardStats, SongStats, SystemSettings, OperationLog } from '@/types'

export const mockUsers: User[] = [
  { id: 1, username: 'user_001', displayName: '张三', phone: '13800001111', status: 'active', isOnline: true, roomCode: 'ABC123', createdAt: '2023-10-25 10:00' },
  { id: 2, username: 'user_002', displayName: '李四', email: 'lisi@example.com', status: 'active', isOnline: true, roomCode: 'DEF456', createdAt: '2023-10-25 11:30' },
  { id: 3, username: 'test_003', displayName: '王五', status: 'disabled', isOnline: false, createdAt: '2023-10-24 15:45' },
  { id: 4, username: 'user_004', displayName: '赵六', phone: '13800004444', status: 'active', isOnline: false, createdAt: '2023-10-23 09:15' },
  { id: 5, username: 'user_005', displayName: '钱七', email: 'qianqi@example.com', status: 'active', isOnline: true, roomCode: 'JKL012', createdAt: '2023-10-22 14:20' },
  { id: 6, username: 'user_006', displayName: '孙八', status: 'active', isOnline: true, roomCode: 'PQR678', createdAt: '2023-10-21 16:30' },
  { id: 7, username: 'user_007', displayName: '周九', status: 'disabled', isOnline: false, createdAt: '2023-10-20 11:00' },
  { id: 8, username: 'user_008', displayName: '吴十', phone: '13800008888', status: 'active', isOnline: false, createdAt: '2023-10-19 13:45' },
]

export const mockRooms: Room[] = [
  { id: 1, roomCode: 'ABC123', status: 'active', createdByUserId: 1, currentUsers: 5, createdAt: '2026-05-15 10:00' },
  { id: 2, roomCode: 'DEF456', status: 'active', createdByUserId: 2, currentUsers: 3, createdAt: '2026-05-15 11:30' },
  { id: 3, roomCode: 'GHI789', status: 'idle_closing', createdByUserId: 4, currentUsers: 0, idleCloseAt: '2026-05-15 14:10', createdAt: '2026-05-15 13:00' },
  { id: 4, roomCode: 'JKL012', status: 'active', createdByUserId: 5, currentUsers: 8, createdAt: '2026-05-15 09:00' },
  { id: 5, roomCode: 'MNO345', status: 'closed', createdByUserId: 6, currentUsers: 0, createdAt: '2026-05-14 20:00', closedAt: '2026-05-15 01:00' },
  { id: 6, roomCode: 'PQR678', status: 'active', createdByUserId: 1, currentUsers: 2, createdAt: '2026-05-15 12:00' },
]

export const mockRoomRequests: RoomRequest[] = [
  { id: 1, userId: 3, status: 'pending', createdAt: '2026-05-15 13:30', username: 'test_003', displayName: '王五' },
  { id: 2, userId: 7, status: 'pending', createdAt: '2026-05-15 12:00', username: 'user_007', displayName: '周九' },
  { id: 3, userId: 4, status: 'approved', roomId: 3, createdAt: '2026-05-15 10:00', processedAt: '2026-05-15 10:05', processedBy: 1, username: 'user_004', displayName: '赵六', roomCode: 'GHI789' },
  { id: 4, userId: 8, status: 'rejected', createdAt: '2026-05-15 09:00', processedAt: '2026-05-15 09:10', processedBy: 1, username: 'user_008', displayName: '吴十' },
]

export const mockFeedbacks: Feedback[] = [
  { id: 1, userId: 1, feedbackType: 'request_song', songName: '七里香', artist: '周杰伦', status: 'pending', createdAt: '2026-05-15 14:00', username: 'user_001', displayName: '张三' },
  { id: 2, userId: 2, feedbackType: 'report_error', songName: '起风了', artist: '', description: '歌手名应为"买辣椒也用券"', status: 'pending', createdAt: '2026-05-15 13:00', username: 'user_002', displayName: '李四' },
  { id: 3, userId: 5, feedbackType: 'other', description: '建议增加日语歌曲分类', status: 'processed', createdAt: '2026-05-14 20:00', processedAt: '2026-05-15 09:00', username: 'user_005', displayName: '钱七' },
  { id: 4, userId: 6, feedbackType: 'request_song', songName: '漠河舞厅', artist: '柳爽', status: 'processed', createdAt: '2026-05-14 18:00', processedAt: '2026-05-15 08:00', username: 'user_006', displayName: '孙八' },
]

export const mockSongs: Song[] = [
  { id: 1, title: '晴天', artist: '周杰伦', genre: '流行', language: '华语', duration: 269, fileSize: null, coverUrl: '', mediaUrl: '', playCount: 999000, status: 'active', createdAt: '2023-01-15', updatedAt: '2023-01-15' },
  { id: 2, title: '起风了', artist: '买辣椒也用券', genre: '流行', language: '华语', duration: 325, fileSize: null, coverUrl: '', mediaUrl: '', playCount: 850000, status: 'active', createdAt: '2023-01-20', updatedAt: '2023-01-20' },
  { id: 3, title: '孤勇者', artist: '陈奕迅', genre: '流行', language: '华语', duration: 262, fileSize: null, coverUrl: '', mediaUrl: '', playCount: 720000, status: 'active', createdAt: '2023-02-01', updatedAt: '2023-02-01' },
  { id: 4, title: '稻香', artist: '周杰伦', genre: '流行', language: '华语', duration: 223, fileSize: null, coverUrl: '', mediaUrl: '', playCount: 600000, status: 'active', createdAt: '2023-02-10', updatedAt: '2023-02-10' },
  { id: 5, title: '海阔天空', artist: 'Beyond', genre: '摇滚', language: '粤语', duration: 326, fileSize: null, coverUrl: '', mediaUrl: '', playCount: 580000, status: 'active', createdAt: '2023-02-15', updatedAt: '2023-02-15' },
  { id: 6, title: '平凡之路', artist: '朴树', genre: '民谣', language: '华语', duration: 295, fileSize: null, coverUrl: '', mediaUrl: '', playCount: 520000, status: 'active', createdAt: '2023-02-20', updatedAt: '2023-02-20' },
  { id: 7, title: '光年之外', artist: '邓紫棋', genre: '流行', language: '华语', duration: 235, fileSize: null, coverUrl: '', mediaUrl: '', playCount: 480000, status: 'active', createdAt: '2023-03-01', updatedAt: '2023-03-01' },
  { id: 8, title: '夜曲', artist: '周杰伦', genre: '流行', language: '华语', duration: 226, fileSize: null, coverUrl: '', mediaUrl: '', playCount: 450000, status: 'inactive', createdAt: '2023-03-05', updatedAt: '2023-03-05' },
]

export const mockDashboardStats: DashboardStats = {
  activeRooms: 4,
  onlineUsers: 18,
  todayRooms: 12,
  totalUsers: 156,
}

export const mockSongStats: SongStats = {
  totalSongs: 25,
  weeklyNew: 5,
  todayPlays: 1280,
}

export const mockSettings: Record<string, string> = {
  platform_name: '声域友',
  contact_info: 'support@hydrosonic.cn',
  log_retention_days: '90',
  sensitive_op_verification: 'false',
  verify_disable_user: 'true',
  verify_close_room: 'true',
  verify_modify_settings: 'true',
  verify_modify_admin: 'true',
}

export const mockOperationLogs: OperationLog[] = [
  { id: 1, username: 'admin', operationType: 'approve', objectType: 'room_request', objectId: '3', details: 'POST /api/roomrequests/3/approve', createdAt: '2026-05-15 10:05:00' },
  { id: 2, username: 'admin', operationType: 'reject', objectType: 'room_request', objectId: '4', details: 'POST /api/roomrequests/4/reject', createdAt: '2026-05-15 09:10:00' },
  { id: 3, username: 'admin', operationType: 'update', objectType: 'settings', objectId: 'settings', details: 'PUT /api/settings', createdAt: '2026-05-15 09:00:00' },
  { id: 4, username: 'admin', operationType: 'disable', objectType: 'user', objectId: '7', details: 'POST /api/accounts/7/disable', createdAt: '2026-05-15 08:30:00' },
  { id: 5, username: 'admin', operationType: 'process', objectType: 'feedback', objectId: '3', details: 'POST /api/feedbacks/3/process', createdAt: '2026-05-15 08:00:00' },
  { id: 6, username: 'admin', operationType: 'close', objectType: 'room', objectId: '5', details: 'POST /api/rooms/5/close', createdAt: '2026-05-15 01:00:00' },
  { id: 7, username: 'admin', operationType: 'login', objectType: 'auth', objectId: null, details: '管理员登录', createdAt: '2026-05-15 09:00:00' },
]

export const mockGenres = ['流行', '摇滚', '民谣', '电子', 'R&B', '嘻哈', '古典', '纯音乐']
