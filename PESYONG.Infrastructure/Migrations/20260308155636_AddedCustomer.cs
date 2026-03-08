using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PESYONG.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddedCustomer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AcknowledgementReceipts_AppUser_AppUserId",
                table: "AcknowledgementReceipts");

            migrationBuilder.DropForeignKey(
                name: "FK_AcknowledgementReceipts_AppUser_CustomerID",
                table: "AcknowledgementReceipts");

            migrationBuilder.DropForeignKey(
                name: "FK_AuditLogs_AppUser_UserID",
                table: "AuditLogs");

            migrationBuilder.DropForeignKey(
                name: "FK_Deliveries_AppUser_DeliveryPersonnelID",
                table: "Deliveries");

            migrationBuilder.DropForeignKey(
                name: "FK_DeliveryUpdates_AppUser_UpdatedById",
                table: "DeliveryUpdates");

            migrationBuilder.DropForeignKey(
                name: "FK_DeliveryUpdates_AppUser_UpdatedByUserID",
                table: "DeliveryUpdates");

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
                name: "FK_Meals_AppUser_OperatorID",
                table: "Meals");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderMealProducts_MealProduct_MealProductID",
                table: "OrderMealProducts");

            migrationBuilder.DropForeignKey(
                name: "FK_Orders_AppUser_AppUserId",
                table: "Orders");

            migrationBuilder.DropForeignKey(
                name: "FK_Orders_AppUser_RecipientID",
                table: "Orders");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MealProduct",
                table: "MealProduct");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AppUser",
                table: "AppUser");

            migrationBuilder.RenameTable(
                name: "MealProduct",
                newName: "MealProducts");

            migrationBuilder.RenameTable(
                name: "AppUser",
                newName: "AppUsers");

            migrationBuilder.RenameIndex(
                name: "IX_MealProduct_PromoID",
                table: "MealProducts",
                newName: "IX_MealProducts_PromoID");

            migrationBuilder.RenameIndex(
                name: "IX_MealProduct_OwnerID",
                table: "MealProducts",
                newName: "IX_MealProducts_OwnerID");

            migrationBuilder.AddColumn<bool>(
                name: "IsCateringPackage",
                table: "MealProducts",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<string>(
                name: "Password",
                table: "AppUsers",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_MealProducts",
                table: "MealProducts",
                column: "MealProductID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AppUsers",
                table: "AppUsers",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "Customers",
                columns: table => new
                {
                    CustomerID = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    FirstName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    LastName = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Email = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    Address = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Customers", x => x.CustomerID);
                });

            migrationBuilder.AddForeignKey(
                name: "FK_AcknowledgementReceipts_AppUsers_AppUserId",
                table: "AcknowledgementReceipts",
                column: "AppUserId",
                principalTable: "AppUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_AcknowledgementReceipts_AppUsers_CustomerID",
                table: "AcknowledgementReceipts",
                column: "CustomerID",
                principalTable: "AppUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_AuditLogs_AppUsers_UserID",
                table: "AuditLogs",
                column: "UserID",
                principalTable: "AppUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Deliveries_AppUsers_DeliveryPersonnelID",
                table: "Deliveries",
                column: "DeliveryPersonnelID",
                principalTable: "AppUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_DeliveryUpdates_AppUsers_UpdatedById",
                table: "DeliveryUpdates",
                column: "UpdatedById",
                principalTable: "AppUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DeliveryUpdates_AppUsers_UpdatedByUserID",
                table: "DeliveryUpdates",
                column: "UpdatedByUserID",
                principalTable: "AppUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_MealProductItem_MealProducts_MealProductID",
                table: "MealProductItem",
                column: "MealProductID",
                principalTable: "MealProducts",
                principalColumn: "MealProductID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MealProducts_AppUsers_OwnerID",
                table: "MealProducts",
                column: "OwnerID",
                principalTable: "AppUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_MealProducts_Promos_PromoID",
                table: "MealProducts",
                column: "PromoID",
                principalTable: "Promos",
                principalColumn: "PromoID");

            migrationBuilder.AddForeignKey(
                name: "FK_Meals_AppUsers_OperatorID",
                table: "Meals",
                column: "OperatorID",
                principalTable: "AppUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_OrderMealProducts_MealProducts_MealProductID",
                table: "OrderMealProducts",
                column: "MealProductID",
                principalTable: "MealProducts",
                principalColumn: "MealProductID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_AppUsers_AppUserId",
                table: "Orders",
                column: "AppUserId",
                principalTable: "AppUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_AppUsers_RecipientID",
                table: "Orders",
                column: "RecipientID",
                principalTable: "AppUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_AcknowledgementReceipts_AppUsers_AppUserId",
                table: "AcknowledgementReceipts");

            migrationBuilder.DropForeignKey(
                name: "FK_AcknowledgementReceipts_AppUsers_CustomerID",
                table: "AcknowledgementReceipts");

            migrationBuilder.DropForeignKey(
                name: "FK_AuditLogs_AppUsers_UserID",
                table: "AuditLogs");

            migrationBuilder.DropForeignKey(
                name: "FK_Deliveries_AppUsers_DeliveryPersonnelID",
                table: "Deliveries");

            migrationBuilder.DropForeignKey(
                name: "FK_DeliveryUpdates_AppUsers_UpdatedById",
                table: "DeliveryUpdates");

            migrationBuilder.DropForeignKey(
                name: "FK_DeliveryUpdates_AppUsers_UpdatedByUserID",
                table: "DeliveryUpdates");

            migrationBuilder.DropForeignKey(
                name: "FK_MealProductItem_MealProducts_MealProductID",
                table: "MealProductItem");

            migrationBuilder.DropForeignKey(
                name: "FK_MealProducts_AppUsers_OwnerID",
                table: "MealProducts");

            migrationBuilder.DropForeignKey(
                name: "FK_MealProducts_Promos_PromoID",
                table: "MealProducts");

            migrationBuilder.DropForeignKey(
                name: "FK_Meals_AppUsers_OperatorID",
                table: "Meals");

            migrationBuilder.DropForeignKey(
                name: "FK_OrderMealProducts_MealProducts_MealProductID",
                table: "OrderMealProducts");

            migrationBuilder.DropForeignKey(
                name: "FK_Orders_AppUsers_AppUserId",
                table: "Orders");

            migrationBuilder.DropForeignKey(
                name: "FK_Orders_AppUsers_RecipientID",
                table: "Orders");

            migrationBuilder.DropTable(
                name: "Customers");

            migrationBuilder.DropPrimaryKey(
                name: "PK_MealProducts",
                table: "MealProducts");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AppUsers",
                table: "AppUsers");

            migrationBuilder.DropColumn(
                name: "IsCateringPackage",
                table: "MealProducts");

            migrationBuilder.DropColumn(
                name: "Password",
                table: "AppUsers");

            migrationBuilder.RenameTable(
                name: "MealProducts",
                newName: "MealProduct");

            migrationBuilder.RenameTable(
                name: "AppUsers",
                newName: "AppUser");

            migrationBuilder.RenameIndex(
                name: "IX_MealProducts_PromoID",
                table: "MealProduct",
                newName: "IX_MealProduct_PromoID");

            migrationBuilder.RenameIndex(
                name: "IX_MealProducts_OwnerID",
                table: "MealProduct",
                newName: "IX_MealProduct_OwnerID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_MealProduct",
                table: "MealProduct",
                column: "MealProductID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_AppUser",
                table: "AppUser",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_AcknowledgementReceipts_AppUser_AppUserId",
                table: "AcknowledgementReceipts",
                column: "AppUserId",
                principalTable: "AppUser",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_AcknowledgementReceipts_AppUser_CustomerID",
                table: "AcknowledgementReceipts",
                column: "CustomerID",
                principalTable: "AppUser",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_AuditLogs_AppUser_UserID",
                table: "AuditLogs",
                column: "UserID",
                principalTable: "AppUser",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Deliveries_AppUser_DeliveryPersonnelID",
                table: "Deliveries",
                column: "DeliveryPersonnelID",
                principalTable: "AppUser",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_DeliveryUpdates_AppUser_UpdatedById",
                table: "DeliveryUpdates",
                column: "UpdatedById",
                principalTable: "AppUser",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_DeliveryUpdates_AppUser_UpdatedByUserID",
                table: "DeliveryUpdates",
                column: "UpdatedByUserID",
                principalTable: "AppUser",
                principalColumn: "Id");

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
                name: "FK_Meals_AppUser_OperatorID",
                table: "Meals",
                column: "OperatorID",
                principalTable: "AppUser",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_OrderMealProducts_MealProduct_MealProductID",
                table: "OrderMealProducts",
                column: "MealProductID",
                principalTable: "MealProduct",
                principalColumn: "MealProductID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_AppUser_AppUserId",
                table: "Orders",
                column: "AppUserId",
                principalTable: "AppUser",
                principalColumn: "Id");

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
