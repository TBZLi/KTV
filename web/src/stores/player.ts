import { defineStore } from 'pinia'
import { ref, computed } from 'vue'

export interface PlayerTrack {
  songId: number
  title: string
  artist: string
  coverUrl: string
  mediaUrl: string
  lrcUrl: string
}

const API_BASE = import.meta.env.VITE_API_BASE_URL?.replace(/\/api$/, '') || 'https://localhost:5001'

export const usePlayerStore = defineStore('player', () => {
  const audio = new Audio()
  audio.preload = 'auto'

  const queue = ref<PlayerTrack[]>([])
  const currentIndex = ref(-1)
  const isPlaying = ref(false)
  const currentTime = ref(0)
  const duration = ref(0)
  const volume = ref(80)
  const playMode = ref<'off' | 'repeat-all' | 'repeat-one' | 'shuffle'>('off')
  const skipNextQueueUpdate = ref(false)

  const currentTrack = computed(() =>
    currentIndex.value >= 0 && currentIndex.value < queue.value.length
      ? queue.value[currentIndex.value]
      : null
  )

  const hasTrack = computed(() => currentTrack.value !== null)

  // Sync audio events
  audio.addEventListener('timeupdate', () => {
    currentTime.value = audio.currentTime
  })
  audio.addEventListener('loadedmetadata', () => {
    duration.value = audio.duration
  })
  audio.addEventListener('play', () => {
    isPlaying.value = true
  })
  audio.addEventListener('pause', () => {
    isPlaying.value = false
  })
  audio.addEventListener('ended', () => {
    if (playMode.value === 'repeat-one') {
      audio.currentTime = 0
      play()
    } else {
      playNext()
    }
  })
  audio.addEventListener('error', () => {
    // Audio file unavailable, skip to next (but don't cascade if track has no media)
    if (currentTrack.value?.mediaUrl) {
      playNext()
    }
  })

  // Set initial volume
  audio.volume = volume.value / 100

  function _loadTrack(index: number) {
    const track = queue.value[index]
    if (!track) return
    currentIndex.value = index
    if (!track.mediaUrl) {
      audio.pause()
      audio.src = ''
      isPlaying.value = false
      return
    }
    audio.src = API_BASE + track.mediaUrl
    audio.load()
  }

  function play() {
    if (!hasTrack.value || !currentTrack.value?.mediaUrl) return
    audio.play().catch(() => {})
  }

  function pause() {
    audio.pause()
  }

  function togglePlay() {
    if (isPlaying.value) {
      pause()
    } else {
      play()
    }
  }

  function seek(time: number) {
    audio.currentTime = time
    currentTime.value = time
  }

  function setVolume(v: number) {
    volume.value = v
    audio.volume = v / 100
  }

  function playNext() {
    if (queue.value.length <= 1) {
      if (playMode.value === 'repeat-all' && queue.value.length === 1) {
        _loadTrack(0)
        play()
        return
      }
      // Queue ended
      currentIndex.value = -1
      isPlaying.value = false
      audio.src = ''
      return
    }

    if (playMode.value === 'shuffle') {
      let next: number
      do { next = Math.floor(Math.random() * queue.value.length) } while (next === 0)
      const [track] = queue.value.splice(next, 1)
      queue.value.unshift(track)
    } else {
      // Move next song to index 0, current goes to end of rotation
      const [track] = queue.value.splice(1, 1)
      queue.value.unshift(track)
    }
    skipNextQueueUpdate.value = true
    _loadTrack(0)
    play()
  }

  function playPrev() {
    // Current song is always at index 0 — restart it
    if (queue.value.length > 0) {
      seek(0)
    }
  }

  /**
   * Add a track to the queue. If the queue was empty, auto-play it.
   */
  function addToQueue(track: PlayerTrack) {
    // Check if already in queue
    const existingIndex = queue.value.findIndex(t => t.songId === track.songId)
    if (existingIndex >= 0) return

    queue.value.push(track)
    if (queue.value.length === 1) {
      _loadTrack(0)
      play()
    }
  }

  /**
   * Load a full queue (from server). Auto-play the first track if nothing is playing.
   */
  function loadQueue(tracks: PlayerTrack[]) {
    // Skip next update after a local reorder (drag-and-drop)
    if (skipNextQueueUpdate.value) {
      skipNextQueueUpdate.value = false
      return
    }
    // Skip update if queue hasn't changed (prevents re-render flash on poll)
    if (tracks.length === queue.value.length && tracks.every((t, i) => t.songId === queue.value[i].songId && t.coverUrl === queue.value[i].coverUrl && t.mediaUrl === queue.value[i].mediaUrl)) {
      return
    }

    const wasPlaying = isPlaying.value
    const prevIndex = currentIndex.value
    const currentSongId = currentTrack.value?.songId

    queue.value = tracks

    if (tracks.length === 0) {
      currentIndex.value = -1
      audio.src = ''
      isPlaying.value = false
      return
    }

    // If we were playing a song that's still in the queue, keep it at index 0
    if (currentSongId != null) {
      const newIndex = tracks.findIndex(t => t.songId === currentSongId)
      if (newIndex >= 0 && newIndex !== 0) {
        const [track] = queue.value.splice(newIndex, 1)
        queue.value.unshift(track)
      }
      currentIndex.value = 0
      return
    }

    // Queue ended (playNext set currentIndex to -1) — stay stopped, don't restart from track 0
    if (prevIndex === -1 && !wasPlaying) return

    // If nothing was playing yet, load first track but don't auto-play
    if (!wasPlaying) {
      _loadTrack(0)
      audio.pause()
    } else {
      _loadTrack(0)
      play()
    }
  }

  function playTrackBySongId(songId: number) {
    const index = queue.value.findIndex(t => t.songId === songId)
    if (index < 0) return

    if (index !== 0) {
      // Move clicked song to top (position 0)
      const [track] = queue.value.splice(index, 1)
      queue.value.unshift(track)
      skipNextQueueUpdate.value = true
    }
    _loadTrack(0)
    play()
  }

  function togglePlayMode() {
    const modes: Array<'off' | 'repeat-all' | 'repeat-one' | 'shuffle'> = ['off', 'repeat-all', 'repeat-one', 'shuffle']
    const idx = modes.indexOf(playMode.value)
    playMode.value = modes[(idx + 1) % modes.length]
  }

  return {
    queue,
    currentIndex,
    isPlaying,
    currentTime,
    duration,
    volume,
    playMode,
    currentTrack,
    hasTrack,
    play,
    pause,
    togglePlay,
    seek,
    setVolume,
    playNext,
    playPrev,
    addToQueue,
    loadQueue,
    playTrackBySongId,
    togglePlayMode,
    skipNextQueueUpdate,
  }
})
