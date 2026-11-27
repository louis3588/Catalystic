using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CatalysticBackend.Migrations
{
    /// <inheritdoc />
    public partial class ReusableBlogLabels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_BlogLabel_BlogPosts_BlogId",
                table: "BlogLabel");

            migrationBuilder.DropPrimaryKey(
                name: "PK_BlogLabel",
                table: "BlogLabel");

            migrationBuilder.DropIndex(
                name: "IX_BlogLabel_BlogId",
                table: "BlogLabel");

            migrationBuilder.RenameTable(
                name: "BlogLabel",
                newName: "BlogLabels");

            migrationBuilder.AddPrimaryKey(
                name: "PK_BlogLabels",
                table: "BlogLabels",
                column: "Id");

            migrationBuilder.CreateTable(
                name: "BlogLabelBlogPost",
                columns: table => new
                {
                    LabelsId = table.Column<int>(type: "integer", nullable: false),
                    PostsId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BlogLabelBlogPost", x => new { x.LabelsId, x.PostsId });
                    table.ForeignKey(
                        name: "FK_BlogLabelBlogPost_BlogLabels_LabelsId",
                        column: x => x.LabelsId,
                        principalTable: "BlogLabels",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_BlogLabelBlogPost_BlogPosts_PostsId",
                        column: x => x.PostsId,
                        principalTable: "BlogPosts",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_BlogLabelBlogPost_PostsId",
                table: "BlogLabelBlogPost",
                column: "PostsId");

            migrationBuilder.Sql("INSERT INTO \"BlogLabelBlogPost\" (\"LabelsId\", \"PostsId\") SELECT \"Id\", \"BlogId\" FROM \"BlogLabels\";");
            migrationBuilder.DropColumn(name: "BlogId", table: "BlogLabels");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.Sql("""
                DO $$ BEGIN
                  IF EXISTS (SELECT 1 FROM "BlogLabels" l LEFT JOIN "BlogLabelBlogPost" j ON j."LabelsId" = l."Id" GROUP BY l."Id" HAVING count(j."PostsId") <> 1) THEN
                    RAISE EXCEPTION 'Cannot reverse reusable labels: every label must belong to exactly one post.';
                  END IF;
                END $$;
                """);

            migrationBuilder.DropForeignKey(name: "FK_BlogLabelBlogPost_BlogLabels_LabelsId", table: "BlogLabelBlogPost");

            migrationBuilder.DropPrimaryKey(
                name: "PK_BlogLabels",
                table: "BlogLabels");

            migrationBuilder.RenameTable(
                name: "BlogLabels",
                newName: "BlogLabel");

            migrationBuilder.AddColumn<int>(
                name: "BlogId",
                table: "BlogLabel",
                type: "integer",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.Sql("UPDATE \"BlogLabel\" l SET \"BlogId\" = j.\"PostsId\" FROM \"BlogLabelBlogPost\" j WHERE j.\"LabelsId\" = l.\"Id\";");
            migrationBuilder.DropTable(name: "BlogLabelBlogPost");

            migrationBuilder.AddPrimaryKey(
                name: "PK_BlogLabel",
                table: "BlogLabel",
                column: "Id");

            migrationBuilder.CreateIndex(
                name: "IX_BlogLabel_BlogId",
                table: "BlogLabel",
                column: "BlogId");

            migrationBuilder.AddForeignKey(
                name: "FK_BlogLabel_BlogPosts_BlogId",
                table: "BlogLabel",
                column: "BlogId",
                principalTable: "BlogPosts",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
