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


		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Confirm(
			int id,
			DateTime? returnDate,
			string? returnStatus,
			int returnPage = PaginationSettings.DefaultPageNumber,
			int returnPageSize = PaginationSettings.DefaultPageSize)
		{
			var success = await _orderService.ConfirmOrderAsync(id);

			if (!success)
			{
				TempData["error"] = "訂單確認失敗，訂單可能不存在或目前狀態無法確認";
			}
			else
			{
				TempData["success"] = "訂單已確認";
			}

			return RedirectToAction(nameof(Details),
				new
				{
					id,
					returnDate,
					returnStatus,
					returnPage,
					returnPageSize
				});
		}

		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> StartProcessing(
			int id,
			DateTime? returnDate,
			string? returnStatus,
			int returnPage = PaginationSettings.DefaultPageNumber,
			int returnPageSize = PaginationSettings.DefaultPageSize)
		{
			var success = await _orderService.StartProcessingOrderAsync(id);

			if (!success)
			{
				TempData["error"] = "開始處理訂單失敗，訂單可能不存在或目前狀態無法處理";
			}
			else
			{
				TempData["success"] = "訂單已開始處理";
			}

			return RedirectToAction(nameof(Details),
				new
				{
					id,
					returnDate,
					returnStatus,
					returnPage,
					returnPageSize
				});
		}

		// 處理物流
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Ship(
			int id,
			string carrier,
			string trackingNumber,
			DateTime? returnDate,
			string? returnStatus,
			int returnPage = PaginationSettings.DefaultPageNumber,
			int returnPageSize = PaginationSettings.DefaultPageSize)
		{
			var success = await _orderService.ShipOrderAsync(id, carrier, trackingNumber);

			if (!success)
			{
				TempData["error"] = "訂單出貨失敗，請確認訂單狀態、物流公司與追蹤編號";
			}
			else
			{
				TempData["success"] = "訂單已標記為出貨";
			}

			return RedirectToAction(nameof(Details),
				new
				{
					id,
					returnDate,
					returnStatus,
					returnPage,
					returnPageSize
				});
		}

		// 取消訂單 + 恢復商品庫存量
		[HttpPost]
		[ValidateAntiForgeryToken]
		public async Task<IActionResult> Cancel(
			int id,
			DateTime? returnDate,
			string? returnStatus,
			int returnPage = PaginationSettings.DefaultPageNumber,
			int returnPageSize = PaginationSettings.DefaultPageSize)
		{
			var success = await _orderService.CancelOrderAsync(id);

			if (!success)
			{
				TempData["error"] = "取消訂單失敗，訂單可能不存在或目前狀態無法取消";
			}
			else
			{
				TempData["success"] = "訂單已取消，商品庫存已恢復";
			}

			return RedirectToAction(nameof(Details),
				new
				{
					id,
					returnDate,
					returnStatus,
					returnPage,
					returnPageSize
				});
		}



	}
}
