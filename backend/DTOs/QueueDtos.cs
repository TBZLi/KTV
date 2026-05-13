namespace backend.DTOs;

public record ReorderQueueRequest(int QueueId, int NewOrder);
public record OrderSongRequest(int SongId, int RoomId);
