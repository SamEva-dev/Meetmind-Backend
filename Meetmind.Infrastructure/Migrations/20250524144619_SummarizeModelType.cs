﻿using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Meetmind.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class SummarizeModelType : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "SummarizeModelType",
                table: "Settings",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "SummarizeModelType",
                table: "Settings");
        }
    }
}
