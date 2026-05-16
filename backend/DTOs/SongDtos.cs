namespace backend.DTOs;

public record CreateSongRequest(string Title, string Artist, string Genre, string? Language, int Duration, long? FileSize, string? CoverUrl, string? MediaUrl, string? OriginalFileName);
public record UpdateSongRequest(string? Title, string? Artist, string? Genre, string? Language, int? Duration, long? FileSize, string? CoverUrl, string? MediaUrl, string? Status, string? OriginalFileName);
public record SongDetailResponse(
    int Id, string Title, string Artist, string Genre, string? Language, int Duration, long? FileSize,
    string? CoverUrl, string? MediaUrl, string? OriginalFileName, int PlayCount, string Status, DateTime CreatedAt, DateTime UpdatedAt,
    int FavoriteCount, int Ranking, int Rating, int CommentCount);
