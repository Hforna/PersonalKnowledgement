using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PersonalKnowledge.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class create_source_in_conversationsdd : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Conversations_AspNetUsers_UserId",
                table: "Conversations");

            migrationBuilder.AddForeignKey(
                name: "FK_Conversations_AspNetUsers_UserId",
                table: "Conversations",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Conversations_AspNetUsers_UserId",
                table: "Conversations");

            migrationBuilder.AddForeignKey(
                name: "FK_Conversations_AspNetUsers_UserId",
                table: "Conversations",
                column: "UserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
