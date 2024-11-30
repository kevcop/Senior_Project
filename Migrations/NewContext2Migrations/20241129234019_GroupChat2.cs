using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Senior_Project.Migrations.NewContext2Migrations
{
    /// <inheritdoc />
    public partial class GroupChat2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ChatParticipants_Register_UserID",
                table: "ChatParticipants");

            migrationBuilder.DropForeignKey(
                name: "FK_Messages_Register_SenderID",
                table: "Messages");

            migrationBuilder.AddForeignKey(
                name: "FK_ChatParticipants_Register_UserID",
                table: "ChatParticipants",
                column: "UserID",
                principalTable: "Register",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);

            migrationBuilder.AddForeignKey(
                name: "FK_Messages_Register_SenderID",
                table: "Messages",
                column: "SenderID",
                principalTable: "Register",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_ChatParticipants_Register_UserID",
                table: "ChatParticipants");

            migrationBuilder.DropForeignKey(
                name: "FK_Messages_Register_SenderID",
                table: "Messages");

            migrationBuilder.AddForeignKey(
                name: "FK_ChatParticipants_Register_UserID",
                table: "ChatParticipants",
                column: "UserID",
                principalTable: "Register",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Messages_Register_SenderID",
                table: "Messages",
                column: "SenderID",
                principalTable: "Register",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
