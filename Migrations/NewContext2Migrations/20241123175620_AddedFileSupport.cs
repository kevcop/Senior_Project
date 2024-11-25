using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Senior_Project.Migrations.NewContext2Migrations
{
    /// <inheritdoc />
    public partial class AddedFileSupport : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageByte",
                table: "Images");

            migrationBuilder.AddColumn<string>(
                name: "FilePath",
                table: "Images",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "FilePath",
                table: "Images");

            migrationBuilder.AddColumn<byte[]>(
                name: "ImageByte",
                table: "Images",
                type: "varbinary(max)",
                nullable: false,
                defaultValue: new byte[0]);
        }
    }
}
