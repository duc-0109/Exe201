using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace SmartCookFinal.Migrations
{
    public partial class AddCategoryForNews : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // Bước 1: Tạo bảng Categories
            migrationBuilder.CreateTable(
                name: "Categories",
                columns: table => new
                {
                    CategoryId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Name = table.Column<string>(type: "nvarchar(max)", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Categories", x => x.CategoryId);
                });

            // Bước 2: Thêm bản ghi mặc định
            migrationBuilder.Sql("INSERT INTO Categories (Name) VALUES (N'Tin tức mặc định')");

            // Bước 3: Thêm cột CategoryId vào News, defaultValue = 1
            migrationBuilder.AddColumn<int>(
                name: "CategoryId",
                table: "News",
                type: "int",
                nullable: false,
                defaultValue: 1); // ID của bản ghi mặc định

            // Bước 4: Tạo index và khoá ngoại
            migrationBuilder.CreateIndex(
                name: "IX_News_CategoryId",
                table: "News",
                column: "CategoryId");

            migrationBuilder.AddForeignKey(
                name: "FK_News_Categories_CategoryId",
                table: "News",
                column: "CategoryId",
                principalTable: "Categories",
                principalColumn: "CategoryId",
                onDelete: ReferentialAction.Cascade);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_News_Categories_CategoryId",
                table: "News");

            migrationBuilder.DropIndex(
                name: "IX_News_CategoryId",
                table: "News");

            migrationBuilder.DropColumn(
                name: "CategoryId",
                table: "News");

            migrationBuilder.DropTable(
                name: "Categories");
        }
    }
}
