namespace backend.DTOs;

public record LoginRequest(string Username, string Password);
public record LoginResponse(string Token, object User);
public record VerifyPasswordRequest(string Password);
public record RegisterRequest(string? Username, string? Phone, string? Email, string Password, string DisplayName);
