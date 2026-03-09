using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PESYONG.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCustomizableFieldsToMealProduct : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "AllowPreferredViands",
                table: "MealProducts",
                newName: "IsCustomizable");

            migrationBuilder.AddColumn<bool>(
                name: "IsViandOption",
                table: "Meals",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsViandOption",
                table: "Meals");

            migrationBuilder.RenameColumn(
                name: "IsCustomizable",
                table: "MealProducts",
                newName: "AllowPreferredViands");
        }
    }
}
