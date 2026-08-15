using Microsoft.EntityFrameworkCore;
using NutriCheck.Domain;

namespace NutriCheck.Infrastructure.Persistence;

public class NutriCheckDbContext(DbContextOptions<NutriCheckDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();

    public DbSet<Meal> Meals => Set<Meal>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasIndex(u => u.Email).IsUnique();
            entity.Property(u => u.Email).IsRequired();
            entity.Property(u => u.PasswordHash).IsRequired();
        });

        modelBuilder.Entity<Meal>(entity =>
        {
            entity.Property(m => m.Name).IsRequired();
            entity.Property(m => m.Calories).HasColumnType("numeric");
            entity.Property(m => m.ProteinGrams).HasColumnType("numeric");
            entity.Property(m => m.CarbsGrams).HasColumnType("numeric");
            entity.Property(m => m.FatGrams).HasColumnType("numeric");
            entity.HasIndex(m => new { m.UserId, m.EatenAt });
            entity.HasOne<User>()
                .WithMany()
                .HasForeignKey(m => m.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });
    }
}
