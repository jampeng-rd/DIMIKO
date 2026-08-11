using ECommerce.Business.Services.IServices;
using ECommerce.Utility;
using ECommerce.Utility.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.Web.Areas.Admin.Controllers
{
	[Area("Admin")]
	[Authorize(Roles = $"{SD.RoleAdmin},{SD.RoleEmployee}")]
	public class DashboardController : Controller
	{
		private readonly IDashboardService _dashboardService;

		public DashboardController(IDashboardService dashboardService)
		{
			_dashboardService = dashboardService;
		}


		public async Task<IActionResult> Index(int? year, int? month)
		{
			var nowTaiwan = TaiwanTimeHelper.ConvertUtcToTaiwan(DateTime.UtcNow);

			var selectedYear = year ?? nowTaiwan.Year;

			var selectedMonth = month ?? nowTaiwan.Month;


			if (selectedYear is < 2000 or > 2100)
			{
				return BadRequest();
			}

			if (selectedMonth is < 1 or > 12)
			{
				return BadRequest();
			}

			var viewModel = await _dashboardService.GetDashboardSummaryAsync(selectedYear, selectedMonth);

			return View(viewModel);
		}


	}
}
