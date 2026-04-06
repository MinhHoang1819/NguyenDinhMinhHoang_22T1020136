using SV22T1020136.Models.Sales;

namespace SV22T1020136.Admin
{
    /// <summary>
    /// Lớp cung cấp các hàm tiện ích liên quan đến giỏ hàng, như tính tổng tiền, thêm/xóa sản phẩm, v.v.
    /// (giỏ hàng lưu trong session)
    /// </summary>
    public class ShoppingCartHelper
    {
        /// <summary>
        /// Tên biến để lưu giỏ hàng trong session. Giá trị này được sử dụng để truy xuất giỏ hàng từ session.
        /// </summary>
        private const string CART = "ShoppingCart";

        /// <summary>
        /// Lấy giỏ hàng từ session (nếu giỏ hàng chưa có thì tạo giỏ hàng rỗng)
        /// </summary>
        /// <returns></returns>
        public static List<OrderDetailViewInfo> GetShoppingCart()
        {
            var cart = ApplicationContext.GetSessionData<List<OrderDetailViewInfo>>(CART);
            if (cart == null)
            {
                cart = new List<OrderDetailViewInfo>();
                ApplicationContext.SetSessionData(CART, cart);
            }
            return cart;
        }
        /// <summary>
        /// thêm giỏ hàng vào giỏ
        /// </summary>
        /// <param name="data"></param>
        public static void AddItemToCart(OrderDetailViewInfo data)
        {
            var cart = GetShoppingCart();

            var existItem = cart.Find(m => m.ProductID == data.ProductID);
            if (existItem == null)
            {
                cart.Add(data);
            }
            else
            {
                existItem.Quantity += data.Quantity;
                existItem.SalePrice = data.SalePrice;
            }
            ApplicationContext.SetSessionData(CART, cart);
        }

        public static void UpdateCartItem(int productId, int quantity, decimal salePrice)
        {
            var cart = GetShoppingCart();
            var existItem = cart.Find(m => m.ProductID == productId);
            if (existItem != null)
            {
                existItem.Quantity = quantity;
                existItem.SalePrice = salePrice;
                ApplicationContext.SetSessionData(CART, cart);
            }
        }
        /// <summary>
        /// xoá một mặt hàng khỏi giỏ dựa vào mã hàng
        /// </summary>
        /// <param name="productId"></param>
        public static void RemoveItemFromCart(int productId)
        {
            var cart = GetShoppingCart();
            int index = cart.FindIndex(m => m.ProductID == productId);
            if (index >= 0)
            {
                cart.RemoveAt(index);
                ApplicationContext.SetSessionData(CART, cart);
            }
        }
        /// <summary>
        /// xóa trống giỏ hàng 
        /// </summary>
        public static void ClearCart()
        {
            var cart = new List<OrderDetailViewInfo>();
            ApplicationContext.SetSessionData(CART, cart);
        }
    }
}
