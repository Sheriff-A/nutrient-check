using NutriCheck.Domain;

namespace NutriCheck.Api.Meals;

public record MealRequest(
    string Name,
    string? Description,
    MealType? MealType,
    DateTimeOffset? EatenAt,
    decimal Calories,
    decimal ProteinGrams,
    decimal CarbsGrams,
    decimal FatGrams);

public record MealResponse(
    Guid Id,
    string Name,
    string? Description,
    MealType MealType,
    DateTimeOffset EatenAt,
    decimal Calories,
    decimal ProteinGrams,
    decimal CarbsGrams,
    decimal FatGrams,
    DateTimeOffset CreatedAt)
{
    public static MealResponse FromEntity(Meal meal) => new(
        meal.Id,
        meal.Name,
        meal.Description,
        meal.MealType,
        meal.EatenAt,
        meal.Calories,
        meal.ProteinGrams,
        meal.CarbsGrams,
        meal.FatGrams,
        meal.CreatedAt);
}
