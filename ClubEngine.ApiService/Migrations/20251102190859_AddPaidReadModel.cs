using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClubEngine.ApiService.Migrations
{
    /// <inheritdoc />
    public partial class AddPaidReadModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "QueryName",
                table: "ReadModels",
                type: "nvarchar(200)",
                maxLength: 200,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddColumn<decimal>(
                name: "AmountPaid_ReadModel",
                table: "MembershipFees",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: false,
                defaultValue: 0m);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AmountPaid_ReadModel",
                table: "MembershipFees");

            migrationBuilder.AlterColumn<string>(
                name: "QueryName",
                table: "ReadModels",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(200)",
                oldMaxLength: 200);
        }
    }
}
