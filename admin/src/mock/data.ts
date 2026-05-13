import type { User, Room, Song, Order, DashboardStats, SongStats, SystemSettings, Holiday, OperationLog } from '@/types'

export const mockUsers: User[] = [
  { id: 1, username: 'user_001', displayName: '张三', balance: 500, isVip: false, status: 'active', createdAt: '2023-10-25 10:00' },
  { id: 2, username: 'VIP_002', displayName: '李四', balance: 1200, isVip: true, status: 'active', createdAt: '2023-10-25 11:30' },
  { id: 3, username: 'test_003', displayName: '王五', balance: 0, isVip: false, status: 'disabled', createdAt: '2023-10-24 15:45' },
  { id: 4, username: 'user_004', displayName: '赵六', balance: 350, isVip: false, status: 'active', createdAt: '2023-10-23 09:15' },
  { id: 5, username: 'VIP_005', displayName: '钱七', balance: 2800, isVip: true, status: 'active', createdAt: '2023-10-22 14:20' },
  { id: 6, username: 'user_006', displayName: '孙八', balance: 180, isVip: false, status: 'active', createdAt: '2023-10-21 16:30' },
  { id: 7, username: 'user_007', displayName: '周九', balance: 0, isVip: false, status: 'disabled', createdAt: '2023-10-20 11:00' },
  { id: 8, username: 'VIP_008', displayName: '吴十', balance: 950, isVip: true, status: 'active', createdAt: '2023-10-19 13:45' },
  { id: 9, username: 'user_009', displayName: '郑冬', balance: 420, isVip: false, status: 'active', createdAt: '2023-10-18 10:30' },
  { id: 10, username: 'user_010', displayName: '冯雪', balance: 75, isVip: false, status: 'active', createdAt: '2023-10-17 08:20' },
  { id: 11, username: 'user_011', displayName: '陈风', balance: 610, isVip: false, status: 'active', createdAt: '2023-10-16 17:10' },
  { id: 12, username: 'VIP_012', displayName: '楚云', balance: 3200, isVip: true, status: 'active', createdAt: '2023-10-15 12:00' },
]

export const mockRooms: Room[] = [
  { id: 1, roomNumber: 'V-001', roomType: 'VIP', status: 'in_use', currentOrderId: 'ORD-8924' },
  { id: 2, roomNumber: 'V-002', roomType: 'VIP', status: 'cleaning', currentOrderId: null },
  { id: 3, roomNumber: 'V-003', roomType: 'VIP', status: 'idle', currentOrderId: null },
  { id: 4, roomNumber: 'M-001', roomType: 'Medium', status: 'in_use', currentOrderId: 'ORD-8923' },
  { id: 5, roomNumber: 'M-002', roomType: 'Medium', status: 'in_use', currentOrderId: 'ORD-8921' },
  { id: 6, roomNumber: 'M-003', roomType: 'Medium', status: 'idle', currentOrderId: null },
  { id: 7, roomNumber: 'M-004', roomType: 'Medium', status: 'idle', currentOrderId: null },
  { id: 8, roomNumber: 'M-005', roomType: 'Medium', status: 'cleaning', currentOrderId: null },
  { id: 9, roomNumber: 'M-006', roomType: 'Medium', status: 'in_use', currentOrderId: 'ORD-8919' },
  { id: 10, roomNumber: 'M-007', roomType: 'Medium', status: 'idle', currentOrderId: null },
  { id: 11, roomNumber: 'S-001', roomType: 'Small', status: 'in_use', currentOrderId: 'ORD-8920' },
  { id: 12, roomNumber: 'S-002', roomType: 'Small', status: 'idle', currentOrderId: null },
  { id: 13, roomNumber: 'S-003', roomType: 'Small', status: 'in_use', currentOrderId: 'ORD-8918' },
  { id: 14, roomNumber: 'S-004', roomType: 'Small', status: 'idle', currentOrderId: null },
  { id: 15, roomNumber: 'S-005', roomType: 'Small', status: 'in_use', currentOrderId: 'ORD-8917' },
  { id: 16, roomNumber: 'S-006', roomType: 'Small', status: 'cleaning', currentOrderId: null },
  { id: 17, roomNumber: 'S-007', roomType: 'Small', status: 'idle', currentOrderId: null },
  { id: 18, roomNumber: 'S-008', roomType: 'Small', status: 'idle', currentOrderId: null },
  { id: 19, roomNumber: 'V-004', roomType: 'VIP', status: 'in_use', currentOrderId: 'ORD-8916' },
  { id: 20, roomNumber: 'V-005', roomType: 'VIP', status: 'idle', currentOrderId: null },
  { id: 21, roomNumber: 'M-008', roomType: 'Medium', status: 'in_use', currentOrderId: 'ORD-8915' },
  { id: 22, roomNumber: 'M-009', roomType: 'Medium', status: 'idle', currentOrderId: null },
  { id: 23, roomNumber: 'S-009', roomType: 'Small', status: 'idle', currentOrderId: null },
  { id: 24, roomNumber: 'S-010', roomType: 'Small', status: 'in_use', currentOrderId: 'ORD-8914' },
]

