using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CatalysticBackend.Migrations
{
    /// <inheritdoc />
    public partial class AddAccountState : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "AccountState",
                table: "Users",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            // Preserve active accounts and distinguish provisioned dormant accounts from pending setup.
            migrationBuilder.Sql("""
                UPDATE "Users" SET "AccountState" = CASE
                    WHEN "IsActive" THEN 1
                    WHEN COALESCE("PasswordHash", '') <> '' THEN 2
                    ELSE 0
                END;
                """);

            migrationBuilder.DropColumn(name: "IsActive", table: "Users");

            migrationBuilder.AddCheckConstraint(
                name: "CK_Users_AccountState",
                table: "Users",
                sql: "\"AccountState\" IN (0, 1, 2)");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropCheckConstraint(
                name: "CK_Users_AccountState",
                table: "Users");

            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "Users",
                type: "boolean",
                nullable: false,
                defaultValue: false);

            migrationBuilder.Sql("UPDATE \"Users\" SET \"IsActive\" = (\"AccountState\" = 1);");
            migrationBuilder.DropColumn(name: "AccountState", table: "Users");
        }
    }
}
