using ECommerce.Models;

namespace ECommerce.Business.Services.IServices
{
	public interface IHeroBannerService
	{
		// 專門給首頁使用
		Task<IEnumerable<HeroBanner>> GetActiveHeroBannersAsync();

		Task<IEnumerable<HeroBanner>> GetAllHeroBannersAsync();

		Task<HeroBanner?> GetHeroBannerByIdAsync(int id);

		Task CreateHeroBannerAsync(HeroBanner heroBanner);

		Task UpdateHeroBannerAsync(HeroBanner heroBanner);

		Task DeleteHeroBannerAsync(int id);
	}
}
