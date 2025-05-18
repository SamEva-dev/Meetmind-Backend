using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Meetmind.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class addcolumncancelmeeting : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "source",
                table: "Meetings");

            migrationBuilder.AddColumn<bool>(
                name: "IsCancelled",
                table: "Meetings",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsCancelled",
                table: "Meetings");

            migrationBuilder.AddColumn<string>(
                name: "source",
                table: "Meetings",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }
    }
}
