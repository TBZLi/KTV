import { defineStore } from 'pinia'
import { ref, computed } from 'vue'

export interface PlayerTrack {
  songId: number
  title: string
  artist: string
  coverUrl: string
  mediaUrl: string
}

const API_BASE = 'http://localhost:5276'

export const usePlayerStore = defineStore('player', () => {
  const audio = new Audio()
  audio.preload = 'auto'

  const queue = ref<PlayerTrack[]>([])
  const currentIndex = ref(-1)
  const isPlaying = ref(false)
  const currentTime = ref(0)
  const duration = ref(0)
  const volume = ref(80)

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
    playNext()
  })
  audio.addEventListener('error', () => {
    // Audio file unavailable, skip to next
    playNext()
  })

  // Set initial volume
  audio.volume = volume.value / 100

  function _loadTrack(index: number) {
    const track = queue.value[index]
    if (!track) return
    currentIndex.value = index
    audio.src = API_BASE + track.mediaUrl
    audio.load()
  }

  function play() {
    if (!hasTrack.value) return
    audio.play()
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
    if (currentIndex.value < queue.value.length - 1) {
      _loadTrack(currentIndex.value + 1)
      play()
    } else {
      // Queue ended
      currentIndex.value = -1
      isPlaying.value = false
      audio.src = ''
    }
  }

  function playPrev() {
    if (currentIndex.value > 0) {
      _loadTrack(currentIndex.value - 1)
      play()
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
    const wasPlaying = isPlaying.value
    const currentSongId = currentTrack.value?.songId

    queue.value = tracks

    if (tracks.length === 0) {
      currentIndex.value = -1
      audio.src = ''
      isPlaying.value = false
      return
    }

    // If we were playing a song that's still in the queue, keep playing it
    if (currentSongId != null) {
      const newIndex = tracks.findIndex(t => t.songId === currentSongId)
      if (newIndex >= 0) {
        currentIndex.value = newIndex
        return
      }
    }

    // If nothing was playing, load first track but don't auto-play
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
    if (index >= 0) {
      _loadTrack(index)
      play()
    }
  }

  return {
    queue,
    currentIndex,
    isPlaying,
    currentTime,
    duration,
    volume,
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
  }
})
