using Microsoft.EntityFrameworkCore;

namespace NutriCheck.Infrastructure.Persistence;

public class NutriCheckDbContext(DbContextOptions<NutriCheckDbContext> options) : DbContext(options)
{
}
