using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace ClubEngine.ApiService.Migrations
{
    /// <inheritdoc />
    public partial class AddFeeOverride : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<decimal>(
                name: "FeeOverride",
                table: "Members",
                type: "decimal(18,2)",
                precision: 18,
                scale: 2,
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FeeOverride",
                table: "Members");
        }
    }
}
