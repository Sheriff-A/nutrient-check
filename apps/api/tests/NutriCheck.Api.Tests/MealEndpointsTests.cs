using System.Net;
using System.Net.Http.Json;
using NutriCheck.Api.Meals;
using NutriCheck.Domain;
using Xunit;

namespace NutriCheck.Api.Tests;

public class MealEndpointsTests : IClassFixture<MealsApiFactory>
{
    private readonly MealsApiFactory _factory;

    public MealEndpointsTests(MealsApiFactory factory)
    {
        _factory = factory;
    }

    private static MealRequest ValidMeal(string name = "Oatmeal") => new(
        name,
        "With berries",
        MealType.Breakfast,
        DateTimeOffset.UtcNow,
        350,
        12,
        60,
        8);

    private async Task<HttpClient> CreateAuthenticatedClientAsync()
    {
        var client = _factory.CreateClient();
        var email = $"{Guid.NewGuid():N}@example.com";

        var registerResponse = await client.PostAsJsonAsync("/auth/register", new { Email = email, Password = "password123" });
        Assert.Equal(HttpStatusCode.Created, registerResponse.StatusCode);

        var loginResponse = await client.PostAsJsonAsync("/auth/login", new { Email = email, Password = "password123" });
        Assert.Equal(HttpStatusCode.OK, loginResponse.StatusCode);

        return client;
    }

    [Fact]
    public async Task Create_ReturnsCreatedMeal_WhenValid()
    {
        var client = await CreateAuthenticatedClientAsync();

        var response = await client.PostAsJsonAsync("/meals", ValidMeal());

        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
        var meal = await response.Content.ReadFromJsonAsync<MealResponse>();
        Assert.NotNull(meal);
        Assert.Equal("Oatmeal", meal!.Name);
    }

    [Fact]
    public async Task Create_ReturnsBadRequest_WhenNameMissing()
    {
        var client = await CreateAuthenticatedClientAsync();
        var request = ValidMeal() with { Name = "" };

        var response = await client.PostAsJsonAsync("/meals", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Create_ReturnsBadRequest_WhenNutrientValueNegative()
    {
        var client = await CreateAuthenticatedClientAsync();
        var request = ValidMeal() with { Calories = -1 };

        var response = await client.PostAsJsonAsync("/meals", request);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Create_ReturnsUnauthorized_WhenNotAuthenticated()
    {
        var client = _factory.CreateClient();

        var response = await client.PostAsJsonAsync("/meals", ValidMeal());

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task List_ReturnsOnlyCallersMeals()
    {
        var ownerClient = await CreateAuthenticatedClientAsync();
        var otherClient = await CreateAuthenticatedClientAsync();

        await ownerClient.PostAsJsonAsync("/meals", ValidMeal("Owner's Breakfast"));
        await otherClient.PostAsJsonAsync("/meals", ValidMeal("Other's Breakfast"));

        var response = await ownerClient.GetAsync("/meals");
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        var meals = await response.Content.ReadFromJsonAsync<List<MealResponse>>();
        Assert.NotNull(meals);
        Assert.Single(meals!);
        Assert.Equal("Owner's Breakfast", meals![0].Name);
    }

    [Fact]
    public async Task List_ReturnsUnauthorized_WhenNotAuthenticated()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/meals");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Get_ReturnsMeal_WhenOwned()
    {
        var client = await CreateAuthenticatedClientAsync();
        var created = await (await client.PostAsJsonAsync("/meals", ValidMeal())).Content.ReadFromJsonAsync<MealResponse>();

        var response = await client.GetAsync($"/meals/{created!.Id}");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Get_ReturnsNotFound_WhenOwnedByAnotherUser()
    {
        var ownerClient = await CreateAuthenticatedClientAsync();
        var otherClient = await CreateAuthenticatedClientAsync();
        var created = await (await ownerClient.PostAsJsonAsync("/meals", ValidMeal())).Content.ReadFromJsonAsync<MealResponse>();

        var response = await otherClient.GetAsync($"/meals/{created!.Id}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Update_UpdatesMeal_WhenOwned()
    {
        var client = await CreateAuthenticatedClientAsync();
        var created = await (await client.PostAsJsonAsync("/meals", ValidMeal())).Content.ReadFromJsonAsync<MealResponse>();
        var update = ValidMeal("Updated Oatmeal");

        var response = await client.PatchAsJsonAsync($"/meals/{created!.Id}", update);

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var updated = await response.Content.ReadFromJsonAsync<MealResponse>();
        Assert.Equal("Updated Oatmeal", updated!.Name);
    }

    [Fact]
    public async Task Update_ReturnsBadRequest_WhenNutrientValueNegative()
    {
        var client = await CreateAuthenticatedClientAsync();
        var created = await (await client.PostAsJsonAsync("/meals", ValidMeal())).Content.ReadFromJsonAsync<MealResponse>();
        var update = ValidMeal() with { ProteinGrams = -5 };

        var response = await client.PatchAsJsonAsync($"/meals/{created!.Id}", update);

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Update_ReturnsNotFound_WhenOwnedByAnotherUser()
    {
        var ownerClient = await CreateAuthenticatedClientAsync();
        var otherClient = await CreateAuthenticatedClientAsync();
        var created = await (await ownerClient.PostAsJsonAsync("/meals", ValidMeal())).Content.ReadFromJsonAsync<MealResponse>();

        var response = await otherClient.PatchAsJsonAsync($"/meals/{created!.Id}", ValidMeal("Hijacked"));

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task Delete_RemovesMeal_WhenOwned()
    {
        var client = await CreateAuthenticatedClientAsync();
        var created = await (await client.PostAsJsonAsync("/meals", ValidMeal())).Content.ReadFromJsonAsync<MealResponse>();

        var deleteResponse = await client.DeleteAsync($"/meals/{created!.Id}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        var getResponse = await client.GetAsync($"/meals/{created.Id}");
        Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
    }

    [Fact]
    public async Task Delete_ReturnsNotFound_WhenOwnedByAnotherUser()
    {
        var ownerClient = await CreateAuthenticatedClientAsync();
        var otherClient = await CreateAuthenticatedClientAsync();
        var created = await (await ownerClient.PostAsJsonAsync("/meals", ValidMeal())).Content.ReadFromJsonAsync<MealResponse>();

        var response = await otherClient.DeleteAsync($"/meals/{created!.Id}");

        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);

        var ownerGetResponse = await ownerClient.GetAsync($"/meals/{created.Id}");
        Assert.Equal(HttpStatusCode.OK, ownerGetResponse.StatusCode);
    }
}
