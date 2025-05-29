using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartCookFinal.Migrations
{
    /// <inheritdoc />
    public partial class AddIsActiveToNguoiDung : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsActive",
                table: "NguoiDungs",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsActive",
                table: "NguoiDungs");
        }
    }
}
