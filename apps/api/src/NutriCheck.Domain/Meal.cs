namespace NutriCheck.Domain;

public class Meal
{
    public Guid Id { get; set; }

    public Guid UserId { get; set; }

    public required string Name { get; set; }

    public string? Description { get; set; }

    public MealType MealType { get; set; }

    public DateTimeOffset EatenAt { get; set; }

    public decimal Calories { get; set; }

    public decimal ProteinGrams { get; set; }

    public decimal CarbsGrams { get; set; }

    public decimal FatGrams { get; set; }

    public DateTimeOffset CreatedAt { get; set; }
}
