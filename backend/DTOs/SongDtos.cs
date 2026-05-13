namespace backend.DTOs;

public record CreateSongRequest(string Title, string Artist, string Genre, int Duration, string? CoverUrl, string? MediaUrl);
public record UpdateSongRequest(string? Title, string? Artist, string? Genre, int? Duration, string? CoverUrl, string? MediaUrl, string? Status);
