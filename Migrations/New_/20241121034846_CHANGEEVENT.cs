using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Senior_Project.Migrations.New_
{
    /// <inheritdoc />
    public partial class CHANGEEVENT : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ExternalEventID",
                table: "Events",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ExternalEventID",
                table: "Events");
        }
    }
}
