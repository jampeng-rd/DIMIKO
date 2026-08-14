using ECommerce.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.DataAccess.Data
{
	public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
	{
		public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
		{ }

		public DbSet<Category> Categories { get; set; }

		public DbSet<Product> Products { get; set; }

		public DbSet<ProductImage> ProductImages { get; set; }

		public DbSet<ApplicationUser> ApplicationUsers { get; set; }

		public DbSet<ShoppingCart> ShoppingCarts { get; set; }

		public DbSet<OrderHeader> OrderHeaders { get; set; }

		public DbSet<OrderDetail> OrderDetails { get; set; }

		public DbSet<HeroBanner> HeroBanners { get; set; }


		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			base.OnModelCreating(modelBuilder);

			modelBuilder.Entity<ProductImage>()
				.HasOne(productImage => productImage.Product)
				.WithMany(product => product.ProductImages)
				.HasForeignKey(productImage => productImage.ProductId)
				.OnDelete(DeleteBehavior.Cascade);

			// 限制同一商品在購物車只出現一次
			modelBuilder.Entity<ShoppingCart>()
				.HasIndex(cart => new
				{
					cart.ApplicationUserId,
					cart.ProductId
				})
				.IsUnique();

			// 刪除整張訂單時，連同該訂單的所有明細一起刪除
			modelBuilder.Entity<OrderDetail>()
				.HasOne(detail => detail.OrderHeader)
				.WithMany(header => header.OrderDetails)
				.HasForeignKey(detail => detail.OrderHeaderId)
				.OnDelete(DeleteBehavior.Cascade);

			// 只要商品有在歷史訂單中時，禁止從資料庫刪除
			modelBuilder.Entity<OrderDetail>()
				.HasOne(detail => detail.Product)
				.WithMany()
				.HasForeignKey(detail => detail.ProductId)
				.OnDelete(DeleteBehavior.Restrict);

			// 限制訂單編號是唯一值
			modelBuilder.Entity<OrderHeader>()
				.HasIndex(order => order.OrderNumber)
				.IsUnique();


			modelBuilder.Entity<Category>().HasData(
				new Category { Id = 1, Name = "帳篷與天幕", DisplayOrder = 1 },
				new Category { Id = 2, Name = "睡眠裝備", DisplayOrder = 2 },
				new Category { Id = 3, Name = "露營家具", DisplayOrder = 3 },
				new Category { Id = 4, Name = "炊具與餐具", DisplayOrder = 4 },
				new Category { Id = 5, Name = "照明與電源", DisplayOrder = 5 },
				new Category { Id = 6, Name = "戶外服飾", DisplayOrder = 6 },
				new Category { Id = 7, Name = "戶外配件", DisplayOrder = 7 }
			);


			modelBuilder.Entity<Product>().HasData(
				new Product 
				{
					Id = 1,
					Title = "星野四人家庭帳篷",
					Description = "適合三至四人使用的雙層家庭帳篷，具備防潑水外帳與通風紗網。",
					SKU = "TENT-001",
					ListPrice = 12800m,
					Price = 10800m,
					Price5 = 10200m,
					Price10 = 9500m,
					StockQuantity = 20,
					CategoryId = 1,
					IsActive = true
				},
				new Product
				{
					Id = 2,
					Title = "自動充氣露營睡墊",
					Description = "厚度 8 公分的自動充氣睡墊，適合露營與車宿使用。",
					SKU = "SLEEP-001",
					ListPrice = 3200m,
					Price = 2680m,
					Price5 = 2500m,
					Price10 = 2300m,
					StockQuantity = 20,
					CategoryId = 2,
					IsActive = true
				},
				new Product
				{
					Id = 3,
					Title = "鋁合金折疊露營桌",
					Description = "輕量鋁合金桌板，可快速折疊收納，適合二至四人使用。",
					SKU = "FURN-001",
					ListPrice = 2800m,
					Price = 2380m,
					Price5 = 2200m,
					Price10 = 2050m,
					StockQuantity = 10,
					CategoryId = 3,
					IsActive = true
				},
				new Product
				{
					Id = 4,
					Title = "戶外不鏽鋼炊具組",
					Description = "包含湯鍋、平底鍋、杯具與收納袋，適合兩至三人露營使用。",
					SKU = "COOK-001",
					ListPrice = 1800m,
					Price = 1480m,
					Price5 = 1380m,
					Price10 = 1280m,
					StockQuantity = 20,
					CategoryId = 4,
					IsActive = true
				},
				new Product
				{
					Id = 5,
					Title = "輕量防風防潑水外套",
					Description = "適合春秋露營穿著的輕量外套，具備防風與基本防潑水功能。",
					SKU = "WEAR-001",
					ListPrice = 2600m,
					Price = 2180m,
					Price5 = 1980m,
					Price10 = 1850m,
					StockQuantity = 60,
					CategoryId = 6,
					IsActive = true
				},
				// ====================
				// 帳篷與天幕
				// ====================
				new Product
				{
					Id = 6,
					Title = "森境雙人輕量帳篷",
					Description = "適合雙人露營與登山使用的輕量雙層帳篷，具備良好通風與防潑水能力。",
					SKU = "TENT-002",
					ListPrice = 6800m,
					Price = 5680m,
					Price5 = 5350m,
					Price10 = 4980m,
					StockQuantity = 25,
					CategoryId = 1,
					IsActive = true
				},
				new Product
				{
					Id = 7,
					Title = "黑岩戶外六角天幕",
					Description = "大型六角天幕提供寬廣遮蔽空間，適合家庭露營與多人活動使用。",
					SKU = "TENT-003",
					ListPrice = 5200m,
					Price = 4380m,
					Price5 = 4100m,
					Price10 = 3850m,
					StockQuantity = 15,
					CategoryId = 1,
					IsActive = true
				},

				// ====================
				// 睡眠裝備
				// ====================
				new Product
				{
					Id = 8,
					Title = "四季保暖羽絨睡袋",
					Description = "適合春秋與低溫露營使用的羽絨睡袋，輕量且方便壓縮收納。",
					SKU = "SLEEP-002",
					ListPrice = 4600m,
					Price = 3980m,
					Price5 = 3750m,
					Price10 = 3500m,
					StockQuantity = 35,
					CategoryId = 2,
					IsActive = true
				},
				new Product
				{
					Id = 9,
					Title = "露營充氣枕",
					Description = "人體工學弧形充氣枕，可快速充放氣，收納後體積小巧。",
					SKU = "SLEEP-003",
					ListPrice = 780m,
					Price = 650m,
					Price5 = 600m,
					Price10 = 550m,
					StockQuantity = 70,
					CategoryId = 2,
					IsActive = true
				},

				// ====================
				// 露營家具
				// ====================
				new Product
				{
					Id = 10,
					Title = "高背折疊露營椅",
					Description = "高背包覆設計搭配透氣布料，適合長時間戶外休息使用。",
					SKU = "FURN-002",
					ListPrice = 2200m,
					Price = 1850m,
					Price5 = 1720m,
					Price10 = 1600m,
					StockQuantity = 45,
					CategoryId = 3,
					IsActive = true
				},
				new Product
				{
					Id = 11,
					Title = "輕量月亮椅",
					Description = "輕量鋁合金骨架搭配耐磨椅布，收納體積小，方便攜帶。",
					SKU = "FURN-003",
					ListPrice = 1600m,
					Price = 1380m,
					Price5 = 1280m,
					Price10 = 1180m,
					StockQuantity = 50,
					CategoryId = 3,
					IsActive = true
				},
				new Product
				{
					Id = 12,
					Title = "三層折疊露營置物架",
					Description = "可快速展開的三層置物架，適合擺放炊具、食材與露營用品。",
					SKU = "FURN-004",
					ListPrice = 2400m,
					Price = 1980m,
					Price5 = 1850m,
					Price10 = 1720m,
					StockQuantity = 28,
					CategoryId = 3,
					IsActive = true
				},

				// ====================
				// 炊具與餐具
				// ====================
				new Product
				{
					Id = 13,
					Title = "戶外琺瑯餐具四件組",
					Description = "包含餐盤、碗與杯具，耐用且方便清潔，適合戶外用餐。",
					SKU = "COOK-002",
					ListPrice = 1200m,
					Price = 980m,
					Price5 = 900m,
					Price10 = 820m,
					StockQuantity = 55,
					CategoryId = 4,
					IsActive = true
				},

				// ====================
				// 照明與電源
				// ====================
				new Product
				{
					Id = 14,
					Title = "復古 LED 露營燈",
					Description = "可調整亮度與色溫的復古造型 LED 燈，適合帳篷與戶外桌面照明。",
					SKU = "LIGHT-001",
					ListPrice = 1680m,
					Price = 1380m,
					Price5 = 1280m,
					Price10 = 1180m,
					StockQuantity = 42,
					CategoryId = 5,
					IsActive = true
				},
				new Product
				{
					Id = 15,
					Title = "戶外行動電源 20000mAh",
					Description = "大容量行動電源，支援多裝置充電，適合露營與戶外活動使用。",
					SKU = "LIGHT-002",
					ListPrice = 2800m,
					Price = 2380m,
					Price5 = 2250m,
					Price10 = 2100m,
					StockQuantity = 32,
					CategoryId = 5,
					IsActive = true
				},

				// ====================
				// 戶外服飾
				// ====================
				new Product
				{
					Id = 16,
					Title = "快乾機能短袖上衣",
					Description = "透氣快乾材質，適合健行、露營與日常戶外活動穿著。",
					SKU = "WEAR-002",
					ListPrice = 1200m,
					Price = 980m,
					Price5 = 900m,
					Price10 = 820m,
					StockQuantity = 80,
					CategoryId = 6,
					IsActive = true
				},
				new Product
				{
					Id = 17,
					Title = "戶外彈性機能長褲",
					Description = "四向彈性布料搭配耐磨設計，適合健行與露營活動。",
					SKU = "WEAR-003",
					ListPrice = 2200m,
					Price = 1880m,
					Price5 = 1750m,
					Price10 = 1620m,
					StockQuantity = 65,
					CategoryId = 6,
					IsActive = true
				},
				new Product
				{
					Id = 18,
					Title = "防曬透氣漁夫帽",
					Description = "寬帽簷設計提供戶外遮陽效果，使用透氣快乾材質。",
					SKU = "WEAR-004",
					ListPrice = 980m,
					Price = 780m,
					Price5 = 720m,
					Price10 = 650m,
					StockQuantity = 90,
					CategoryId = 6,
					IsActive = true
				},
				new Product
				{
					Id = 19,
					Title = "保暖刷毛機能外套",
					Description = "柔軟刷毛內層提供良好保暖效果，適合秋冬露營與戶外活動。",
					SKU = "WEAR-005",
					ListPrice = 3200m,
					Price = 2680m,
					Price5 = 2500m,
					Price10 = 2320m,
					StockQuantity = 48,
					CategoryId = 6,
					IsActive = true
				},

				// ====================
				// 戶外配件
				// ====================
				new Product
				{
					Id = 20,
					Title = "多功能戶外收納箱",
					Description = "大容量耐用收納箱，可收納露營裝備，也可作為戶外桌面使用。",
					SKU = "ACC-001",
					ListPrice = 1800m,
					Price = 1480m,
					Price5 = 1380m,
					Price10 = 1280m,
					StockQuantity = 38,
					CategoryId = 7,
					IsActive = true
				},
				new Product
				{
					Id = 21,
					Title = "防水戶外裝備袋",
					Description = "耐磨防潑水材質的大容量裝備袋，適合攜帶衣物與露營用品。",
					SKU = "ACC-002",
					ListPrice = 1500m,
					Price = 1280m,
					Price5 = 1180m,
					Price10 = 1080m,
					StockQuantity = 52,
					CategoryId = 7,
					IsActive = true
				},
				new Product
				{
					Id = 22,
					Title = "鋁合金營繩調節器組",
					Description = "耐用鋁合金營繩調節器，可快速調整帳篷與天幕營繩張力。",
					SKU = "ACC-003",
					ListPrice = 520m,
					Price = 420m,
					Price5 = 380m,
					Price10 = 350m,
					StockQuantity = 100,
					CategoryId = 7,
					IsActive = true
				}
			);



		}

	}
}
