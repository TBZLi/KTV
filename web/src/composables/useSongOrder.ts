import { ref } from 'vue'
import { roomApi } from '@/api'
import { usePlayerStore } from '@/stores/player'
import type { Song } from '@/types'

export function useSongOrder() {
  const player = usePlayerStore()
  const ordering = ref(false)

  async function orderSong(song: Song) {
    if (ordering.value) return
    ordering.value = true
    try {
      // backend ignores roomId - it finds the user's room automatically
      await roomApi.orderSong(song.id, 0)

      player.addToQueue({
        songId: song.id,
        title: song.title,
        artist: song.artist,
        coverUrl: song.coverUrl,
        mediaUrl: song.mediaUrl,
        lrcUrl: song.lrcUrl || '',
      })
    } catch (err: any) {
      const msg = err.response?.data?.message || err.response?.data || '点歌失败'
      alert(msg)
    } finally {
      ordering.value = false
    }
  }

  return { orderSong, ordering }
}
