using ECommerce.Business.Services.IServices;

namespace ECommerce.Web.BackgroundServices
{
	public class ExpiredOrderCleanupService : BackgroundService
	{
		private readonly IServiceScopeFactory _scopeFactory;
		private readonly ILogger<ExpiredOrderCleanupService> _logger;

		public ExpiredOrderCleanupService(
			IServiceScopeFactory scopeFactory,
			ILogger<ExpiredOrderCleanupService> logger)
		{
			_scopeFactory = scopeFactory;
			_logger = logger;
		}

		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			while (!stoppingToken.IsCancellationRequested)
			{
				try
				{
					using var scope = _scopeFactory.CreateScope();

					var orderService = scope.ServiceProvider.GetRequiredService<IOrderService>();

					var cancelledCount = await orderService.CancelExpiredOrdersAsync();

					if (cancelledCount > 0)
					{
						_logger.LogInformation("已自動取消 {CancelledCount} 筆逾期未付款訂單", cancelledCount);
					}
				}
				catch (Exception exception)
				{
					_logger.LogError(exception, "自動取消逾期未付款訂單時發生錯誤");
				}

				try
				{
					// 每 5 分鐘檢查一次
					await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
				}
				catch (OperationCanceledException)
					when (stoppingToken.IsCancellationRequested)
				{
					break;
				}
				
			}
		}

	}
}
