using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using NutriCheck.Api.Auth;
using NutriCheck.Domain;
using NutriCheck.Infrastructure.Persistence;

namespace NutriCheck.Api.Meals;

public static class MealEndpoints
{
    private const int DefaultPageSize = 50;
    private const int MaxPageSize = 200;

    public static IEndpointRouteBuilder MapMealEndpoints(this IEndpointRouteBuilder app)
    {
        var group = app.MapGroup("/meals").RequireAuthorization();

        group.MapPost("/", async (
            MealRequest request,
            NutriCheckDbContext db,
            HttpContext http,
            CancellationToken ct) =>
        {
            var validationError = Validate(request);
            if (validationError is not null)
            {
                return Results.Json(new ErrorResponse(validationError), statusCode: StatusCodes.Status400BadRequest);
            }

            var meal = new Meal
            {
                Id = Guid.NewGuid(),
                UserId = CurrentUserId(http),
                Name = request.Name.Trim(),
                Description = request.Description,
                MealType = request.MealType!.Value,
                EatenAt = request.EatenAt!.Value,
                Calories = request.Calories,
                ProteinGrams = request.ProteinGrams,
                CarbsGrams = request.CarbsGrams,
                FatGrams = request.FatGrams,
                CreatedAt = DateTimeOffset.UtcNow,
            };

            db.Meals.Add(meal);
            await db.SaveChangesAsync(ct);

            return Results.Json(MealResponse.FromEntity(meal), statusCode: StatusCodes.Status201Created);
        });

        group.MapGet("/", async (
            NutriCheckDbContext db,
            HttpContext http,
            CancellationToken ct,
            int? take,
            int? skip) =>
        {
            var pageSize = Math.Clamp(take ?? DefaultPageSize, 1, MaxPageSize);
            var offset = Math.Max(skip ?? 0, 0);
            var userId = CurrentUserId(http);

            var meals = await db.Meals
                .Where(m => m.UserId == userId)
                .OrderByDescending(m => m.EatenAt)
                .Skip(offset)
                .Take(pageSize)
                .ToListAsync(ct);

            return Results.Ok(meals.Select(MealResponse.FromEntity));
        });

        group.MapGet("/{id:guid}", async (
            Guid id,
            NutriCheckDbContext db,
            HttpContext http,
            CancellationToken ct) =>
        {
            var meal = await FindOwnedMeal(db, id, CurrentUserId(http), ct);
            return meal is null ? Results.NotFound() : Results.Ok(MealResponse.FromEntity(meal));
        });

        group.MapPatch("/{id:guid}", async (
            Guid id,
            MealRequest request,
            NutriCheckDbContext db,
            HttpContext http,
            CancellationToken ct) =>
        {
            var meal = await FindOwnedMeal(db, id, CurrentUserId(http), ct);
            if (meal is null)
            {
                return Results.NotFound();
            }

            var validationError = Validate(request);
            if (validationError is not null)
            {
                return Results.Json(new ErrorResponse(validationError), statusCode: StatusCodes.Status400BadRequest);
            }

            meal.Name = request.Name.Trim();
            meal.Description = request.Description;
            meal.MealType = request.MealType!.Value;
            meal.EatenAt = request.EatenAt!.Value;
            meal.Calories = request.Calories;
            meal.ProteinGrams = request.ProteinGrams;
            meal.CarbsGrams = request.CarbsGrams;
            meal.FatGrams = request.FatGrams;

            await db.SaveChangesAsync(ct);

            return Results.Ok(MealResponse.FromEntity(meal));
        });

        group.MapDelete("/{id:guid}", async (
            Guid id,
            NutriCheckDbContext db,
            HttpContext http,
            CancellationToken ct) =>
        {
            var meal = await FindOwnedMeal(db, id, CurrentUserId(http), ct);
            if (meal is null)
            {
                return Results.NotFound();
            }

            db.Meals.Remove(meal);
            await db.SaveChangesAsync(ct);

            return Results.NoContent();
        });

        return app;
    }

    private static Task<Meal?> FindOwnedMeal(NutriCheckDbContext db, Guid id, Guid userId, CancellationToken ct) =>
        db.Meals.SingleOrDefaultAsync(m => m.Id == id && m.UserId == userId, ct);

    private static Guid CurrentUserId(HttpContext http) =>
        Guid.Parse(http.User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    private static string? Validate(MealRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            return "Name is required.";
        }

        if (request.MealType is null)
        {
            return "MealType is required.";
        }

        if (request.EatenAt is null)
        {
            return "EatenAt is required.";
        }

        if (request.Calories < 0 || request.ProteinGrams < 0 || request.CarbsGrams < 0 || request.FatGrams < 0)
        {
            return "Nutrient values must not be negative.";
        }

        return null;
    }
}
