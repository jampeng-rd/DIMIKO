using ECommerce.Business.Services.IServices;
using ECommerce.DataAccess.Data;
using ECommerce.Models;
using Microsoft.EntityFrameworkCore;

namespace ECommerce.Business.Services
{
	public class HeroBannerService : IHeroBannerService
	{
		private readonly ApplicationDbContext _dbContext;

		public HeroBannerService(ApplicationDbContext dbContext)
		{
			_dbContext = dbContext;
		}


		// 前台首頁：只取得啟用中的輪播圖
		public async Task<IEnumerable<HeroBanner>> GetActiveHeroBannersAsync()
		{
			return await _dbContext.HeroBanners
				.AsNoTracking()
				.Where(heroBanner => heroBanner.IsActive)
				.OrderBy(heroBanner => heroBanner.DisplayOrder)
				.ThenBy(heroBanner => heroBanner.Id)
				.ToListAsync();
		}


		// 後台管理：取得全部輪播圖
		public async Task<IEnumerable<HeroBanner>> GetAllHeroBannersAsync()
		{
			return await _dbContext.HeroBanners
				.AsNoTracking()
				.OrderBy(heroBanner => heroBanner.DisplayOrder)
				.ThenBy(heroBanner => heroBanner.Id)
				.ToListAsync();
		}


		// 依 Id 取得單一輪播圖
		public async Task<HeroBanner?> GetHeroBannerByIdAsync(int id)
		{
			return await _dbContext.HeroBanners.FirstOrDefaultAsync(heroBanner => heroBanner.Id == id);
		}


		// 新增輪播圖
		public async Task CreateHeroBannerAsync(HeroBanner heroBanner)
		{
			await _dbContext.HeroBanners.AddAsync(heroBanner);
			await _dbContext.SaveChangesAsync();
		}


		// 更新輪播圖
		public async Task UpdateHeroBannerAsync(HeroBanner heroBanner)
		{
			_dbContext.HeroBanners.Update(heroBanner);
			await _dbContext.SaveChangesAsync();
		}


		// 刪除輪播圖
		public async Task DeleteHeroBannerAsync(int id)
		{
			var heroBanner = await _dbContext.HeroBanners.FindAsync(id);

			if (heroBanner == null)
			{
				return;
			}

			_dbContext.HeroBanners.Remove(heroBanner);
			await _dbContext.SaveChangesAsync();
		}


	}
}
