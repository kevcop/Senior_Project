using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Senior_Project.Migrations.New_
{
    /// <inheritdoc />
    public partial class FixProfileModel : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Events_Profiles_ProfileId",
                table: "Events");

            migrationBuilder.DropForeignKey(
                name: "FK_Events_Profiles_ProfileId1",
                table: "Events");

            migrationBuilder.DropIndex(
                name: "IX_Events_ProfileId",
                table: "Events");

            migrationBuilder.DropIndex(
                name: "IX_Events_ProfileId1",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "ProfileId",
                table: "Events");

            migrationBuilder.DropColumn(
                name: "ProfileId1",
                table: "Events");

            migrationBuilder.AddColumn<string>(
                name: "AttendingEvents",
                table: "Profiles",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "[]");

            migrationBuilder.AddColumn<string>(
                name: "PastEvents",
                table: "Profiles",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "[]");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "AttendingEvents",
                table: "Profiles");

            migrationBuilder.DropColumn(
                name: "PastEvents",
                table: "Profiles");

            migrationBuilder.AddColumn<int>(
                name: "ProfileId",
                table: "Events",
                type: "int",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "ProfileId1",
                table: "Events",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Events_ProfileId",
                table: "Events",
                column: "ProfileId");

            migrationBuilder.CreateIndex(
                name: "IX_Events_ProfileId1",
                table: "Events",
                column: "ProfileId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Events_Profiles_ProfileId",
                table: "Events",
                column: "ProfileId",
                principalTable: "Profiles",
                principalColumn: "Id");

            migrationBuilder.AddForeignKey(
                name: "FK_Events_Profiles_ProfileId1",
                table: "Events",
                column: "ProfileId1",
                principalTable: "Profiles",
                principalColumn: "Id");
        }
    }
}
