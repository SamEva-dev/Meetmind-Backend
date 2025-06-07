﻿using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Meetmind.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class LiveTranscriptionEnabled : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "LiveTranscriptionEnabled",
                table: "Settings",
                type: "INTEGER",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "LiveTranscriptionEnabled",
                table: "Settings");
        }
    }
}
