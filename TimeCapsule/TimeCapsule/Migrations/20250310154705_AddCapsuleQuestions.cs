using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace TimeCapsule.Migrations
{
    /// <inheritdoc />
    public partial class AddCapsuleQuestions : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CapsuleAnswer_AspNetUsers_UserId1",
                table: "CapsuleAnswer");

            migrationBuilder.DropForeignKey(
                name: "FK_Capsules_AspNetUsers_CreatedByUserId",
                table: "Capsules");

            migrationBuilder.DropIndex(
                name: "IX_CapsuleAnswer_UserId1",
                table: "CapsuleAnswer");

            migrationBuilder.DropColumn(
                name: "QuestionName",
                table: "CapsuleAnswer");

            migrationBuilder.DropColumn(
                name: "UserId1",
                table: "CapsuleAnswer");

            migrationBuilder.RenameColumn(
                name: "UserId",
                table: "CapsuleAnswer",
                newName: "CapsuleQuestionId");

            migrationBuilder.CreateTable(
                name: "CapsuleQuestions",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    QuestionText = table.Column<string>(type: "text", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CapsuleQuestions", x => x.Id);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Capsules_Title",
                table: "Capsules",
                column: "Title");

            migrationBuilder.CreateIndex(
                name: "IX_CapsuleAnswer_CapsuleQuestionId",
                table: "CapsuleAnswer",
                column: "CapsuleQuestionId");

            migrationBuilder.AddForeignKey(
                name: "FK_CapsuleAnswer_CapsuleQuestions_CapsuleQuestionId",
                table: "CapsuleAnswer",
                column: "CapsuleQuestionId",
                principalTable: "CapsuleQuestions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Capsules_AspNetUsers_CreatedByUserId",
                table: "Capsules",
                column: "CreatedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CapsuleAnswer_CapsuleQuestions_CapsuleQuestionId",
                table: "CapsuleAnswer");

            migrationBuilder.DropForeignKey(
                name: "FK_Capsules_AspNetUsers_CreatedByUserId",
                table: "Capsules");

            migrationBuilder.DropTable(
                name: "CapsuleQuestions");

            migrationBuilder.DropIndex(
                name: "IX_Capsules_Title",
                table: "Capsules");

            migrationBuilder.DropIndex(
                name: "IX_CapsuleAnswer_CapsuleQuestionId",
                table: "CapsuleAnswer");

            migrationBuilder.RenameColumn(
                name: "CapsuleQuestionId",
                table: "CapsuleAnswer",
                newName: "UserId");

            migrationBuilder.AddColumn<string>(
                name: "QuestionName",
                table: "CapsuleAnswer",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "UserId1",
                table: "CapsuleAnswer",
                type: "text",
                nullable: false,
                defaultValue: "");

            migrationBuilder.CreateIndex(
                name: "IX_CapsuleAnswer_UserId1",
                table: "CapsuleAnswer",
                column: "UserId1");

            migrationBuilder.AddForeignKey(
                name: "FK_CapsuleAnswer_AspNetUsers_UserId1",
                table: "CapsuleAnswer",
                column: "UserId1",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Capsules_AspNetUsers_CreatedByUserId",
                table: "Capsules",
                column: "CreatedByUserId",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
