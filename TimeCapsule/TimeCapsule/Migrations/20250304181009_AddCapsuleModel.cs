using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace TimeCapsule.Migrations
{
    /// <inheritdoc />
    public partial class AddCapsuleModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Discriminator",
                table: "AspNetUsers",
                type: "character varying(21)",
                maxLength: 21,
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateTable(
                name: "Capsules",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Type = table.Column<int>(type: "integer", nullable: false),
                    Title = table.Column<string>(type: "text", nullable: false),
                    Icon = table.Column<string>(type: "text", nullable: false),
                    Color = table.Column<string>(type: "text", nullable: false),
                    Introduction = table.Column<string>(type: "text", nullable: false),
                    Question1Answer = table.Column<string>(type: "text", nullable: false),
                    MessageContent = table.Column<string>(type: "text", nullable: false),
                    OpeningDate = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    CreatedByUserId = table.Column<string>(type: "text", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Status = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Capsules", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Capsules_AspNetUsers_CreatedByUserId",
                        column: x => x.CreatedByUserId,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CapsuleAnswer",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CapsuleId = table.Column<int>(type: "integer", nullable: false),
                    QuestionName = table.Column<string>(type: "text", nullable: false),
                    AnswerText = table.Column<string>(type: "text", nullable: false),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    UserId1 = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CapsuleAnswer", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CapsuleAnswer_AspNetUsers_UserId1",
                        column: x => x.UserId1,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CapsuleAnswer_Capsules_CapsuleId",
                        column: x => x.CapsuleId,
                        principalTable: "Capsules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CapsuleAttachment",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CapsuleId = table.Column<int>(type: "integer", nullable: false),
                    FilePath = table.Column<string>(type: "text", nullable: false),
                    FileName = table.Column<string>(type: "text", nullable: false),
                    Link = table.Column<string>(type: "text", nullable: false),
                    Type = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CapsuleAttachment", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CapsuleAttachment_Capsules_CapsuleId",
                        column: x => x.CapsuleId,
                        principalTable: "Capsules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "CapsuleRecipient",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CapsuleId = table.Column<int>(type: "integer", nullable: false),
                    RecipientUserId = table.Column<int>(type: "integer", nullable: false),
                    EmailSent = table.Column<bool>(type: "boolean", nullable: false),
                    Accepted = table.Column<bool>(type: "boolean", nullable: false),
                    RecipientUserId1 = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CapsuleRecipient", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CapsuleRecipient_AspNetUsers_RecipientUserId1",
                        column: x => x.RecipientUserId1,
                        principalTable: "AspNetUsers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_CapsuleRecipient_Capsules_CapsuleId",
                        column: x => x.CapsuleId,
                        principalTable: "Capsules",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_CapsuleAnswer_CapsuleId",
                table: "CapsuleAnswer",
                column: "CapsuleId");

            migrationBuilder.CreateIndex(
                name: "IX_CapsuleAnswer_UserId1",
                table: "CapsuleAnswer",
                column: "UserId1");

            migrationBuilder.CreateIndex(
                name: "IX_CapsuleAttachment_CapsuleId",
                table: "CapsuleAttachment",
                column: "CapsuleId");

            migrationBuilder.CreateIndex(
                name: "IX_CapsuleRecipient_CapsuleId",
                table: "CapsuleRecipient",
                column: "CapsuleId");

            migrationBuilder.CreateIndex(
                name: "IX_CapsuleRecipient_RecipientUserId1",
                table: "CapsuleRecipient",
                column: "RecipientUserId1");

            migrationBuilder.CreateIndex(
                name: "IX_Capsules_CreatedByUserId",
                table: "Capsules",
                column: "CreatedByUserId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "CapsuleAnswer");

            migrationBuilder.DropTable(
                name: "CapsuleAttachment");

            migrationBuilder.DropTable(
                name: "CapsuleRecipient");

            migrationBuilder.DropTable(
                name: "Capsules");

            migrationBuilder.DropColumn(
                name: "Discriminator",
                table: "AspNetUsers");
        }
    }
}
