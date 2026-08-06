using ECommerce.Models;

namespace ECommerce.Business.Services.IServices
{
	public interface IShoppingCartService
	{
		Task<ShoppingCart?> GetCartByIdAsync(int cartId, string userId);

		Task<int> GetCartCountAsync(string userId);

		Task<IEnumerable<ShoppingCart>> GetUserCartItemsAsync(string userId);

		Task<ShoppingCart> AddToCartAsync(ShoppingCart cart);

		// 一般 更新購物車的寫法 (有安全性問題)
		//Task UpdateCartAsync(ShoppingCart cart);

		// 優化 更新購物車的寫法
		Task<bool> UpdateCartQuantityAsync(int cartId, string userId, int count);

		// 移除單一商品
		Task<bool> RemoveCartItemAsync(int cartId, string userId);

		Task ClearCartAsync(string userId);
	}
}
