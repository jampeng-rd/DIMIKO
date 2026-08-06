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
					StockQuantity = 40,
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
					StockQuantity = 30,
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
					StockQuantity = 50,
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
				}

			);

		}

	}
}
