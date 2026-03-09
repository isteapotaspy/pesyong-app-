using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PESYONG.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SyncOrderRelatedColumns : Migration
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
                name: "FK_MealProducts_AppUser_OwnerID",
                table: "MealProducts");

            migrationBuilder.DropForeignKey(
                name: "FK_Meals_AppUser_OperatorID",
                table: "Meals");

            migrationBuilder.DropForeignKey(
                name: "FK_Orders_AppUser_AppUserId",
                table: "Orders");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AppUser",
                table: "AppUser");

            migrationBuilder.RenameTable(
                name: "AppUser",
                newName: "AppUsers");

            migrationBuilder.AddColumn<byte[]>(
                name: "ImageBytes",
                table: "MealProducts",
                type: "varbinary(max)",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "PaxCount",
                table: "MealProducts",
                type: "int",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddPrimaryKey(
                name: "PK_AppUsers",
                table: "AppUsers",
                column: "Id");

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
                name: "FK_MealProducts_AppUsers_OwnerID",
                table: "MealProducts",
                column: "OwnerID",
                principalTable: "AppUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Meals_AppUsers_OperatorID",
                table: "Meals",
                column: "OperatorID",
                principalTable: "AppUsers",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_AppUsers_AppUserId",
                table: "Orders",
                column: "AppUserId",
                principalTable: "AppUsers",
                principalColumn: "Id");
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
                name: "FK_MealProducts_AppUsers_OwnerID",
                table: "MealProducts");

            migrationBuilder.DropForeignKey(
                name: "FK_Meals_AppUsers_OperatorID",
                table: "Meals");

            migrationBuilder.DropForeignKey(
                name: "FK_Orders_AppUsers_AppUserId",
                table: "Orders");

            migrationBuilder.DropPrimaryKey(
                name: "PK_AppUsers",
                table: "AppUsers");

            migrationBuilder.DropColumn(
                name: "ImageBytes",
                table: "MealProducts");

            migrationBuilder.DropColumn(
                name: "PaxCount",
                table: "MealProducts");

            migrationBuilder.RenameTable(
                name: "AppUsers",
                newName: "AppUser");

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
                name: "FK_MealProducts_AppUser_OwnerID",
                table: "MealProducts",
                column: "OwnerID",
                principalTable: "AppUser",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Meals_AppUser_OperatorID",
                table: "Meals",
                column: "OperatorID",
                principalTable: "AppUser",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Orders_AppUser_AppUserId",
                table: "Orders",
                column: "AppUserId",
                principalTable: "AppUser",
                principalColumn: "Id");
        }
    }
}
