using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace TimeCapsule.Migrations
{
    /// <inheritdoc />
    public partial class AddCapsuleAttachmentModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CapsuleAttachment_Capsules_CapsuleId",
                table: "CapsuleAttachment");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CapsuleAttachment",
                table: "CapsuleAttachment");

            migrationBuilder.DropColumn(
                name: "Discriminator",
                table: "AspNetUsers");

            migrationBuilder.RenameTable(
                name: "CapsuleAttachment",
                newName: "CapsuleAttachments");

            migrationBuilder.RenameIndex(
                name: "IX_CapsuleAttachment_CapsuleId",
                table: "CapsuleAttachments",
                newName: "IX_CapsuleAttachments_CapsuleId");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CapsuleAttachments",
                table: "CapsuleAttachments",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_CapsuleAttachments_Capsules_CapsuleId",
                table: "CapsuleAttachments",
                column: "CapsuleId",
                principalTable: "Capsules",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_CapsuleAttachments_Capsules_CapsuleId",
                table: "CapsuleAttachments");

            migrationBuilder.DropPrimaryKey(
                name: "PK_CapsuleAttachments",
                table: "CapsuleAttachments");

            migrationBuilder.RenameTable(
                name: "CapsuleAttachments",
                newName: "CapsuleAttachment");

            migrationBuilder.RenameIndex(
                name: "IX_CapsuleAttachments_CapsuleId",
                table: "CapsuleAttachment",
                newName: "IX_CapsuleAttachment_CapsuleId");

            migrationBuilder.AddColumn<string>(
                name: "Discriminator",
                table: "AspNetUsers",
                type: "character varying(21)",
                maxLength: 21,
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddPrimaryKey(
                name: "PK_CapsuleAttachment",
                table: "CapsuleAttachment",
                column: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_CapsuleAttachment_Capsules_CapsuleId",
                table: "CapsuleAttachment",
                column: "CapsuleId",
                principalTable: "Capsules",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
