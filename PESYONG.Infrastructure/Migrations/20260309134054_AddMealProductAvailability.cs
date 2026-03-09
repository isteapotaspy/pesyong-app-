using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PESYONG.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddMealProductAvailability : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsAvailable",
                table: "MealProducts",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsAvailable",
                table: "MealProducts");
        }
    }
}
