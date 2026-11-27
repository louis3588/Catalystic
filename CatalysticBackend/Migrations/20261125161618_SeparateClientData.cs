using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace CatalysticBackend.Migrations
{
    /// <inheritdoc />
    public partial class SeparateClientData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WellnessAssessments_Users_ClientId",
                table: "WellnessAssessments");

            migrationBuilder.RenameColumn(
                name: "ClientId",
                table: "WellnessAssessments",
                newName: "ClientDataId");

            migrationBuilder.DropIndex(
                name: "IX_WellnessAssessments_ClientId_AssessmentDate",
                table: "WellnessAssessments");

            migrationBuilder.CreateTable(
                name: "ClientData",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: true),
                    FirstName = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: false),
                    LastName = table.Column<string>(type: "character varying(64)", maxLength: 64, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClientData", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClientData_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ClientData_UserId",
                table: "ClientData",
                column: "UserId",
                unique: true);

            // Preserve existing assessments while replacing user IDs with the new record IDs.
            migrationBuilder.Sql("""
                INSERT INTO "ClientData" ("UserId", "FirstName", "LastName")
                SELECT DISTINCT u."Id", u."FirstName", u."LastName"
                FROM "Users" u INNER JOIN "WellnessAssessments" a ON a."ClientDataId" = u."Id";
                UPDATE "WellnessAssessments" a SET "ClientDataId" = c."Id"
                FROM "ClientData" c WHERE c."UserId" = a."ClientDataId";
                """);

            migrationBuilder.CreateIndex(
                name: "IX_WellnessAssessments_ClientDataId_AssessmentDate",
                table: "WellnessAssessments",
                columns: new[] { "ClientDataId", "AssessmentDate" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_WellnessAssessments_ClientData_ClientDataId",
                table: "WellnessAssessments",
                column: "ClientDataId",
                principalTable: "ClientData",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WellnessAssessments_ClientData_ClientDataId",
                table: "WellnessAssessments");

            migrationBuilder.Sql("""
                DO $$ BEGIN
                  IF EXISTS (SELECT 1 FROM "WellnessAssessments" a JOIN "ClientData" c ON c."Id" = a."ClientDataId" WHERE c."UserId" IS NULL) THEN
                    RAISE EXCEPTION 'Cannot reverse migration: retained assessments have no user account.';
                  END IF;
                END $$;
                """);

            migrationBuilder.DropIndex(
                name: "IX_WellnessAssessments_ClientDataId_AssessmentDate",
                table: "WellnessAssessments");

            migrationBuilder.Sql("""
                UPDATE "WellnessAssessments" a SET "ClientDataId" = c."UserId"
                FROM "ClientData" c WHERE c."Id" = a."ClientDataId";
                """);

            migrationBuilder.DropTable(name: "ClientData");

            migrationBuilder.RenameColumn(
                name: "ClientDataId",
                table: "WellnessAssessments",
                newName: "ClientId");

            migrationBuilder.CreateIndex(
                name: "IX_WellnessAssessments_ClientId_AssessmentDate",
                table: "WellnessAssessments",
                columns: new[] { "ClientId", "AssessmentDate" },
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_WellnessAssessments_Users_ClientId",
                table: "WellnessAssessments",
                column: "ClientId",
                principalTable: "Users",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
