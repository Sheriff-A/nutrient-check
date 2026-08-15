namespace NutriCheck.Api.Auth;

public record RegisterRequest(string Email, string Password);

public record LoginRequest(string Email, string Password);

public record AuthUserResponse(string Email);

public record MeResponse(bool Authenticated, string? Email);

public record ErrorResponse(string Error);
