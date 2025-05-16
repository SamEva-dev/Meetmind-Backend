using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Meetmind.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialcreateSetting : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Settings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Language = table.Column<string>(type: "TEXT", nullable: false),
                    AutoStartRecord = table.Column<bool>(type: "INTEGER", nullable: false),
                    AutoTranscript = table.Column<bool>(type: "INTEGER", nullable: false),
                    AutoSummarize = table.Column<bool>(type: "INTEGER", nullable: false),
                    AutoTranslate = table.Column<bool>(type: "INTEGER", nullable: false),
                    NotifyBeforeMinutesJson = table.Column<string>(type: "TEXT", nullable: false),
                    RequireConsent = table.Column<bool>(type: "INTEGER", nullable: false),
                    RetentionDays = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Settings", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Settings");
        }
    }
}
