using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClubEngine.ApiService.Migrations
{
    /// <inheritdoc />
    public partial class SourceIdType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ExternalId",
                table: "PayoutRequests",
                newName: "SourceId");

            migrationBuilder.RenameColumn(
                name: "SegmentId",
                table: "PaymentAssignments",
                newName: "SourceId");

            migrationBuilder.AddColumn<Guid>(
                name: "PartitionId",
                table: "PayoutRequests",
                type: "uniqueidentifier",
                nullable: false,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.AddColumn<string>(
                name: "SourceType",
                table: "PayoutRequests",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "SourceType",
                table: "PaymentAssignments",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_PayoutRequests_PartitionId",
                table: "PayoutRequests",
                column: "PartitionId");

            migrationBuilder.AddForeignKey(
                name: "FK_PayoutRequests_Clubs_PartitionId",
                table: "PayoutRequests",
                column: "PartitionId",
                principalTable: "Clubs",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_PayoutRequests_Clubs_PartitionId",
                table: "PayoutRequests");

            migrationBuilder.DropIndex(
                name: "IX_PayoutRequests_PartitionId",
                table: "PayoutRequests");

            migrationBuilder.DropColumn(
                name: "PartitionId",
                table: "PayoutRequests");

            migrationBuilder.DropColumn(
                name: "SourceType",
                table: "PayoutRequests");

            migrationBuilder.DropColumn(
                name: "SourceType",
                table: "PaymentAssignments");

            migrationBuilder.RenameColumn(
                name: "SourceId",
                table: "PayoutRequests",
                newName: "ExternalId");

            migrationBuilder.RenameColumn(
                name: "SourceId",
                table: "PaymentAssignments",
                newName: "SegmentId");
        }
    }
}