export const mockSongs: Song[] = [
  { id: 1, title: '晴天', artist: '周杰伦', genre: '流行', duration: 269, coverUrl: '', playCount: 999000, status: 'active', createdAt: '2023-01-15' },
  { id: 2, title: '起风了', artist: '买辣椒也用券', genre: '流行', duration: 325, coverUrl: '', playCount: 850000, status: 'active', createdAt: '2023-01-20' },
  { id: 3, title: '孤勇者', artist: '陈奕迅', genre: '流行', duration: 262, coverUrl: '', playCount: 720000, status: 'active', createdAt: '2023-02-01' },
  { id: 4, title: '稻香', artist: '周杰伦', genre: '流行', duration: 223, coverUrl: '', playCount: 600000, status: 'active', createdAt: '2023-02-10' },
  { id: 5, title: '海阔天空', artist: 'Beyond', genre: '摇滚', duration: 326, coverUrl: '', playCount: 580000, status: 'active', createdAt: '2023-02-15' },
  { id: 6, title: '平凡之路', artist: '朴树', genre: '民谣', duration: 295, coverUrl: '', playCount: 520000, status: 'active', createdAt: '2023-02-20' },
  { id: 7, title: '光年之外', artist: '邓紫棋', genre: '流行', duration: 235, coverUrl: '', playCount: 480000, status: 'active', createdAt: '2023-03-01' },
  { id: 8, title: '夜曲', artist: '周杰伦', genre: '流行', duration: 226, coverUrl: '', playCount: 450000, status: 'active', createdAt: '2023-03-05' },
  { id: 9, title: '红玫瑰', artist: '陈奕迅', genre: '流行', duration: 264, coverUrl: '', playCount: 430000, status: 'active', createdAt: '2023-03-10' },
  { id: 10, title: '后来', artist: '刘若英', genre: '流行', duration: 337, coverUrl: '', playCount: 410000, status: 'active', createdAt: '2023-03-15' },
  { id: 11, title: '倔强', artist: '五月天', genre: '摇滚', duration: 264, coverUrl: '', playCount: 390000, status: 'active', createdAt: '2023-03-20' },
  { id: 12, title: '成都', artist: '赵雷', genre: '民谣', duration: 329, coverUrl: '', playCount: 370000, status: 'active', createdAt: '2023-03-25' },
  { id: 13, title: '告白气球', artist: '周杰伦', genre: '流行', duration: 215, coverUrl: '', playCount: 350000, status: 'active', createdAt: '2023-04-01' },
  { id: 14, title: '说散就散', artist: '袁娅维', genre: 'R&B', duration: 237, coverUrl: '', playCount: 330000, status: 'active', createdAt: '2023-04-05' },
  { id: 15, title: '南山南', artist: '马頔', genre: '民谣', duration: 312, coverUrl: '', playCount: 310000, status: 'active', createdAt: '2023-04-10' },
  { id: 16, title: '体面', artist: '于文文', genre: '流行', duration: 268, coverUrl: '', playCount: 290000, status: 'active', createdAt: '2023-04-15' },
  { id: 17, title: '消愁', artist: '毛不易', genre: '民谣', duration: 315, coverUrl: '', playCount: 270000, status: 'active', createdAt: '2023-04-20' },
  { id: 18, title: '李白', artist: '李荣浩', genre: '流行', duration: 264, coverUrl: '', playCount: 250000, status: 'active', createdAt: '2023-04-25' },
  { id: 19, title: '泡沫', artist: '邓紫棋', genre: '流行', duration: 270, coverUrl: '', playCount: 230000, status: 'active', createdAt: '2023-05-01' },
  { id: 20, title: '无条件', artist: '陈奕迅', genre: '流行', duration: 273, coverUrl: '', playCount: 210000, status: 'active', createdAt: '2023-05-05' },
  { id: 21, title: '以父之名', artist: '周杰伦', genre: '嘻哈', duration: 341, coverUrl: '', playCount: 195000, status: 'active', createdAt: '2023-05-10' },
  { id: 22, title: '富士山下', artist: '陈奕迅', genre: '流行', duration: 284, coverUrl: '', playCount: 180000, status: 'active', createdAt: '2023-05-15' },
  { id: 23, title: '春风十里', artist: '鹿先森乐队', genre: '民谣', duration: 305, coverUrl: '', playCount: 165000, status: 'active', createdAt: '2023-05-20' },
  { id: 24, title: '安河桥', artist: '宋冬野', genre: '民谣', duration: 338, coverUrl: '', playCount: 150000, status: 'active', createdAt: '2023-05-25' },
  { id: 25, title: '玫瑰花的葬礼', artist: '许嵩', genre: '流行', duration: 258, coverUrl: '', playCount: 140000, status: 'active', createdAt: '2023-06-01' },
]

