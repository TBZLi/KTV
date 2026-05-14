import { ref } from 'vue'
import { roomApi, ordersApi } from '@/api'
import { usePlayerStore } from '@/stores/player'
import type { Song } from '@/types'

let cachedRoomId: number | null = null

async function getRoomId(): Promise<number> {
  if (cachedRoomId) return cachedRoomId
  const { data } = await roomApi.getCurrent()
  cachedRoomId = data.roomId
  return cachedRoomId
}

export function useSongOrder() {
  const player = usePlayerStore()
  const ordering = ref(false)

  async function orderSong(song: Song) {
    if (ordering.value) return
    ordering.value = true
    try {
      const roomId = await getRoomId()
      await ordersApi.orderSong(song.id, roomId)

      player.addToQueue({
        songId: song.id,
        title: song.title,
        artist: song.artist,
        coverUrl: song.coverUrl,
        mediaUrl: song.mediaUrl,
      })
    } finally {
      ordering.value = false
    }
  }

  return { orderSong, ordering }
}
