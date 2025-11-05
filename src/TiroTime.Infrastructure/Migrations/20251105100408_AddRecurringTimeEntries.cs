using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TiroTime.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRecurringTimeEntries : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "RecurringTimeEntryId",
                table: "TimeEntries",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "RecurringTimeEntries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    UserId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    ProjectId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    StartTime = table.Column<TimeSpan>(type: "time", nullable: false),
                    EndTime = table.Column<TimeSpan>(type: "time", nullable: false),
                    Pattern_Frequency = table.Column<int>(type: "int", nullable: false),
                    Pattern_Interval = table.Column<int>(type: "int", nullable: false),
                    Pattern_DaysOfWeek = table.Column<string>(type: "nvarchar(max)", nullable: true),
                    Pattern_DayOfMonth = table.Column<int>(type: "int", nullable: true),
                    Pattern_StartDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    Pattern_EndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Pattern_MaxOccurrences = table.Column<int>(type: "int", nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false),
                    LastGeneratedDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecurringTimeEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_RecurringTimeEntries_Projects_ProjectId",
                        column: x => x.ProjectId,
                        principalTable: "Projects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TimeEntries_RecurringTimeEntryId",
                table: "TimeEntries",
                column: "RecurringTimeEntryId");

            migrationBuilder.CreateIndex(
                name: "IX_RecurringTimeEntries_IsActive",
                table: "RecurringTimeEntries",
                column: "IsActive");

            migrationBuilder.CreateIndex(
                name: "IX_RecurringTimeEntries_IsActive_LastGeneratedDate",
                table: "RecurringTimeEntries",
                columns: new[] { "IsActive", "LastGeneratedDate" },
                filter: "[IsActive] = 1");

            migrationBuilder.CreateIndex(
                name: "IX_RecurringTimeEntries_ProjectId",
                table: "RecurringTimeEntries",
                column: "ProjectId");

            migrationBuilder.CreateIndex(
                name: "IX_RecurringTimeEntries_UserId",
                table: "RecurringTimeEntries",
                column: "UserId");

            migrationBuilder.AddForeignKey(
                name: "FK_TimeEntries_RecurringTimeEntries_RecurringTimeEntryId",
                table: "TimeEntries",
                column: "RecurringTimeEntryId",
                principalTable: "RecurringTimeEntries",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TimeEntries_RecurringTimeEntries_RecurringTimeEntryId",
                table: "TimeEntries");

            migrationBuilder.DropTable(
                name: "RecurringTimeEntries");

            migrationBuilder.DropIndex(
                name: "IX_TimeEntries_RecurringTimeEntryId",
                table: "TimeEntries");

            migrationBuilder.DropColumn(
                name: "RecurringTimeEntryId",
                table: "TimeEntries");
        }
    }
}