export const mockOrders: Order[] = [
  { id: 'ORD-8924', roomId: 1, roomNumber: 'V-001', roomType: 'VIP', userId: 2, amount: 450, status: 'in_progress', createdAt: '2023-10-26 14:30' },
  { id: 'ORD-8923', roomId: 4, roomNumber: 'M-001', roomType: 'Medium', userId: 1, amount: 128.50, status: 'completed', createdAt: '2023-10-26 13:15' },
  { id: 'ORD-8922', roomId: 11, roomNumber: 'S-001', roomType: 'Small', userId: 3, amount: 65, status: 'cancelled', createdAt: '2023-10-26 12:00' },
  { id: 'ORD-8921', roomId: 5, roomNumber: 'M-002', roomType: 'Medium', userId: 4, amount: 198, status: 'in_progress', createdAt: '2023-10-26 11:45' },
  { id: 'ORD-8920', roomId: 11, roomNumber: 'S-001', roomType: 'Small', userId: 5, amount: 85, status: 'in_progress', createdAt: '2023-10-26 11:30' },
  { id: 'ORD-8919', roomId: 9, roomNumber: 'M-006', roomType: 'Medium', userId: 6, amount: 210, status: 'in_progress', createdAt: '2023-10-26 10:00' },
  { id: 'ORD-8918', roomId: 13, roomNumber: 'S-003', roomType: 'Small', userId: 7, amount: 72, status: 'in_progress', createdAt: '2023-10-26 09:30' },
  { id: 'ORD-8917', roomId: 15, roomNumber: 'S-005', roomType: 'Small', userId: 8, amount: 95, status: 'in_progress', createdAt: '2023-10-26 09:00' },
  { id: 'ORD-8916', roomId: 19, roomNumber: 'V-004', roomType: 'VIP', userId: 9, amount: 520, status: 'in_progress', createdAt: '2023-10-26 08:30' },
  { id: 'ORD-8915', roomId: 21, roomNumber: 'M-008', roomType: 'Medium', userId: 10, amount: 165, status: 'in_progress', createdAt: '2023-10-26 08:00' },
  { id: 'ORD-8914', roomId: 24, roomNumber: 'S-010', roomType: 'Small', userId: 11, amount: 88, status: 'in_progress', createdAt: '2023-10-26 07:30' },
  { id: 'ORD-8913', roomId: 1, roomNumber: 'V-001', roomType: 'VIP', userId: 12, amount: 680, status: 'completed', createdAt: '2023-10-25 22:00' },
  { id: 'ORD-8912', roomId: 4, roomNumber: 'M-001', roomType: 'Medium', userId: 1, amount: 245, status: 'completed', createdAt: '2023-10-25 20:30' },
  { id: 'ORD-8911', roomId: 6, roomNumber: 'M-003', roomType: 'Medium', userId: 2, amount: 175, status: 'completed', createdAt: '2023-10-25 19:00' },
  { id: 'ORD-8910', roomId: 12, roomNumber: 'S-002', roomType: 'Small', userId: 3, amount: 55, status: 'cancelled', createdAt: '2023-10-25 18:00' },
]

