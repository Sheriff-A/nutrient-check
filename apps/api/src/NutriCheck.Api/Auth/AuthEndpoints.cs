using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using NutriCheck.Domain;
using NutriCheck.Infrastructure.Persistence;

namespace NutriCheck.Api.Auth;

public static class AuthEndpoints
{
    private const int MinPasswordLength = 8;

    public static IEndpointRouteBuilder MapAuthEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/auth");

        group.MapPost("/register", async (
            RegisterRequest request,
            NutriCheckDbContext db,
            IPasswordHasher<User> hasher,
            CancellationToken ct) =>
        {
            var email = NormalizeEmail(request.Email);
            if (string.IsNullOrWhiteSpace(email))
            {
                return Results.Json(new ErrorResponse("Email is required."), statusCode: StatusCodes.Status400BadRequest);
            }

            if (string.IsNullOrEmpty(request.Password) || request.Password.Length < MinPasswordLength)
            {
                return Results.Json(
                    new ErrorResponse($"Password must be at least {MinPasswordLength} characters."),
                    statusCode: StatusCodes.Status400BadRequest);
            }

            var emailInUse = await db.Users.AnyAsync(u => u.Email == email, ct);
            if (emailInUse)
            {
                return Results.Json(new ErrorResponse("Email is already registered."), statusCode: StatusCodes.Status409Conflict);
            }

            var user = new User
            {
                Id = Guid.NewGuid(),
                Email = email,
                PasswordHash = string.Empty,
                CreatedAt = DateTimeOffset.UtcNow,
            };
            user.PasswordHash = hasher.HashPassword(user, request.Password);

            db.Users.Add(user);
            await db.SaveChangesAsync(ct);

            return Results.Json(new AuthUserResponse(user.Email), statusCode: StatusCodes.Status201Created);
        });

        group.MapPost("/login", async (
            LoginRequest request,
            NutriCheckDbContext db,
            IPasswordHasher<User> hasher,
            HttpContext http,
            CancellationToken ct) =>
        {
            var email = NormalizeEmail(request.Email);
            var user = await db.Users.SingleOrDefaultAsync(u => u.Email == email, ct);

            if (user is null || hasher.VerifyHashedPassword(user, user.PasswordHash, request.Password) == PasswordVerificationResult.Failed)
            {
                return Results.Json(new ErrorResponse("Invalid email or password."), statusCode: StatusCodes.Status401Unauthorized);
            }

            var identity = new ClaimsIdentity(
                [
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Email, user.Email),
                ],
                CookieAuthenticationDefaults.AuthenticationScheme);

            await http.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, new ClaimsPrincipal(identity));

            return Results.Ok(new AuthUserResponse(user.Email));
        });

        group.MapPost("/logout", async (HttpContext http) =>
        {
            await http.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return Results.Ok();
        });

        group.MapGet("/me", (HttpContext http) =>
        {
            if (http.User.Identity?.IsAuthenticated == true)
            {
                var email = http.User.FindFirstValue(ClaimTypes.Email) ?? string.Empty;
                return Results.Ok(new MeResponse(true, email));
            }

            return Results.Ok(new MeResponse(false, null));
        });

        return app;
    }

    private static string NormalizeEmail(string email) => email.Trim().ToLowerInvariant();
}
