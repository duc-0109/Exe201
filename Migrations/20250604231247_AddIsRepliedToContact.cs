using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartCookFinal.Migrations
{
    /// <inheritdoc />
    public partial class AddIsRepliedToContact : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "IsReplied",
                table: "Contacts",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "IsReplied",
                table: "Contacts");
        }
    }
}
