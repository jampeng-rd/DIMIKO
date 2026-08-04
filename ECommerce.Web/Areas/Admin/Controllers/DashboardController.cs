using ECommerce.DataAccess.Data;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.Web.Areas.Admin.Controllers
{
	[Area("Admin")]
	public class DashboardController : Controller
	{
		private readonly ApplicationDbContext _dbContext;

		public DashboardController(ApplicationDbContext dbContext)
		{
			_dbContext = dbContext;
		}

		public IActionResult Index()
		{
			return View();
		}


	}
}
