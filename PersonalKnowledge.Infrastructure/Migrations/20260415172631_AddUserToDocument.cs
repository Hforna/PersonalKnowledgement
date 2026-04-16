using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PersonalKnowledge.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddUserToDocument : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Clear existing documents to avoid FK conflict with non-existent users
            migrationBuilder.Sql("DELETE FROM Documents");

            migrationBuilder.DropForeignKey(
                name: "FK_Documents_AspNetUsers_UserId",
                table: "Documents");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "Documents",
                type: "nvarchar(450)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(450)",
                oldNullable: true);

            migrationBuilder.AddForeignKey(
                name: "FK_Documents_AspNetUsers_UserId",
                table: "Documents",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Documents_AspNetUsers_UserId",
                table: "Documents");

            migrationBuilder.AlterColumn<string>(
                name: "UserId",
                table: "Documents",
                type: "nvarchar(450)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AddForeignKey(
                name: "FK_Documents_AspNetUsers_UserId",
                table: "Documents",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }
    }
}
