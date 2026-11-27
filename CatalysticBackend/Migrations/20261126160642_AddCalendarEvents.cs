using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace CatalysticBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddCalendarEvents : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Pillar",
                table: "ClientTasks",
                type: "character varying(20)",
                maxLength: 20,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20);

            migrationBuilder.AddColumn<DateTime>(
                name: "EndsAt",
                table: "ClientTasks",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "EventType",
                table: "ClientTasks",
                type: "character varying(13)",
                maxLength: 13,
                nullable: false,
                defaultValue: "Task");

            migrationBuilder.AddColumn<DateTime>(
                name: "StartsAt",
                table: "ClientTasks",
                type: "timestamp with time zone",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "CallRequests",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ClientId = table.Column<int>(type: "integer", nullable: false),
                    Message = table.Column<string>(type: "character varying(1000)", maxLength: 1000, nullable: false),
                    Status = table.Column<string>(type: "character varying(16)", maxLength: 16, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ResolvedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    ScheduledCallId = table.Column<long>(type: "bigint", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CallRequests", x => x.Id);
                    table.CheckConstraint("CK_CallRequests_Status", "\"Status\" IN ('Pending', 'Scheduled', 'Declined')");
                    table.ForeignKey(
                        name: "FK_CallRequests_ClientTasks_ScheduledCallId",
                        column: x => x.ScheduledCallId,
                        principalTable: "ClientTasks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_CallRequests_Users_ClientId",
                        column: x => x.ClientId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CallRequests_ClientId_CreatedAt",
                table: "CallRequests",
                columns: new[] { "ClientId", "CreatedAt" });

            migrationBuilder.CreateIndex(
                name: "IX_CallRequests_ScheduledCallId",
                table: "CallRequests",
                column: "ScheduledCallId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            // A rollback must not silently turn scheduled calls or meetings into tasks.
            migrationBuilder.Sql("DO $$ BEGIN IF EXISTS (SELECT 1 FROM \"ClientTasks\" WHERE \"EventType\" <> 'Task') THEN RAISE EXCEPTION 'Remove scheduled events before reverting this migration'; END IF; END $$;");
            migrationBuilder.DropTable(
                name: "CallRequests");

            migrationBuilder.DropColumn(
                name: "EndsAt",
                table: "ClientTasks");

            migrationBuilder.DropColumn(
                name: "EventType",
                table: "ClientTasks");

            migrationBuilder.DropColumn(
                name: "StartsAt",
                table: "ClientTasks");

            migrationBuilder.AlterColumn<string>(
                name: "Pillar",
                table: "ClientTasks",
                type: "character varying(20)",
                maxLength: 20,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(20)",
                oldMaxLength: 20,
                oldNullable: true);
        }
    }
}
