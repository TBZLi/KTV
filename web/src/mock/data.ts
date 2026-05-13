import type { User, Song, PlayQueueItem, Favorite, RoomInfo, NowPlaying } from '@/types'

export const mockUser: User = {
  id: 1, username: 'user_001', displayName: '张三', balance: 500, isVip: false, status: 'active', createdAt: '2023-10-25 10:00',
}

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

export const mockGenres = ['流行', '摇滚', '民谣', '电子', 'R&B', '嘻哈', '古典']

export const mockFavorites: Favorite[] = [
  { id: 1, userId: 1, songId: 1, song: mockSongs[0], createdAt: '2023-10-20' },
  { id: 2, userId: 1, songId: 3, song: mockSongs[2], createdAt: '2023-10-21' },
  { id: 3, userId: 1, songId: 5, song: mockSongs[4], createdAt: '2023-10-22' },
  { id: 4, userId: 1, songId: 6, song: mockSongs[5], createdAt: '2023-10-23' },
  { id: 5, userId: 1, songId: 12, song: mockSongs[11], createdAt: '2023-10-24' },
  { id: 6, userId: 1, songId: 8, song: mockSongs[7], createdAt: '2023-10-24' },
  { id: 7, userId: 1, songId: 14, song: mockSongs[13], createdAt: '2023-10-25' },
  { id: 8, userId: 1, songId: 17, song: mockSongs[16], createdAt: '2023-10-25' },
]

export const mockRoomInfo: RoomInfo = {
  roomNumber: '888',
  roomType: 'VIP',
  songsQueued: 12,
}

export const mockNowPlaying: NowPlaying = {
  songId: 1,
  title: '晴天',
  artist: '周杰伦',
  coverUrl: '',
  duration: 269,
  currentTime: 89,
}

export const mockPlayQueue: PlayQueueItem[] = [
  { id: 1, roomId: 1, songId: 2, songTitle: '起风了', artist: '买辣椒也用券', coverUrl: '', orderedBy: '用户昵称', sortOrder: 1, createdAt: '2023-10-26 14:00' },
  { id: 2, roomId: 1, songId: 3, songTitle: '孤勇者', artist: '陈奕迅', coverUrl: '', orderedBy: '用户昵称', sortOrder: 2, createdAt: '2023-10-26 14:05' },
  { id: 3, roomId: 1, songId: 5, songTitle: '海阔天空', artist: 'Beyond', coverUrl: '', orderedBy: '用户昵称', sortOrder: 3, createdAt: '2023-10-26 14:10' },
  { id: 4, roomId: 1, songId: 7, songTitle: '光年之外', artist: '邓紫棋', coverUrl: '', orderedBy: '用户昵称', sortOrder: 4, createdAt: '2023-10-26 14:15' },
  { id: 5, roomId: 1, songId: 9, songTitle: '红玫瑰', artist: '陈奕迅', coverUrl: '', orderedBy: '用户昵称', sortOrder: 5, createdAt: '2023-10-26 14:20' },
  { id: 6, roomId: 1, songId: 11, songTitle: '倔强', artist: '五月天', coverUrl: '', orderedBy: '用户昵称', sortOrder: 6, createdAt: '2023-10-26 14:22' },
  { id: 7, roomId: 1, songId: 13, songTitle: '告白气球', artist: '周杰伦', coverUrl: '', orderedBy: '用户昵称', sortOrder: 7, createdAt: '2023-10-26 14:25' },
  { id: 8, roomId: 1, songId: 15, songTitle: '南山南', artist: '马頔', coverUrl: '', orderedBy: '用户昵称', sortOrder: 8, createdAt: '2023-10-26 14:28' },
  { id: 9, roomId: 1, songId: 18, songTitle: '李白', artist: '李荣浩', coverUrl: '', orderedBy: '用户昵称', sortOrder: 9, createdAt: '2023-10-26 14:30' },
  { id: 10, roomId: 1, songId: 20, songTitle: '无条件', artist: '陈奕迅', coverUrl: '', orderedBy: '用户昵称', sortOrder: 10, createdAt: '2023-10-26 14:32' },
  { id: 11, roomId: 1, songId: 22, songTitle: '富士山下', artist: '陈奕迅', coverUrl: '', orderedBy: '用户昵称', sortOrder: 11, createdAt: '2023-10-26 14:35' },
  { id: 12, roomId: 1, songId: 24, songTitle: '安河桥', artist: '宋冬野', coverUrl: '', orderedBy: '用户昵称', sortOrder: 12, createdAt: '2023-10-26 14:38' },
]

export const mockDailyCharts: Song[] = mockSongs.slice(0, 10)
export const mockWeeklyCharts: Song[] = [...mockSongs].sort((a, b) => b.playCount - a.playCount).slice(0, 10)
