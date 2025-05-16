using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Meetmind.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class updatesettingcolumn : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NotifyBeforeMinutesJson",
                table: "Settings");

            migrationBuilder.AddColumn<int>(
                name: "NotificationRepeatInterval",
                table: "Settings",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<int>(
                name: "NotifyBeforeMinutes",
                table: "Settings",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "NotificationRepeatInterval",
                table: "Settings");

            migrationBuilder.DropColumn(
                name: "NotifyBeforeMinutes",
                table: "Settings");

            migrationBuilder.AddColumn<string>(
                name: "NotifyBeforeMinutesJson",
                table: "Settings",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }
    }
}
