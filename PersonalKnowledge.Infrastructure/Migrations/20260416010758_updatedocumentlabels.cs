using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PersonalKnowledge.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class updatedocumentlabels : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Label",
                table: "Documents",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Label",
                table: "Documents");
        }
    }
}
