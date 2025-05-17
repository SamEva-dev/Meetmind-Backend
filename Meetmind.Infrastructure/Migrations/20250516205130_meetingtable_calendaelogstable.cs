using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Meetmind.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class meetingtable_calendaelogstable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "CalendarSyncLogs",
                columns: table => new
                {
                    Id = table.Column<int>(type: "INTEGER", nullable: false)
                        .Annotation("Sqlite:Autoincrement", true),
                    TimestampUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    Source = table.Column<string>(type: "TEXT", nullable: false),
                    TotalEventsFound = table.Column<int>(type: "INTEGER", nullable: false),
                    MeetingsCreated = table.Column<int>(type: "INTEGER", nullable: false),
                    ErrorMessage = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CalendarSyncLogs", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MeetingReadModels",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Title = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    StartUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    EndUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    State = table.Column<string>(type: "TEXT", nullable: false),
                    TranscriptPath = table.Column<string>(type: "TEXT", nullable: true),
                    SummaryPath = table.Column<string>(type: "TEXT", nullable: true),
                    ExternalId = table.Column<string>(type: "TEXT", nullable: true),
                    ExternalSource = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MeetingReadModels", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Meetings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Title = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    ExternalId = table.Column<string>(type: "TEXT", maxLength: 256, nullable: true),
                    ExternalSource = table.Column<string>(type: "TEXT", maxLength: 50, nullable: true),
                    StartUtc = table.Column<DateTime>(type: "TEXT", nullable: false),
                    EndUtc = table.Column<DateTime>(type: "TEXT", nullable: true),
                    State = table.Column<int>(type: "INTEGER", nullable: false),
                    TranscriptState = table.Column<int>(type: "INTEGER", nullable: false),
                    TranscriptPath = table.Column<string>(type: "TEXT", nullable: true),
                    SummaryState = table.Column<int>(type: "INTEGER", nullable: false),
                    SummaryPath = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Meetings", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MeetingReadModels_StartUtc",
                table: "MeetingReadModels",
                column: "StartUtc");

            migrationBuilder.CreateIndex(
                name: "IX_Meetings_ExternalId_ExternalSource",
                table: "Meetings",
                columns: new[] { "ExternalId", "ExternalSource" },
                unique: true,
                filter: "[ExternalId] IS NOT NULL AND [ExternalSource] IS NOT NULL");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CalendarSyncLogs");

            migrationBuilder.DropTable(
                name: "MeetingReadModels");

            migrationBuilder.DropTable(
                name: "Meetings");
        }
    }
}
