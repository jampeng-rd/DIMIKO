
namespace ECommerce.Models.ServiceResults
{
	public class CreateOrderResult
	{
		public bool Succeeded { get; init; }

		public int? OrderId { get; init; }

		public string? ErrorMessage { get; init; }

		public static CreateOrderResult Success(int orderId)
		{
			return new CreateOrderResult
			{
				Succeeded = true,
				OrderId = orderId
			};
		}

		public static CreateOrderResult Failure(string errorMessage)
		{
			return new CreateOrderResult
			{
				Succeeded = false,
				ErrorMessage = errorMessage
			};
		}
	}
}
