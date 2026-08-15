using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace NutriCheck.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMeals : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Meals",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    UserId = table.Column<Guid>(type: "uuid", nullable: false),
                    Name = table.Column<string>(type: "text", nullable: false),
                    Description = table.Column<string>(type: "text", nullable: true),
                    MealType = table.Column<int>(type: "integer", nullable: false),
                    EatenAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false),
                    Calories = table.Column<decimal>(type: "numeric", nullable: false),
                    ProteinGrams = table.Column<decimal>(type: "numeric", nullable: false),
                    CarbsGrams = table.Column<decimal>(type: "numeric", nullable: false),
                    FatGrams = table.Column<decimal>(type: "numeric", nullable: false),
                    CreatedAt = table.Column<DateTimeOffset>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Meals", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Meals_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Meals_UserId_EatenAt",
                table: "Meals",
                columns: new[] { "UserId", "EatenAt" });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Meals");
        }
    }
}
