using ECommerce.Models.ViewModels;

namespace ECommerce.Business.Services.IServices
{
	public interface IDashboardService
	{
		Task<DashboardViewModel> GetDashboardSummaryAsync(int year, int month);

	}
}
