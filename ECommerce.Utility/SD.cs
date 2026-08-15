namespace ECommerce.Utility
{
	public static class SD
	{
		// 使用者角色
		public const string RoleCustomer = "Customer";
		public const string RoleEmployee = "Employee";
		public const string RoleAdmin = "Admin";


		// 訂單狀態
		public const string OrderStatusPending = "Pending";
		public const string OrderStatusApproved = "Approved";
		public const string OrderStatusInProcess = "Processing";
		public const string OrderStatusShipped = "Shipped";
		public const string OrderStatusCancelled = "Cancelled";


		// 付款狀態
		public const string PaymentStatusPending = "Pending";
		public const string PaymentStatusApproved = "Approved";
		public const string PaymentStatusRejected = "Rejected";
		public const string PaymentStatusRefunded = "Refunded";


		// 單次付款交易狀態
		public const string PaymentTransactionPending = "Pending";
		public const string PaymentTransactionSuccess = "Success";
		public const string PaymentTransactionFailed = "Failed";


		// Session 用來儲存目前購物車商品數量(讓版面右上角顯示)
		public const string SessionCart = "SessionShoppingCart";
	}
}
