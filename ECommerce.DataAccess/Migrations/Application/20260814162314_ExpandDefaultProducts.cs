using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

#pragma warning disable CA1814 // Prefer jagged arrays over multidimensional

namespace ECommerce.DataAccess.Migrations.Application
{
    /// <inheritdoc />
    public partial class ExpandDefaultProducts : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 2,
                column: "StockQuantity",
                value: 20);

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 3,
                column: "StockQuantity",
                value: 10);

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 4,
                column: "StockQuantity",
                value: 20);

            migrationBuilder.InsertData(
                table: "Products",
                columns: new[] { "Id", "CategoryId", "Description", "IsActive", "ListPrice", "Price", "Price10", "Price5", "SKU", "StockQuantity", "Title" },
                values: new object[,]
                {
                    { 6, 1, "適合雙人露營與登山使用的輕量雙層帳篷，具備良好通風與防潑水能力。", true, 6800m, 5680m, 4980m, 5350m, "TENT-002", 25, "森境雙人輕量帳篷" },
                    { 7, 1, "大型六角天幕提供寬廣遮蔽空間，適合家庭露營與多人活動使用。", true, 5200m, 4380m, 3850m, 4100m, "TENT-003", 15, "黑岩戶外六角天幕" },
                    { 8, 2, "適合春秋與低溫露營使用的羽絨睡袋，輕量且方便壓縮收納。", true, 4600m, 3980m, 3500m, 3750m, "SLEEP-002", 35, "四季保暖羽絨睡袋" },
                    { 9, 2, "人體工學弧形充氣枕，可快速充放氣，收納後體積小巧。", true, 780m, 650m, 550m, 600m, "SLEEP-003", 70, "露營充氣枕" },
                    { 10, 3, "高背包覆設計搭配透氣布料，適合長時間戶外休息使用。", true, 2200m, 1850m, 1600m, 1720m, "FURN-002", 45, "高背折疊露營椅" },
                    { 11, 3, "輕量鋁合金骨架搭配耐磨椅布，收納體積小，方便攜帶。", true, 1600m, 1380m, 1180m, 1280m, "FURN-003", 50, "輕量月亮椅" },
                    { 12, 3, "可快速展開的三層置物架，適合擺放炊具、食材與露營用品。", true, 2400m, 1980m, 1720m, 1850m, "FURN-004", 28, "三層折疊露營置物架" },
                    { 13, 4, "包含餐盤、碗與杯具，耐用且方便清潔，適合戶外用餐。", true, 1200m, 980m, 820m, 900m, "COOK-002", 55, "戶外琺瑯餐具四件組" },
                    { 14, 5, "可調整亮度與色溫的復古造型 LED 燈，適合帳篷與戶外桌面照明。", true, 1680m, 1380m, 1180m, 1280m, "LIGHT-001", 42, "復古 LED 露營燈" },
                    { 15, 5, "大容量行動電源，支援多裝置充電，適合露營與戶外活動使用。", true, 2800m, 2380m, 2100m, 2250m, "LIGHT-002", 32, "戶外行動電源 20000mAh" },
                    { 16, 6, "透氣快乾材質，適合健行、露營與日常戶外活動穿著。", true, 1200m, 980m, 820m, 900m, "WEAR-002", 80, "快乾機能短袖上衣" },
                    { 17, 6, "四向彈性布料搭配耐磨設計，適合健行與露營活動。", true, 2200m, 1880m, 1620m, 1750m, "WEAR-003", 65, "戶外彈性機能長褲" },
                    { 18, 6, "寬帽簷設計提供戶外遮陽效果，使用透氣快乾材質。", true, 980m, 780m, 650m, 720m, "WEAR-004", 90, "防曬透氣漁夫帽" },
                    { 19, 6, "柔軟刷毛內層提供良好保暖效果，適合秋冬露營與戶外活動。", true, 3200m, 2680m, 2320m, 2500m, "WEAR-005", 48, "保暖刷毛機能外套" },
                    { 20, 7, "大容量耐用收納箱，可收納露營裝備，也可作為戶外桌面使用。", true, 1800m, 1480m, 1280m, 1380m, "ACC-001", 38, "多功能戶外收納箱" },
                    { 21, 7, "耐磨防潑水材質的大容量裝備袋，適合攜帶衣物與露營用品。", true, 1500m, 1280m, 1080m, 1180m, "ACC-002", 52, "防水戶外裝備袋" },
                    { 22, 7, "耐用鋁合金營繩調節器，可快速調整帳篷與天幕營繩張力。", true, 520m, 420m, 350m, 380m, "ACC-003", 100, "鋁合金營繩調節器組" }
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 6);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 7);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 8);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 9);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 10);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 11);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 12);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 13);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 14);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 15);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 16);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 17);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 18);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 19);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 20);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 21);

            migrationBuilder.DeleteData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 22);

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 2,
                column: "StockQuantity",
                value: 40);

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 3,
                column: "StockQuantity",
                value: 30);

            migrationBuilder.UpdateData(
                table: "Products",
                keyColumn: "Id",
                keyValue: 4,
                column: "StockQuantity",
                value: 50);
        }
    }
}
