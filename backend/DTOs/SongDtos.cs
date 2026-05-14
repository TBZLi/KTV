namespace backend.DTOs;

public record CreateSongRequest(string Title, string Artist, string Genre, string? Language, int Duration, long? FileSize, string? CoverUrl, string? MediaUrl);
public record UpdateSongRequest(string? Title, string? Artist, string? Genre, string? Language, int? Duration, long? FileSize, string? CoverUrl, string? MediaUrl, string? Status);
public record SongDetailResponse(
    int Id, string Title, string Artist, string Genre, string? Language, int Duration, long? FileSize,
    string? CoverUrl, string? MediaUrl, int PlayCount, string Status, DateTime CreatedAt, DateTime UpdatedAt,
    int FavoriteCount, int Ranking, int Rating, int CommentCount);
