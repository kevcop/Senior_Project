using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Senior_Project.Migrations.NewContext2Migrations
{
    /// <inheritdoc />
    public partial class GroupDisc : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "EventId",
                table: "Chats",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsDiscussion",
                table: "Chats",
                type: "bit",
                nullable: false,
                defaultValue: false);

            migrationBuilder.CreateIndex(
                name: "IX_Chats_EventId",
                table: "Chats",
                column: "EventId");

            migrationBuilder.AddForeignKey(
                name: "FK_Chats_Events_EventId",
                table: "Chats",
                column: "EventId",
                principalTable: "Events",
                principalColumn: "EventID",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Chats_Events_EventId",
                table: "Chats");

            migrationBuilder.DropIndex(
                name: "IX_Chats_EventId",
                table: "Chats");

            migrationBuilder.DropColumn(
                name: "EventId",
                table: "Chats");

            migrationBuilder.DropColumn(
                name: "IsDiscussion",
                table: "Chats");
        }
    }
}
