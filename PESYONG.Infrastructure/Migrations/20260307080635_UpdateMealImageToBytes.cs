using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PESYONG.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class UpdateMealImageToBytes : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageSourceString",
                table: "Meals");

            migrationBuilder.AddColumn<byte[]>(
                name: "ImageBytes",
                table: "Meals",
                type: "varbinary(max)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Meals_OperatorID",
                table: "Meals",
                column: "OperatorID");

            migrationBuilder.AddForeignKey(
                name: "FK_Meals_AppUser_OperatorID",
                table: "Meals",
                column: "OperatorID",
                principalTable: "AppUser",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Meals_AppUser_OperatorID",
                table: "Meals");

            migrationBuilder.DropIndex(
                name: "IX_Meals_OperatorID",
                table: "Meals");

            migrationBuilder.DropColumn(
                name: "ImageBytes",
                table: "Meals");

            migrationBuilder.AddColumn<string>(
                name: "ImageSourceString",
                table: "Meals",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
