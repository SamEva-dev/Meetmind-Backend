using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Meetmind.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SummarizeDetailLevel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "SummarizeDetailLevel",
                table: "Settings",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SummarizeDetailLevel",
                table: "Settings");
        }
    }
}
