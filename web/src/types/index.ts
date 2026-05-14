export interface User {
  id: number
  username: string
  displayName: string
  balance: number
  isVip: boolean
  status: 'active' | 'disabled'
  createdAt: string
}

export interface Song {
  id: number
  title: string
  artist: string
  genre: string
  duration: number
  coverUrl: string
  mediaUrl: string
  playCount: number
  status: 'active' | 'inactive'
  createdAt: string
}

export interface PlayQueueItem {
  id: number
  roomId: number
  songId: number
  songTitle: string
  artist: string
  coverUrl: string
  mediaUrl: string
  orderedBy: string
  sortOrder: number
  createdAt: string
}

export interface Favorite {
  id: number
  userId: number
  songId: number
  song: Song
  createdAt: string
}

export interface RoomInfo {
  roomId: number
  roomNumber: string
  roomType: string
  songsQueued: number
}

export interface NowPlaying {
  songId: number
  title: string
  artist: string
  coverUrl: string
  duration: number
  currentTime: number
}

export interface PaginatedResult<T> {
  items: T[]
  total: number
  page: number
  pageSize: number
}
