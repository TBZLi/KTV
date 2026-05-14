import MockAdapter from 'axios-mock-adapter'
import { apiClient } from '../api/client'
import { mockUser, mockSongs, mockGenres, mockFavorites, mockRoomInfo, mockPlayQueue, mockDailyCharts, mockWeeklyCharts } from './data'

export function setupMockAdapter() {
  const mock = new MockAdapter(apiClient, { delayResponse: 300 })

  // Auth
  mock.onPost('/api/auth/login').reply(200, { token: 'mock-jwt-token', user: mockUser })
  mock.onPost('/api/auth/logout').reply(200)

  // Songs
  mock.onGet('/api/songs/genres').reply(200, mockGenres)
  mock.onGet('/api/songs').reply((config) => {
    let songs = [...mockSongs]
    const params = config.params || {}
    if (params.search) {
      const q = params.search.toLowerCase()
      songs = songs.filter(s => s.title.toLowerCase().includes(q) || s.artist.toLowerCase().includes(q))
    }
    if (params.genre) {
      songs = songs.filter(s => s.genre === params.genre)
    }
    const page = params.page || 1
    const pageSize = params.pageSize || 10
    const start = (page - 1) * pageSize
    return [200, { items: songs.slice(start, start + pageSize), total: songs.length, page, pageSize }]
  })

  // Charts
  mock.onGet('/api/charts/daily').reply(200, mockDailyCharts)
  mock.onGet('/api/charts/weekly').reply(200, mockWeeklyCharts)

  // Favorites
  mock.onGet('/api/favorites').reply(200, mockFavorites)
  mock.onPost(/\/api\/favorites\/\d+/).reply(200)
  mock.onDelete(/\/api\/favorites\/\d+/).reply(200)

  // Room
  mock.onGet('/api/room/current').reply(200, mockRoomInfo)
  mock.onGet('/api/room/queue').reply(200, mockPlayQueue)
  mock.onPost('/api/room/queue/reorder').reply(200)
  mock.onDelete(/\/api\/room\/queue\/\d+/).reply(200)

  // Orders
  mock.onPost('/api/orders/song').reply(200, { success: true })

  return mock
}
