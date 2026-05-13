export interface User {
  id: number
  username: string
  displayName: string
  balance: number
  isVip: boolean
  status: 'active' | 'disabled'
  createdAt: string
}

export interface Room {
  id: number
  roomNumber: string
  roomType: 'VIP' | 'Medium' | 'Small'
  status: 'in_use' | 'idle' | 'cleaning'
  currentOrderId: string | null
}

export interface Song {
  id: number
  title: string
  artist: string
  genre: string
  duration: number
  coverUrl: string
  playCount: number
  status: 'active' | 'inactive'
  createdAt: string
}

export interface Order {
  id: string
  roomId: number
  roomNumber: string
  roomType: 'VIP' | 'Medium' | 'Small'
  userId: number
  username: string
  amount: number
  status: 'in_progress' | 'completed' | 'cancelled' | 'refunded'
  createdAt: string
}

export interface DashboardStats {
  totalRooms: number
  todayOrders: number
  totalRevenue: number
  activeUsers: number
}

export interface SongStats {
  totalSongs: number
  weeklyNew: number
  todayPlays: number
}

export interface PaginatedResult<T> {
  items: T[]
  total: number
  page: number
  pageSize: number
}

export interface SystemSettings {
  storeName: string
  storePhone: string
  storeAddress: string
  businessHours: string
  holidayPricingEnabled: boolean
  baseHourlyRate: number
  logRetentionDays: number
  sensitiveOpVerification: boolean
  verifyDeleteOrder: boolean
  verifyBalanceAdjust: boolean
  verifyDisableUser: boolean
  verifyBatchSongStatus: boolean
  verifyModifySettings: boolean
  verifyModifyAdmin: boolean
}

export interface Holiday {
  id: number
  startDate: string
  endDate: string
  vipMultiplier: number
  mediumMultiplier: number
  smallMultiplier: number
  createdAt: string
}

export interface OperationLog {
  id: number
  username: string
  operationType: string
  objectType: string
  objectId: string | null
  details: string | null
  createdAt: string
}
