using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace HW_06.Migrations
{
    /// <inheritdoc />
    public partial class AddParticipantPrivateFiles : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "ParticipantPrivateFiles",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    OriginalFileName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    StoredFileName = table.Column<string>(type: "nvarchar(255)", maxLength: 255, nullable: false),
                    ContentType = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    SizeBytes = table.Column<long>(type: "bigint", nullable: false),
                    UploadedAtUtc = table.Column<DateTime>(type: "datetime2", nullable: false),
                    SenderParticipantId = table.Column<int>(type: "int", nullable: false),
                    RecipientParticipantId = table.Column<int>(type: "int", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ParticipantPrivateFiles", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ParticipantPrivateFiles_Participants_RecipientParticipantId",
                        column: x => x.RecipientParticipantId,
                        principalTable: "Participants",
                        principalColumn: "ParticipantId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ParticipantPrivateFiles_Participants_SenderParticipantId",
                        column: x => x.SenderParticipantId,
                        principalTable: "Participants",
                        principalColumn: "ParticipantId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ParticipantPrivateFiles_RecipientParticipantId",
                table: "ParticipantPrivateFiles",
                column: "RecipientParticipantId");

            migrationBuilder.CreateIndex(
                name: "IX_ParticipantPrivateFiles_SenderParticipantId",
                table: "ParticipantPrivateFiles",
                column: "SenderParticipantId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ParticipantPrivateFiles");
        }
    }
}
