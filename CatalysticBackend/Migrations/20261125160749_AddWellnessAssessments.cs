using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace CatalysticBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddWellnessAssessments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "WellnessAssessments",
                columns: table => new
                {
                    Id = table.Column<long>(type: "bigint", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ClientId = table.Column<int>(type: "integer", nullable: false),
                    AssessmentDate = table.Column<DateOnly>(type: "date", nullable: false),
                    Physical = table.Column<decimal>(type: "numeric(4,2)", precision: 4, scale: 2, nullable: false),
                    Dietary = table.Column<decimal>(type: "numeric(4,2)", precision: 4, scale: 2, nullable: false),
                    Social = table.Column<decimal>(type: "numeric(4,2)", precision: 4, scale: 2, nullable: false),
                    Emotional = table.Column<decimal>(type: "numeric(4,2)", precision: 4, scale: 2, nullable: false),
                    Intellectual = table.Column<decimal>(type: "numeric(4,2)", precision: 4, scale: 2, nullable: false),
                    Habitual = table.Column<decimal>(type: "numeric(4,2)", precision: 4, scale: 2, nullable: false),
                    Financial = table.Column<decimal>(type: "numeric(4,2)", precision: 4, scale: 2, nullable: false),
                    PREOS = table.Column<decimal>(type: "numeric(4,2)", precision: 4, scale: 2, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WellnessAssessments", x => x.Id);
                    table.CheckConstraint("CK_WellnessAssessment_Dietary", "\"Dietary\" >= 0 AND \"Dietary\" <= 10");
                    table.CheckConstraint("CK_WellnessAssessment_Emotional", "\"Emotional\" >= 0 AND \"Emotional\" <= 10");
                    table.CheckConstraint("CK_WellnessAssessment_Financial", "\"Financial\" >= 0 AND \"Financial\" <= 10");
                    table.CheckConstraint("CK_WellnessAssessment_Habitual", "\"Habitual\" >= 0 AND \"Habitual\" <= 10");
                    table.CheckConstraint("CK_WellnessAssessment_Intellectual", "\"Intellectual\" >= 0 AND \"Intellectual\" <= 10");
                    table.CheckConstraint("CK_WellnessAssessment_Physical", "\"Physical\" >= 0 AND \"Physical\" <= 10");
                    table.CheckConstraint("CK_WellnessAssessment_PREOS", "\"PREOS\" >= 0 AND \"PREOS\" <= 10");
                    table.CheckConstraint("CK_WellnessAssessment_Social", "\"Social\" >= 0 AND \"Social\" <= 10");
                    table.ForeignKey(
                        name: "FK_WellnessAssessments_Users_ClientId",
                        column: x => x.ClientId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_WellnessAssessments_ClientId_AssessmentDate",
                table: "WellnessAssessments",
                columns: new[] { "ClientId", "AssessmentDate" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "WellnessAssessments");
        }
    }
}
