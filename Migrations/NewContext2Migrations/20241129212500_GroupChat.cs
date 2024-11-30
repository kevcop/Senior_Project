using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Senior_Project.Migrations.NewContext2Migrations
{
    /// <inheritdoc />
    public partial class GroupChat : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Messages_Register_ReceiverID",
                table: "Messages");

            migrationBuilder.DropForeignKey(
                name: "FK_Messages_Register_SenderID",
                table: "Messages");

            migrationBuilder.DropColumn(
                name: "IsRead",
                table: "Messages");

            migrationBuilder.RenameColumn(
                name: "ReceiverID",
                table: "Messages",
                newName: "ChatID");

            migrationBuilder.RenameIndex(
                name: "IX_Messages_ReceiverID",
                table: "Messages",
                newName: "IX_Messages_ChatID");

            migrationBuilder.AlterColumn<string>(
                name: "Content",
                table: "Messages",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(1000)",
                oldMaxLength: 1000);

            migrationBuilder.CreateTable(
                name: "Chats",
                columns: table => new
                {
                    ChatID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    CreatedDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ChatName = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    LastMessageDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    IsGroupChat = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Chats", x => x.ChatID);
                });

            migrationBuilder.CreateTable(
                name: "ChatParticipants",
                columns: table => new
                {
                    ChatParticipantID = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    ChatID = table.Column<int>(type: "int", nullable: false),
                    UserID = table.Column<int>(type: "int", nullable: false),
                    IsAdmin = table.Column<bool>(type: "bit", nullable: false),
                    LastReadMessageDate = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ChatParticipants", x => x.ChatParticipantID);
                    table.ForeignKey(
                        name: "FK_ChatParticipants_Chats_ChatID",
                        column: x => x.ChatID,
                        principalTable: "Chats",
                        principalColumn: "ChatID",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ChatParticipants_Register_UserID",
                        column: x => x.UserID,
                        principalTable: "Register",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ChatParticipants_ChatID",
                table: "ChatParticipants",
                column: "ChatID");

            migrationBuilder.CreateIndex(
                name: "IX_ChatParticipants_UserID",
                table: "ChatParticipants",
                column: "UserID");

            migrationBuilder.AddForeignKey(
                name: "FK_Messages_Chats_ChatID",
                table: "Messages",
                column: "ChatID",
                principalTable: "Chats",
                principalColumn: "ChatID",
                onDelete: ReferentialAction.Cascade);

            migrationBuilder.AddForeignKey(
                name: "FK_Messages_Register_SenderID",
                table: "Messages",
                column: "SenderID",
                principalTable: "Register",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Messages_Chats_ChatID",
                table: "Messages");

            migrationBuilder.DropForeignKey(
                name: "FK_Messages_Register_SenderID",
                table: "Messages");

            migrationBuilder.DropTable(
                name: "ChatParticipants");

            migrationBuilder.DropTable(
                name: "Chats");

            migrationBuilder.RenameColumn(
                name: "ChatID",
                table: "Messages",
                newName: "ReceiverID");

            migrationBuilder.RenameIndex(
                name: "IX_Messages_ChatID",
                table: "Messages",
                newName: "IX_Messages_ReceiverID");

            migrationBuilder.AlterColumn<string>(
                name: "Content",
                table: "Messages",
                type: "nvarchar(1000)",
                maxLength: 1000,
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddColumn<bool>(
                name: "IsRead",
                table: "Messages",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.AddForeignKey(
                name: "FK_Messages_Register_ReceiverID",
                table: "Messages",
                column: "ReceiverID",
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
    }
}
