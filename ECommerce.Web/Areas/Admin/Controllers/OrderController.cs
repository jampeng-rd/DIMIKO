using ECommerce.Business.Services.IServices;
using ECommerce.Models.ViewModels;
using ECommerce.Utility;
using ECommerce.Utility.Helpers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerce.Web.Areas.Admin.Controllers
{
	[Area("Admin")]
	[Authorize(Roles = $"{SD.RoleAdmin},{SD.RoleEmployee}")]
	public class OrderController : Controller
	{
		private readonly IOrderService _orderService;

		public OrderController(IOrderService orderService)
		{
			_orderService = orderService;
		}


		public async Task<IActionResult> Index(int? year, int? month)
		{
			var taiwanNow = TaiwanTimeHelper.GetTaiwanNow();

			var selectedYear = year ?? taiwanNow.Year;

			var selectedMonth = month ?? taiwanNow.Month;

			if (selectedYear is < 2000 or > 2100)
			{
				return BadRequest("年份超出允許範圍");
			}

			if (selectedMonth is < 1 or > 12)
			{
				return BadRequest("月份必須介於 1 到 12");
			}

			var monthStart = new DateTime(selectedYear, selectedMonth, 1);

			var previousMonth = monthStart.AddMonths(-1);

			var nextMonth = monthStart.AddMonths(1);

			var dailyOrderCounts = await _orderService.GetMonthlyOrderCountsAsync(selectedYear, selectedMonth);

			var viewModel =
				new AdminOrderCalendarViewModel
				{
					Year = selectedYear,
					Month = selectedMonth,
					MonthStart = monthStart,

					DaysInMonth = DateTime.DaysInMonth(selectedYear, selectedMonth),

					StartDayOffset = (int)monthStart.DayOfWeek,

					DailyOrderCounts = dailyOrderCounts,

					TotalOrders = dailyOrderCounts.Values.Sum(),

					PreviousYear = previousMonth.Year,

					PreviousMonth = previousMonth.Month,

					NextYear = nextMonth.Year,

					NextMonth = nextMonth.Month
				};

			return View(viewModel);
		}


		public async Task<IActionResult> Daily(
			DateTime? date,
			string? status,
			int page = PaginationSettings.DefaultPageNumber,
			int pageSize = PaginationSettings.DefaultPageSize)
		{
			if (!date.HasValue)
			{
				return BadRequest("請提供要查詢的日期");
			}

			var selectedDate = date.Value.Date;

			if (selectedDate.Year is < 2000 or > 2100)
			{
				return BadRequest("日期超出允許範圍");
			}

			page = PaginationSettings.NormalizePageNumber(page);
			pageSize = PaginationSettings.NormalizePageSize(pageSize);

			var pagedOrders = await _orderService.GetOrdersByDateAsync(selectedDate, status, page, pageSize);
			var totalAmount = await _orderService.GetOrderTotalByDateAsync(selectedDate, status);

			var viewModel = new AdminDailyOrderListViewModel
				{
					Date = selectedDate,
					Status = status,
					PagedOrders = pagedOrders,
					TotalAmount = totalAmount
				};

			return View(viewModel);
		}


		public async Task<IActionResult> Details(
			int? id,
			DateTime? returnDate,
			string? returnStatus,
			int returnPage = PaginationSettings.DefaultPageNumber,
			int returnPageSize = PaginationSettings.DefaultPageSize)
		{
			if (id is null or <= 0)
			{
				return NotFound();
			}

			var order = await _orderService.GetOrderDetailsByIdAsync(id.Value);

			if (order == null)
			{
				return NotFound();
			}

			var viewModel = new AdminOrderDetailsViewModel
				{
					Order = order,
					ReturnDate = returnDate?.Date,
					ReturnStatus = returnStatus,
					ReturnPage = PaginationSettings.NormalizePageNumber(returnPage),
					ReturnPageSize = PaginationSettings.NormalizePageSize(returnPageSize)
				};

			return View(viewModel);
		}
		

	}
}
