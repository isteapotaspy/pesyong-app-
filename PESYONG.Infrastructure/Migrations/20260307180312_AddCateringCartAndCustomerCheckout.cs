using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PESYONG.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddCateringCartAndCustomerCheckout : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MealProduct_AppUser_OwnerID",
                table: "MealProduct");

            migrationBuilder.DropForeignKey(
                name: "FK_MealProduct_Promos_PromoID",
                table: "MealProduct");

            migrationBuilder.DropForeignKey(
                name: "FK_MealProductItem_MealProduct_MealProductID",
                table: "MealProductItem");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderMealProducts_MealProduct_MealProductID",
                table: "OrderMealProducts");

            migrationBuilder.DropForeignKey(
                name: "FK_Orders_AppUser_RecipientID",
                table: "Orders");

            migrationBuilder.DropIndex(
                name: "IX_Orders_RecipientID",
                table: "Orders");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MealProduct",
                table: "MealProduct");

            migrationBuilder.DropColumn(
                name: "RecipientID",
                table: "Orders");

            migrationBuilder.RenameTable(
                name: "MealProduct",
                newName: "MealProducts");

            migrationBuilder.RenameIndex(
                name: "IX_MealProduct_PromoID",
                table: "MealProducts",
                newName: "IX_MealProducts_PromoID");

            migrationBuilder.RenameIndex(
                name: "IX_MealProduct_OwnerID",
                table: "MealProducts",
                newName: "IX_MealProducts_OwnerID");

            migrationBuilder.AddColumn<Guid>(
                name: "CustomerID",
                table: "Orders",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Password",
                table: "AppUser",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AlterColumn<int>(
                name: "OwnerID",
                table: "MealProducts",
                type: "int",
                nullable: true,
                oldClrType: typeof(int),
                oldType: "int");

            migrationBuilder.AddColumn<bool>(
                name: "IsCateringPackage",
                table: "MealProducts",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddPrimaryKey(
                name: "PK_MealProducts",
                table: "MealProducts",
                column: "MealProductID");

            migrationBuilder.CreateTable(
                name: "Customers",
                columns: table => new
                {
                    CustomerID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: true),
                    PhoneNumber = table.Column<string>(type: "nvarchar(20)", maxLength: 20, nullable: false),
                    Address = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: true),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Customers", x => x.CustomerID);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Orders_CustomerID",
                table: "Orders",
                column: "CustomerID");

            migrationBuilder.AddForeignKey(
                name: "FK_MealProductItem_MealProducts_MealProductID",
                table: "MealProductItem",
                column: "MealProductID",
                principalTable: "MealProducts",
                principalColumn: "MealProductID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MealProducts_AppUser_OwnerID",
                table: "MealProducts",
                column: "OwnerID",
                principalTable: "AppUser",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_MealProducts_Promos_PromoID",
                table: "MealProducts",
                column: "PromoID",
                principalTable: "Promos",
                principalColumn: "PromoID");

            migrationBuilder.AddForeignKey(
                name: "FK_OrderMealProducts_MealProducts_MealProductID",
                table: "OrderMealProducts",
                column: "MealProductID",
                principalTable: "MealProducts",
                principalColumn: "MealProductID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_Customers_CustomerID",
                table: "Orders",
                column: "CustomerID",
                principalTable: "Customers",
                principalColumn: "CustomerID",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_MealProductItem_MealProducts_MealProductID",
                table: "MealProductItem");

            migrationBuilder.DropForeignKey(
                name: "FK_MealProducts_AppUser_OwnerID",
                table: "MealProducts");

            migrationBuilder.DropForeignKey(
                name: "FK_MealProducts_Promos_PromoID",
                table: "MealProducts");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderMealProducts_MealProducts_MealProductID",
                table: "OrderMealProducts");

            migrationBuilder.DropForeignKey(
                name: "FK_Orders_Customers_CustomerID",
                table: "Orders");

            migrationBuilder.DropTable(
                name: "Customers");

            migrationBuilder.DropIndex(
                name: "IX_Orders_CustomerID",
                table: "Orders");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MealProducts",
                table: "MealProducts");

            migrationBuilder.DropColumn(
                name: "CustomerID",
                table: "Orders");

            migrationBuilder.DropColumn(
                name: "Password",
                table: "AppUser");

            migrationBuilder.DropColumn(
                name: "IsCateringPackage",
                table: "MealProducts");

            migrationBuilder.RenameTable(
                name: "MealProducts",
                newName: "MealProduct");

            migrationBuilder.RenameIndex(
                name: "IX_MealProducts_PromoID",
                table: "MealProduct",
                newName: "IX_MealProduct_PromoID");

            migrationBuilder.RenameIndex(
                name: "IX_MealProducts_OwnerID",
                table: "MealProduct",
                newName: "IX_MealProduct_OwnerID");

            migrationBuilder.AddColumn<int>(
                name: "RecipientID",
                table: "Orders",
                type: "int",
                nullable: true);

            migrationBuilder.AlterColumn<int>(
                name: "OwnerID",
                table: "MealProduct",
                type: "int",
                nullable: false,
                defaultValue: 0,
                oldClrType: typeof(int),
                oldType: "int",
                oldNullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_MealProduct",
                table: "MealProduct",
                column: "MealProductID");

            migrationBuilder.CreateIndex(
                name: "IX_Orders_RecipientID",
                table: "Orders",
                column: "RecipientID");

            migrationBuilder.AddForeignKey(
                name: "FK_MealProduct_AppUser_OwnerID",
                table: "MealProduct",
                column: "OwnerID",
                principalTable: "AppUser",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MealProduct_Promos_PromoID",
                table: "MealProduct",
                column: "PromoID",
                principalTable: "Promos",
                principalColumn: "PromoID");

            migrationBuilder.AddForeignKey(
                name: "FK_MealProductItem_MealProduct_MealProductID",
                table: "MealProductItem",
                column: "MealProductID",
                principalTable: "MealProduct",
                principalColumn: "MealProductID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_OrderMealProducts_MealProduct_MealProductID",
                table: "OrderMealProducts",
                column: "MealProductID",
                principalTable: "MealProduct",
                principalColumn: "MealProductID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_AppUser_RecipientID",
                table: "Orders",
                column: "RecipientID",
                principalTable: "AppUser",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }
    }
}
