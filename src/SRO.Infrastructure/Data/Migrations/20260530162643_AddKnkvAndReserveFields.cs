using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SRO.Infrastructure.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddKnkvAndReserveFields : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsKnkvMatch",
                table: "Matches",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddColumn<bool>(
                name: "IsReserveAssignment",
                table: "Matches",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsKnkvMatch",
                table: "Matches");

            migrationBuilder.DropColumn(
                name: "IsReserveAssignment",
                table: "Matches");
        }
    }
}
