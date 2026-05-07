using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace PersonalKnowledge.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class create_source_in_conversations : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "ConversationSource",
                table: "Conversations",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ConversationSource",
                table: "Conversations");
        }
    }
}
