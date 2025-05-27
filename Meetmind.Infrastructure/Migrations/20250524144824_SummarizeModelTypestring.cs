using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Meetmind.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SummarizeModelTypestring : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "SummarizeModelType",
                table: "Settings",
                type: "TEXT",
                nullable: false,
                oldClrType: typeof(int),
                oldType: "INTEGER");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<int>(
                name: "SummarizeModelType",
                table: "Settings",
                type: "INTEGER",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "TEXT");
        }
    }
}
