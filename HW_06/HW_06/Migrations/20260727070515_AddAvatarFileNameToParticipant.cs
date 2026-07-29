using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HW_06.Migrations
{
    /// <inheritdoc />
    public partial class AddAvatarFileNameToParticipant : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "AvatarFileName",
                table: "Participants",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AvatarFileName",
                table: "Participants");
        }
    }
}
