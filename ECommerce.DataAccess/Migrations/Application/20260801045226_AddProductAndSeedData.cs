using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ECommerce.DataAccess.Migrations.Application
{
    /// <inheritdoc />
    public partial class AddProductAndSeedData : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Products",
                columns: table => new
                {
                    Id = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    Title = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: false),
                    SKU = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    ListPrice = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Price = table.Column<decimal>(type: "decimal(18,2)", nullable: false),
                    Price5 = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    Price10 = table.Column<decimal>(type: "decimal(18,2)", nullable: true),
                    StockQuantity = table.Column<int>(type: "int", nullable: false),
                    CategoryId = table.Column<int>(type: "int", nullable: false),
                    ImageUrl = table.Column<string>(type: "nvarchar(2048)", maxLength: 2048, nullable: true),
                    IsActive = table.Column<bool>(type: "bit", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Products", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Products_Categories_CategoryId",
                        column: x => x.CategoryId,
                        principalTable: "Categories",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 1,
                column: "Name",
                value: "帳篷與天幕");

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 2,
                column: "Name",
                value: "睡眠裝備");

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 3,
                column: "Name",
                value: "露營家具");

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 4,
                column: "Name",
                value: "炊具與餐具");

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 5,
                column: "Name",
                value: "照明與電源");

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 6,
                column: "Name",
                value: "戶外服飾");

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 7,
                column: "Name",
                value: "戶外配件");

            migrationBuilder.InsertData(
                table: "Products",
                columns: new[] { "Id", "CategoryId", "Description", "ImageUrl", "IsActive", "ListPrice", "Price", "Price10", "Price5", "SKU", "StockQuantity", "Title" },
                values: new object[,]
                {
                    { 1, 1, "適合三至四人使用的雙層家庭帳篷，具備防潑水外帳與通風紗網。", null, true, 12800m, 10800m, 9500m, 10200m, "TENT-001", 20, "星野四人家庭帳篷" },
                    { 2, 2, "厚度 8 公分的自動充氣睡墊，適合露營與車宿使用。", null, true, 3200m, 2680m, 2300m, 2500m, "SLEEP-001", 40, "自動充氣露營睡墊" },
                    { 3, 3, "輕量鋁合金桌板，可快速折疊收納，適合二至四人使用。", null, true, 2800m, 2380m, 2050m, 2200m, "FURN-001", 30, "鋁合金折疊露營桌" },
                    { 4, 4, "包含湯鍋、平底鍋、杯具與收納袋，適合兩至三人露營使用。", null, true, 1800m, 1480m, 1280m, 1380m, "COOK-001", 50, "戶外不鏽鋼炊具組" },
                    { 5, 6, "適合春秋露營穿著的輕量外套，具備防風與基本防潑水功能。", null, true, 2600m, 2180m, 1850m, 1980m, "WEAR-001", 60, "輕量防風防潑水外套" }
                });

            migrationBuilder.CreateIndex(
                name: "IX_Products_CategoryId",
                table: "Products",
                column: "CategoryId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Products");

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 1,
                column: "Name",
                value: "客廳家具");

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 2,
                column: "Name",
                value: "臥室家具");

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 3,
                column: "Name",
                value: "餐廳家具");

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 4,
                column: "Name",
                value: "書房家具");

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 5,
                column: "Name",
                value: "辦公家具");

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 6,
                column: "Name",
                value: "戶外家具");

            migrationBuilder.UpdateData(
                table: "Categories",
                keyColumn: "Id",
                keyValue: 7,
                column: "Name",
                value: "收納家具");
        }
    }
}