export const mockDashboardStats: DashboardStats = {
  totalRooms: 24,
  todayOrders: 42,
  totalRevenue: 28650,
  activeUsers: 156,
}

export const mockSongStats: SongStats = {
  totalSongs: 25,
  weeklyNew: 5,
  todayPlays: 1280,
}

// Settings stored as snake_case string values (matching real backend DB format)
export const mockSettings: Record<string, string> = {
  store_name: '声域友 KTV (旗舰店)',
  store_phone: '010-88888888',
  store_address: '北京市朝阳区建国路88号',
  business_hours: '14:00 - 02:00',
  holiday_pricing_enabled: 'true',
  base_hourly_rate: '120',
  log_retention_days: '90',
  sensitive_op_verification: 'false',
  verify_delete_order: 'true',
  verify_balance_adjust: 'true',
  verify_disable_user: 'true',
  verify_batch_song_status: 'true',
  verify_modify_settings: 'true',
  verify_modify_admin: 'true',
}

export const mockHolidays: Holiday[] = [
  { id: 1, startDate: '2026-10-01', endDate: '2026-10-07', vipMultiplier: 1.5, mediumMultiplier: 1.3, smallMultiplier: 1.2, createdAt: '2026-05-01 10:00' },
  { id: 2, startDate: '2026-01-28', endDate: '2026-02-04', vipMultiplier: 1.5, mediumMultiplier: 1.3, smallMultiplier: 1.2, createdAt: '2026-05-02 14:00' },
  { id: 3, startDate: '2026-05-01', endDate: '2026-05-03', vipMultiplier: 1.2, mediumMultiplier: 1.1, smallMultiplier: 1.0, createdAt: '2026-05-10 09:00' },
]

export const mockOperationLogs: OperationLog[] = [
  { id: 1, username: 'admin', operationType: 'create', objectType: 'order', objectId: 'ORD2505121430123', details: 'POST /api/orders', createdAt: '2026-05-12 14:30:15' },
  { id: 2, username: 'admin', operationType: 'complete', objectType: 'order', objectId: 'ORD250512132', details: 'POST /api/orders/ORD250512132/complete', createdAt: '2026-05-12 14:25:10' },
  { id: 3, username: 'admin', operationType: 'balance_adjust', objectType: 'user', objectId: '5', details: 'POST /api/accounts/5/recharge', createdAt: '2026-05-12 14:20:05' },
  { id: 4, username: 'admin', operationType: 'update', objectType: 'settings', objectId: 'settings', details: 'PUT /api/settings', createdAt: '2026-05-12 13:50:00' },
  { id: 5, username: 'admin', operationType: 'disable', objectType: 'user', objectId: '7', details: 'POST /api/accounts/7/disable', createdAt: '2026-05-12 13:30:22' },
  { id: 6, username: 'admin', operationType: 'cancel', objectType: 'order', objectId: 'ORD250512089', details: 'POST /api/orders/ORD250512089/cancel', createdAt: '2026-05-12 12:15:30' },
  { id: 7, username: 'admin', operationType: 'create', objectType: 'holiday', objectId: '3', details: 'POST /api/holidays', createdAt: '2026-05-10 09:00:00' },
  { id: 8, username: 'admin', operationType: 'login', objectType: 'auth', objectId: null, details: '管理员登录', createdAt: '2026-05-12 09:00:00' },
]

export const mockGenres = ['流行', '摇滚', '民谣', '电子', 'R&B', '嘻哈', '古典']
