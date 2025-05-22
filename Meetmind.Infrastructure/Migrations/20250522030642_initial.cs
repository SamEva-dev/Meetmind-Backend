using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Meetmind.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class initial : Migration
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
                    Source = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
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
                    Start = table.Column<DateTime>(type: "TEXT", nullable: false),
                    End = table.Column<DateTime>(type: "TEXT", nullable: true),
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
                    Start = table.Column<DateTime>(type: "TEXT", nullable: false),
                    End = table.Column<DateTime>(type: "TEXT", nullable: true),
                    State = table.Column<int>(type: "INTEGER", nullable: false),
                    TranscriptState = table.Column<int>(type: "INTEGER", nullable: false),
                    TranscriptPath = table.Column<string>(type: "TEXT", nullable: true),
                    SummaryState = table.Column<int>(type: "INTEGER", nullable: false),
                    SummaryPath = table.Column<string>(type: "TEXT", nullable: true),
                    IsCancelled = table.Column<bool>(type: "INTEGER", nullable: false),
                    AudioPath = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Meetings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Settings",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Language = table.Column<string>(type: "TEXT", nullable: false),
                    AutoStartRecord = table.Column<bool>(type: "INTEGER", nullable: false),
                    AutoStopRecord = table.Column<bool>(type: "INTEGER", nullable: false),
                    AutoTranscript = table.Column<bool>(type: "INTEGER", nullable: false),
                    AutoSummarize = table.Column<bool>(type: "INTEGER", nullable: false),
                    AutoTranslate = table.Column<bool>(type: "INTEGER", nullable: false),
                    NotifyBeforeMinutes = table.Column<int>(type: "INTEGER", nullable: false),
                    NotificationRepeatInterval = table.Column<int>(type: "INTEGER", nullable: false),
                    RequireConsent = table.Column<bool>(type: "INTEGER", nullable: false),
                    UseGoogleCalendar = table.Column<bool>(type: "INTEGER", nullable: false),
                    UseOutlookCalendar = table.Column<bool>(type: "INTEGER", nullable: false),
                    RetentionDays = table.Column<int>(type: "INTEGER", nullable: false),
                    AutoCancelMeeting = table.Column<bool>(type: "INTEGER", nullable: false),
                    AutoDeleteMeeting = table.Column<bool>(type: "INTEGER", nullable: false),
                    TranscriptionType = table.Column<string>(type: "TEXT", nullable: false),
                    AudioRecordingType = table.Column<string>(type: "TEXT", nullable: false),
                    WhisperModelType = table.Column<string>(type: "TEXT", nullable: false),
                    WhisperDeviceType = table.Column<string>(type: "TEXT", nullable: false),
                    WhisperComputeType = table.Column<string>(type: "TEXT", nullable: false),
                    DiarizationModelType = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Settings", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Transcriptions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Tilte = table.Column<string>(type: "TEXT", maxLength: 255, nullable: false),
                    SourceFile = table.Column<string>(type: "TEXT", nullable: false),
                    Text = table.Column<string>(type: "TEXT", nullable: false),
                    Language = table.Column<string>(type: "TEXT", nullable: false),
                    LanguageProbability = table.Column<double>(type: "REAL", nullable: true),
                    Duration = table.Column<double>(type: "REAL", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "TEXT", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    Speakers = table.Column<string>(type: "TEXT", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Transcriptions", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Segments",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    TranscriptionId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Speaker = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Text = table.Column<string>(type: "TEXT", nullable: false),
                    Start = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false),
                    End = table.Column<string>(type: "TEXT", maxLength: 20, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Segments", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Segments_Transcriptions_TranscriptionId",
                        column: x => x.TranscriptionId,
                        principalTable: "Transcriptions",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MeetingReadModels_StartUtc",
                table: "MeetingReadModels",
                column: "StartUtc");

            migrationBuilder.CreateIndex(
                name: "IX_Meetings_ExternalId_ExternalSource",
                table: "Meetings",
                columns: new[] { "ExternalId", "ExternalSource" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Segments_TranscriptionId",
                table: "Segments",
                column: "TranscriptionId");
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

            migrationBuilder.DropTable(
                name: "Segments");

            migrationBuilder.DropTable(
                name: "Settings");

            migrationBuilder.DropTable(
                name: "Transcriptions");
        }
    }
}
