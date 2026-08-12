namespace ECommerce.Models.ViewModels
{
	public class HomeIndexViewModel
	{
		public List<Product> Products { get; set; } = new();

		public List<HeroBanner> HeroBanners { get; set; } = new();
	}
}
