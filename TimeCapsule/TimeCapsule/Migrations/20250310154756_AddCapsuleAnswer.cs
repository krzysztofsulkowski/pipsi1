using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TimeCapsule.Migrations
{
    /// <inheritdoc />
    public partial class AddCapsuleAnswer : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CapsuleAnswer_CapsuleQuestions_CapsuleQuestionId",
                table: "CapsuleAnswer");

            migrationBuilder.DropForeignKey(
                name: "FK_CapsuleAnswer_Capsules_CapsuleId",
                table: "CapsuleAnswer");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CapsuleAnswer",
                table: "CapsuleAnswer");

            migrationBuilder.RenameTable(
                name: "CapsuleAnswer",
                newName: "CapsuleAnswers");

            migrationBuilder.RenameIndex(
                name: "IX_CapsuleAnswer_CapsuleQuestionId",
                table: "CapsuleAnswers",
                newName: "IX_CapsuleAnswers_CapsuleQuestionId");

            migrationBuilder.RenameIndex(
                name: "IX_CapsuleAnswer_CapsuleId",
                table: "CapsuleAnswers",
                newName: "IX_CapsuleAnswers_CapsuleId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CapsuleAnswers",
                table: "CapsuleAnswers",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_CapsuleAnswers_CapsuleQuestions_CapsuleQuestionId",
                table: "CapsuleAnswers",
                column: "CapsuleQuestionId",
                principalTable: "CapsuleQuestions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CapsuleAnswers_Capsules_CapsuleId",
                table: "CapsuleAnswers",
                column: "CapsuleId",
                principalTable: "Capsules",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CapsuleAnswers_CapsuleQuestions_CapsuleQuestionId",
                table: "CapsuleAnswers");

            migrationBuilder.DropForeignKey(
                name: "FK_CapsuleAnswers_Capsules_CapsuleId",
                table: "CapsuleAnswers");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CapsuleAnswers",
                table: "CapsuleAnswers");

            migrationBuilder.RenameTable(
                name: "CapsuleAnswers",
                newName: "CapsuleAnswer");

            migrationBuilder.RenameIndex(
                name: "IX_CapsuleAnswers_CapsuleQuestionId",
                table: "CapsuleAnswer",
                newName: "IX_CapsuleAnswer_CapsuleQuestionId");

            migrationBuilder.RenameIndex(
                name: "IX_CapsuleAnswers_CapsuleId",
                table: "CapsuleAnswer",
                newName: "IX_CapsuleAnswer_CapsuleId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CapsuleAnswer",
                table: "CapsuleAnswer",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_CapsuleAnswer_CapsuleQuestions_CapsuleQuestionId",
                table: "CapsuleAnswer",
                column: "CapsuleQuestionId",
                principalTable: "CapsuleQuestions",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_CapsuleAnswer_Capsules_CapsuleId",
                table: "CapsuleAnswer",
                column: "CapsuleId",
                principalTable: "Capsules",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
