using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace CatalysticBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddWeeklyReviews : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "WeeklyReviews",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ClientDataId = table.Column<int>(type: "integer", nullable: false),
                    ReviewDate = table.Column<DateOnly>(type: "date", nullable: false),
                    NextReviewDate = table.Column<DateOnly>(type: "date", nullable: false),
                    Physical = table.Column<decimal>(type: "numeric(4,2)", precision: 4, scale: 2, nullable: false),
                    Dietary = table.Column<decimal>(type: "numeric(4,2)", precision: 4, scale: 2, nullable: false),
                    Social = table.Column<decimal>(type: "numeric(4,2)", precision: 4, scale: 2, nullable: false),
                    Emotional = table.Column<decimal>(type: "numeric(4,2)", precision: 4, scale: 2, nullable: false),
                    Intellectual = table.Column<decimal>(type: "numeric(4,2)", precision: 4, scale: 2, nullable: false),
                    Habitual = table.Column<decimal>(type: "numeric(4,2)", precision: 4, scale: 2, nullable: false),
                    Financial = table.Column<decimal>(type: "numeric(4,2)", precision: 4, scale: 2, nullable: false),
                    Status = table.Column<string>(type: "character varying(24)", maxLength: 24, nullable: false),
                    SubmittedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    ContactedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CompletedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WeeklyReviews", x => x.Id);
                    table.CheckConstraint("CK_WeeklyReview_Dietary", "\"Dietary\" >= 0 AND \"Dietary\" <= 10");
                    table.CheckConstraint("CK_WeeklyReview_Emotional", "\"Emotional\" >= 0 AND \"Emotional\" <= 10");
                    table.CheckConstraint("CK_WeeklyReview_Financial", "\"Financial\" >= 0 AND \"Financial\" <= 10");
                    table.CheckConstraint("CK_WeeklyReview_Habitual", "\"Habitual\" >= 0 AND \"Habitual\" <= 10");
                    table.CheckConstraint("CK_WeeklyReview_Intellectual", "\"Intellectual\" >= 0 AND \"Intellectual\" <= 10");
                    table.CheckConstraint("CK_WeeklyReview_Physical", "\"Physical\" >= 0 AND \"Physical\" <= 10");
                    table.CheckConstraint("CK_WeeklyReview_Social", "\"Social\" >= 0 AND \"Social\" <= 10");
                    table.CheckConstraint("CK_WeeklyReview_Status", "\"Status\" IN ('PendingContact', 'Contacted', 'Completed')");
                    table.ForeignKey(
                        name: "FK_WeeklyReviews_ClientData_ClientDataId",
                        column: x => x.ClientDataId,
                        principalTable: "ClientData",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_WeeklyReviews_ClientDataId",
                table: "WeeklyReviews",
                column: "ClientDataId",
                unique: true,
                filter: "\"Status\" <> 'Completed'");

            migrationBuilder.CreateIndex(
                name: "IX_WeeklyReviews_ClientDataId_ReviewDate",
                table: "WeeklyReviews",
                columns: new[] { "ClientDataId", "ReviewDate" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WeeklyReviews");
        }
    }
}
