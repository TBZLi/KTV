import MockAdapter from 'axios-mock-adapter'
import { apiClient } from '../api/client'
import { mockUsers, mockRooms, mockSongs, mockOrders, mockDashboardStats, mockSongStats, mockSettings, mockGenres, mockHolidays, mockOperationLogs } from './data'

export function setupMockAdapter() {
  const mock = new MockAdapter(apiClient, { delayResponse: 300 })

  // Auth
  mock.onPost('/api/auth/login').reply(200, { token: 'mock-jwt-token', user: mockUsers[0] })
  mock.onPost('/api/auth/logout').reply(200)
  mock.onGet('/api/auth/me').reply(200, mockUsers[0])
  mock.onPost('/api/auth/verify-password').reply((config) => {
    const data = JSON.parse(config.data)
    return data.password === 'demo_hash_admin' ? [200] : [401, { message: '密码错误' }]
  })

  // Dashboard
  mock.onGet('/api/dashboard/stats').reply(200, mockDashboardStats)
  mock.onGet('/api/dashboard/latest-orders').reply(200, mockOrders.slice(0, 5))
  mock.onGet('/api/dashboard/top-songs').reply(200, mockSongs.slice(0, 5))

  // Rooms
  mock.onGet('/api/rooms').reply((config) => {
    let rooms = [...mockRooms]
    const params = config.params || {}
    if (params.status && params.status !== 'all') {
      rooms = rooms.filter(r => r.status === params.status)
    }
    if (params.search) {
      rooms = rooms.filter(r => r.roomNumber.toLowerCase().includes(params.search.toLowerCase()))
    }
    const page = params.page || 1
    const pageSize = params.pageSize || 10
    const start = (page - 1) * pageSize
    return [200, { items: rooms.slice(start, start + pageSize), total: rooms.length, page, pageSize }]
  })
  mock.onPut(/\/api\/rooms\/\d+\/status/).reply(200)
  mock.onPost(/\/api\/rooms\/\d+\/end-session/).reply(200)

  // Songs
  mock.onGet('/api/songs/stats').reply(200, mockSongStats)
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
  mock.onPost('/api/songs').reply(200)
  mock.onPut(/\/api\/songs\/\d+/).reply(200)
  mock.onDelete(/\/api\/songs\/\d+/).reply(200)

  // Orders
  mock.onGet('/api/orders').reply((config) => {
    let orders = [...mockOrders]
    const params = config.params || {}
    if (params.status && params.status !== 'all') {
      orders = orders.filter(o => o.status === params.status)
    }
    if (params.searchKeyword && params.searchField) {
      const q = params.searchKeyword.toLowerCase()
      if (params.searchField === 'orderId') orders = orders.filter(o => o.id.toLowerCase().includes(q))
      if (params.searchField === 'username') orders = orders.filter(o => (o.username || '').toLowerCase().includes(q))
      if (params.searchField === 'roomNumber') orders = orders.filter(o => (o.roomNumber || '').toLowerCase().includes(q))
    }
    const page = params.page || 1
    const pageSize = params.pageSize || 10
    const start = (page - 1) * pageSize
    return [200, { items: orders.slice(start, start + pageSize), total: orders.length, page, pageSize }]
  })
  mock.onPost(/\/api\/orders\/.*\/refund/).reply(200)
  mock.onPost(/\/api\/orders\/.*\/complete/).reply(200)
  mock.onPost(/\/api\/orders\/.*\/cancel/).reply(200)
  mock.onPost(/\/api\/orders\/.*\/restore/).reply(200)
  mock.onDelete(/\/api\/orders\/.*/).reply(200)

  // Accounts
  mock.onGet('/api/accounts').reply((config) => {
    let users = [...mockUsers]
    const params = config.params || {}
    if (params.search) {
      const q = params.search.toLowerCase()
      users = users.filter(u => u.username.toLowerCase().includes(q))
    }
    if (params.status && params.status !== 'all') {
      users = users.filter(u => u.status === params.status)
    }
    const page = params.page || 1
    const pageSize = params.pageSize || 10
    const start = (page - 1) * pageSize
    return [200, { items: users.slice(start, start + pageSize), total: users.length, page, pageSize }]
  })
  mock.onPost('/api/accounts').reply(200)
  mock.onPut(/\/api\/accounts\/\d+/).reply(200)
  mock.onPost(/\/api\/accounts\/\d+\/recharge/).reply(200)
  mock.onPut(/\/api\/accounts\/\d+\/toggle-status/).reply(200)
  mock.onGet(/\/api\/accounts\/\d+\/disable-preview/).reply(200, { inProgressCount: 0 })
  mock.onPost(/\/api\/accounts\/\d+\/disable/).reply(200)

  // Settings (mutable store matching real backend's snake_case dictionary format)
  const settingsStore: Record<string, string> = { ...mockSettings }
  mock.onGet('/api/settings').reply(200, { ...settingsStore })
  mock.onPut('/api/settings').reply((config) => {
    const data = JSON.parse(config.data)
    Object.assign(settingsStore, data)
    return [200]
  })
  mock.onGet('/api/settings/admin-account').reply(200, { username: 'admin' })
  mock.onPost('/api/settings/admin-account/username').reply(200)
  mock.onPost('/api/settings/admin-account/password').reply(200)

  // Holidays
  mock.onGet('/api/holidays').reply(200, mockHolidays)
  mock.onPost('/api/holidays').reply(200)
  mock.onDelete(/\/api\/holidays\/\d+/).reply(200)

  // Operation Logs
  mock.onGet('/api/operationlogs').reply((config) => {
    let logs = [...mockOperationLogs]
    const params = config.params || {}
    if (params.operationType && params.operationType !== 'all') {
      logs = logs.filter(l => l.operationType === params.operationType)
    }
    if (params.username) {
      const q = params.username.toLowerCase()
      logs = logs.filter(l => l.username.toLowerCase().includes(q))
    }
    const page = params.page || 1
    const pageSize = params.pageSize || 20
    const start = (page - 1) * pageSize
    return [200, { items: logs.slice(start, start + pageSize), total: logs.length, page, pageSize }]
  })

  return mock
}
