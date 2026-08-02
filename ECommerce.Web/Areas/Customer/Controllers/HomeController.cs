using ECommerce.Business.Services.IServices;
using ECommerce.Models.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.Web.Areas.Customer.Controllers
{
	[Area("Customer")]
	public class HomeController : Controller
	{
		private readonly IProductService _productService;


		public HomeController(IProductService productService)
		{
			_productService = productService;
		}

		public async Task<IActionResult> Index()
		{
			var products = await _productService.GetAllProductsAsync(includeCategory: true, includeImages: true);

			var activeProducts = products
				.Where(product => product.IsActive)
				.ToList();

			return View(activeProducts);
		}

		public async Task<IActionResult> Detail(int? id)
		{
			if (id is null or 0)
			{
				return NotFound();
			}

			var product = await _productService.GetProductByIdAsync(
			   id.Value,
			   includeCategory: true,
			   includeImages: true
		   );

			if (product == null || !product.IsActive)
			{
				return NotFound();
			}

			var viewModel = new ProductDetailViewModel
			{
				Product = product,
				Quantity = 1
			};

			return View(viewModel);
		}



	}
}
