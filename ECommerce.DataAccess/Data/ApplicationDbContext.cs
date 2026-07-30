using ECommerce.Models;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.DataAccess.Data
{
	public class ApplicationDbContext : DbContext
	{
		public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
		{ }

		public DbSet<Category> Categories { get; set; }


		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			base.OnModelCreating(modelBuilder);

			modelBuilder.Entity<Category>().HasData(
				new Category { Id = 1, Name = "客廳家具", DisplayOrder = 1 },
				new Category { Id = 2, Name = "臥室家具", DisplayOrder = 2 },
				new Category { Id = 3, Name = "餐廳家具", DisplayOrder = 3 },
				new Category { Id = 4, Name = "書房家具", DisplayOrder = 4 },
				new Category { Id = 5, Name = "辦公家具", DisplayOrder = 5 },
				new Category { Id = 6, Name = "戶外家具", DisplayOrder = 6 },
				new Category { Id = 7, Name = "收納家具", DisplayOrder = 7 }
			);

		}
	}
}
